# Tesa Lab (Mettler Toledo ML204T App)

## 1. Tổng quan dự án (Overview)
- **Làm gì:** App WinForms tự động đọc số cân Mettler Toledo (chuẩn MT-SICS) qua cổng COM, lưu SQLite, đồng bộ tự động lên Cloud Server.
- **Phục vụ ai:** Kỹ thuật viên Tesa Lab. Giải quyết bài toán nhập liệu tay sai sót, mất dữ liệu.
- **Trạng thái:** Đang chạy Production.
- **Link:** [Repo Local], [Tài liệu hãng Mettler Toledo ML204T/M00]

## 2. Kiến trúc hệ thống (Architecture)
- **Sơ đồ:** Cân ML204T -> RS232 (Cổng COM) -> WinForms App (Background Thread) -> SQLite (Local) -> Background API Sync -> ScaleDataServer.
- **Tech stack:** .NET 10.0 (WinForms). EF Core (SQLite). ClosedXML (Excel). CsvHelper. OxyPlot (Biểu đồ).
- **Luồng dữ liệu:** COM Port nhả string -> MT-SICS Parser bóc tách -> EF Core Insert DB -> NetworkSyncWorker nhặt DB đẩy lên API.

## 3. Cấu trúc thư mục (Project structure)
```text
Test/
├── Data/            # EF Core DbContext. Cầu nối Database.
├── Models/          # Entity class (ScaleRecord).
├── Services/        # Logic lõi (COM Port, Sync API, Export/Import).
├── Form1.cs         # Giao diện chính (Dashboard, Data Grid).
├── AppConfig.cs     # Cấu hình API Key lưu JSON.
└── Test.csproj      # File gốc dự án.
```
- **Quy ước:** Services xử lý ngầm, bọc SemaphoreSlim chống lock. UI Thread chỉ vẽ và gọi hàm.

## 4. Hướng dẫn cài đặt môi trường (Setup)
- **Yêu cầu:** Máy tính Windows 10/11. Đã cắm cáp USB-RS232 nối với cân.
- **Cài đặt:** Cài .NET 10.0 SDK.
- **Steps:**
```bash
cd Test
dotnet restore
```
- **Cấu hình:** App không dùng `.env`. Mọi setting (COM Port, API URL) cấu hình trên UI (Lưu vào SQLite `Settings` table). Khóa bí mật API lưu vào `appsettings.json` bằng AppConfig.

## 5. Cách chạy và build
```bash
# Code/Dev: Mở Test.sln bằng Visual Studio
# Build xuất xưởng:
dotnet build --configuration Release
# Chạy thẳng:
dotnet run
```
- File `.exe` sẽ bung ra ở `bin/Release/net10.0-windows/`. Cầm vứt sang máy khác chạy luôn.

## 6. Database & Data model
- **DB:** SQLite `ScaleData.db`.
- **Model chính:** `ScaleRecord` (Id, WeightValue, Unit, TestedAt, IsSynced, Nart, BatchCode...).
- **Migration:** Code-First bằng EF Core. Đổi schema thì `Add-Migration` rồi `Update-Database`. App có cắm sắn `context.Database.Migrate()` lúc khởi động.

## 7. Các module/tính năng quan trọng
- **ConnectionManager.cs:** Bắt SerialPort sự kiện DataReceived. Parse chuỗi `S S Weight g` (MT-SICS). Cực kỳ nhạy cảm với rác cổng COM.
- **NetworkSyncWorker.cs:** Vòng lặp ngầm 5s check DB. Lấy bản ghi `IsSynced = 0` đẩy qua `/api/scale/sync`. Kèm `Idempotency-Key` (Guid) và `X-Api-Key`. Lỗi mạng thì ngâm đó, có mạng tự đẩy tiếp.
- **Form1.cs:** Dashboard Real-time. Dùng Timer giật OxyPlot update đồ thị.

## 8. Authentication/Authorization & bảo mật
- **Trên App:** Nút "Đăng nhập Admin" góc trái. Nhập pass cứng để mở khóa Tab `Settings` (Tránh công nhân bấm nhầm đổi cổng COM).
- **Giao tiếp Server:** App dùng `X-Api-Key` đính ở Header để thông chốt Server. Key này nhập trong tab Settings (chỉ Admin thấy).

## 9. Tích hợp bên thứ ba (Third-party integrations)
- **ScaleDataServer (Nội bộ):** API nhận dữ liệu cân. Giới hạn 50 bản ghi/lần bắn để mượt mạng.
- **Thiết bị cân Mettler Toledo:** Cắm cáp đọc sống, không cần cài tool hãng.

## 10. Testing
- Test thủ công với cân thật.
- **Chú ý:** Sửa code luồng COM Port phải cắm giả lập RS232 (Virtual Serial Port) băm chuỗi test liên tục xem app có chết Thread không. Chưa có Unit Test.

## 11. Deployment & CI/CD
- **Deploy:** Build Release thủ công ra folder `bin`. Nén ZIP quăng qua Zalo hoặc copy USB cài vào máy trạm phòng Lab.
- **Rollback:** Cóp lại bản `.exe` cũ. Database tự tương thích nếu không xóa cột.

## 12. Monitoring & Logging
- **Màn hình:** Thanh trạng thái dưới cùng nháy xanh (Đồng bộ OK) hoặc đỏ (Rớt mạng/Lỗi API).
- Hiện chưa tích hợp Sentry. Debug bằng try-catch báo thẳng lên MessageBox.

## 13. Known issues / Technical debt
- **Database is locked:** Đã fix bằng `SemaphoreSlim` ở `DatabaseHelper` ép luồng xếp hàng. Không được đụng vào cơ chế này.
- **Technical Debt:** File `Form1.cs` ôm hơi nhiều logic (God Object). Mùa sau rảnh thì tách bớt logic UI ra mô hình MVP/MVVM.

## 14. Liên hệ & tài nguyên khác
- **Email Hỏi đáp:** treepoo2023@gmail.com.
- **Tài liệu:** Đọc "Mettler Toledo MT-SICS Reference Manual" (Tìm Google model ML204T/M00) để hiểu chuỗi Hex Cân trả về.
