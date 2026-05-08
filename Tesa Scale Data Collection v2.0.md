# 🚀 TÓM TẮT PHÁT HÀNH: Tesa Scale Data Collection v2.0 (Enterprise SaaS Edition)

Phiên bản 2.0 đánh dấu một bước chuyển mình toàn diện của hệ thống thu thập dữ liệu cân điện tử. Chúng tôi đã đập bỏ hoàn toàn cảm giác "phần mềm kế toán cũ" của WinForms truyền thống, thay thế bằng trải nghiệm UI/UX chuẩn Modern Web, đồng thời nâng cấp sâu kiến trúc lõi để đáp ứng tiêu chuẩn khắt khe về **Zero-Trust Security** và **Hiệu năng Real-time** trong môi trường xưởng sản xuất.

Dưới đây là những nâng cấp đột phá trong phiên bản v2.0:

## 🎨 1. Trải nghiệm Giao diện (Modern UI/UX)

* **Giao diện Material Design:** Tích hợp hoàn toàn `MaterialSkin.2`, mang lại ngôn ngữ thiết kế phẳng, hiện đại tương tự các ứng dụng Web SaaS.
* **Chế độ Sáng/Tối (Light/Dark Mode):** Hỗ trợ chuyển đổi giao diện linh hoạt, giúp bảo vệ mắt cho người vận hành trong các điều kiện ánh sáng khác nhau tại xưởng.
* **Data Grid Chuẩn Web:** Bảng dữ liệu (`DataGridView`) được "độ" lại hoàn toàn bằng Custom Paint: loại bỏ viền cứng nhắc, bo góc mềm mại, hiệu ứng hover highlight (đổi màu nền nhạt khi lướt chuột) và bôi đậm dòng đang chọn.

## 📊 2. Giám sát & Biểu đồ Thời gian thực (Real-time Analytics)

* **Động cơ Biểu đồ ScottPlot:** Thay thế hiển thị số tĩnh bằng biểu đồ sóng tín hiệu (Signal Plot) tốc độ siêu cao.
* **Hiển thị mượt mà 10Hz (Throttling):** Áp dụng thuật toán *Decoupling* với mảng đệm (Buffered Data) và Timer độc lập để render biểu đồ ở tốc độ 10 khung hình/giây. Giúp đồ thị chạy mượt như sóng điện tim (ECG) mà mức tiêu thụ CPU của máy tính luôn được kìm hãm **dưới 5%**.

## 🛡️ 3. Kiến trúc Bảo mật Zero-Trust

* **Mã hóa Dữ liệu Cục bộ (AES-256):** Tích hợp `SQLCipher` để mã hóa toàn bộ file database SQLite. Ngăn chặn hoàn toàn việc rò rỉ dữ liệu hoặc can thiệp số liệu thủ công dù kẻ gian có copy được file DB ra USB.
* **Xác thực JWT & Phân quyền (RBAC):** Bổ sung màn hình Đăng nhập độc lập. Hệ thống phân quyền chặt chẽ:
* *Operator (Người vận hành):* Chỉ được phép xem số liệu, chốt dữ liệu và xuất báo cáo.
* *Admin (Quản trị viên):* Có quyền thay đổi IP/Port mạng, cấu hình cân và reset hệ thống.



## ⚡ 4. Tối ưu Hiệu suất Dữ liệu Lớn

* **Cơ chế VirtualMode:** Giải quyết triệt để tình trạng giật/lag giao diện khi dữ liệu phình to. Bảng lịch sử cân giờ đây có thể cuộn mượt mà ngay cả khi lưu trữ hơn **50.000+ bản ghi**.
* **Global Search Bar:** Tích hợp thanh tìm kiếm toàn cục, cho phép truy vấn nhanh dữ liệu theo Mã NAT, Lô hàng (Batch) hoặc Tên mẫu với tốc độ phản hồi cực nhanh (< 200ms).

## 🏗️ 5. Nâng cấp Kiến trúc Cốt lõi (Under the Hood)

* **Clean Architecture:** Tách bạch hoàn toàn phần hiển thị giao diện (UI) và logic nghiệp vụ.
* Các tác vụ nặng được module hóa thành các Service độc lập chạy ngầm: `ScaleService` (Xử lý Socket TCP/IP), `DatabaseService` (Thao tác DB) và `AuthService` (Quản lý phiên đăng nhập). Giúp ứng dụng bền bỉ, dễ dàng bảo trì và mở rộng trong tương lai.