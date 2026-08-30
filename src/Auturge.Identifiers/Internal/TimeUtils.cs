namespace Auturge.Identifiers.Internal;

internal static class TimeUtils
{
    public static DateTime FindRolloverDateTime(long epoch, int stampBits)
    {
        // Max value that can be held by b bits = 2^b - 1. `stampBits` is well under 64 here
        // (FlakeConfig reserves the sign bit and the id/sequence fields), so the shift is safe.
        long availableMillis = (1L << stampBits) - 1;
        long rolloverStamp = epoch + availableMillis;

        // Beyond DateTimeOffset's range there is no representable rollover; treat it as "never".
        if (rolloverStamp < epoch || rolloverStamp > _maxUnixMillis)
        {
            return DateTime.MaxValue;
        }

        return DateTimeOffset.FromUnixTimeMilliseconds(rolloverStamp).UtcDateTime;
    }

    /// <summary>
    /// Unix-millisecond value of <see cref="DateTimeOffset.MaxValue"/>.
    /// </summary>
    private const long _maxUnixMillis = 253402300799999L;

    public static long GetUnixTimeMillis(this DateTime dateTime)
    {
        // An Unspecified DateTime is taken to be UTC; the epoch constants are declared that way.
        DateTime utc = dateTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            : dateTime.ToUniversalTime();
        return new DateTimeOffset(utc).ToUnixTimeMilliseconds();
    }
}

// Timestamps are stored as milliseconds since the epoch, not as unpacked date/time fields.
// One year of milliseconds (~31.6 billion) needs 35 bits, which is why FlakeConfig requires
// at least that many timestamp bits.
