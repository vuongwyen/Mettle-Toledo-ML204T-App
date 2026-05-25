# BIÊN BẢN BÁO CÁO KỸ THUẬT
## Hệ Thống Thu Thập Dữ Liệu Cân (Scale Data Collection System)

---

**Phiên bản:** 1.1  
**Ngày lập:** 2026-05-25  
**Phạm vi:** Phân tích mã nguồn, luồng dữ liệu, và cơ chế đồng bộ luồng  
**Trạng thái:** Hoàn chỉnh (Post-Patch)

---

## MỤC LỤC

1. [Module Breakdown — Bảng Vai Trò Thành Phần](#1-module-breakdown)
2. [Data Flow Mapping — Sơ Đồ Luồng Dữ Liệu](#2-data-flow-mapping)
3. [Threading Mechanism — Báo Cáo Đồng Bộ Luồng](#3-threading-mechanism)
4. [Rủi Ro Kỹ Thuật (Bottlenecks) & Tình Trạng Khắc Phục](#4-rủi-ro-kỹ-thuật-bottlenecks--tình-trạng-khắc-phục)
5. [Security & Compliance — Kiểm Soát Bảo Mật](#5-security--compliance--kiểm-soát-bảo-mật)

---

## 1. MODULE BREAKDOWN

Bảng dưới đây liệt kê vai trò của từng thành phần, bao gồm **Input**, **Output**, và **cơ chế bảo vệ luồng** sau khi đã được nâng cấp.

### 1.1 Tầng Kết Nối & Phân Tích Cú Pháp

| Class / File | Input | Output | Ghi Chú |
| :--- | :--- | :--- | :--- |
| `ConnectionManager.cs` | IP:Port do người dùng nhập; Lệnh `SIR` sau handshake | Sự kiện `OnDataReceived(string rawData)` phát cho subscriber; Sự kiện `OnStateChanged(bool, bool)` | Chạy vòng lặp đọc TCP trên Thread Pool. Gửi chuỗi lệnh I2, I4 để handshake, sau đó lệnh SIR kích hoạt stream dữ liệu tần số cao (10Hz). |
| `MtSicsParser.cs` | Chuỗi ASCII thô (vd: `S S   1.5204 g`) | `ScaleData?` struct (Status, Weight decimal, Unit string) | Static class. Biên dịch mã máy với Compiled Regex để xử lý tốc độ cao không trễ. |
| `ScaleInfo.cs` | Chuỗi phản hồi lệnh `I2`, `I4` từ đầu cân | Struct `ScaleInfo` (Model, SerialNumber, IpAddress) | Được `ConnectionManager` điền sau handshake thành công. |

### 1.2 Tầng Lưu Trữ Nội Địa (SQLite)

Hệ thống bảo toàn tuyệt đối độ chính xác của kiểu `decimal` bằng cách lưu vào SQLite dưới dạng trường `TEXT`. Điều này loại bỏ hoàn toàn các lỗi sai số nhị phân (Floating-point precision issues).

| Class / File | Input | Output | Ghi Chú |
| :--- | :--- | :--- | :--- |
| `DatabaseHelper.cs` | Biến môi trường `TESA_DB_KEY` | Khởi tạo file `ScaleData.db` với mã hóa AES-256 (SQLCipher). | Static class. Quản lý `DbAccessLock` (SemaphoreSlim) dùng chung toàn hệ thống. |
| `DataRepository.cs` | `ScaleRecord` (domain model, Weight = `decimal`) | Bản ghi được ghi vào SQLite; `List<ScaleRecord>` khi đọc | Được bảo vệ bởi `DatabaseHelper.DbAccessLock.Wait()`. Thread-safe cho các tác vụ UI. |
| `DatabaseService.cs` | `ScaleRecord`; `string deviceId` | Ghi SQLite với cờ `IsSynced`; Gọi `NetworkService.PushDataAsync` | Sử dụng chung `DatabaseHelper.DbAccessLock` với luồng UI để đảm bảo không xung đột ghi (Deadlock-free). Tự động đẩy bù ngầm mỗi 30 giây. |

### 1.3 Tầng Mạng & Đồng Bộ Cloud (WLAN)

| Class / File | Input | Output | Ghi Chú |
| :--- | :--- | :--- | :--- |
| `AuthService.cs` | Username / Password | Lưu JWT Token trên bộ nhớ (RAM) | Singleton. Xử lý đăng nhập / xác thực JWT, không lưu credential xuống disk. |
| `NetworkService.cs` | `List<Test.Models.ScaleRecord>` (Weight = `decimal`) | `List<Guid>` các ID server xác nhận đã lưu | Tích hợp `BearerTokenHandler` để đính kèm JWT per-request. Tự sinh `X-Idempotency-Key`. Chỉ cho phép kết nối HTTPS an toàn. |
| `Test.Models.ScaleRecord` | — (DTO) | JSON payload đẩy lên server | Trọng lượng (Weight) sử dụng kiểu `decimal` nhằm giữ nguyên độ chính xác khi truyền mạng. |
| `Test.Models.SyncPayload<T>` | — (DTO wrapper) | JSON envelope chứa `deviceId`, `syncTime`, `data` | Wrapper dùng khi gọi `POST /api/scale/sync`. |

### 1.4 Tầng Giao Diện (UI)

| Class / File | Input | Output | Ghi Chú |
| :--- | :--- | :--- | :--- |
| `Form1.cs` | Sự kiện nhận dữ liệu TCP (10Hz) | Hiển thị trọng lượng; Vẽ đồ thị OxyPlot | Sử dụng cơ chế `ConcurrentQueue<ScaleData>` và `WinForms.Timer(100ms)` rút dữ liệu xử lý theo lô. Không sử dụng `BeginInvoke`. |
| `AppColors.cs` | — | Hằng số màu sắc cho UI | Static class. |

---

## 2. DATA FLOW MAPPING

### 2.1 Luồng 1 — Thu Thập Dữ Liệu Thời Gian Thực (LAN → UI)

Đây là cơ chế Producer-Consumer lock-free mới, giúp luồng giao diện tránh được nguy cơ quá tải (UI Flooding) từ dữ liệu tần số 10Hz.

```plaintext
┌─────────────────────────────────────────────────────────────────────┐
│  SCALE DEVICE (Đầu Cân)                                             │
│  Lệnh khởi tạo stream: SIR (10Hz)                                   │
└──────────────────────────────┬──────────────────────────────────────┘
                               │ Chuỗi ASCII (vd: "S S 1.5204 g\n")
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│  ConnectionManager.ReadLoopAsync()          [BACKGROUND THREAD]     │
│  ─────────────────────────────────────────────────────────────────  │
│  • Đọc socket TCP bằng await StreamReader.ReadLineAsync()           │
│  • Parser.Parse() trả về cấu trúc ScaleData                         │
│  • Đưa dữ liệu thẳng vào: _dataQueue.Enqueue(scaleData)             │
└──────────────────────────────┬──────────────────────────────────────┘
                               │ (Lock-free ConcurrentQueue)
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Form1.UiTimer_Tick(100ms)                  [WINFORMS UI THREAD]    │
│  ─────────────────────────────────────────────────────────────────  │
│  • Quét lấy mọi điểm mới: while (_dataQueue.TryDequeue(out data))   │
│  • Lọc lấy dữ liệu mới nhất cập nhật Text (Chống giật hiển thị)     │
│  • Đưa lô dữ liệu (Batch) vào mảng vẽ biểu đồ OxyPlot              │
│  • Kích hoạt InvalidPlot() duy nhất 1 lần                           │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Luồng 2 — Lưu Trữ & Đồng Bộ Cloud (Data → API / Fallback SQLite)

```plaintext
┌─────────────────────────────────────────────────────────────────────┐
│  SaveCurrentWeight() / Auto-Poll            [WINFORMS UI THREAD]    │
│  • Gọi _databaseService.SaveRecordAsync(record)                    │
└──────────────────────────────┬──────────────────────────────────────┘
                               │ ScaleRecord (Decimal)
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│  DatabaseService.SaveRecordAsync(record)                            │
│  ─────────────────────────────────────────────────────────────────  │
│  Bước 1: Ping Health Check Cloud Server (Tối đa 3 giây)             │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
              ┌────────────────┴─────────────────┐
              │ isOnline == true                 │ isOnline == false
              ▼                                  ▼
┌─────────────────────────┐         ┌─────────────────────────────────┐
│  NHÁNH A: ONLINE        │         │  NHÁNH B: OFFLINE               │
│  ─────────────────────  │         │  ───────────────────────────    │
│  1. MapToApiModel       │         │  1. Ghi vào AES-256 SQLite:     │
│  2. NetworkService      │         │       IsSynced = 0              │
│     .PushDataAsync()    │         │  2. Return (Hoạt động bình      │
│     POST (HTTPS + JWT)  │         │     thường không lỗi UI).       │
│  3. Ghi vào AES-256     │         └──────────────┬──────────────────┘
│     SQLite: IsSynced = 1│                        │
└──────────────┬──────────┘                        │
               │                                   ▼
               │                    ┌─────────────────────────────────┐
               │                    │  DatabaseService.SyncPendingAsync│
               │                    │  (Background Timer — mỗi 30s)   │
               │                    │  ─────────────────────────────  │
               │                    │  1. Khóa DbAccessLock.Wait()     │
               │                    │  2. SELECT IsSynced=0 (Max 100)  │
               │                    │  3. PushDataAsync(batch)         │
               │                    │  4. UPDATE IsSynced=1            │
               │                    │  5. Nhả khóa DbAccessLock        │
               └────────────────────┤                                  │
                                    └──────────────┬──────────────────┘
                                                   │
                                                   ▼
                                    ┌──────────────────────────────────┐
                                    │  CENTRAL SaaS SERVER             │
                                    │  POST /api/scale/sync            │
                                    │  Phản hồi: { syncedIds: Guid[] } │
                                    └──────────────────────────────────┘
```

---

## 3. THREADING MECHANISM

### 3.1 Bảng Các Biến Và Cấu Trúc Đồng Bộ Luồng (Post-Patch)

| Biến / Thành Phần | Kiểu | Thread-Safe? | Cơ Chế Bảo Vệ | File |
| :--- | :--- | :---: | :--- | :--- |
| `DatabaseHelper.DbAccessLock` | `SemaphoreSlim(1,1)` | ✅ | Static. Bảo vệ chung các thao tác ghi/đọc SQLite từ luồng UI (`DataRepository`) và Timer chạy ngầm (`DatabaseService`). | `DatabaseHelper.cs` |
| `_dataQueue` | `ConcurrentQueue<ScaleData>` | ✅ | Lock-free, đảm bảo an toàn thao tác cho Enqueue từ luồng mạng và Dequeue hàng loạt trên luồng UI. | `Form1.cs` |
| `_uiTimer` | `System.Windows.Forms.Timer` | ✅ | Bộ đếm nhịp 100ms trên luồng UI để batching hiển thị, loại bỏ việc marshalling thủ công qua Windows Queue. | `Form1.cs` |
| `AuthService._instance` | `Lazy<AuthService>` | ✅ | Singleton kết hợp khóa chặn `lock(_tokenLock)` mỗi lần Get/Set/Clear Access Token. | `AuthService.cs` |
| `NetworkService._httpClient` | `HttpClient` | ✅ | Static, tái sử dụng xuyên suốt vòng đời. Có thêm `BearerTokenHandler` thread-safe. | `NetworkService.cs` |
| `_connectLock` | `SemaphoreSlim(1,1)` | ✅ | Ngăn nhiều vòng reconnect chạy song song. `WaitAsync(0)`. | `ConnectionManager.cs` |

---

## 4. RỦI RO KỸ THUẬT (BOTTLENECKS) & TÌNH TRẠNG KHẮC PHỤC

Các rủi ro hiệu suất đã được xác định và xử lý dứt điểm trong đợt refactor (`/enhance`).

### Danh sách khắc phục

| ID | Vị Trí / Mô Tả Cũ | Tình Trạng Khắc Phục | Ghi Chú Patch |
| :--- | :--- | :---: | :--- |
| **R-01** | `BeginInvoke` gây nghẽn UI Message Queue | ✅ Đã vá | Chuyển sang mô hình Producer-Consumer dùng `ConcurrentQueue` và Timer 100ms xử lý theo lô. Không còn hiện tượng giật hiển thị. |
| **R-02** | `DataRepository` và `DatabaseService` ghi đụng độ SQLite | ✅ Đã vá | Áp dụng khóa chung `DatabaseHelper.DbAccessLock` (SemaphoreSlim) bọc toàn bộ các hàm đọc ghi. Đảm bảo toàn vẹn giao dịch. |
| **R-03** | Khả năng duplicate data khi timeout đẩy batch lên API | ✅ Đã vá | Bổ sung `ComputeIdempotencyKey()` tạo ra `X-Idempotency-Key` (MD5 hash) duy nhất cho mỗi batch. Dù client timeout và đẩy lại, server vẫn nhận diện đúng lô trùng. |
| **R-04** | Sai số nhị phân `double` khi truyền JSON qua API | ✅ Đã vá | Đồng nhất sử dụng `decimal` từ Domain Model (`ScaleRecord`) cho đến DTO API và `JsonSerializer`. Bãi bỏ ép kiểu số thực. |

---

## 5. SECURITY & COMPLIANCE — KIỂM SOÁT BẢO MẬT

Các lỗ hổng bảo mật cấp thiết đã được nâng cấp dựa trên nguyên tắc Zero-Trust.

### 5.1 Data at Rest — Mã Hóa Dữ Liệu Lưu Trữ Nội Địa (SQLite)
- **Tình trạng:** ✅ Hoàn tất mã hóa AES-256.
- Đã cài đặt module `SQLitePCLRaw.bundle_e_sqlcipher` (v2.1.10).
- Hệ thống trích xuất key qua `TESA_DB_KEY` (Environment Variables) truyền vào Connection String dưới tham số `Password=`.

### 5.2 Secrets Management — Quản Lý Thông Tin Nhạy Cảm
- **Tình trạng:** ✅ Đã làm sạch mã nguồn.
- URL API được kiểm tra nghiêm ngặt `StartsWith("https://")`.
- **Đã xóa bỏ hoàn toàn chuỗi fallback `http://localhost:5000`**. Chương trình sẽ ném `InvalidOperationException` nếu env var `SCALE_API_URL` không hợp lệ.

### 5.3 Data in Transit & IAM — Mã Hóa Truyền Tải & Xác Thực Danh Tính
- **Tình trạng:** ✅ Triển khai HTTPS Enforced + JWT Authentication.
- Kiến trúc tạo mới lớp `AuthService.cs` quản lý thông tin bảo mật JWT trong RAM.
- Gắn `BearerTokenHandler` (DelegatingHandler) vào gốc của pipeline `HttpClient`, tự động đính header `Authorization: Bearer <Token>` cho mọi payload gửi về server.

### 5.4 Ma Trận Tuân Thủ Bảo Mật Tổng Hợp

| Hạng Mục Kiểm Soát | Yêu Cầu Theo Plan | Trạng Thái Thực Tế | Ghi Chú |
| :--- | :--- | :---: | :--- |
| **Data at Rest** | SQLCipher AES-256 | ✅ Triển khai | Khởi chạy cùng `TESA_DB_KEY`. |
| **Secrets Management** | Env Var cho API URL / Key | ✅ Triển khai | Không còn hardcoded plaintext HTTP fallback. |
| **Data in Transit** | HTTPS (SSL/TLS) | ✅ Triển khai | Buộc validate endpoint `https://`. |
| **IAM** | JWT Authentication | ✅ Triển khai | Bộ lọc DelegatingHandler tự động nhúng Token. |
| **SQL Injection** | Parameterized Queries | ✅ Triển khai | Tham số hóa (Parameterized) chuẩn mực. |
| **Safe Parsing** | Validated parsing | ✅ Triển khai | Kỹ thuật `decimal.TryParse` bảo vệ đầu vào từ Socket. |

---

*Báo cáo được trích xuất dựa trên logic code thực tế tại thời điểm 2026-05-25 (Bản cập nhật Post-Patch).*
