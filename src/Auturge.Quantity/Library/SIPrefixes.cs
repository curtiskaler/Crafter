// ReSharper disable InconsistentNaming
namespace Auturge.Quantity;

public static class SIPrefixes
{
    public static readonly SIPrefix Quecto = new("quecto", "q", 1, Math.Pow(10, 30));
    public static readonly SIPrefix Ronto = new("ronto", "r", 1, Math.Pow(10, 27));
    public static readonly SIPrefix Yocto = new("yocto", "y", 1, Math.Pow(10, 24));
    public static readonly SIPrefix Zepto = new("zepto", "z", 1, Math.Pow(10, 21));
    public static readonly SIPrefix Atto = new("atto", "a", 1, Math.Pow(10, 18));
    public static readonly SIPrefix Femto = new("femto", "f", 1, Math.Pow(10, 15));
    public static readonly SIPrefix Pico = new("pico", "p", 1, Math.Pow(10, 12));
    public static readonly SIPrefix Nano = new("nano", "n", 1, Math.Pow(10, 9));
    public static readonly SIPrefix Micro = new("micro", "μ", 1, Math.Pow(10, 6));
    public static readonly SIPrefix Milli = new("milli", "m", 1, 1000);
    public static readonly SIPrefix Centi = new("centi", "c", 1, 100);
    public static readonly SIPrefix Deci = new("deci", "d", 1, 10);
    public static readonly SIPrefix None = new("", "", 1);
    public static readonly SIPrefix Deca = new("deca", "da", 10);
    public static readonly SIPrefix Hecto = new("hecto", "h", 100);
    public static readonly SIPrefix Kilo = new("kilo", "k", 1000);
    public static readonly SIPrefix Mega = new("mega", "M", Math.Pow(10, 6));
    public static readonly SIPrefix Giga = new("giga", "G", Math.Pow(10, 9));
    public static readonly SIPrefix Tera = new("tera", "T", Math.Pow(10, 12));
    public static readonly SIPrefix Peta = new("peta", "P", Math.Pow(10, 15));
    public static readonly SIPrefix Exa = new("exa", "E", Math.Pow(10, 18));
    public static readonly SIPrefix Zetta = new("zetta", "Z", Math.Pow(10, 21));
    public static readonly SIPrefix Yotta = new("yotta", "Y", Math.Pow(10, 24));
    public static readonly SIPrefix Ronna = new("ronna", "R", Math.Pow(10, 27));
    public static readonly SIPrefix Quetta = new("quetta", "Q", Math.Pow(10, 30));
}
