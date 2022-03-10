namespace Ummati.Infrastructure;

using System;
using System.Text.RegularExpressions;

internal static class Guard
{
    public static void NotNull(string name, string value) => ArgumentNullException.ThrowIfNull(value, name);

    public static void NotNullOrEmpty(string name, string value)
    {
        ArgumentNullException.ThrowIfNull(value, name);
        if (value.Length == 0)
        {
            throw new ArgumentException($"{name} cannot be empty.", name);
        }
    }

    public static void IsMatch(string name, string value, string pattern)
    {
        ArgumentNullException.ThrowIfNull(value, name);

        if (!Regex.IsMatch(value, pattern))
        {
            throw new ArgumentException(
                $"{name} with value '{value}' must match the pattern '{pattern}'.",
                name);
        }
    }

    public static void IsBetween<T>(string name, IEnumerable<T> value, T? minimum = null, T? maximum = null)
        where T : struct, IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(value, name);
        foreach (var item in value)
        {
            IsBetween(name, item, minimum, maximum);
        }
    }

    public static void IsBetween<T>(string name, T value, T? minimum = null, T? maximum = null)
        where T : struct, IComparable<T>
    {
        if (minimum.HasValue && value.CompareTo(minimum.Value) < 0)
        {
            throw new ArgumentOutOfRangeException(name, $"{name} with value '{value}' must be more than {minimum}");
        }

        if (maximum.HasValue && value.CompareTo(maximum.Value) > 0)
        {
            throw new ArgumentOutOfRangeException(name, $"{name} with value '{value}' must be less than {maximum}");
        }
    }
}
