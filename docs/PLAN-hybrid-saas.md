# Kế Hoạch Chuyển Đổi Kiến Trúc Hybrid SaaS (Edge & Central)

## 1. Tổng quan Kiến trúc (Architecture Overview)
- **Mô hình:** Hybrid Enterprise (Kết hợp Edge Computing tại xưởng và Server tập trung).
- **Mục tiêu:** Mở rộng linh hoạt (Scale) & Đảm bảo tính liên tục (High Availability) tại các trạm cân.

## 2. Phân tách Hạ tầng & Giao tiếp (Infrastructure)

### 2.1. Edge PC (Máy Trạm tại phòng Lab)
- **Ứng dụng:** Client App (WinForms chạy trên .NET 10 LTS).
- **Lưu trữ nội bộ (Local Buffer):** SQLite được mã hóa bằng SQLCipher (AES-256).
- **Mạng Kép (Dual-Network):**
  - **LAN (Cáp Cat5):** Kết nối trực tiếp vào cân điện tử qua cổng RJ45 (đảm bảo stream 10Hz ổn định, không độ trễ).
  - **WLAN (Wi-Fi):** Kết nối mạng nội bộ nhà máy để gửi dữ liệu lên server và xác thực (JWT).

### 2.2. Central Server (Hạ tầng Trung tâm)
- **Môi trường triển khai:** Máy ảo (VM) Ubuntu Server chạy Docker.
- **Database:** SQL Server (Linux Container).
- **Backend:** .NET 10 Web API (Tiếp nhận JSON, xử lý luồng dữ liệu tập trung từ nhiều Edge PC).

## 3. Lộ trình Triển khai (Task Breakdown)

### Phase 1: Nâng cấp Edge Client (WinForms .NET 10)
- Nâng cấp dự án hiện tại lên .NET 10 LTS.
- Tích hợp SQLCipher để mã hóa file SQLite nội bộ.
- Xây dựng module đồng bộ dữ liệu (Sync Service) lên Central Server (cơ chế lưu đệm khi rớt mạng Wi-Fi và đẩy bù khi có mạng).
- Quản lý cơ chế Dual-Network (Ưu tiên mạng LAN cho cân, mạng Wi-Fi cho API).

### Phase 2: Xây dựng Central Server (Web API & Database)
- Cấu hình Ubuntu VM và cài đặt Docker, Docker Compose.
- Thiết lập SQL Server Linux Container và schema cơ sở dữ liệu.
- Xây dựng .NET 10 Web API với JWT Authentication.
- Viết các endpoints để hứng dữ liệu (JSON) từ các trạm cân.

### Phase 3: Tích hợp & Kiểm thử (Integration & Testing)
- Kiểm thử cơ chế mã hóa AES-256 dưới Client.
- Kiểm thử chịu tải Web API khi nhiều Client gửi dữ liệu 10Hz đồng thời.
- Mô phỏng ngắt kết nối Wi-Fi để kiểm tra cơ chế Buffer & Sync của Client.
- Triển khai Pilot tại 1 máy trạm và nghiệm thu kết quả.

## 4. Phân công Agent
- `@frontend-specialist`: Nâng cấp giao diện và logic WinForms, quản lý Dual-Network, Sync Service, SQLCipher.
- `@backend-specialist`: Xây dựng Web API (.NET 10), cấu hình Docker, SQL Server Container.
- `@security-auditor`: Kiểm tra bảo mật JWT, mã hóa AES-256 trên file DB cục bộ.
