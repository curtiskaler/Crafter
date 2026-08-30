using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

// ReSharper disable InconsistentNaming

namespace Auturge.Quantity;

public class UnitConversionGraph<T> : DirectedGraph<Unit> where T : INumber<T>, IConvertible
{
    protected List<UnitConversion<T>> _conversions { get; set; }

    internal UnitConversionGraph(IEnumerable<UnitConversion<T>> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        // distinctify the conversions
        var conversions = list.Distinct().ToList();

        _conversions = conversions;
        conversions.ForEach(conv =>
        {
            AddEdge(conv.SourceUnit, conv.TargetUnit);

            // also handle base units
            if (conv.SourceUnit.Base != null)
            {
                AddEdge(conv.SourceUnit, conv.SourceUnit.Base);
            }

            if (conv.TargetUnit.Base != null)
            {
                AddEdge(conv.TargetUnit, conv.TargetUnit.Base);
            }
        });

        Debug.WriteLine(_conversions.Count);
    }

    public bool TryFindPath(Unit start, Unit target, [MaybeNullWhen(false)] out UnitConversion<T> conversion)
    {
        conversion = null;

        // probably need to consider numerator and denominator separately.
        // take them to their base
        // then take them back up

        // this might be extra-special naive, as voltage includes things like time, so maybe
        // specific dimensions becomes important...

        // ex: m/s -> mi/hr
        List<Unit> sNumUnits = start.GetNumeratorUnits();
        List<Unit> tNumUnits = target.GetNumeratorUnits();
        bool foundNumeratorConversion = TryGetConversion(sNumUnits, tNumUnits, out UnitConversion<T>? nConversion);
        if (!foundNumeratorConversion || nConversion == null)
        {
            return false;
        }

        List<Unit> sDenUnits = start.GetDenominatorUnits();
        List<Unit> tDenUnits = target.GetDenominatorUnits();
        bool foundDenominatorConversion = TryGetConversion(sDenUnits, tDenUnits, out UnitConversion<T>? dConversion);
        if (!foundDenominatorConversion || dConversion == null)
        {
            return false;
        }

        // Now we've got the numerator and denominator converters. Combine them.
        Func<T, T> conv = GetFractionConverter(nConversion, dConversion);
        Func<T, T> inv = GetFractionConverter(nConversion.Invert(), dConversion.Invert());
        conversion = new UnitConversion<T>(start, target, conv, inv);
        return true;
    }

    private bool TryGetConversion(List<Unit> sUnits, List<Unit> tUnits,
        [MaybeNullWhen(false)] out UnitConversion<T> conversion)
    {
        conversion = null;
        List<UnitConversion<T>> list = [];
        foreach (Unit sourceUnit in sUnits)
        {
            // find the target numerator units in the same dimension as this guy
            var numSameDimension = tUnits.Where(x => x.Dimension == sourceUnit.Dimension).ToList();
            if (numSameDimension.Count != 1)
            {
                //  There shouldn't be more than one Unit in each dimension in a unit
                return false;
            }

            Unit targetUnit = numSameDimension[0];

            bool found = _tryFindConverter(sourceUnit, targetUnit, out UnitConversion<T>? converter);
            if (!found || converter == null)
            {
                return false;
            }

            list.Add(converter);
        }

        conversion = new UnitConversion<T>(list);
        return true;
    }


    public Func<T, T> GetFractionConverter(UnitConversion<T> nConversion, UnitConversion<T> dConversion)
        => (T number) =>
        {
            decimal dec = number.ToDecimal(CultureInfo.CurrentCulture);
            // Rational.FromDecimal is exact (decimal already IS a scaled integer, via
            // decimal.GetBits), and T.CreateChecked bridges the numerator/denominator straight to T.
            Rational frac = Rational.FromDecimal(dec);

            T topT = T.CreateChecked(frac.Numerator);
            T bottomT = T.CreateChecked(frac.Denominator);

            T top = nConversion.Conversion.Execute(topT);
            T bottom = dConversion.Conversion.Execute(bottomT);
            return top / bottom;
        };


    private bool _tryFindConverter(Unit source, Unit target, [MaybeNullWhen(false)] out UnitConversion<T> conversion)
    {
        conversion = null;
        List<UnitConversion<T>> convertersForThisUnit = [];

        // find the shortest path from the source to the target
        // BFS is wrong. Collects too many things.
        bool found = TryFindBFS(source, target, out IEnumerable<Unit> path);
        if (!found)
        {
            return false;
        }

        var unitList = path.ToImmutableList();

        int srcIndex = unitList.IndexOf(source); // should be zero
        int trgIndex = unitList.IndexOf(target); // should be the last

        if (srcIndex == -1 || trgIndex == -1) // couldn't find it
        {
            return false;
        }

        if (trgIndex == 0) // it's the identity
        {
            conversion = new UnitConversion<T>(source, target, x => x, x => x);
            return true;
        }

        // build the chain
        for (int i = 0; i < trgIndex; i++)
        {
            // TODO: yea this isn't right :) It really only proves that it can be done.

            Unit sourceUnit = unitList[i];
            Unit targetUnit = unitList[i + 1];

            conversion = _conversions
                .Find(x => x.CanHandle(sourceUnit, targetUnit));

            if (conversion == null)
            {
                return false;
            }

            UnitConversion<T> directional = conversion.SourceUnit == sourceUnit ? conversion : conversion.Invert();
            convertersForThisUnit.Add(directional);
        }

        Console.WriteLine("Now inspect it to find a path from start to finish.");
        conversion = new UnitConversion<T>(convertersForThisUnit);
        return true;
    }

    private bool _TryFindPath(Unit start, Unit target,
        [MaybeNullWhen(false)] out UnitConversion<T> conversion)
    {
        List<UnitConversion<T>> convertersForThisUnit = [];

        // find the shortest path from the source to the target
        // BFS is wrong. Collects too many things.
        HashSet<Unit> numeratorSet = BFS(start);
        var numeratorList = numeratorSet.ToImmutableList();

        int index = numeratorList.IndexOf(target);

        if (index == -1) // couldn't find it
        {
            conversion = null;
            return false;
        }

        if (index == 0) // it's the identity
        {
            conversion = new UnitConversion<T>(start, target, x => x, x => x);
            return true;
        }

        // build the chain
        for (int i = 0; i < index; i++)
        {
            // TODO: yea this isn't right :) It really only proves that it can be done.

            Unit sourceUnit = numeratorList[i];
            Unit targetUnit = numeratorList[i + 1];

            conversion = _conversions
                .Find(x => x.CanHandle(sourceUnit, target));

            if (conversion == null)
            {
                return false;
            }

            UnitConversion<T> directional = conversion.SourceUnit == sourceUnit ? conversion : conversion.Invert();
            convertersForThisUnit.Add(directional);
        }

        Console.WriteLine("Now inspect it to find a path from start to finish.");
        conversion = new UnitConversion<T>(convertersForThisUnit);
        return true;
    }
}
