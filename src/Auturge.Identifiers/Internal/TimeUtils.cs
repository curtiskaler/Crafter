namespace Auturge.Identifiers.Internal;

internal static class TimeUtils
{
    public static DateTime FindRolloverDateTime(long epoch, int stampBits)
    {
        // Max value that can be held by b bits = 2^b - 1
        long availableMillis = (2 ^ stampBits) - 1;
        long rolloverStamp = epoch + availableMillis;
        var dateTimeOffsetFromMilliseconds = DateTimeOffset.FromUnixTimeMilliseconds(rolloverStamp);
        DateTime dateTimeFromMilliseconds = dateTimeOffsetFromMilliseconds.UtcDateTime; // Or .LocalDateTime for local time
        return dateTimeFromMilliseconds;
    }

    public static long GetUnixTimeMillis(this DateTime dateTime)
    {
        string dateTimeString = dateTime.ToUniversalTime().ToLongTimeString();
        var utc = DateTimeOffset.Parse(dateTimeString);
        return utc.ToUnixTimeMilliseconds();
    }
}
 
// Storing an actual timestamp naively (bits for each time unit), we could use:
// 1000 ms / sec => 10 bits (2^10=1024)
// 60 seconds/Min => 6 bits (2^6=64)
// 60 minutes/Hour => 6 bits (2^6=64)
// 24 Hours/Day => 5 bits (2^5=32)
// 366 days/Leap Year => 9 bits (2^9=512)
// 10 + 6 + 6 + 5 + 9 = 36 bits per year;

// A better Timestamp is the # of ms since epoch, not a specific unpacked date/time stamp.

// AVERAGE number of ms in a year = 1000 x 60 x 60 x 24 x 365.25 ~ 31,557,600,000 = 31557600000L
// Using that would not give an exact rollover date, since it uses the average number of days in a year. 

// n years since epoch => log_2(n) bits

// const long msInYear = 31536000000L;
// 35 bits (34.87 bits)

// const long msInLeapYear = 31622400000L;
// 35 bits (34.88 bits)

// Minimum number of bits for ONE year is 35 bits.

// Max value that can be held by b bits = 2^b - 1
// Total (AVG) years ~ (2^b - 1) / 31557600000L
