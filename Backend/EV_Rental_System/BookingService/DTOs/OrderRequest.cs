using BookingService.DTOs;
using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs
{
    /// <summary>
    /// INPUT for GetOrderPreviewAsync. Basic info + base pricing data.
    /// </summary>
    public class OrderRequest
    {
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        [BusinessHours]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        [BusinessHours]
        [ValidRentalPeriod]
        public DateTime ToDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Phí thuê theo giờ phải lớn hơn 0")]
        public decimal RentFeeForHour { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Giá trị xe phải lớn hơn 0")]
        public decimal ModelPrice { get; set; }

        public string PaymentMethod { get; set; }
    }
}

// ============== Business Hours Configuration ==============

public static class BusinessHoursConfig
{
    private static readonly TimeZoneInfo VietnamTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public static readonly Dictionary<DayOfWeek, (TimeSpan Open, TimeSpan Close)> WorkingHours = new()
    {
        { DayOfWeek.Monday, (new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0)) },
        { DayOfWeek.Tuesday, (new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0)) },
        { DayOfWeek.Wednesday, (new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0)) },
        { DayOfWeek.Thursday, (new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0)) },
        { DayOfWeek.Friday, (new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0)) },
        { DayOfWeek.Saturday, (new TimeSpan(9, 0, 0), new TimeSpan(20, 0, 0)) },
        { DayOfWeek.Sunday, (new TimeSpan(10, 0, 0), new TimeSpan(18, 0, 0)) }
    };

    /// <summary>
    /// Convert UTC DateTime sang giờ Việt Nam
    /// </summary>
    public static DateTime ToVietnamTime(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.ToUniversalTime(), VietnamTimeZone);
    }

    /// <summary>
    /// Lấy thời gian hiện tại theo giờ Việt Nam
    /// </summary>
    public static DateTime NowVietnam => ToVietnamTime(DateTime.UtcNow);

    /// <summary>
    /// Kiểm tra thời gian có nằm trong giờ làm việc không (tự động convert sang giờ VN)
    /// </summary>
    public static bool IsWithinBusinessHours(DateTime dateTime)
    {
        var vnTime = ToVietnamTime(dateTime);

        if (!WorkingHours.TryGetValue(vnTime.DayOfWeek, out var hours))
            return false;

        return vnTime.TimeOfDay >= hours.Open && vnTime.TimeOfDay <= hours.Close;
    }

    /// <summary>
    /// Lấy message giờ làm việc theo ngày
    /// </summary>
    public static string GetBusinessHoursMessage(DayOfWeek day)
    {
        if (!WorkingHours.TryGetValue(day, out var hours))
            return "Không hoạt động";

        return $"{hours.Open:hh\\:mm} - {hours.Close:hh\\:mm}";
    }

    /// <summary>
    /// Lấy tên ngày trong tuần tiếng Việt
    /// </summary>
    public static string GetDayName(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Thứ Hai",
        DayOfWeek.Tuesday => "Thứ Ba",
        DayOfWeek.Wednesday => "Thứ Tư",
        DayOfWeek.Thursday => "Thứ Năm",
        DayOfWeek.Friday => "Thứ Sáu",
        DayOfWeek.Saturday => "Thứ Bảy",
        DayOfWeek.Sunday => "Chủ Nhật",
        _ => day.ToString()
    };
}

// ============== Custom Validation Attributes ==============

