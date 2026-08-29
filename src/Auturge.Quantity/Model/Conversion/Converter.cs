namespace Auturge.Quantity;

public class Converter<TIn, TOut>(Func<TIn, TOut> convert, Func<TIn, TOut> invert)
{
    public Func<TIn, TOut> Convert { get; } = convert;
    public Func<TIn, TOut> Invert { get; } = invert;
}

public class Converter<T>(Func<T, T> convert, Func<T, T> invert) : Converter<T, T>(convert, invert);

public class Converter(Func<object, object> convert, Func<object, object> invert) : Converter<object>(convert, invert);
