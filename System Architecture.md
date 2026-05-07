# 🏗️ PHẦN 1: BẢN THIẾT KẾ HỆ THỐNG (SYSTEM ARCHITECTURE BLUEPRINT)

Hệ thống được thiết kế theo mô hình **3-Tier Architecture (3 Lớp)** ngay trên ứng dụng Desktop, đảm bảo tính tách biệt, dễ bảo trì và không gây nghẽn giao diện (UI Thread).

### 1. Lớp Giao diện (Presentation Layer - UI)
* **Công nghệ:** Windows Forms (.NET 10 LTS).
* **Cơ chế Layout:** Sử dụng `TableLayoutPanel` và thuộc tính `Dock`/`Anchor` để giao diện Responsive (co giãn tự động).
* **Cấu trúc UI:**
    * **Tab 1 (Dashboard):** Quản lý kết nối (Network Config), Nhập tham chiếu (Context Data), và Màn hình Live View (chữ số cỡ lớn, đổi màu theo trạng thái ổn định/dao động).
    * **Tab 2 (Data Sheet):** Bảng `DataGridView` (chỉ đọc) hiển thị lịch sử đo, kèm nút Export CSV.

### 2. Lớp Xử lý Logic (Business Logic Layer)
Phân tách thành các Service riêng biệt chạy ngầm (Background Tasks):
* **`ConnectionManager`:** Quản lý vòng đời của `TcpClient`. Chịu trách nhiệm mở kết nối, ngắt kết nối, bắt lỗi rớt mạng (SocketException) và kích hoạt cơ chế Tự động kết nối lại (Auto-Reconnect).
* **`StreamListener`:** Một luồng (Task) chạy độc lập, liên tục hứng chuỗi byte ASCII từ cân gửi về.
* **`MtSicsParser`:** Bộ giải mã đặc nhiệm. Nhận chuỗi thô, cắt bỏ khoảng trắng, kiểm tra ký tự đầu (`S S` hoặc `S D`), loại bỏ đơn vị (`g`), và ép kiểu sang `decimal`.
* **`ThreadDispatcher`:** Đảm nhiệm việc dùng `Control.Invoke` để đẩy dữ liệu an toàn từ luồng ngầm (Background Thread) lên màn hình chính (UI Thread).

### 3. Lớp Dữ liệu (Data Access Layer & Storage)
* **Công nghệ:** SQLite (sử dụng thư viện `Microsoft.Data.Sqlite`).
* **Kho lưu trữ:** Một file duy nhất `ScaleData.db` nằm cùng thư mục gốc của file thực thi (`.exe`). Không cần cài đặt máy chủ CSDL.
* **Dịch vụ (Services):**
    * **`DatabaseInitializer`:** Tự động tạo file DB và cấu trúc bảng `ScaleRecords` nếu chưa tồn tại (khi bật app lần đầu).
    * **`DataRepository`:** Chứa các hàm Insert (lưu bản ghi khi Polling) và Select (lấy dữ liệu đổ ra Grid hoặc Export).
    * **`CsvExportService`:** Sử dụng thư viện `CsvHelper` để chuyển đổi danh sách object thành file `.csv` tốc độ cao.

### 4. Luồng Truyền thông (Communication Flow)
* **Giao thức:** TCP/IP Socket (Local LAN).
* **Ngôn ngữ giao tiếp:** Tập lệnh MT-SICS của Mettler Toledo.
    * *App -> Cân (Khi Connect):* Gửi lệnh `SIR\r\n` (Yêu cầu cân stream liên tục).
    * *Cân -> App (Liên tục):* Trả về `S D <weight> g\r\n` (đang dao động) hoặc `S S <weight> g\r\n` (ổn định).
    * *App -> Cân (Khi bấm Chốt số):* Gửi lệnh `S\r\n` (Yêu cầu giá trị ổn định duy nhất để lưu DB).

---

# 🚀 PHẦN 2: LỘ TRÌNH TRIỂN KHAI (IMPLEMENTATION ROADMAP)

Lộ trình này được chia làm 4 Sprint (Giai đoạn). Bạn có thể giao từng Sprint cho Gemini 3.1 xử lý để kiểm soát chất lượng code.

### Sprint 1: Dựng Khung (Scaffolding & UI)
**Mục tiêu:** Hoàn thiện giao diện tĩnh và khởi tạo cơ sở dữ liệu.
1.  Khởi tạo dự án WinForms App (.NET 10).
2.  Thiết kế Giao diện (kéo thả các Control, thiết lập Layout chia tỷ lệ phần trăm).
3.  Cài đặt các gói NuGet (`Microsoft.Data.Sqlite`, `CsvHelper`).
4.  *(Task cho AI)*: Viết class `DatabaseHelper` để khởi tạo file `ScaleData.db` và tạo bảng `ScaleRecords`.

### Sprint 2: Động cơ Thời gian thực (Real-time Engine)
**Mục tiêu:** Thiết lập kết nối TCP và hiển thị số nhảy liên tục trên màn hình.
1.  *(Task cho AI)*: Viết class `ConnectionManager` khởi tạo `TcpClient` trong một `Task` ngầm.
2.  *(Task cho AI)*: Viết class `MtSicsParser` bằng Regex hoặc String Split để bóc tách chuỗi `S S 50.0123 g`.
3.  *(Task cho AI)*: Viết logic đẩy số liệu từ Parser lên nhãn `lblLiveWeight` bằng lệnh `Invoke`, kết hợp đổi màu giao diện (xanh/đỏ/cam).

### Sprint 3: Quản lý Phiên & Lưu trữ (Session & Storage)
**Mục tiêu:** Ráp nối luồng dữ liệu vào Database và hiển thị lên bảng.
1.  *(Task cho AI)*: Viết logic cho nút "Chốt số liệu" (Polling): Khi bấm, gom giá trị cân hiện tại và các trường Context (NAT, Batch, Vị trí) thành một đối tượng `ScaleRecord`, sau đó gọi `DatabaseHelper.Insert`.
2.  *(Task cho AI)*: Viết hàm Load dữ liệu từ SQLite đổ vào `DataGridView` ở tab Data Sheet.
3.  *(Task cho AI)*: Viết logic cho nút Export CSV sử dụng `SaveFileDialog`.

### Sprint 4: Đóng gói & Xử lý sự cố (Hardening & Packaging)
**Mục tiêu:** Làm cho phần mềm "bất tử" trước các lỗi vật lý và sẵn sàng cài đặt.
1.  *(Task cho AI)*: Viết cơ chế Try-Catch đặc biệt cho luồng TCP: Khi rút cáp mạng, phần mềm không được crash mà chuyển UI sang màu đỏ (Disconnected) và bắt đầu vòng lặp thử kết nối lại sau mỗi 3 giây.
2.  *(Task cho AI)*: Xử lý ngoại lệ (Exception Handling) khi người dùng nhập sai IP/Port (Validate Input).
3.  **Đóng gói (Publish):** Cấu hình tính năng Publish của Visual Studio để xuất ra file thực thi độc lập (Self-contained, Single-file) chứa toàn bộ Runtime .NET 10, copy sang máy xưởng là chạy ngay.
