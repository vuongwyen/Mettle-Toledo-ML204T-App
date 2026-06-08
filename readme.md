# Tesa Lab Data Server (Scale Integration System)

Ứng dụng quản lý và thu thập dữ liệu tự động từ các thiết bị Cân điện tử (chuẩn MT-SICS) thông qua cổng COM (RS232). Phần mềm cung cấp giải pháp lưu trữ cục bộ mã hóa bằng SQLite (EF Core), khả năng đồng bộ dữ liệu lên Server Đám mây (Cloud), và phân tích dữ liệu trực quan.

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![.NET](https://img.shields.io/badge/.NET-10.0-purple)]()
[![EF Core](https://img.shields.io/badge/EF_Core-SQLite-blue)]()
[![License](https://img.shields.io/badge/license-MIT-blue)]()

---

## 📋 Mục lục
- [Tính năng chính](#-tính-năng-chính-features)
- [Yêu cầu hệ thống](#-yêu-cầu-hệ-thống-requirements)
- [Cài đặt](#-cài-đặt-installation)
- [Cấu trúc thư mục](#-cấu-trúc-thư-mục-project-structure)
- [Hướng dẫn sử dụng](#-hướng-dẫn-sử-dụng-usage)
- [Quản trị & Phân quyền](#-quản-trị--phân-quyền-roles)
- [Kiến trúc Database](#-kiến-trúc-database)
- [Troubleshooting](#-troubleshooting)
- [Đóng góp](#-đóng-góp-contributing)
- [License](#-license)

---

## ✨ Tính năng chính (Features)
- ✅ **Giao tiếp Cân Tự Động:** Đọc dữ liệu liên tục từ các dòng cân hỗ trợ chuẩn giao thức **MT-SICS** (Mettler Toledo) qua cổng Serial (COM).
- ✅ **Lưu trữ cục bộ An Toàn:** Sử dụng **SQLite** kết hợp công nghệ ORM **Entity Framework Core**. Có khả năng tương thích với mã hóa SQLCipher AES-256.
- ✅ **Đồng bộ Cloud:** Service chạy ngầm tự động gom dữ liệu đẩy lên Server trung tâm qua HTTP POST API, đảm bảo không rớt gói tin kể cả khi mất mạng.
- ✅ **Quản lý dữ liệu trực quan:** Bảng DataGridView cho phép chỉnh sửa nội tuyến (inline-edit), lọc theo cột, tìm kiếm siêu tốc và xóa hàng loạt (Bulk Delete).
- ✅ **Export / Import:** Xuất nhập dữ liệu chuẩn định dạng `.xlsx` (ClosedXML) và `.csv` (CsvHelper).
- ✅ **Phân quyền (Roles):** Tách biệt không gian thao tác giữa `Admin` và `User` để bảo mật cấu hình cổng COM và URL máy chủ.
- ✅ **Biểu đồ (Analytics):** Dashboard thống kê tổng tải trọng và số lượng mẫu đo được theo thời gian thực (Real-time OxyPlot).

---

## 💻 Yêu cầu hệ thống (Requirements)

- **Hệ điều hành:** Windows 10 / Windows 11 (yêu cầu hỗ trợ WinForms).
- **Runtime:** .NET 10.0 SDK (hoặc tương đương cấu hình trong tệp `.csproj`).
- **Phần cứng:** Có cổng USB / COM Port RS232, Cáp kết nối RS232-to-USB.
- **Thư viện bên thứ 3 (Nuget):**
  - `Microsoft.EntityFrameworkCore.Sqlite.Core` (Quản lý Database)
  - `ClosedXML` (Xử lý Excel)
  - `CsvHelper` (Xử lý CSV)
  - `OxyPlot.WindowsForms` (Vẽ biểu đồ)

---

## 📦 Cài đặt (Installation)

Tải Source Code về máy và biên dịch bằng CLI hoặc Visual Studio.

```bash
# 1. Clone repository hoặc giải nén source code
cd path/to/Test

# 2. Khôi phục (Restore) các thư viện Nuget cần thiết
dotnet restore

# 3. Biên dịch dự án
dotnet build --configuration Release

# 4. Chạy ứng dụng (hoặc mở file .exe trong thư mục bin/Release)
dotnet run
```

---

## 📂 Cấu trúc thư mục (Project Structure)

```text
Test/
├── Data/
│   └── ScaleDbContext.cs       # Cấu hình kết nối và ánh xạ EF Core
├── Models/
│   └── ScaleRecord.cs          # Model ánh xạ Database và API Payload (JSON)
├── Services/
│   ├── ConnectionManager.cs    # Lõi xử lý SerialPort và giao thức MT-SICS
│   ├── DatabaseHelper.cs       # Quản lý SemaphoreSlim chống lock đa luồng
│   ├── DatabaseService.cs      # Background Thread đồng bộ dữ liệu lên Server
│   ├── CsvExportService.cs     # Logic Export/Import CSV
│   └── ExcelExportService.cs   # Logic Export/Import Excel
├── Form1.cs                    # UI logic chính (WinForms)
├── Form1.Designer.cs           # Giao diện tĩnh sinh tự động
├── ScaleRecord.cs              # Entity chính của EF Core
├── Test.csproj                 # Cấu hình Dependencies & Target Framework
└── README.md                   # Tài liệu bàn giao này
```

---

## 🚀 Hướng dẫn sử dụng (Usage)

1. Mở ứng dụng, tại màn hình **Dashboard**, bấm `Đăng nhập Admin` (Mật khẩu mặc định tùy hệ thống bàn giao).
2. Chuyển sang Tab **Settings** (Chỉ hiện khi là Admin):
   - Chọn đúng cổng COM đang kết nối với Cân.
   - Nhập **Server API URL** để đồng bộ dữ liệu.
3. Quay lại **Dashboard**, bấm nút **▶ Bắt đầu chạy Cân**. 
   - Ứng dụng sẽ khóa các cài đặt và bắt đầu lắng nghe cổng COM. 
   - Trọng lượng ổn định sẽ tự động nhảy số lên màn hình và lưu vào Local DB.

---

## 🔐 Quản trị & Phân quyền (Roles)

Hệ thống có 2 cấp quyền:
- **User (Mặc định):** Chỉ có thể xem dữ liệu, xuất file (Export), theo dõi biểu đồ. Không thể sửa thiết lập hay nhập dữ liệu rác (Import).
- **Admin:** Bấm nút "Đăng nhập Admin" ở góc trên bên trái. Sau khi xác thực, Tab `Settings` sẽ xuất hiện, đồng thời nút `Nhập dữ liệu` (Import) sẽ được kích hoạt (chuyển từ Xám sang Xanh lá).

---

## 🗄 Kiến trúc Database (EF Core)

Hệ thống sử dụng **Entity Framework Core** với SQLite.
- File vật lý: `ScaleData.db` (Nằm cùng thư mục với file `.exe`).
- Cấu trúc bảo vệ: Mọi truy vấn Insert, Update, Delete đều bị bọc trong cờ lê `DatabaseHelper.DbAccessLock.Wait()`. Điều này giải quyết hoàn toàn bài toán **"Database is locked"** khét tiếng của SQLite khi Background Thread (Đồng bộ) và UI Thread (Người dùng thao tác bảng) chạm nhau.
- Tính dễ mở rộng (Scalability): Lõi DataRepository được viết thuần túy bằng LINQ. Trong tương lai IT Engineer chỉ cần vào `ScaleDbContext.cs`, đổi `UseSqlite()` thành `UseSqlServer()` là có thể cắm thẳng lên máy chủ MS SQL Server của công ty mà không cần viết lại câu lệnh SQL.

---

## 🛠 Troubleshooting (Xử lý sự cố thường gặp)

**1. Không tìm thấy cổng COM Port trên UI?**
- **Giải pháp:** Kiểm tra lại Driver của dây cáp USB-RS232 (như cáp CH340, PL2303, FTDI). Hãy vào `Device Manager` của Windows để xem thiết bị có bị chấm than vàng không.

**2. Báo lỗi "The data is NULL at ordinal X" khi mở tab Data Sheet?**
- **Nguyên nhân:** Có một dòng dữ liệu rác trong `ScaleData.db` chứa cột bị `NULL` mà bản thiết kế code không lường trước.
- **Giải pháp:** Lỗi này đã được fix triệt để. Tuy nhiên nếu tái diễn, IT Engineer cần check lại file `ScaleRecord.cs`, đảm bảo mọi biến có thể nhận NULL từ DB đều có dấu chấm hỏi `?` (ví dụ: `public string? SampleName { get; set; }`).

**3. Bấm "Nhập dữ liệu" không phản hồi?**
- **Giải pháp:** Nhìn màu nút. Nếu nút màu Xám, nghĩa là bạn đang ở quyền User. Hãy ấn "Đăng nhập Admin".

**4. Dữ liệu cân không nảy lên màn hình?**
- **Giải pháp:** Chuẩn giao thức hiện tại là `MT-SICS`. Nếu sử dụng cân hãng khác (Cas, Ohaus...), cần cấu hình lại hàm `MtSicsParser.Parse()` để phù hợp với định dạng chuỗi chuỗi Hex/ASCII trả về từ cân đó.

---

## 🤝 Đóng góp (Contributing)

Khi bàn giao, các kỹ sư muốn nâng cấp hệ thống vui lòng tuân thủ quy tắc:
1. **Tuyệt đối không dùng `cat` hay `echo`** để sửa code thủ công trên môi trường Windows Server (dùng IDE như Visual Studio).
2. Bất cứ khi nào thêm cột vào Database, hãy đảm bảo bạn đánh dấu `[NotMapped]` cho các biến chỉ dùng trên UI (như `IsSelected`) để tránh EF Core sinh lỗi.
3. Chạy `dotnet build` trước khi push code lên nhánh chính.

---

## ✍️ Tác giả (Authors)
- **Truong, Quyen/tSH PHp** - Thiết kế và phát triển kiến trúc hệ thống

---

## 📄 License
Tài liệu nội bộ & Bàn giao độc quyền - Do hệ thống thiết kế riêng cho Tesa Lab.
```
