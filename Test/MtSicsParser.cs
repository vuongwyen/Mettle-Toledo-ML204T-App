using System;
using System.Text.RegularExpressions;

namespace Test
{
    public struct ScaleData
    {
        public bool IsStable { get; set; }
        public decimal Weight { get; set; }
        public string Unit { get; set; }
    }

    public static class MtSicsParser
    {
        // MT-SICS Format: S S     100.00 g\r\n
        // Regex matches: S (S|D)\s+(-?\d+\.?\d*)\s+([a-zA-Z]+)
        private static readonly Regex ParserRegex = new Regex(@"S\s+(S|D)\s+(-?\d+\.?\d*)\s+([a-zA-Z]+)", RegexOptions.Compiled);

        public static ScaleData? Parse(string rawData)
        {
            if (string.IsNullOrWhiteSpace(rawData)) return null;

            var match = ParserRegex.Match(rawData);
            if (match.Success)
            {
                string stability = match.Groups[1].Value;
                string weightStr = match.Groups[2].Value;
                string unitStr = match.Groups[3].Value;

                if (decimal.TryParse(weightStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal weight))
                {
                    return new ScaleData
                    {
                        IsStable = (stability == "S"),
                        Weight = weight,
                        Unit = unitStr
                    };
                }
            }
            return null;
        }
    }
}
