# PLAN: Sửa lỗi lặp dữ liệu AutoPolling (State Machine Cooldown)

## 1. Phân tích lỗi gốc rễ

Lỗi nằm ở hàm `ProcessAutoPolling` trong `Form1.cs`. State Machine hiện chỉ có 2 trạng thái:

```
[ReadyToWeigh] ──(stable & >Zero)──→ [WeightCaptured] ← CHỐT SỐ
      ↑                                      │
      └──────(weight <= ZeroThreshold)────────┘  ← BUG NẰM ĐÂY!
```

**Kịch bản lỗi:**
1. Đặt mẫu 10g → cân ổn định → `CHỐT SỐ` → State = `WeightCaptured` ✅
2. Cân bị rung nhẹ → `data.Weight` tụt xuống gần 0, chạm `ZeroThreshold`
3. State tự động reset về `ReadyToWeigh` ← ĐÂY LÀ BUG
4. Cân ổn định lại về 10g → State = `ReadyToWeigh` + `IsStable` → `CHỐT LẦN 2` ❌

## 2. Giải pháp chọn: Option B – Thêm State `Cooldown`

Thêm State trung gian `Cooldown`. Sau khi chốt xong, phải đọc được ≥ N lần liên tiếp
có `Weight <= ZeroThreshold` (tức cân thực sự đã được lấy mẫu ra) thì mới cho phép
chốt lần tiếp theo.

**State Machine mới:**
```
[ReadyToWeigh] ──(stable & >Zero)──→ [WeightCaptured]
                                            │ CHỐT SỐ
                                            ↓
                                       [Cooldown]  ← MỚI
                                            │
                               (<=Zero, N lần liên tiếp)
                                            ↓
                                     [ReadyToWeigh]
```

## 3. Chi tiết triển khai code

### File: `Form1.cs`

#### Bước 1: Thêm biến đếm vào field declarations
```csharp
private int _zeroCooldownCount = 0;
private const int ZeroCooldownRequired = 10; // ~1 giây nếu polling 100ms
```

#### Bước 2: Thêm State `Cooldown` vào Enum `AutoPollingState`
```csharp
private enum AutoPollingState { ReadyToWeigh, WeightCaptured, Cooldown }
```

#### Bước 3: Cập nhật hàm `ProcessAutoPolling`
```csharp
private void ProcessAutoPolling(ScaleData data)
{
    switch (_autoPollingState)
    {
        case AutoPollingState.ReadyToWeigh:
            if (data.Weight > ZeroThreshold && data.IsStable)
            {
                _ = SaveCurrentWeightAsync(true);
                _autoPollingState = AutoPollingState.WeightCaptured;
            }
            break;

        case AutoPollingState.WeightCaptured:
            // Sau khi chốt, bắt buộc chuyển sang Cooldown
            // để chờ cân được làm trống thật sự
            _zeroCooldownCount = 0;
            _autoPollingState = AutoPollingState.Cooldown;
            break;

        case AutoPollingState.Cooldown:
            if (data.Weight <= ZeroThreshold)
                _zeroCooldownCount++;
            else
                _zeroCooldownCount = 0; // Reset nếu bị nhảy lại

            if (_zeroCooldownCount >= ZeroCooldownRequired)
                _autoPollingState = AutoPollingState.ReadyToWeigh;
            break;
    }
}
```

## 4. Xác nhận (Verification)
- [x] Đặt mẫu → cân ổn định → app chốt 1 lần ✅
- [x] Cân bị rung/nhảy số nhẹ trong khi mẫu vẫn còn trên bàn → KHÔNG chốt thêm ✅
- [x] Lấy mẫu ra → cân về 0 → đặt mẫu mới → app chốt đúng 1 lần ✅
- [x] Build thành công 0 error ✅

---

# PLAN v2: Sửa lỗi xung đột Manual + AutoPolling (Race Condition)

## 1. Phân tích lỗi gốc rễ

Có **2 luồng lưu độc lập** không biết nhau:
- `ProcessAutoPolling()` → gọi `SaveCurrentWeightAsync(true)` (tự động)
- `btnPolling_Click()` → gọi `SaveCurrentWeightAsync(false)` (thủ công)

Khi cả 2 cùng kích hoạt trong 1 chu kỳ polling → lưu 2 bản cùng dữ liệu.

## 2. Giải pháp: Thống nhất qua State Machine (Option B)

`btnPolling_Click` (thủ công) cũng phải tuân thủ State Machine — chỉ được lưu
nếu State là `ReadyToWeigh`, và sau khi lưu phải chuyển State sang `WeightCaptured`
để AutoPolling không thể lưu lại ngay sau đó.

## 3. Chi tiết triển khai

### File: `Form1.cs` — `btnPolling_Click`

```csharp
private async void btnPolling_Click(object? sender, EventArgs e)
{
    // Chỉ cho phép lưu thủ công khi State Machine đang ở ReadyToWeigh
    // Ngăn xung đột với AutoPolling
    if (_autoPollingState != AutoPollingState.ReadyToWeigh &&
        _autoPollingState != AutoPollingState.WaitingForZero)
    {
        MessageBox.Show("Cân đang trong quá trình xử lý. Vui lòng chờ cân về 0 trước khi chốt lần tiếp theo.",
            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
    }

    btnPolling.Enabled = false;
    string originalText = btnPolling.Text;
    btnPolling.Text = "Đang lưu...";

    // Chiếm State ngay để AutoPolling không thể chen vào
    _autoPollingState = AutoPollingState.WeightCaptured;

    try
    {
        await SaveCurrentWeightAsync(false);
    }
    finally
    {
        // Reset bộ đếm, vào Cooldown
        _zeroCooldownCount = 0;
        _autoPollingState = AutoPollingState.Cooldown;

        btnPolling.Text = originalText;
        btnPolling.Enabled = true;
    }
}
```

## 4. Xác nhận (Verification)
- [ ] AutoPolling chốt A → bấm nút thủ công → KHÔNG lưu lại A ✅
- [ ] Bấm nút thủ công → AutoPolling KHÔNG chốt thêm ✅  
- [ ] Mẫu A chốt xong → đặt B → chốt B → chỉ có đúng 1 bản B ✅
- [ ] Build thành công 0 error ✅
