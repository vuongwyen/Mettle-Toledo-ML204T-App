---
trigger: manual
---

# 🎯 Prompt Templates — Guna UI2 WinForms Vibecoding
# Copy & paste vào Claude Code / Cursor / Copilot Chat

================================================================
## PROMPT 1 — Tạo Form mới
================================================================

Tạo một WinForms Form tên [TênForm] với Guna UI2, dark theme.
Yêu cầu:
- Borderless (FormBorderStyle.None + Guna2BorderlessForm)
- Custom title bar draggable với nút đóng/minimize macOS-style
- Layout: [mô tả layout, vd: sidebar trái 220px + content area phải]
- Controls cần có: [liệt kê, vd: DataGridView, 3 card stats, search box]
- Màu nền: #14141E (dark) hoặc theme từ ThemeHelper
- Dùng ThemeHelper.Apply() cho mọi control
- Viết code-behind (không dùng Designer tự gen), đặt trong constructor sau InitializeComponent()

================================================================
## PROMPT 2 — Tạo Sidebar Navigation
================================================================

Tạo UserControl tên SidebarControl cho WinForms + Guna UI2:
- Width cố định 220px, DockStyle.Left
- Background #12121C, shadow bên phải
- Logo/avatar tròn ở trên (Guna2CirclePictureBox 60px)
- Menu items: [Dashboard, Sản phẩm, Đơn hàng, Khách hàng, Cài đặt]
- Mỗi menu item: Guna2Button 190x44, icon bên trái, text, hover effect
- Active state: background tím mờ + text trắng + border trái 3px tím
- Event MenuItemClicked expose ra ngoài
- Toggle thu gọn sidebar (icon-only mode 60px)

================================================================
## PROMPT 3 — Dashboard với Charts
================================================================

Tạo DashboardForm với Guna UI2 + LiveCharts (WinForms):
- Borderless dark form
- Sidebar (dùng SidebarControl)
- Header: tên trang + search box + avatar
- 4 stat cards (doanh thu, đơn hàng, khách hàng, tỉ lệ chuyển đổi)
  mỗi card: icon, số, phần trăm tăng/giảm, mini sparkline
- Biểu đồ Line chart (doanh thu 7 ngày) — chiếm 60% width
- Biểu đồ Donut chart (phân loại sản phẩm) — 40% width
- Recent orders table: Guna2DataGridView, custom row style
- Dùng ThemeHelper cho tất cả màu sắc

================================================================
## PROMPT 4 — Form CRUD (DataGrid + Form nhập)
================================================================

Tạo màn hình quản lý [TênEntity] với Guna UI2:
- Split layout: DataGrid trái, Form nhập phải (hoặc popup)
- Guna2DataGridView:
  + Zebra rows (row xen kẽ màu)
  + Header style tùy chỉnh
  + Custom cell cho cột trạng thái (badge màu)
  + Pagination tự làm (Guna2Button prev/next)
- Form nhập bên phải (hoặc Guna2Panel slide in):
  + Fields: [liệt kê fields]
  + Validation inline
  + Nút Lưu (primary) + Hủy (ghost)
- Search + Filter bar phía trên
- Nút Thêm mới (gradient)

================================================================
## PROMPT 5 — Settings / Profile Form
================================================================

Tạo SettingsForm với Guna UI2, tab-based:
- Tabs: Tài khoản | Giao diện | Thông báo | Bảo mật
- Tab "Tài khoản":
  + Avatar tròn với nút upload
  + Fields: Họ tên, Email, SĐT, Địa chỉ
  + Nút Lưu thay đổi
- Tab "Giao diện":
  + Guna2ToggleSwitch chọn Dark/Light mode
  + Accent color picker (6 ô màu Guna2Panel nhỏ)
  + Guna2ComboBox chọn font size
- Tab "Bảo mật":
  + Đổi mật khẩu (3 TextBox password)
  + Toggle bật/tắt 2FA

================================================================
## PROMPT 6 — Refactor / Beautify existing Form
================================================================

Đây là code WinForms hiện tại của tôi: [paste code]
Hãy refactor để:
1. Thay tất cả controls mặc định bằng Guna UI2 tương đương
2. Áp dụng dark theme với ThemeHelper
3. Borderless form + custom title bar
4. Bo góc, shadow, gradient theo convention trong CLAUDE.md
5. Giữ nguyên toàn bộ logic/events, chỉ thay phần UI
6. Trả về code-behind đầy đủ

================================================================
## PROMPT 7 — Custom Control
================================================================

Tạo UserControl tên [TênControl] với Guna UI2:
Chức năng: [mô tả]
Expose properties:
- [Property1]: [kiểu dữ liệu]
- [Property2]: [kiểu dữ liệu]
Expose events:
- [Event1]
- [Event2]
Hỗ trợ Dark/Light theme qua phương thức ApplyTheme(AppTheme)
Có thể dùng trong Designer (Designer-safe)

================================================================
## TIPS VIBECODING HIỆU QUẢ
================================================================

✅ Luôn bắt đầu prompt với context:
   "Dự án WinForms .NET 6, dùng Guna UI2 v2.x, ThemeHelper đã có sẵn"

✅ Chỉ định kích thước rõ ràng:
   "Form 1200x720", "Sidebar 220px", "Card 280x160"

✅ Dùng từ khóa Guna2:
   Thay "button" → "Guna2Button gradient tím"
   Thay "textbox" → "Guna2TextBox với placeholder và icon search"

✅ Yêu cầu code-behind thay vì Designer:
   "Viết tất cả trong constructor sau InitializeComponent(), không dùng Designer"

✅ Nhắc về ThemeHelper:
   "Dùng ThemeHelper.Apply() và ThemeHelper.Colors.* cho tất cả màu"

✅ Yêu cầu animation/state:
   "Button phải có loading state khi click", "Input có validation inline màu đỏ"
