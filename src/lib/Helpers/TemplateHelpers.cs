using System.Collections;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Globalization;

namespace LowlandTech.TinyTools;

/// <summary>
/// Provides helper methods for template variable transformation.
/// Helpers are invoked using pipe syntax: ${Context.Name | upper}
/// </summary>
public static class TemplateHelpers
{
    private static readonly ConcurrentDictionary<string, Func<object?, string?, object?>> Registry = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The registered helpers, exposed read-only so a template cannot mutate
    /// process-wide helper state while other renders run. Extend behavior per render
    /// through <see cref="ExecutionContext.RegisterService"/>; <see cref="Register"/>
    /// stays for process-wide additions and <see cref="Unregister"/> undoes one.
    /// </summary>
    public static IReadOnlyDictionary<string, Func<object?, string?, object?>> Helpers => Registry;

    static TemplateHelpers()
    {
        // String helpers
        Registry["upper"] = (value, _) => value?.ToString()?.ToUpperInvariant();
        Registry["lower"] = (value, _) => value?.ToString()?.ToLowerInvariant();
        Registry["trim"] = (value, _) => value?.ToString()?.Trim();
        Registry["capitalize"] = (value, _) => Capitalize(value?.ToString());
        Registry["camelcase"] = (value, _) => ToCamelCase(value?.ToString());
        Registry["pascalcase"] = (value, _) => ToPascalCase(value?.ToString());
        Registry["kebabcase"] = (value, _) => ToKebabCase(value?.ToString());
        Registry["snakecase"] = (value, _) => ToSnakeCase(value?.ToString());
        Registry["titlecase"] = (value, _) => ToTitleCase(value?.ToString());
        Registry["truncate"] = (value, arg) => Truncate(value?.ToString(), arg);
        Registry["replace"] = (value, arg) => Replace(value?.ToString(), arg);
        Registry["padleft"] = (value, arg) => PadLeft(value?.ToString(), arg);
        Registry["padright"] = (value, arg) => PadRight(value?.ToString(), arg);

        // Date helpers
        Registry["format"] = (value, arg) => Format(value, arg);
        Registry["date"] = (value, arg) => FormatDate(value, arg);

        // Number helpers
        Registry["number"] = (value, arg) => FormatNumber(value, arg);
        Registry["round"] = (value, arg) => Round(value, arg);
        Registry["floor"] = (value, _) => Floor(value);
        Registry["ceiling"] = (value, _) => Ceiling(value);

        // Collection helpers
        Registry["count"] = (value, _) => Count(value);
        Registry["first"] = (value, _) => First(value);
        Registry["last"] = (value, _) => Last(value);
        Registry["join"] = (value, arg) => Join(value, arg);
        Registry["reverse"] = (value, _) => Reverse(value);

        // Conditional helpers
        Registry["default"] = (value, arg) => IsEmpty(value) ? arg : value;
        Registry["ifempty"] = (value, arg) => IsEmpty(value) ? arg : value;
        Registry["yesno"] = (value, arg) => YesNo(value, arg);

        // Text helpers
        Registry["slice"] = (value, arg) => Slice(value, arg);
        Registry["match"] = (value, arg) => Matches(value, arg);
        Registry["contains"] = (value, arg) => Contains(value, arg);
        Registry["split"] = (value, arg) => Split(value, arg);
        Registry["where"] = (value, arg) => Where(value, arg);
        Registry["orderby"] = (value, arg) => OrderBy(value, arg);
        Registry["groupby"] = (value, arg) => GroupBy(value, arg);
        Registry["distinct"] = (value, _) => Distinct(value);
        Registry["select"] = (value, arg) => Select(value, arg);
        Registry["sum"] = (value, _) => Aggregate(value, AggregateKind.Sum);
        Registry["avg"] = (value, _) => Aggregate(value, AggregateKind.Average);
        Registry["min"] = (value, _) => Aggregate(value, AggregateKind.Minimum);
        Registry["max"] = (value, _) => Aggregate(value, AggregateKind.Maximum);
    }

