using System.Globalization;

namespace BookingSerivce.Models.VNPAY
{
    /// <summary>
    /// Custom comparer for VNPay parameters (case-insensitive alphabetical order)
    /// </summary>
    public class VNPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
