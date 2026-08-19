using System;
using System.Text.RegularExpressions;

namespace Test
{

    public enum ScaleStatus
    {
        Stable,      // S
        Dynamic,     // D
        Overload,    // +
        Underload,   // -
        Invalid      // I or others
    }

    public struct ScaleData
    {
        public ScaleStatus Status { get; set; }
        public bool IsStable => Status == ScaleStatus.Stable;
        public decimal Weight { get; set; }
        public string Unit { get; set; }
        public bool IsError => Status == ScaleStatus.Overload || Status == ScaleStatus.Underload || Status == ScaleStatus.Invalid;
    }

    public static class MtSicsParser
    {
        // MT-SICS Standard Format: <ID> <Status> <WeightValue> <Unit>
        // Regex giải thích:
        // ^S\s+           : Bắt đầu bằng 'S' và khoảng trắng
        // (S|D|\+|\-|I)   : Trạng thái (S, D, +, -, I)
        // (?:             : Nhóm không bắt giữ cho phần khối lượng (có thể không có nếu overload/underload)
        //   \s+           : Khoảng trắng đệm
        //   (-?\d+\.?\d*) : Giá trị số (Weight)
        //   \s+           : Khoảng trắng
        //   ([a-zA-Z]+)   : Đơn vị (Unit)
        // )?              : Phần khối lượng có thể vắng mặt ở một số trạng thái lỗi
        private static readonly Regex ParserRegex = new Regex(@"^S\s+(S|D|\+|\-|I)(?:\s+(-?\d+\.?\d*)\s+([a-zA-Z]+))?", RegexOptions.Compiled);

        public static ScaleData? Parse(string rawData)
        {
            if (string.IsNullOrWhiteSpace(rawData)) return null;

            var match = ParserRegex.Match(rawData.Trim());
            if (match.Success)
            {
                string statusChar = match.Groups[1].Value;
                var data = new ScaleData
                {
                    Status = ParseStatus(statusChar),
                    Unit = "g" // Default
                };

                // Nếu có phần giá trị khối lượng
                if (match.Groups[2].Success)
                {
                    string weightStr = match.Groups[2].Value;
                    if (decimal.TryParse(weightStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal weight))
                    {
                        data.Weight = weight * 100m; // [QA Req] Đổi g/mm2 sang g/cm2
                    }
                    data.Unit = "g/cm²"; // Ép unit hiển thị chuẩn
                }

                return data;
            }
            return null;
        }

        private static ScaleStatus ParseStatus(string s)
        {
            return s switch
            {
                "S" => ScaleStatus.Stable,
                "D" => ScaleStatus.Dynamic,
                "+" => ScaleStatus.Overload,
                "-" => ScaleStatus.Underload,
                _   => ScaleStatus.Invalid
            };
        }
    }
}
