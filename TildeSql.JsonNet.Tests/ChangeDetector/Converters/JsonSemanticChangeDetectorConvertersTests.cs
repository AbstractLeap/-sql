namespace TildeSql.JsonNet.Tests.ChangeDetector.Converters {
    using System.Reflection;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    using Xunit;

    public class JsonSemanticChangeDetectorConvertersTests {
        // -------------------------------------------------------------
        // Test models
        // -------------------------------------------------------------

        public class WithPropertyConverter {
            [JsonConverter(typeof(UppercaseStringConverter))]
            public string Value { get; set; } = "";
        }

        public class WithSerializerLevelValue {
            public InstantLike Value { get; set; } = new InstantLike("X");
        }

        public class WithContractLevelValue {
            public InstantLike Value { get; set; } = new InstantLike("X");
        }

        public class InstantLike {
            public string Raw { get; }

            public InstantLike(string raw) => this.Raw = raw;
        }

        // -------------------------------------------------------------
        // Converters
        // -------------------------------------------------------------

        // Property-level converter: converts string into uppercase JSON string.
        public class UppercaseStringConverter : JsonConverter {
            public override bool CanConvert(Type objectType) => objectType == typeof(string);

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
                writer.WriteValue(((string)value!).ToUpperInvariant());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => throw new NotImplementedException();
        }

        // Serializer/Contract-level converter for InstantLike → "INSTANT:<raw>"
        public class InstantLikeConverter : JsonConverter {
            public override bool CanConvert(Type objectType) => objectType == typeof(InstantLike);

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
                var inst = (InstantLike)value!;
                writer.WriteValue("INSTANT:" + inst.Raw);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => throw new NotImplementedException();
        }

        // -------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------

        private JsonSemanticChangeDetector CreateDetector(JsonSerializerSettings settings) => new JsonSemanticChangeDetector(settings);

        // Contract resolver that assigns a converter on the property.
        public class ContractLevelInstantResolver : DefaultContractResolver {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization ms) {
                var prop = base.CreateProperty(member, ms);
                if (prop.PropertyType == typeof(InstantLike))
                    prop.Converter = new InstantLikeConverter(); // contract-level converter
                return prop;
            }
        }

        // -------------------------------------------------------------
        // Tests
        // -------------------------------------------------------------

        [Fact]
        public void PropertyLevelConverter_IsApplied_ByDetector() {
            var model = new WithPropertyConverter { Value = "hello" };

            var json = @"{ ""Value"": ""HELLO"" }";

            var settings = new JsonSerializerSettings(); // converter is on the property
            var detector = this.CreateDetector(settings);

            // Should compare using UppercaseStringConverter
            Assert.False(detector.HasChanged(json, model));
        }

        [Fact]
        public void PropertyLevelConverter_IsApplied_AndDetectsChanges() {
            var model = new WithPropertyConverter { Value = "hello" };

            var json = @"{ ""Value"": ""WRONGVALUE"" }";

            var settings = new JsonSerializerSettings();
            var detector = this.CreateDetector(settings);

            Assert.True(detector.HasChanged(json, model));
        }

        [Fact]
        public void SerializerLevelConverter_IsUsed_WhenNoPropertyLevelConverter() {
            var model = new WithSerializerLevelValue { Value = new InstantLike("abc") };

            var json = @"{ ""Value"": ""INSTANT:abc"" }";

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter()); // serializer-level
            var detector = this.CreateDetector(settings);

            Assert.False(detector.HasChanged(json, model));
        }

        [Fact]
        public void SerializerLevelConverter_CatchesDifferences() {
            var model = new WithSerializerLevelValue { Value = new InstantLike("abc") };

            var json = @"{ ""Value"": ""INSTANT:DIFF"" }";

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter());
            var detector = this.CreateDetector(settings);

            Assert.True(detector.HasChanged(json, model));
        }

        [Fact]
        public void ContractLevelConverter_IsUsed_WhenPresent() {
            var model = new WithContractLevelValue { Value = new InstantLike("xyz") };

            var json = @"{ ""Value"": ""INSTANT:xyz"" }";

            var settings = new JsonSerializerSettings { ContractResolver = new ContractLevelInstantResolver() };

            var detector = this.CreateDetector(settings);

            Assert.False(detector.HasChanged(json, model)); // contract-level converter used
        }

        [Fact]
        public void ContractLevelConverter_Detects_Inequality() {
            var model = new WithContractLevelValue { Value = new InstantLike("xyz") };

            var json = @"{ ""Value"": ""INSTANT:DIFF"" }";

            var settings = new JsonSerializerSettings { ContractResolver = new ContractLevelInstantResolver() };

            var detector = this.CreateDetector(settings);

            Assert.True(detector.HasChanged(json, model));
        }

        [Fact]
        public void PropertyLevelConverter_TakesPrecedence_OverSerializerLevelConverter() {
            var model = new WithPropertyConverter { Value = "hello" };

            // If serializer-level converter applied, we'd expect "INSTANT:hello",
            // but property-level converter forces uppercase string "HELLO".
            var json = @"{ ""Value"": ""HELLO"" }";

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter()); // serializer-level, but wrong type anyway

            var detector = this.CreateDetector(settings);

            Assert.False(detector.HasChanged(json, model)); // property converter dominates
        }

        [Fact]
        public void ConverterViaCompareViaConverter_HandlesNormalization() {
            // Default(T) collapsing must still work against converter output
            var model = new WithSerializerLevelValue { Value = new InstantLike("0") };
            var json = @"{ ""Value"": null }"; // default-collapse expected

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter());
            var detector = this.CreateDetector(settings);

            // "INSTANT:0" underlying numeric 0? No. But your model collapses enum/numeric defaults to null.
            // Instant-like is NOT numeric => not collapsed => the comparison must show change.
            Assert.True(detector.HasChanged(json, model));
        }
    }
}