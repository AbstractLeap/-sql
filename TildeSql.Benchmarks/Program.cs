
// --- Benchmarks ---

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using TildeSql;
using TildeSql.Serialization;

[MemoryDiagnoser]
[HideColumns("Error", "StdDev", "Median")]
public class JsonEqualityBenchmarks {
    // Parameters to switch between test datasets
    [Params(
        nameof(SampleSet.SmallEqual),
        nameof(SampleSet.SmallUnequalEarly),
        nameof(SampleSet.MediumEqual_DiffWhitespaceOrder),
        nameof(SampleSet.MediumEqual_NullVsMissing),
        nameof(SampleSet.LargeEqual),
        nameof(SampleSet.LargeUnequalTail))]
    public string Case { get; set; } = default!;

    private string _a = default!;

    private string _b = default!;

    [GlobalSetup]
    public void Setup() {
        (_a, _b) =
            Case switch {
                nameof(SampleSet.SmallEqual) => SampleSet.SmallEqual(),
                nameof(SampleSet.SmallUnequalEarly) => SampleSet.SmallUnequalEarly(),
                nameof(SampleSet.MediumEqual_DiffWhitespaceOrder) => SampleSet.MediumEqual_DiffWhitespaceOrder(),
                nameof(SampleSet.MediumEqual_NullVsMissing) => SampleSet.MediumEqual_NullVsMissing(),
                nameof(SampleSet.LargeEqual) => SampleSet.LargeEqual(),
                nameof(SampleSet.LargeUnequalTail) => SampleSet.LargeUnequalTail(),
                _ => throw new ArgumentOutOfRangeException(nameof(Case))
            };
    }

    [Benchmark(Baseline = true)]
    public bool Raw_StringEquals_Ordinal() => string.Equals(_a, _b, StringComparison.Ordinal);

    [Benchmark]
    public bool Semantic_JsonEquality() => JsonEquality.JsonEquals(_a, _b);
}

public static class SampleSet {
    // Small equal JSONs (identical)
    public static (string a, string b) SmallEqual() => (@"{""x"":1,""y"":true,""s"":""abc""}", @"{""x"":1,""y"":true,""s"":""abc""}");

    // Small unequal where difference appears early (fast early-exit)
    public static (string a, string b) SmallUnequalEarly() => (@"{""x"":1,""y"":true}", @"{""x"":2,""y"":true}");

    // Medium: equal semantically, but different whitespace and property order (Raw string.Equals should be false)
    public static (string a, string b) MediumEqual_DiffWhitespaceOrder() =>
        (
            // a
            @"{
           ""id"": 1,
           ""name"": ""Mark"",
           ""settings"": { ""beta"": false, ""theme"": ""dark"" },
           ""items"": [1,2,3]
         }",
            // b (reordered, spaced differently)
            @"{ ""items"": [1,2,3],
           ""settings"": { ""theme"": ""dark"", ""beta"": false },
           ""name"": ""Mark"",
           ""id"": 1
         }");

    // Medium: equal semantically because of null-vs-missing rule (Raw string.Equals should be false)
    public static (string a, string b) MediumEqual_NullVsMissing() =>
        (
            // a has nulls
            @"{
           ""id"": 1,
           ""profile"": { ""name"": null, ""email"": ""a@b.com"" },
           ""flags"": { ""trial"": null }
         }",
            // b missing those null props, but otherwise equal
            @"{
           ""id"": 1,
           ""profile"": { ""email"": ""a@b.com"" },
           ""flags"": { }
         }");

    // Large equal JSONs (synthetic but sizable)
    public static (string a, string b) LargeEqual() {
        // Build a moderately large JSON with nested arrays/objects
        var a =
            @"{
  ""id"": 1001,
  ""meta"": { ""version"": 5, ""tags"": [""x"",""y"",""z""] },
  ""users"": [
    { ""id"": 1, ""name"": ""Alice"", ""prefs"": { ""theme"": ""dark"", ""beta"": false } },
    { ""id"": 2, ""name"": ""Bob"", ""prefs"": { ""theme"": ""light"", ""beta"": true } },
    { ""id"": 3, ""name"": ""Carol"", ""prefs"": { ""theme"": ""dark"", ""beta"": false } }
  ],
  ""data"": { ""numbers"": [1,2,3,4,5,6,7,8,9,10] }
}";
        var b =
            @"{
  ""data"": { ""numbers"": [1,2,3,4,5,6,7,8,9,10] },
  ""users"": [
    { ""prefs"": { ""beta"": false, ""theme"": ""dark"" }, ""name"": ""Alice"", ""id"": 1 },
    { ""prefs"": { ""beta"": true, ""theme"": ""light"" }, ""name"": ""Bob"", ""id"": 2 },
    { ""prefs"": { ""theme"": ""dark"", ""beta"": false }, ""name"": ""Carol"", ""id"": 3 }
  ],
  ""meta"": { ""tags"": [""x"",""y"",""z""], ""version"": 5 },
  ""id"": 1001
}";
        return (a, b);
    }

    // Large unequal where the difference appears late (forces traversal)
    public static (string a, string b) LargeUnequalTail() {
        var a =
            @"{
  ""id"": 2002,
  ""meta"": { ""version"": 5, ""tags"": [""x"",""y"",""z""] },
  ""users"": [
    { ""id"": 1, ""name"": ""Alice"", ""prefs"": { ""theme"": ""dark"", ""beta"": false } },
    { ""id"": 2, ""name"": ""Bob"", ""prefs"": { ""theme"": ""light"", ""beta"": true } },
    { ""id"": 3, ""name"": ""Carol"", ""prefs"": { ""theme"": ""dark"", ""beta"": false } }
  ],
  ""data"": { ""numbers"": [1,2,3,4,5,6,7,8,9,10] },
  ""tail"": { ""ok"": true }
}";
        var b =
            @"{
  ""id"": 2002,
  ""meta"": { ""version"": 5, ""tags"": [""x"",""y"",""z""] },
  ""users"": [
    { ""id"": 1, ""name"": ""Alice"", ""prefs"": { ""theme"": ""dark"", ""beta"": false } },
    { ""id"": 2, ""name"": ""Bob"", ""prefs"": { ""theme"": ""light"", ""beta"": true } },
    { ""id"": 3, ""name"": ""Carol"", ""prefs"": { ""theme"": ""dark"", ""beta"": false } }
  ],
  ""data"": { ""numbers"": [1,2,3,4,5,6,7,8,9,10] },
  ""tail"": { ""ok"": false }
}";
        return (a, b);
    }
}

public class Program {
    public static void Main(string[] args) {
        BenchmarkRunner.Run<JsonEqualityBenchmarks>();
    }
}
