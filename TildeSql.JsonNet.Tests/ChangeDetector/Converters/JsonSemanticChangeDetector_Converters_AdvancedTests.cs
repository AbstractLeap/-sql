namespace TildeSql.JsonNet.Tests.ChangeDetector.Converters {
    using Newtonsoft.Json;

    using Xunit;

    public class JsonSemanticChangeDetector_Converters_AdvancedTests {
        // -------------------------------------------------------------
        // Test model types
        // -------------------------------------------------------------

        public class WithNullableConverterValue {
            [JsonConverter(typeof(NullableInstantLikeConverter))]
            public InstantLike? Value { get; set; }
        }

        public class WithArrayOfConverted {
            public InstantLike[] Items { get; set; } = Array.Empty<InstantLike>();
        }

        public class WithListOfConverted {
            public List<InstantLike> Items { get; set; } = new();
        }

        public class NestedConvertedObject {
            public Inner Inner { get; set; } = new();
        }

        public class Inner {
            [JsonConverter(typeof(InstantLikeConverter))]
            public InstantLike? Stamp { get; set; }
        }

        public class NestedWithNullableInner {
            public NullableInner? Data { get; set; }
        }

        public class NullableInner {
            [JsonConverter(typeof(InstantLikeConverter))]
            public InstantLike? Instant { get; set; }
        }

        public class InstantLike {
            public string Raw { get; }
            public InstantLike(string raw) => this.Raw = raw;
        }

        // -------------------------------------------------------------
        // Converters
        // -------------------------------------------------------------

        // Serializer-level converter: InstantLike -> "INSTANT:<raw>"
        public class InstantLikeConverter : JsonConverter {
            public override bool CanConvert(Type objectType)
                => objectType == typeof(InstantLike);

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
                var inst = (InstantLike)value!;
                writer.WriteValue("INSTANT:" + inst.Raw);
            }

            public override object ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer s)
                => throw new NotImplementedException();
        }

        // Property-level converter for Nullable<InstantLike>
        public class NullableInstantLikeConverter : JsonConverter {
            public override bool CanConvert(Type typeToConvert)
                => typeToConvert == typeof(Nullable<>).MakeGenericType(typeof(InstantLike)) || typeToConvert == typeof(InstantLike);

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
                if (value == null) {
                    writer.WriteNull();
                    return;
                }

                var inst = (InstantLike)value;
                writer.WriteValue("OPT_INSTANT:" + inst.Raw);
            }

            public override object? ReadJson(JsonReader reader, Type t, object? v, JsonSerializer s)
                => throw new NotImplementedException();
        }

        // -------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------

        private JsonSemanticChangeDetector Create(JsonSerializerSettings? settings = null)
            => new JsonSemanticChangeDetector(settings ?? new JsonSerializerSettings());

        // -------------------------------------------------------------
        // Tests
        // -------------------------------------------------------------

        // -----------------------------
        // Nullable tests (property-level converter)
        // -----------------------------

        [Fact]
        public void NullableConverter_Null_EqualsNullJson() {
            var model = new WithNullableConverterValue { Value = null };
            var json = @"{ ""Value"": null }";

            var det = this.Create();
            Assert.False(det.HasChanged(json, model));
        }

        [Fact]
        public void NullableConverter_Null_EqualsMissingJson() {
            var model = new WithNullableConverterValue { Value = null };
            var json = @"{ }";

            var det = this.Create();
            Assert.False(det.HasChanged(json, model));
        }

        [Fact]
        public void NullableConverter_Value_MatchesStringOutput() {
            // Property-level converter outputs "OPT_INSTANT:<raw>"
            var model = new WithNullableConverterValue { Value = new InstantLike("abc") };

            var json = @"{ ""Value"": ""OPT_INSTANT:abc"" }";

            var det = this.Create();
            Assert.False(det.HasChanged(json, model));
        }

        [Fact]
        public void NullableConverter_Value_DetectsMismatch() {
            var model = new WithNullableConverterValue { Value = new InstantLike("abc") };

            var json = @"{ ""Value"": ""OPT_INSTANT:DIFF"" }";

            var det = this.Create();
            Assert.True(det.HasChanged(json, model));
        }

        // -----------------------------
        // Arrays + lists of converted objects
        // -----------------------------

        [Fact]
        public void ArrayOfConverted_Equal() {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter());

            var model = new WithArrayOfConverted {
                Items = new[] { new InstantLike("a"), new InstantLike("b") }
            };

            var json = @"{
                ""Items"": [""INSTANT:a"", ""INSTANT:b""]
            }";

            var det = this.Create(settings);
            Assert.False(det.HasChanged(json, model));
        }

        [Fact]
        public void ArrayOfConverted_Mismatch() {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter());

            var model = new WithArrayOfConverted {
                Items = new[] { new InstantLike("a"), new InstantLike("b") }
            };

            var json = @"{
                ""Items"": [""INSTANT:a"", ""WRONG:b""]
            }";

            var det = this.Create(settings);
            Assert.True(det.HasChanged(json, model));
        }

        [Fact]
        public void ListOfConverted_Equal() {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter());

            var model = new WithListOfConverted {
                Items = new List<InstantLike> { new InstantLike("x"), new InstantLike("y") }
            };

            var json = @"{
                ""Items"": [""INSTANT:x"", ""INSTANT:y""]
            }";

            var det = this.Create(settings);
            Assert.False(det.HasChanged(json, model));
        }

        [Fact]
        public void ListOfConverted_DetectsMismatch() {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter());

            var model = new WithListOfConverted {
                Items = new List<InstantLike> { new InstantLike("x"), new InstantLike("y") }
            };

            var json = @"{
                ""Items"": [""INSTANT:x"", ""INSTANT:WRONG""]
            }";

            var det = this.Create(settings);
            Assert.True(det.HasChanged(json, model));
        }

        [Fact]
        public void ArrayOfConverted_DetectsCountMismatch() {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter());

            var model = new WithArrayOfConverted {
                Items = new[] { new InstantLike("a"), new InstantLike("b") }
            };

            var json = @"{
                ""Items"": [""INSTANT:a""]
            }";

            var det = this.Create(settings);
            Assert.True(det.HasChanged(json, model));
        }

        // -----------------------------
        // Nested objects with converters
        // -----------------------------

        [Fact]
        public void NestedProperty_WithConverter_Equal() {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter());

            var model = new NestedConvertedObject {
                Inner = new Inner { Stamp = new InstantLike("foo") }
            };

            var json = @"{
                ""Inner"": {
                    ""Stamp"": ""INSTANT:foo""
                }
            }";

            var det = this.Create(settings);
            Assert.False(det.HasChanged(json, model));
        }

        [Fact]
        public void NestedProperty_WithConverter_DetectsMismatch() {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter());

            var model = new NestedConvertedObject {
                Inner = new Inner { Stamp = new InstantLike("foo") }
            };

            var json = @"{
                ""Inner"": {
                    ""Stamp"": ""INSTANT:DIFF""
                }
            }";

            var det = this.Create(settings);
            Assert.True(det.HasChanged(json, model));
        }

        // -----------------------------
        // Nested nullable converted objects
        // -----------------------------

        [Fact]
        public void NestedNullableObject_Null_EqualsNull() {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter());

            var model = new NestedWithNullableInner { Data = null };

            var json = @"{ ""Data"": null }";

            var det = this.Create(settings);
            Assert.False(det.HasChanged(json, model));
        }

        [Fact]
        public void NestedNullableObject_Null_EqualsMissing() {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter());

            var model = new NestedWithNullableInner { Data = null };

            var json = @"{}";

            var det = this.Create(settings);
            Assert.False(det.HasChanged(json, model));
        }

        [Fact]
        public void NestedNullableObject_WithConvertedValue_Equal() {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter());

            var model = new NestedWithNullableInner {
                Data = new NullableInner {
                    Instant = new InstantLike("bar")
                }
            };

            var json = @"{
                ""Data"": {
                    ""Instant"": ""INSTANT:bar""
                }
            }";

            var det = this.Create(settings);
            Assert.False(det.HasChanged(json, model));
        }

        [Fact]
        public void NestedNullableObject_WithConvertedValue_DetectsMismatch() {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new InstantLikeConverter());

            var model = new NestedWithNullableInner {
                Data = new NullableInner {
                    Instant = new InstantLike("bar")
                }
            };

            var json = @"{
                ""Data"": {
                    ""Instant"": ""INSTANT:WRONG""
                }
            }";

            var det = this.Create(settings);
            Assert.True(det.HasChanged(json, model));
        }
    }
}