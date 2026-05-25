# PLAN: Gộp Báo Cáo Kỹ Thuật (Merge Technical Reports)

## Goal
Hợp nhất toàn bộ tài liệu kiến trúc và báo cáo kỹ thuật vào một file duy nhất `docs/TECHNICAL-REPORT-scale-data-system.md` để dễ dàng tra cứu, đồng thời xóa bỏ các file trung gian/thừa thãi.

---

## Tình Trạng Hiện Tại

Chúng ta đang có 3 file chứa thông tin trùng lặp hoặc phân mảnh:
1. `docs/TECHNICAL-REPORT-scale-data-system.md` (Bản báo cáo gốc, đang mô tả trạng thái trước khi vá lỗi).
2. `docs/PLAN-report-update.md` (Bản nháp kế hoạch cập nhật 10 tasks để phản ánh trạng thái mã nguồn *sau* khi vá bảo mật và hiệu năng).
3. `architecture_of_system.md` (Tài liệu kiến trúc ban đầu, chứa các phân tích chi tiết về `ConnectionManager`, `MtSicsParser`, và lý thuyết cơ chế `ConcurrentQueue`).

---

## Proposed Changes

Kế hoạch thực thi sẽ tiến hành theo các bước sau:

### 1. Cập Nhật Báo Cáo Chính (`TECHNICAL-REPORT-scale-data-system.md`)
Sẽ thực hiện viết lại toàn diện file này, bao gồm:
- **Áp dụng 10 Tasks cập nhật từ `PLAN-report-update.md`**: Sửa đổi toàn bộ các phần mô tả cũ (Double -> Decimal, BeginInvoke -> ConcurrentQueue, lock-free timer, JWT AuthService, IdempotencyKey, SQLCipher).
- **Hấp thụ nội dung từ `architecture_of_system.md`**: Bổ sung các đoạn phân tích chuyên sâu (như Regex sinh mã máy của `MtSicsParser`, cơ chế Handshake `SIR` của `ConnectionManager`) vào các mục tương ứng trong báo cáo chính để làm phong phú tài liệu.
- **Chuẩn hóa Format**: Sử dụng thống nhất format markdown, bảng biểu, sơ đồ luồng dữ liệu ASCII (đã cập nhật ConcurrentQueue flow).

### 2. Dọn Dẹp Workspace (Xóa file thừa)
Sau khi hợp nhất thành công toàn bộ nội dung vào `TECHNICAL-REPORT-scale-data-system.md`, tiến hành xóa các file không còn giá trị sử dụng để làm sạch dự án:
- [DELETE] `docs/PLAN-report-update.md`
- [DELETE] `architecture_of_system.md`

---

## Verification Plan

- Kiểm tra file `docs/TECHNICAL-REPORT-scale-data-system.md` đã chứa đầy đủ thông tin:
  - Security Patch (SQLCipher, HTTPS, JWT).
  - Bottleneck Patch (ConcurrentQueue, Decimal, SemaphoreSlim, Idempotency).
  - Sơ đồ Data Flow cập nhật.
- Đảm bảo 2 file `PLAN-report-update.md` và `architecture_of_system.md` đã bị xóa hoàn toàn khỏi thư mục dự án.

## User Review Required
> [!IMPORTANT]
> Bạn có đồng ý với kế hoạch hợp nhất toàn diện này không? Sau khi bạn duyệt, tôi sẽ sử dụng kỹ năng `/enhance` để thực thi viết đè báo cáo chính và xóa các file rác.
