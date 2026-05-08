# Tesa Scale Data Collection - Modern Enterprise SaaS Edition

# Preview

*Ứng dụng Desktop thu thập dữ liệu cân điện tử thời gian thực với giao diện Material Design, tích hợp biểu đồ hiệu năng cao và bảo mật Zero-trust.*

# Introduction

**Tesa Scale Data Collection** là giải pháp phần mềm chuyên dụng để kết nối, thu thập và quản lý dữ liệu từ cân phân tích **Mettler Toledo ML204T/00**. Khởi đầu là một công cụ tiện ích đơn giản, dự án đã được nâng cấp lên phiên bản **v2.0** với kiến trúc **Modern Enterprise SaaS**, tập trung vào trải nghiệm người dùng hiện đại, bảo mật dữ liệu tuyệt đối và khả năng xử lý dữ liệu lớn trong môi trường nhà máy thông minh (Smart Factory).

# Features

Hệ thống được tích hợp đầy đủ các tính năng từ cơ bản đến nâng cao qua các giai đoạn phát triển:

### 🌟 Tính năng Cốt lõi (Core)

* **Kết nối Real-time TCP/IP:** Giao tiếp hai chiều với cân qua giao thức MT-SICS chuẩn công nghiệp.
* **Bộ giải mã MtSicsParser:** Tự động phân tích trạng thái ổn định (`S S`) hoặc dao động (`S D`).
* **Đa luồng (Multi-threading):** Đảm bảo UI luôn mượt mà ngay cả khi nhận dữ liệu tần suất cao.
* **Offline-first:** Lưu trữ dữ liệu cục bộ ổn định ngay cả khi mất kết nối máy chủ trung tâm.

### ⚡ Cập nhật v1.1: Tự động hóa & Tiện ích (Automation)

* **Auto-Polling (Chốt số tự động):** Hệ thống tự nhận diện trạng thái ổn định > 0g để tự động lưu DB và phát tiếng Beep.
* **Tích hợp súng bắn mã vạch (Barcode Scanner):** Lắng nghe phím Enter để tự động nhảy focus giữa các trường NAT -> Batch -> Sample, giúp công nhân rảnh tay.
* **Âm thanh phản hồi (Audio Feedback):** Phát tiếng "Tít" khi chốt số thành công và cảnh báo khi mất kết nối.
* **Chạy ngầm (System Tray):** Thu nhỏ xuống khay hệ thống để tiết kiệm diện tích Taskbar.

### 🚀 Cập nhật v2.0: Enterprise SaaS & Security

* **Material Design UI:** Sử dụng `MaterialSkin.2` hỗ trợ **Dark/Light Mode** chuyên nghiệp.
* **Real-time Analytics:** Biểu đồ sóng `ScottPlot` chạy mượt 10Hz với CPU usage < 5%.
* **Bảo mật Zero-trust:** * Mã hóa Database SQLite bằng **SQLCipher (AES-256)**.
* Xác thực người dùng qua **JWT** và phân quyền **RBAC** (Admin/Operator).


* **Tối ưu dữ liệu lớn:** Cơ chế `VirtualMode` cho DataGrid xử lý 50.000+ bản ghi cực mượt.
* **Global Search Bar:** Tìm kiếm nhanh lịch sử cân trong < 200ms.

# Tech Stack

* **Framework:** .NET 10 LTS (Windows Forms App).
* **UI/UX:** MaterialSkin.2.
* **Biểu đồ:** ScottPlot (High-performance Signal Plotting).
* **Cơ sở dữ liệu:** SQLite + SQLCipher (Encryption at rest).
* **Thư viện phụ:** CsvHelper (Export CSV), ClosedXML (Excel reporting).

# Installation

1. **Thiết lập Cân:** Cài đặt IP tĩnh cho cân ML204T trong cùng mạng LAN với laptop.
2. **Tải ứng dụng:** Tải bản phát hành `.exe` (Self-contained).
3. **Khởi chạy:** Lưu thư mục vào phân vùng ổ D và chạy file `ScaleDataApp.exe`.
4. **Cấu hình:** Nhập IP/Port vào tab Settings để kích hoạt kết nối.

# Usage

1. **Đăng nhập:** Nhập thông tin tài khoản (Yêu cầu xác thực JWT ở v2.0).
2. **Kết nối:** Nhấn **Connect** để bắt đầu nhận luồng dữ liệu Live View.
3. **Nhập liệu:** Quét barcode mã NAT/Batch.
4. **Chốt số:** Nhấn **Polling** hoặc kích hoạt **Auto-mode** để lưu vào DB.
5. **Báo cáo:** Vào tab **Data Sheet** để xem lịch sử và **Export** dữ liệu ra file.

# Folder Structure

```text
Test/
├── Services/
│   ├── ScaleService.cs        (Xử lý TCP/IP & MT-SICS)
│   ├── AuthService.cs         (Quản lý JWT & RBAC)
│   └── DatabaseService.cs     (Quản lý SQLite + SQLCipher)
├── Models/
│   ├── ScaleRecord.cs         (Data Model)
│   └── UserProfile.cs         (User Identity)
├── UI/
│   ├── Controls/
│   │   └── CustomDataGrid.cs  (Modern UI DataGrid)
│   └── Forms/
│       ├── MainView.cs        (Material Main Dashboard)
│       └── LoginView.cs       (Security Entry)
└── Utils/
    └── CryptoHelper.cs        (AES Helper)

```

# Roadmap

* **v1.0 (Stable):** Hoàn thiện kết nối Socket TCP và Parser cơ bản.
* **v1.1 (Automation):** Thêm Auto-polling, Barcode support và Audio Feedback.
* **v2.0 (Enterprise):** Refactor sang Material Design, mã hóa SQLCipher và đồ thị ScottPlot.
* **Future:** Đồng bộ Cloud API (Azure/AWS) và ứng dụng Mobile Monitoring.

# Author

* **Trương Vương Quyền**
* *IT Support Intern @ tesa Site Haiphong*
* *Student @ Vietnam Maritime University (VIMARU)*
