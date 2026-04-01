using System.Globalization;

namespace AtomUI.Controls.Utils;

internal static class DateTimeUtils
{
    public static string FormatTimeSpan(TimeSpan? time, bool is12HourClock = false, string? amText = null, string? pmText = null)
    {
        if (time is null)
        {
            return string.Empty;
        }
        var dateTime = DateTime.Today.Add(time.Value);
        if (is12HourClock)
        {
            var formatInfo = new DateTimeFormatInfo();
            formatInfo.AMDesignator = amText ?? "AM";
            formatInfo.PMDesignator = pmText ?? "PM";
            return dateTime.ToString("hh:mm:ss tt", formatInfo);
        }

        return dateTime.ToString("HH:mm:ss");
    }
}