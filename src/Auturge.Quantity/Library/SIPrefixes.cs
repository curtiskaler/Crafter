// ReSharper disable InconsistentNaming
namespace Auturge.Quantity;

public static class SIPrefixes
{
    public static readonly SIPrefix<Rational> Quecto = new("quecto", "q", 1, Math.Pow(10, 30));
    public static readonly SIPrefix<Rational> Ronto = new("ronto", "r", 1, Math.Pow(10, 27));
    public static readonly SIPrefix<Rational> Yocto = new("yocto", "y", 1, Math.Pow(10, 24));
    public static readonly SIPrefix<Rational> Zepto = new("zepto", "z", 1, Math.Pow(10, 21));
    public static readonly SIPrefix<Rational> Atto = new("atto", "a", 1, Math.Pow(10, 18));
    public static readonly SIPrefix<Rational> Femto = new("femto", "f", 1, Math.Pow(10, 15));
    public static readonly SIPrefix<Rational> Pico = new("pico", "p", 1, Math.Pow(10, 12));
    public static readonly SIPrefix<Rational> Nano = new("nano", "n", 1, Math.Pow(10, 9));
    public static readonly SIPrefix<Rational> Micro = new("micro", "μ", 1, Math.Pow(10, 6));
    public static readonly SIPrefix<Rational> Milli = new("milli", "m", 1, 1000);
    public static readonly SIPrefix<Rational> Centi = new("centi", "c", 1, 100);
    public static readonly SIPrefix<Rational> Deci = new("deci", "d", 1, 10);
    public static readonly SIPrefix<Rational> None = new("", "", 1);
    public static readonly SIPrefix<Rational> Deca = new("deca", "da", 10);
    public static readonly SIPrefix<Rational> Hecto = new("hecto", "h", 100);
    public static readonly SIPrefix<Rational> Kilo = new("kilo", "k", 1000);
    public static readonly SIPrefix<Rational> Mega = new("mega", "M", Math.Pow(10, 6));
    public static readonly SIPrefix<Rational> Giga = new("giga", "G", Math.Pow(10, 9));
    public static readonly SIPrefix<Rational> Tera = new("tera", "T", Math.Pow(10, 12));
    public static readonly SIPrefix<Rational> Peta = new("peta", "P", Math.Pow(10, 15));
    public static readonly SIPrefix<Rational> Exa = new("exa", "E", Math.Pow(10, 18));
    public static readonly SIPrefix<Rational> Zetta = new("zetta", "Z", Math.Pow(10, 21));
    public static readonly SIPrefix<Rational> Yotta = new("yotta", "Y", Math.Pow(10, 24));
    public static readonly SIPrefix<Rational> Ronna = new("ronna", "R", Math.Pow(10, 27));
    public static readonly SIPrefix<Rational> Quetta = new("quetta", "Q", Math.Pow(10, 30));
}