/// <summary>
/// Validate thời gian phải trong giờ làm việc (UTC -> VN)
/// </summary>
public class BusinessHoursAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is not DateTime dateTime)
            return ValidationResult.Success;

        if (!BusinessHoursConfig.IsWithinBusinessHours(dateTime))
        {
            var vnTime = BusinessHoursConfig.ToVietnamTime(dateTime);
            var dayName = BusinessHoursConfig.GetDayName(vnTime.DayOfWeek);
            var hoursMessage = BusinessHoursConfig.GetBusinessHoursMessage(vnTime.DayOfWeek);

            return new ValidationResult(
                $"Thời gian {vnTime:dd/MM/yyyy HH:mm} không nằm trong giờ làm việc. " +
                $"Giờ làm việc {dayName}: {hoursMessage}");
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validate khoảng thời gian thuê xe phải hợp lệ
/// </summary>
public class ValidRentalPeriodAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var request = (OrderRequest)validationContext.ObjectInstance;
        var errors = new List<string>();

        // Convert sang giờ VN
        var vnFromDate = BusinessHoursConfig.ToVietnamTime(request.FromDate);
        var vnToDate = BusinessHoursConfig.ToVietnamTime(request.ToDate);
        var vnNow = BusinessHoursConfig.NowVietnam;

        // 1. FromDate phải trước ToDate
        if (vnFromDate >= vnToDate)
        {
            errors.Add("Thời gian bắt đầu phải trước thời gian kết thúc.");
        }

        // 2. FromDate phải trong tương lai (so với giờ VN)
        if (vnFromDate < vnNow)
        {
            errors.Add($"Thời gian bắt đầu ({vnFromDate:dd/MM/yyyy HH:mm}) không được là thời điểm trong quá khứ. " +
                      $"Thời gian hiện tại: {vnNow:dd/MM/yyyy HH:mm}");
        }

        // 3. Validate thời gian thuê qua nhiều ngày
        if (vnFromDate.Date != vnToDate.Date)
        {
            var crossDayErrors = ValidateCrossDayRental(vnFromDate, vnToDate);
            errors.AddRange(crossDayErrors);
        }

        return errors.Any()
            ? new ValidationResult(string.Join(" ", errors))
            : ValidationResult.Success;
    }

    /// <summary>
    /// Validate khi thuê xe qua nhiều ngày
    /// </summary>
    private List<string> ValidateCrossDayRental(DateTime vnFromDate, DateTime vnToDate)
    {
        var errors = new List<string>();
        var currentDate = vnFromDate.Date;

        while (currentDate <= vnToDate.Date)
        {
            var dayOfWeek = currentDate.DayOfWeek;

            // Kiểm tra ngày này có giờ làm việc không
            if (!BusinessHoursConfig.WorkingHours.ContainsKey(dayOfWeek))
            {
                var dayName = BusinessHoursConfig.GetDayName(dayOfWeek);
                errors.Add($"{dayName} ({currentDate:dd/MM/yyyy}) không có giờ làm việc.");
            }
            else
            {
                // Validate giờ bắt đầu (ngày đầu tiên)
                if (currentDate == vnFromDate.Date)
                {
                    var hours = BusinessHoursConfig.WorkingHours[dayOfWeek];
                    if (vnFromDate.TimeOfDay < hours.Open || vnFromDate.TimeOfDay > hours.Close)
                    {
                        var dayName = BusinessHoursConfig.GetDayName(dayOfWeek);
                        var hoursMsg = BusinessHoursConfig.GetBusinessHoursMessage(dayOfWeek);
                        errors.Add($"Thời gian bắt đầu ({vnFromDate:HH:mm}) không nằm trong giờ làm việc {dayName}: {hoursMsg}");
                    }
                }
                // Validate giờ kết thúc (ngày cuối cùng)
                else if (currentDate == vnToDate.Date)
                {
                    var hours = BusinessHoursConfig.WorkingHours[dayOfWeek];
                    if (vnToDate.TimeOfDay < hours.Open || vnToDate.TimeOfDay > hours.Close)
                    {
                        var dayName = BusinessHoursConfig.GetDayName(dayOfWeek);
                        var hoursMsg = BusinessHoursConfig.GetBusinessHoursMessage(dayOfWeek);
                        errors.Add($"Thời gian kết thúc ({vnToDate:HH:mm}) không nằm trong giờ làm việc {dayName}: {hoursMsg}");
                    }
                }
            }

            currentDate = currentDate.AddDays(1);
        }

        return errors;
    }
}