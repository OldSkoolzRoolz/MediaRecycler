// Project Name: MediaRecycler
// File Name: FormattedLogValues.cs
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using System.Collections;
using System.Collections.Concurrent;



namespace MediaRecycler.Logging;


/// <summary>
///     Represents a formatted log message and its named values for structured logging.
///     <para>
///         <b>Purpose:</b> This struct is used to format log messages with named placeholders (e.g., "User {UserId} logged
///         in")
///         and expose both the formatted string and its named values as key-value pairs. This enables efficient structured
///         logging,
///         where log consumers can access both the rendered message and its parameters.
///     </para>
///     <para>
///         <b>How it works:</b>
///         <list type="bullet">
///             <item>
///                 <description>
///                     When constructed, it takes a format string and an array of values. If the format string and values
///                     are valid,
///                     it uses a cached <see cref="LogValuesFormatter" /> to parse and format the message. The cache is
///                     limited to <see cref="MaxCachedFormatters" /> entries for efficiency.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                     The struct implements <see cref="IReadOnlyList{T}" /> of
///                     <see cref="KeyValuePair{String, Object}" />, exposing each named value in the format string,
///                     plus a final entry with the key "{OriginalFormat}" containing the original format string.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                     The <see cref="ToString" /> method returns the formatted message, caching the result for repeated
///                     calls.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                     If no formatter is available (e.g., null or empty format/values), it simply returns the original
///                     message or a "[null]" placeholder.
///                 </description>
///             </item>
///         </list>
///     </para>
///     <para>
///         <b>Example:</b>
///         <code>
/// var logValues = new FormattedLogValues("User {UserId} logged in at {Time}", 42, DateTime.Now);
/// string message = logValues.ToString(); // "User 42 logged in at 7/2/2025 10:00:00 AM"
/// foreach (var pair in logValues)
/// {
///     // pair.Key: "UserId", "Time", "{OriginalFormat}"
///     // pair.Value: 42, DateTime.Now, "User {UserId} logged in at {Time}"
/// }
/// </code>
///     </para>
/// </summary>
internal struct FormattedLogValues : IReadOnlyList<KeyValuePair<string, object?>>
{

    internal const int MaxCachedFormatters = 1024;
    private const string NullFormat = "[null]";

    private static int s_count;
    private static readonly ConcurrentDictionary<string, LogValuesFormatter> s_formatters = new();

    private readonly LogValuesFormatter? _formatter;
    private readonly object?[]? _values;
    private readonly string _originalMessage;
    private string? _cachedToString;

    // for testing purposes
    internal LogValuesFormatter? Formatter => _formatter;






    public FormattedLogValues(string? format, params object?[]? values)
    {
        if (values != null && values.Length != 0 && format != null)
        {
            if (s_count >= MaxCachedFormatters)
            {
                if (!s_formatters.TryGetValue(format, out _formatter)) _formatter = new LogValuesFormatter(format);
            }
            else
            {
                _formatter = s_formatters.GetOrAdd(format, f =>
                {
                    Interlocked.Increment(ref s_count);
                    return new LogValuesFormatter(f);
                });
            }
        }
        else
        {
            _formatter = null;
        }

        _originalMessage = format ?? NullFormat;
        _values = values;
        _cachedToString = null;
    }






    public KeyValuePair<string, object?> this[int index]
    {
        get
        {
            if (index < 0 || index >= Count) throw new IndexOutOfRangeException(nameof(index));

            if (index == Count - 1) return new KeyValuePair<string, object?>("{OriginalFormat}", _originalMessage);

            return _formatter!.GetValue(_values!, index);
        }
    }

    public int Count
    {
        get
        {
            if (_formatter == null) return 1;

            return _formatter.ValueNames.Count + 1;
        }
    }






    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        for (int i = 0; i < Count; ++i) yield return this[i];
    }






    public override string ToString()
    {
        if (_formatter == null) return _originalMessage;

        return _cachedToString ??= _formatter.Format(_values);
    }






    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

}