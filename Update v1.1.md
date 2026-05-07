# 1. Tự động hóa (Automation)

* **Auto-Polling (Chốt số tự động):** Thay vì nhân viên phải bấm nút `btnPolling` thủ công mỗi lần cân, hệ thống sẽ tự động bắt nhịp: **Cân đang ở 0g** -> **Đặt vật lên** -> **Cân nhảy số và đạt trạng thái Ổn định (S S) > 0g** -> **Tự động lưu DB và phát ra tiếng Beep** -> **Nhấc vật ra (về 0g)**. Tính năng này giúp tăng tốc độ làm việc lên gấp 3 lần.
* **Tích hợp súng bắn mã vạch (Barcode Scanner):** Lắng nghe phím *Enter* (từ súng quét) để tự động nhảy con trỏ (focus) lần lượt qua các ô **Mã NAT** -> **Lô hàng** -> **Tên mẫu**, giúp công nhân không cần sử dụng chuột.

# 2. Nâng cấp Giao diện & Trải nghiệm (UX/UI)

* **Dark Mode & Modern UI:** Cải tiến WinForms thành giao diện tối (Dark Theme) công nghiệp, giảm mỏi mắt cho công nhân và mang lại cảm giác chuyên nghiệp hơn cho ứng dụng.
* **Âm thanh phản hồi (Audio Feedback):** Phát tiếng "Tít" khi chốt số liệu thành công và tiếng còi ngắn cảnh báo khi mất kết nối mạng. Đây là tính năng cần thiết trong môi trường nhà máy ồn ào.
* **Chạy ngầm (System Tray):** Cho phép thu nhỏ phần mềm xuống khay hệ thống (Tray Icon) để tiết kiệm không gian trên Taskbar trong khi vẫn duy trì kết nối liên tục với cân.

# 3. Phân tích & Xử lý Dữ liệu sâu (Data & Analytics)

* **Vẽ biểu đồ Real-time (Live Chart):** Tích hợp thư viện như `LiveCharts` hoặc `OxyPlot` vào Dashboard để vẽ biểu đồ dao động của cân theo thời gian thực (hỗ trợ phòng Lab theo dõi độ bay hơi của mẫu chất lỏng).
* **Báo cáo thống kê nhanh:** Hiển thị trực tiếp trên Dashboard số lượng mẫu đã cân trong ngày và tổng khối lượng của lô hàng hiện tại.
* **Xuất chuẩn Excel (.xlsx):** Sử dụng thư viện `ClosedXML` để xuất file Excel có định dạng màu sắc, căn chỉnh cột và tự động *AutoFit* (thay thế cho định dạng `.csv` thô hiện tại).