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
        
        var tmpReadings = list.Where(r => string.Equals(r.Sensor, "tmp", StringComparison.OrdinalIgnoreCase));
        double lowestTemp = tmpReadings.Any() ? tmpReadings.Min(r => r.Value) : 0; // lub inna wartość domyślna
        
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
        
        // if (_readings.Count == 0) {
        //     return new Data(
        //         HighestValue: 0,
        //         LowestTemp: 0,
        //         MostReadings: string.Empty,
        //         TotalDuration: TimeSpan.Zero,
        //         UniqueSensors: new List<string>(),
        //         AverageReadings: new List<(string, double)>(),
        //         LargestGaps: new List<(string, TimeSpan)>(),
        //         SignChanges: new List<(Reading, Reading)>()
        //     );
        // }
        //
        // // 1. Najwyższa wartość i najniższa temperatura ogółem
        // double highestValue = _readings[0].Value;
        // double lowestTemp = _readings[0].Value;
        // DateTime minAt = _readings[0].At;
        // DateTime maxAt = _readings[0].At;
        //
        // for (int i = 0; i < _readings.Count; i++) {
        //     var r = _readings[i];
        //     if (r.Value > highestValue) highestValue = r.Value;
        //     if (r.Value < lowestTemp) lowestTemp = r.Value;
        //     if (r.At < minAt) minAt = r.At;
        //     if (r.At > maxAt) maxAt = r.At;
        // }
        //
        // TimeSpan totalDuration = maxAt - minAt;
        //
        // // Grupowanie odczytów wg czujników za pomocą słownika
        // Dictionary<string, List<Reading>> sensorDict = new Dictionary<string, List<Reading>>();
        // for (int i = 0; i < _readings.Count; i++) {
        //     var r = _readings[i];
        //     if (!sensorDict.ContainsKey(r.Sensor)) {
        //         sensorDict[r.Sensor] = new List<Reading>();
        //     }
        //     sensorDict[r.Sensor].Add(r);
        // }
        //
        // // 2. Lista unikalnych czujników
        // List<string> uniqueSensors = new List<string>();
        // foreach (var key in sensorDict.Keys) {
        //     uniqueSensors.Add(key);
        // }
        //
        // // 3. Czujnik z największą liczbą odczytów
        // string mostReadings = string.Empty;
        // int maxCount = -1;
        // foreach (var kvp in sensorDict) {
        //     if (kvp.Value.Count > maxCount) {
        //         maxCount = kvp.Value.Count;
        //         mostReadings = kvp.Key;
        //     }
        // }
        //
        // List<(string, double)> averageReadings = new List<(string, double)>();
        // List<(string, TimeSpan)> largestGaps = new List<(string, TimeSpan)>();
        // List<(Reading, Reading)> signChanges = new List<(Reading, Reading)>();
        //
        // foreach (var kvp in sensorDict) {
        //     string sensor = kvp.Key;
        //     List<Reading> list = kvp.Value;
        //
        //     // Sortowanie chronologiczne odczytów dla danego czujnika
        //     list.Sort((a, b) => a.At.CompareTo(b.At));
        //
        //     // 4. Średnia wartość odczytów dla czujnika
        //     double sum = 0;
        //     for (int i = 0; i < list.Count; i++) {
        //         sum += list[i].Value;
        //     }
        //     double avg = list.Count > 0 ? sum / list.Count : 0;
        //     averageReadings.Add((sensor, avg));
        //
        //     // 5. Największy odstęp czasowy (Largest Gap) dla czujnika
        //     TimeSpan maxGap = TimeSpan.Zero;
        //     if (list.Count > 1) {
        //         for (int i = 1; i < list.Count; i++) {
        //             TimeSpan gap = list[i].At - list[i - 1].At;
        //             if (gap > maxGap) {
        //                 maxGap = gap;
        //             }
        //         }
        //     }
        //     largestGaps.Add((sensor, maxGap));
        //
        //     // 6. Zmiany znaków (Sign Changes: pierwszy < drugi, czyli ujemny -> dodatni)
        //     if (list.Count > 1) {
        //         for (int i = 1; i < list.Count; i++) {
        //             var r1 = list[i - 1];
        //             var r2 = list[i];
        //             if (r1.Value < 0 && r2.Value > 0) {
        //                 signChanges.Add((r1, r2));
        //             }
        //         }
        //     }
        // }
        //
        // return new Data(
        //     HighestValue: highestValue,
        //     LowestTemp: lowestTemp,
        //     MostReadings: mostReadings,
        //     TotalDuration: totalDuration,
        //     UniqueSensors: uniqueSensors,
        //     AverageReadings: averageReadings,
        //     LargestGaps: largestGaps,
        //     SignChanges: signChanges
        // );
        
        // if (_readings.Count == 0) {
        //     return new Data(
        //         HighestValue: 0,
        //         LowestTemp: 0,
        //         MostReadings: string.Empty,
        //         TotalDuration: TimeSpan.Zero,
        //         UniqueSensors: new List<string>(),
        //         AverageReadings: new List<(string, double)>(),
        //         LargestGaps: new List<(string, TimeSpan)>(),
        //         SignChanges: new List<(Reading, Reading)>()
        //     );
        // }
        //
        // // Inicjalizacja wartości globalnych
        // double highestValue = _readings[0].Value;
        //
        // // Szukanie początkowej najniższej temperatury dla czujnika "tmp"
        // double lowestTemp = double.MaxValue;
        // bool foundTmp = false;
        //
        // DateTime minAt = _readings[0].At;
        // DateTime maxAt = _readings[0].At;
        //
        // // 1. Przebieg globalny przez wszystkie odczyty (tylko foreach)
        // foreach (var r in _readings) {
        //     if (r.Value > highestValue) {
        //         highestValue = r.Value;
        //     }
        //
        //     // Najniższa temperatura tylko dla sensorów o nazwie "tmp" (ignorując wielkość liter)
        //     if (string.Equals(r.Sensor, "tmp", StringComparison.OrdinalIgnoreCase)) {
        //         if (!foundTmp || r.Value < lowestTemp) {
        //             lowestTemp = r.Value;
        //             foundTmp = true;
        //         }
        //     }
        //
        //     if (r.At < minAt) {
        //         minAt = r.At;
        //     }
        //     if (r.At > maxAt) {
        //         maxAt = r.At;
        //     }
        // }
        //
        // // Jeśli nie znaleziono żadnego odczytu z sensora "tmp", można ustawić np. 0 lub wartość domyślną
        // if (!foundTmp) {
        //     lowestTemp = 0; 
        // }
        //
        // TimeSpan totalDuration = maxAt - minAt;
        //
        // // Grupowanie odczytów wg czujników za pomocą słownika (foreach)
        // Dictionary<string, List<Reading>> sensorDict = new Dictionary<string, List<Reading>>();
        // foreach (var r in _readings) {
        //     if (!sensorDict.ContainsKey(r.Sensor)) {
        //         sensorDict[r.Sensor] = new List<Reading>();
        //     }
        //     sensorDict[r.Sensor].Add(r);
        // }
        //
        // // 2. Lista unikalnych czujników
        // List<string> uniqueSensors = new List<string>();
        // foreach (var key in sensorDict.Keys) {
        //     uniqueSensors.Add(key);
        // }
        //
        // // 3. Czujnik z największą liczbą odczytów
        // string mostReadings = string.Empty;
        // int maxCount = -1;
        // foreach (var kvp in sensorDict) {
        //     if (kvp.Value.Count > maxCount) {
        //         maxCount = kvp.Value.Count;
        //         mostReadings = kvp.Key;
        //     }
        // }
        //
        // List<(string, double)> averageReadings = new List<(string, double)>();
        // List<(string, TimeSpan)> largestGaps = new List<(string, TimeSpan)>();
        // List<(Reading, Reading)> signChanges = new List<(Reading, Reading)>();
        //
        // foreach (var kvp in sensorDict) {
        //     string sensor = kvp.Key;
        //     List<Reading> list = kvp.Value;
        //
        //     // Sortowanie chronologiczne odczytów dla danego czujnika (wymaga operacji na liście)
        //     list.Sort((a, b) => a.At.CompareTo(b.At));
        //
        //     // 4. Średnia wartość odczytów dla czujnika (użycie foreach)
        //     double sum = 0;
        //     int count = 0;
        //     foreach (var r in list) {
        //         sum += r.Value;
        //         count++;
        //     }
        //     double avg = count > 0 ? sum / count : 0;
        //     averageReadings.Add((sensor, avg));
        //
        //     // 5. Największy odstęp czasowy (Largest Gap) dla czujnika
        //     TimeSpan maxGap = TimeSpan.Zero;
        //     if (list.Count > 1) {
        //         Reading previous = list[0];
        //         foreach (var current in list) {
        //             // Pomijamy pierwszy element, bo nie ma poprzednika w tej iteracji bezpośredniej
        //             if (current.At == previous.At && current.Value == previous.Value) {
        //                 // Jeśli to ten sam obiekt, idziemy dalej
        //                 continue;
        //             }
        //             // Alternatywnie bezpieczniejsze sprawdzanie po indeksie lub parze:
        //             // Zamiast tego zrobimy czystą iterację par za pomocą pętli foreach i pomocniczej zmiennej:
        //         }
        //     }
        //     
        //     // Poprawne i czyste przejście przez pary sąsiednie za pomocą foreach:
        //     if (list.Count > 1) {
        //         TimeSpan currentMaxGap = TimeSpan.Zero;
        //         Reading? prev = null;
        //         foreach (var r in list) {
        //             if (prev.HasValue) {
        //                 TimeSpan gap = r.At - prev.Value.At;
        //                 if (gap > currentMaxGap) {
        //                     currentMaxGap = gap;
        //                 }
        //             }
        //             prev = r;
        //         }
        //         largestGaps.Add((sensor, currentMaxGap));
        //     } else {
        //         largestGaps.Add((sensor, TimeSpan.Zero));
        //     }
        //
        //     // 6. Zmiany znaków (Sign Changes: pierwszy < drugi, czyli ujemny -> dodatni) przy użyciu foreach
        //     if (list.Count > 1) {
        //         Reading? prevSign = null;
        //         foreach (var r in list) {
        //             if (prevSign.HasValue) {
        //                 if (prevSign.Value.Value < 0 && r.Value > 0) {
        //                     signChanges.Add((prevSign.Value, r));
        //                 }
        //             }
        //             prevSign = r;
        //         }
        //     }
        // }
        //
        // return new Data(
        //     HighestValue: highestValue,
        //     LowestTemp: lowestTemp,
        //     MostReadings: mostReadings,
        //     TotalDuration: totalDuration,
        //     UniqueSensors: uniqueSensors,
        //     AverageReadings: averageReadings,
        //     LargestGaps: largestGaps,
        //     SignChanges: signChanges
        // );

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