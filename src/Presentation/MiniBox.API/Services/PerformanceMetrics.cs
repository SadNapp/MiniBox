using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace MiniBox.API.Services;

public sealed class PerformanceMetrics
{
    private static readonly double[] DurationBoundsSeconds = [0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10];
    private readonly ConcurrentDictionary<(string Method, int Status), long> _requests = new();
    private readonly long[] _durationBuckets = new long[DurationBoundsSeconds.Length];
    private long _requestCount;
    private long _elapsedTicks;

    public void Record(string method, int statusCode, TimeSpan elapsed)
    {
        method = method.All(char.IsAsciiLetter) ? method.ToUpperInvariant() : "OTHER";
        _requests.AddOrUpdate((method, statusCode), 1, static (_, value) => value + 1);
        Interlocked.Increment(ref _requestCount);
        Interlocked.Add(ref _elapsedTicks, elapsed.Ticks);
        var seconds = elapsed.TotalSeconds;
        for (var index = 0; index < DurationBoundsSeconds.Length; index++)
        {
            if (seconds <= DurationBoundsSeconds[index]) Interlocked.Increment(ref _durationBuckets[index]);
        }
    }

    public string ToPrometheus()
    {
        var output = new StringBuilder();
        output.AppendLine("# HELP http_requests_total Total HTTP requests served.");
        output.AppendLine("# TYPE http_requests_total counter");
        foreach (var entry in _requests.OrderBy(entry => entry.Key.Method).ThenBy(entry => entry.Key.Status))
            output.Append("http_requests_total{method=\"").Append(entry.Key.Method).Append("\",status=\"")
                .Append(entry.Key.Status.ToString(CultureInfo.InvariantCulture)).Append("\"} ")
                .AppendLine(entry.Value.ToString(CultureInfo.InvariantCulture));
        output.AppendLine("# HELP http_request_duration_seconds HTTP request duration in seconds.");
        output.AppendLine("# TYPE http_request_duration_seconds histogram");
        for (var index = 0; index < DurationBoundsSeconds.Length; index++)
        {
            output.Append("http_request_duration_seconds_bucket{le=\"")
                .Append(DurationBoundsSeconds[index].ToString("R", CultureInfo.InvariantCulture)).Append("\"} ")
                .AppendLine(Interlocked.Read(ref _durationBuckets[index]).ToString(CultureInfo.InvariantCulture));
        }
        output.Append("http_request_duration_seconds_bucket{le=\"+Inf\"} ")
            .AppendLine(Interlocked.Read(ref _requestCount).ToString(CultureInfo.InvariantCulture));
        output.Append("http_request_duration_seconds_sum ")
            .AppendLine(TimeSpan.FromTicks(Interlocked.Read(ref _elapsedTicks)).TotalSeconds.ToString("R", CultureInfo.InvariantCulture));
        output.Append("http_request_duration_seconds_count ")
            .AppendLine(Interlocked.Read(ref _requestCount).ToString(CultureInfo.InvariantCulture));
        return output.ToString();
    }
}