    /// <summary>
    /// Applies a helper to a value.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <param name="helperName">The helper name (e.g., "upper", "format").</param>
    /// <param name="argument">Optional argument for the helper (e.g., "yyyy-MM-dd" for format).</param>
    /// <returns>The transformed value.</returns>
    public static object? Apply(object? value, string helperName, string? argument = null)
    {
        if (Registry.TryGetValue(helperName, out var helper))
        {
            return helper(value, argument);
        }

        // Unknown helper - return original value
        return value;
    }

    /// <summary>
    /// Registers a process-wide custom helper. Prefer
    /// <see cref="ExecutionContext.RegisterService"/> for per-render behavior, which
    /// cannot change what other renders see.
    /// </summary>
    public static void Unregister(string name) => Registry.TryRemove(name, out _);

    /// <summary>
    /// Registers a process-wide custom helper. Prefer
    /// <see cref="ExecutionContext.RegisterService"/> for per-render behavior, which
    /// cannot change what other renders in the process see.
    /// </summary>
    public static void Register(string name, Func<object?, string?, object?> helper)
    {
        Registry[name] = helper;
    }

    /// <summary>
    /// Checks if a helper exists.
    /// </summary>
    public static bool Exists(string name) => Registry.ContainsKey(name);

    #region String Helpers

