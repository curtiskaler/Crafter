// ReSharper disable MemberCanBePrivate.Global

using System.Reflection;

namespace Auturge.Quantity;

#pragma warning disable CS8604
internal static class Reflection
{
    internal static List<TElement> GetStaticElements<TClass, TElement>()
        where TElement : class
    {
        return GetValuesOfType<TElement>(typeof(TClass), true);
    }

    // If GetUnderlyingType returns non-null, T is a Nullable<U> for some value type U.
    internal static bool IsNullableValueType<T>()
        => Nullable.GetUnderlyingType(typeof(T)) != null;

    /// <summary>
    /// Searches the Type <paramref name="type"/>
    /// for static members of the given type <typeparam name="T"/>,
    /// and returns all the associated values.
    /// </summary>
    /// <param name="type">The class/type to search for values.</param>
    /// <param name="public">Include public members?</param>
    /// <typeparam name="T">The type of the members to find.</typeparam>
    internal static List<T> GetValuesOfType<T>(Type type, bool @public = true) where T : class
        => GetValuesOfType<T>(type, null, @public);

    /// <summary>
    /// Searches the object <see paramref="instance"/>
    /// (or the Type <paramref name="type"/>, if <paramref name="instance"/> is null)
    /// for members of the given type <typeparam name="T"/>,
    /// and returns all the associated values.
    /// </summary>
    /// <param name="type">The class/type to search for values.</param>
    /// <param name="instance">
    /// The object instance to get the values from.
    /// If <see langword="null"/>, gets the values of the static members of the given type.
    /// </param>
    /// <param name="public">Include public members?</param>
    /// <typeparam name="T">The type of the members to find.</typeparam>
    internal static List<T> GetValuesOfType<T>(Type type, object? instance = null, bool @public = true) where T : class
    {
        var @static = instance is null;
        var flags = GetBindingFlags(@public, @static);
        return GetValuesOfType<T>(type, flags, instance);
    }

    /// <summary>
    /// Searches the object <see paramref="instance"/>
    /// (or the Type <paramref name="type"/>, if <paramref name="instance"/> is null)
    /// for members of the given type <typeparam name="T"/> using the given <see cref="BindingFlags"/>,
    /// and returns all the associated values.
    /// </summary>
    /// <param name="type">The class/type to search for values.</param>
    /// <param name="flags">The <see cref="BindingFlags"/> to use during reflection.</param>
    /// <param name="instance">
    /// The object instance to get the values from.
    /// If <see langword="null"/>, gets the values of the static members of the given type.
    /// </param>
    /// <typeparam name="T">The type of the members to find.</typeparam>
    internal static List<T> GetValuesOfType<T>(Type type, BindingFlags flags, object? instance = null) where T : class
    {
        // TODO: make sure that the static properties are initialized
        
        
        // Make sure that it won't explode trying to find static members.
        if (!flags.HasFlag(BindingFlags.Static) && instance is null)
        {
            flags |= BindingFlags.Static;
        }

        // catch the fields
        var fis = GetFieldsOfType<T>(type, flags);
        var fieldValues = fis.GetValues(instance);

        // catch the properties
        var pis = GetPropertiesOfType<T>(type, flags);
        var propValues = pis.GetValues(instance);

        // Combine the list of values, then cast them to the desired type.
        List<object?> combined = [..fieldValues, ..propValues];
        
        // trim out the nulls
        var values = combined.As<T>().NotNull();

        // Now uniquify the list
        var distinct = values.Distinct().ToList();
        return distinct;
    }

    internal static PropertyInfo[] GetPropertiesOfType<T>(Type type, BindingFlags flags)
    {
        var props = type.GetProperties(flags)
            .Where(p => p.PropertyType == typeof(T))
            .ToArray();
        return props;
    }

    internal static FieldInfo[] GetFieldsOfType<T>(Type type, BindingFlags flags)
    {
        var fields = type.GetFields(flags)
            .Where(p => p.FieldType == typeof(T))
            .ToArray();
        return fields;
    }

    internal static BindingFlags GetBindingFlags(bool @public = true, bool @static = false)
    {
        BindingFlags flags = BindingFlags.Default;
        if (@public)
        {
            flags |= BindingFlags.Public;
        }

        flags |= @static ? BindingFlags.Static : BindingFlags.Instance;
        return flags;
    }

    internal static List<object?> GetValues(this FieldInfo[] fields, object? instance = null)
        => fields.Select(field => field.GetValue(instance)).ToList();

    internal static List<object?> GetValues(this PropertyInfo[] properties, object? instance = null)
        => properties.Select(prop => prop.GetValue(instance)).ToList();

    /// <summary>
    /// Converts a List&lt;object?&gt; to a typed List&lt;T&gt;. Filters out null values.
    /// </summary>
    /// <param name="values">A List&lt;object?&gt;</param>
    /// <typeparam name="T">The type to convert the list entries to.</typeparam>
    /// <returns></returns>
    internal static List<T> As<T>(this List<object?> values) where T : class
    {
        var isNullable = IsNullableValueType<T>();

        var result = new List<T>();

        // validate the list
        foreach (object? obj in values)
        {
            // if the list is of a nullable type, and the element is null, that's fine; don't include it.
            if (isNullable && obj is null)
            {
                continue;
            }

            // Now we know that the return type is either NOT nullable, or the obj isn't null.
            var value = obj as T;

            // if it just didn't cast right, then explode:
            if (value is null && obj is not null) throw new InvalidCastException();
            result.Add(value);
        }

        return result;
    }

    internal static List<T> NotNull<T>(this List<T> values)
    {
        return values
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();
    }
}
