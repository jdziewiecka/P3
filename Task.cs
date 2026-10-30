using System.Globalization;

namespace Task2;
public readonly record struct Reading(string Sensor, double Value, DateTime At);
public record class Data(
    double HighestValue,                        // highest recorded value across all sensors
    double LowestTemp,                          // lowest recorded temperature
    string MostReadings,                        // name of the sensor with most readings
    TimeSpan TotalDuration,                     // difference between first and last recorded reading
    IList<string> UniqueSensors,                // list of all sensor names without duplicates
    IList<(string, double)> AverageReadings,    // average reading per sensor
    IList<(string, TimeSpan)> LargestGaps,      // a largest timespan between two consecutive readings (time order) per sensor
    IList<(Reading, Reading)> SignChanges       // two consecutive readings (time order) having two differnet signs (first < second) 
);

public class SensorLog {
    private readonly List<Reading> _readings = new();
    public void Add(Reading r) { _readings.Add(r); }
    public IReadOnlyList<Reading> All => _readings;

    public SensorLog LoadFromCsv(string path) {
        bool first = true;
        foreach (var line in File.ReadLines(path)) {
            if (first) { first = false; continue; } // assuming header            
            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }
            var parts = line.Split(',');
            Add(new Reading(parts[0].Trim(), double.Parse(parts[1], CultureInfo.InvariantCulture), DateTime.Parse(parts[2], CultureInfo.InvariantCulture)));
        }
        return this;
    }
}

// =============================================================
// TODO 1
// =============================================================

public static class T1 {
    public static IEnumerable<Reading> InRange(IEnumerable<Reading> source, double min, double max) {

        foreach (var reading in source)
        {
            if (reading.Value >= min && reading.Value <= max)
            {
                yield return reading;
            }
        }
        
    }
}

// =============================================================
// TODO 2 
// =============================================================

public static class T2 {
    public static IEnumerable<Reading> Repeat(string sensor, double value, int count, DateTime start) {
        for (double i = 0; i < count; i ++)
        {
            double currentValue = value + i * 0.2;
            DateTime currentTime = start.AddMinutes(i * 2);
            yield return new Reading(sensor, currentValue, currentTime);
        }
    }
}

// =============================================================
// TODO 3
// =============================================================

public class ReadingBySensorThenValue : Comparer<Reading> {
    public override int Compare(Reading x, Reading y)
    {
        int sensorComparison = string.Compare(x.Sensor, y.Sensor, StringComparison.Ordinal);
        if (sensorComparison != 0) {
            return sensorComparison;
        }
        
        int valueComparison = y.Value.CompareTo(x.Value);
        if (valueComparison != 0) {
            return valueComparison;
        }
        
        return x.At.CompareTo(y.At);
    }
}

// =============================================================
// TODO 4 
// =============================================================

public static class T4 {
    public static Data Summarize(IEnumerable<Reading> source) {
        var list = source.ToList();
        if (list.Count == 0) {
            return new Data(0, 0, string.Empty, TimeSpan.Zero, new List<string>(), new List<(string, double)>(), new List<(string, TimeSpan)>(), new List<(Reading, Reading)>());
        }

        double highestValue = list.Max(reading => reading.Value);
        double lowestValue = list.Min(reading => reading.Value);
        
        string MostReadings = list.GroupBy(r => r.Sensor)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .First().Key;
        
        TimeSpan TotalDuration = list.Max(r => r.At) -  list.Min(r => r.At);

        IList<string> UniqueSensors = list.Select(r => r.Sensor).OrderBy(s => s).Distinct().ToList();
        
        IList<(string, double)> AverageReadings = list.GroupBy(r => r.Sensor)
            .Select(g => (g.Key, g.Average(reading => reading.Value))).OrderBy(g => g.Key).ToList<(string, double)>();
        
        IList<(string, TimeSpan)> LargestGaps = list.GroupBy(r => r.Sensor)
            .Select(g =>
            {
                var ordered = g.OrderBy(r => r.At).ToList();
                TimeSpan maxGap = TimeSpan.Zero;
                for (int i = 1; i < ordered.Count; i++) {
                    var gap = ordered[i].At - ordered[i - 1].At;
                    if (gap > maxGap) {
                        maxGap = gap;
                    }
                }
                return (g.Key, maxGap);
            }).OrderBy(g => g.Key).ToList<(string, TimeSpan)>();
        
        IList<(Reading, Reading)> SignChanges = list.GroupBy(r => r.Sensor)
            .OrderBy(g => g.Key)
            .SelectMany(g => {
                var ordered = g.OrderBy(r => r.At).ToList();
                var changes = new List<(Reading, Reading)>();
                for (int i = 1; i < ordered.Count; i++) {
                    var prev = ordered[i - 1];
                    var curr = ordered[i];
                    if (prev.Value > 0 && curr.Value < 0) {
                        changes.Add((prev, curr));
                    }
                }
                return changes;
            })
            .ToList();
        
        return new Data(highestValue, lowestValue, MostReadings, TotalDuration, UniqueSensors, AverageReadings, LargestGaps, SignChanges);
        

    }
}

// =============================================================
// TODO 5 
// =============================================================

public static class T5 {
    public static string Classify(IReadOnlyList<Reading> readings)
    {
        return readings switch 
        {
            [] => "empty",
            [{ }] => "single",
            [{ Value: > 100 }, ..] => "starts-high",
            [.., { Value: < 0 }] => "ends-low",
            [{ Value: var v1 }, { Value: var v2 }] when v1 < v2 => "pair-asc",
            [{ Value: var v1 }, { Value: var v2 }] when v1 > v2 => "pair-desc",
            [{ Value: var v1 }, { Value: var v2 }] when v1 == v2 => "pair-eq",
            
            _ => "other"
        };
    }
}