    private static string? Capitalize(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length == 1) return value.ToUpperInvariant();
        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }

    private static string? Truncate(string? value, string? arg)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(arg)) return value;
        if (!int.TryParse(arg, out var length)) return value;
        if (value.Length <= length) return value;
        return value[..(length - 3)] + "...";
    }

    private static string? Replace(string? value, string? arg)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(arg)) return value;
        var parts = arg.Split(',', 2);
        if (parts.Length != 2) return value;
        return value.Replace(parts[0], parts[1]);
    }

    private static string? PadLeft(string? value, string? arg)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(arg)) return value;
        var parts = arg.Split(',', 2);
        if (!int.TryParse(parts[0], out var width)) return value;
        var padChar = parts.Length > 1 && parts[1].Length > 0 ? parts[1][0] : ' ';
        return value.PadLeft(width, padChar);
    }


    private static string? PadRight(string? value, string? arg)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(arg)) return value;
        var parts = arg.Split(',', 2);
        if (!int.TryParse(parts[0], out var width)) return value;
        var padChar = parts.Length > 1 && parts[1].Length > 0 ? parts[1][0] : ' ';
        return value.PadRight(width, padChar);
    }

    private static string? ToCamelCase(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        
        var words = SplitIntoWords(value);
        if (words.Length == 0) return value;
        
        var result = words[0].ToLowerInvariant();
        for (int i = 1; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                result += char.ToUpperInvariant(words[i][0]) + words[i][1..].ToLowerInvariant();
            }
        }
        return result;
    }

    private static string? ToPascalCase(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        
        var words = SplitIntoWords(value);
        var result = "";
        foreach (var word in words)
        {
            if (word.Length > 0)
            {
                result += char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();
            }
        }
        return result;
    }

    private static string? ToKebabCase(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var words = SplitIntoWords(value);
        return string.Join("-", words.Select(w => w.ToLowerInvariant()));
    }

    private static string? ToSnakeCase(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var words = SplitIntoWords(value);
        return string.Join("_", words.Select(w => w.ToLowerInvariant()));
    }

    private static string? ToTitleCase(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var words = SplitIntoWords(value);
        return string.Join(" ", words.Select(w => Capitalize(w)));
    }

    private static string[] SplitIntoWords(string value)
    {
        // Handle camelCase boundaries, then split on non-alphanumeric characters
        var normalized = System.Text.RegularExpressions.Regex.Replace(value, @"([a-z])([A-Z])", "$1 $2");
        return System.Text.RegularExpressions.Regex
            .Split(normalized, @"[^a-zA-Z0-9]+")
            .Where(w => w.Length > 0)
            .ToArray();
    }

    #endregion

    #region Date Helpers

    private static string? Format(object? value, string? arg)
    {
        if (value == null || string.IsNullOrEmpty(arg)) return value?.ToString();

        return value switch
        {
            DateTime dt => dt.ToString(arg, CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString(arg, CultureInfo.InvariantCulture),
#if NET6_0_OR_GREATER
            DateOnly d => d.ToString(arg, CultureInfo.InvariantCulture),
            TimeOnly t => t.ToString(arg, CultureInfo.InvariantCulture),
#endif
            IFormattable f => f.ToString(arg, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    private static string? FormatDate(object? value, string? arg)
    {
        var format = string.IsNullOrEmpty(arg) ? "yyyy-MM-dd" : arg;
        return Format(value, format);
    }

    #endregion

    #region Number Helpers

    private static string? FormatNumber(object? value, string? arg)
    {
        var format = string.IsNullOrEmpty(arg) ? "N0" : arg;
        return Format(value, format);
    }

    private static object? Round(object? value, string? arg)
    {
        if (value == null) return null;
        var decimals = 0;
        if (!string.IsNullOrEmpty(arg)) int.TryParse(arg, out decimals);

        return value switch
        {
            double d => Math.Round(d, decimals),
            decimal dec => Math.Round(dec, decimals),
            float f => Math.Round(f, decimals),
            _ => value
        };
    }

    private static object? Floor(object? value)
    {
        return value switch
        {
            double d => Math.Floor(d),
            decimal dec => Math.Floor(dec),
            float f => Math.Floor(f),
            _ => value
        };
    }

    private static object? Ceiling(object? value)
    {
        return value switch
        {
            double d => Math.Ceiling(d),
            decimal dec => Math.Ceiling(dec),
            float f => Math.Ceiling(f),
            _ => value
        };
    }

    #endregion

    #region Collection Helpers

    private static object? Count(object? value)
    {
        return value switch
        {
            null => 0,
            string s => s.Length,
            ICollection c => c.Count,
            IEnumerable e => e.Cast<object>().Count(),
            _ => 1
        };
    }

    private static object? First(object? value)
    {
        return value switch
        {
            null => null,
            string s => s.Length > 0 ? s[0].ToString() : null,
            IEnumerable e => e.Cast<object>().FirstOrDefault(),
            _ => value
        };
    }

    private static object? Last(object? value)
    {
        return value switch
        {
            null => null,
            string s => s.Length > 0 ? s[^1].ToString() : null,
            IEnumerable e => e.Cast<object>().LastOrDefault(),
            _ => value
        };
    }

    private static string? Join(object? value, string? arg)
    {
        if (value is not IEnumerable enumerable || value is string) return value?.ToString();
        var separator = arg ?? ", ";
        return string.Join(separator, enumerable.Cast<object>());
    }

    private static object? Reverse(object? value)
    {
        return value switch
        {
            null => null,
            string s => new string(s.Reverse().ToArray()),
            IEnumerable e => e.Cast<object>().Reverse().ToList(),
            _ => value
        };
    }

    #endregion

    private static IEnumerable<object?> Items(object? value) =>
        value is IEnumerable enumerable and not string
            ? enumerable.Cast<object?>()
            : [];

    private static object? ReadMember(object? item, string name)
    {
        if (item is null) return null;
        if (item is IDictionary<string, object?> map)
        {
            foreach (var pair in map)
            {
                if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase)) return pair.Value;
            }

            return null;
        }

        var property = item.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return property?.GetValue(item);
    }

    private static bool TryReadMember(object? item, string name, out object? value)
    {
        if (item is IDictionary<string, object?>)
        {
            value = ReadMember(item, name);
            return item is IDictionary<string, object?> map
                && map.Keys.Any(key => string.Equals(key, name, StringComparison.OrdinalIgnoreCase));
        }

        var property = item?.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        value = property?.GetValue(item);
        return property is not null;
    }

    private static bool MatchesPredicate(object? item, string predicate)
    {
        var negate = predicate.Contains("!=", StringComparison.Ordinal);
        var parts = predicate.Split(negate ? "!=" : "==", 2, StringSplitOptions.None);
        if (parts.Length != 2) throw new ArgumentException($"where expects 'path==value', received '{predicate}'.");

        var actual = ReadMember(item, parts[0].Trim());
        var expected = parts[1].Trim().Trim('"', '\'');
        var equal = string.Equals(Text(actual), expected, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(Convert.ToString(actual, CultureInfo.InvariantCulture), expected, StringComparison.OrdinalIgnoreCase);
        return negate ? !equal : equal;
    }

    private static string Text(object? value) => value switch
    {
        null => string.Empty,
        bool flag => flag ? "true" : "false",
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
    };

    private static List<object?> Where(object? value, string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            throw new ArgumentException("where requires a 'path==value' predicate.", nameof(argument));

        return Items(value).Where(item => MatchesPredicate(item, argument)).ToList();
    }

    private static List<object?> OrderBy(object? value, string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            throw new ArgumentException("orderby requires a member path.", nameof(argument));

        var path = argument.Trim();
        var items = Items(value).ToList();
        foreach (var item in items)
        {
            if (!TryReadMember(item, path, out _))
            {
                throw new ArgumentException($"orderby: '{path}' is not a member of every item.");
            }
        }

        // LINQ ordering is stable, so equal keys keep their source order.
        return items
            .OrderBy(item => ReadMember(item, path), Comparer<object?>.Create(Compare))
            .ToList();
    }

    private static int Compare(object? left, object? right)
    {
        if (left is null && right is null) return 0;
        if (left is null) return -1;
        if (right is null) return 1;

        if (left is IComparable comparable && left.GetType() == right.GetType())
        {
            return comparable.CompareTo(right);
        }

        return string.CompareOrdinal(Text(left), Text(right));
    }

    private static List<IGrouping<object?, object?>> GroupBy(object? value, string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            throw new ArgumentException("groupby requires a member path.", nameof(argument));

        var path = argument.Trim();
        var items = Items(value).ToList();
        foreach (var item in items)
        {
            if (!TryReadMember(item, path, out _))
            {
                throw new ArgumentException($"groupby: '{path}' is not a member of every item.");
            }
        }

        // Grouping keeps the first-seen key order and the source order inside each group.
        return items
            .GroupBy(item => ReadMember(item, path))
            .Select(group => (IGrouping<object?, object?>)group)
            .ToList();
    }

    private enum AggregateKind { Sum, Average, Minimum, Maximum }

    private static List<object?> Select(object? value, string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            throw new ArgumentException("select requires a member path.", nameof(argument));

        var path = argument.Trim();
        var items = Items(value).ToList();
        foreach (var item in items)
        {
            if (!TryReadMember(item, path, out _))
            {
                throw new ArgumentException($"select: '{path}' is not a member of every item.");
            }
        }

        return items.Select(item => ReadMember(item, path)).ToList();
    }

    private static object? Aggregate(object? value, AggregateKind kind)
    {
        var numbers = new List<decimal>();
        foreach (var item in Items(value))
        {
            // Non-numeric and null entries are skipped rather than coerced, so a mixed collection
            // aggregates its numbers instead of failing.
            if (item is null || item is string) continue;
            try
            {
                numbers.Add(Convert.ToDecimal(item, CultureInfo.InvariantCulture));
            }
            catch (Exception error) when (error is InvalidCastException or FormatException or OverflowException)
            {
                // skipped on purpose
            }
        }

        return kind switch
        {
            AggregateKind.Sum => numbers.Sum(),
            AggregateKind.Average => numbers.Count == 0 ? null : numbers.Average(),
            AggregateKind.Minimum => numbers.Count == 0 ? null : numbers.Min(),
            AggregateKind.Maximum => numbers.Count == 0 ? null : numbers.Max(),
            _ => null,
        };
    }
    private static List<object?> Distinct(object? value)
    {
        var seen = new List<object?>();
        foreach (var item in Items(value))
        {
            if (!seen.Any(existing => Equals(existing, item))) seen.Add(item);
        }

        return seen;
    }
    #region Conditional Helpers

    private static bool IsEmpty(object? value)
    {
        return value switch
        {
            null => true,
            string s => string.IsNullOrEmpty(s),
            ICollection c => c.Count == 0,
            IEnumerable e => !e.Cast<object>().Any(),
            _ => false
        };
    }

    private static string? YesNo(object? value, string? arg)
    {
        var parts = (arg ?? "Yes,No").Split(',', 2);
        var yesValue = parts[0];
        var noValue = parts.Length > 1 ? parts[1] : "No";

        var isTruthy = value switch
        {
            null => false,
            bool b => b,
            int i => i != 0,
            string s => !string.IsNullOrEmpty(s) && !s.Equals("false", StringComparison.OrdinalIgnoreCase),
            _ => true
        };

        return isTruthy ? yesValue : noValue;
    }

    #endregion

    #region Text Helpers

    /// <summary>
    /// slice:start,end - zero-based UTF-16 range with an inclusive start and an
    /// exclusive end. Equal in-range indexes yield the empty string.
    /// </summary>
    private static string? Slice(object? value, string? argument)
    {
        if (value is null) return null;

        if (string.IsNullOrWhiteSpace(argument))
            throw new ArgumentException("slice requires a start,end argument.", nameof(argument));

        var parts = argument.Split(',');
        if (parts.Length != 2
            || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var start)
            || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var end))
        {
            throw new ArgumentException($"slice requires a start,end pair of integers, not '{argument}'.", nameof(argument));
        }

        var text = value.ToString() ?? string.Empty;
        if (start < 0 || end < start || end > text.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(argument), argument, $"slice bounds '{argument}' are outside the range 0..{text.Length}.");
        }

        return text[start..end];
    }

    /// <summary>
    /// match:pattern - .NET regular-expression match with case-sensitive defaults
    /// and a fixed one second timeout. A timeout is never reported as false.
    /// </summary>
    private static bool Matches(object? value, string? argument)
    {
        if (value is null) return false;

        if (string.IsNullOrEmpty(argument))
            throw new ArgumentException("match requires a regular expression.", nameof(argument));

        Regex expression;
        try
        {
            expression = new Regex(argument, RegexOptions.None, TimeSpan.FromSeconds(1));
        }
#if NET7_0_OR_GREATER
        catch (RegexParseException error)
#else
        catch (ArgumentException error)
#endif
        {
            // One named refusal for a malformed pattern, so the parser's own
            // subtype is normalized to ArgumentException.
            throw new ArgumentException($"match received a malformed pattern: {error.Message}", nameof(argument), error);
        }

        return expression.IsMatch(value.ToString() ?? string.Empty);
    }

    /// <summary>
    /// contains:value - ordinal case-sensitive substring match for strings, and
    /// element equality for other enumerables. Values are never coerced to text.
    /// </summary>
    private static bool Contains(object? value, string? argument)
    {
        if (argument is null)
            throw new ArgumentException("contains requires a value.", nameof(argument));

        if (value is null) return false;

        if (value is string text)
            return text.Contains(argument, StringComparison.Ordinal);

        if (value is IEnumerable sequence)
        {
            foreach (var item in sequence)
            {
                if (Equals(item, argument)) return true;
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// split:separator - literal separator with leading, adjacent and trailing
    /// empty entries preserved. A null input yields an empty array.
    /// </summary>
    private static string[] Split(object? value, string? argument)
    {
        if (value is null) return [];

        if (string.IsNullOrEmpty(argument))
            throw new ArgumentException("split requires a separator.", nameof(argument));

        return (value.ToString() ?? string.Empty).Split([argument], StringSplitOptions.None);
    }

    #endregion
}
