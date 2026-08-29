namespace Auturge.Quantity;

public static class DoubleExtensions
{
    /// <param name="lhs"></param>
    extension(double lhs)
    {
        /// <summary>
        /// Given the specified <paramref name="epsilon"/>,
        /// can the computer tell the difference between the specified floating-point values?
        /// Essentially, this is the "equality" check for floating-point numbers.
        /// </summary>
        /// <param name="rhs"></param>
        /// <param name="epsilon">The precision to demand. </param>
        /// <returns><see langword="true"/> if | lhs - rhs | &lt;= &#949;; otherwise, <see langword="false"/>.</returns>
        public bool ApproxEqual(double rhs, double epsilon = double.Epsilon) 
            => Math.Abs(lhs - rhs) <= epsilon;

        internal void ExpectOne()
        {
            if (!lhs.ApproxEqual( 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(lhs), lhs, "argument must be one (1).");
            }
        }
    }
}
