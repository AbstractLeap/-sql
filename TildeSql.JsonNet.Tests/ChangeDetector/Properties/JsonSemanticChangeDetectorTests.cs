namespace TildeSql.JsonNet.Tests.ChangeDetector.Properties {
    using Xunit;

    public class JsonSemanticChangeDetectorTests {
        static JsonSemanticChangeDetector GetDetector() {
            return new JsonSemanticChangeDetector(null);
        }

        // ---------------------------------------------------------------
        // BASELINE TESTS
        // ---------------------------------------------------------------

        [Xunit.Fact]
        public void HasChanged_SimpleEquality() {            
            var json = @"{ ""Name"": ""Mark"", ""Age"": 42 }";
            var obj = new Person { Name = "Mark", Age = 42 };

            Xunit.Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Xunit.Fact]
        public void HasChanged_IgnoresPropertyOrder() {
            var json = @"{ ""Age"": 42, ""Name"": ""Mark"" }";
            var obj = new Person { Name = "Mark", Age = 42 };

            Xunit.Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Xunit.Fact]
        public void HasChanged_NestedEquality() {
            var json = @"{
            ""Name"": ""Mark"",
            ""Address"": { ""Line1"": ""123 Road"", ""Postcode"": ""AB1 2CD"" }
        }";

            var obj = new Person {
                Name    = "Mark",
                Address = new Address { Line1 = "123 Road", Postcode = "AB1 2CD" }
            };

            Xunit.Assert.False(GetDetector().HasChanged(json, obj));
        }

        // ---------------------------------------------------------------
        // NULL / MISSING / DEFAULT HANDLING
        // ---------------------------------------------------------------

        [Xunit.Fact]
        public void MissingProperty_EqualsNull() {
            var json = @"{ ""Name"": null }";
            var obj = new Person { Name = null };

            Xunit.Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Xunit.Fact]
        public void MissingNestedObject_EqualsNull() {
            var json = @"{ }";
            var obj = new Person { Address = null };

            Xunit.Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Xunit.Fact]
        public void DefaultInt_EqualsNull() {
            var json = @"{ ""Age"": null }";
            var obj = new Person { Age = 0 };

            Xunit.Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Xunit.Fact]
        public void DefaultBool_EqualsNull() {
            var json = @"{ ""IsAdmin"": null }";
            var obj = new Person { IsAdmin = false };

            Xunit.Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Xunit.Fact]
        public void DefaultString_EqualsNull() {
            var json = @"{ ""Name"": null }";
            var obj = new Person { Name = "" };

            Xunit.Assert.True(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableBool_False_DoesNotEqualNull() {
            var json = @"{ ""NullableBool"": null }";
            var obj = new Person { NullableBool = false };

            Assert.True(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableBool_True_EqualsTrueJson() {
            var json = @"{ ""NullableBool"": true }";
            var obj = new Person { NullableBool = true };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableBool_Null_EqualsMissingProperty() {
            var json = @"{ }";
            var obj = new Person { NullableBool = null };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableInt_Zero_DoesNotEqualNull() {
            var json = @"{ ""NullableInt"": null }";
            var obj = new Person { NullableInt = 0 };

            Assert.True(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableInt_Null_EqualsMissingProperty() {
            var json = @"{ }";
            var obj = new Person { NullableInt = null };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableInt_NumericSemantics_OneEqualsOnePointZero() {
            var json = @"{ ""NullableInt"": 1.0 }";
            var obj = new Person { NullableInt = 1 };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableFloat_ZeroPointZero_DoesNotEqualNull() {
            var json = @"{ ""NullableFloat"": null }";
            var obj = new Person { NullableFloat = 0.0f };

            Assert.True(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableFloat_Null_EqualsMissingProperty() {
            var json = @"{ }";
            var obj = new Person { NullableFloat = null };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableDecimal_Zero_DoesNotEqualNull() {
            var json = @"{ ""NullableDecimal"": null }";
            var obj = new Person { NullableDecimal = 0m };

            Assert.True(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableDecimal_Null_EqualsMissingProperty() {
            var json = @"{ }";
            var obj = new Person { NullableDecimal = null };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableDecimal_NumericSemantics() {
            var json = @"{ ""NullableDecimal"": 1.00 }";
            var obj = new Person { NullableDecimal = 1m };

            Assert.False(GetDetector().HasChanged(json, obj));
        }


        // ---------------------------------------------------------------
        // NUMERIC SEMANTICS
        // ---------------------------------------------------------------

        [Xunit.Fact]
        public void Numbers_OneEqualsOnePointZero() {
            var json = @"{ ""Age"": 1.0 }";
            var obj = new Person { Age = 1 };

            Xunit.Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Xunit.Fact]
        public void Numbers_DifferentValues_NotEqual() {
            var json = @"{ ""Age"": 2 }";
            var obj = new Person { Age = 1 };

            Xunit.Assert.True(GetDetector().HasChanged(json, obj));
        }

        // ---------------------------------------------------------------
        // ARRAY TESTS
        // ---------------------------------------------------------------

        public class Wrapper {
            public int[]? Values { get; set; }
        }

        [Xunit.Fact]
        public void Arrays_EqualOrderRequired() {
            var json = @"{ ""Values"": [1,2,3] }";
            var obj = new Wrapper { Values = new[] { 1, 2, 3 } };

            Xunit.Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Xunit.Fact]
        public void Arrays_DifferentOrder_NotEqual() {
            var json = @"{ ""Values"": [1,3,2] }";
            var obj = new Wrapper { Values = new[] { 1, 2, 3 } };

            Xunit.Assert.True(GetDetector().HasChanged(json, obj));
        }

        // ---------------------------------------------------------------
        // CONTRACT / ATTRIBUTE HANDLING
        // ---------------------------------------------------------------

        public class Renamed {
            [Newtonsoft.Json.JsonProperty("renamed")]
            public string? Name { get; set; }
        }

        [Xunit.Fact]
        public void Contract_RenamedPropertyHonoured() {
            var json = @"{ ""renamed"": ""abc"" }";
            var obj = new Renamed { Name = "abc" };

            Xunit.Assert.False(GetDetector().HasChanged(json, obj));
        }

        public class Ignored {
            public string? Name { get; set; }

            [Newtonsoft.Json.JsonIgnore]
            public string? Secret { get; set; }
        }

        [Xunit.Fact]
        public void Contract_IgnoredProperty_IsIgnored() {
            var json = @"{ ""Name"": ""abc"" }";
            var obj = new Ignored { Name = "abc", Secret = "should-not-appear" };

            Xunit.Assert.False(GetDetector().HasChanged(json, obj));
        }

        // ---------------------------------------------------------------
        // NEGATIVE TESTS
        // ---------------------------------------------------------------

        [Xunit.Fact]
        public void DifferentStrings_NotEqual() {
            var json = @"{ ""Name"": ""Mark"" }";
            var obj = new Person { Name = "Sam" };

            Xunit.Assert.True(GetDetector().HasChanged(json, obj));
        }

        [Xunit.Fact]
        public void MissingProperty_WhenNotDefault_NotEqual() {
            var json = @"{ }";
            var obj = new Person { Name = "not-default" };

            Xunit.Assert.True(GetDetector().HasChanged(json, obj));
        }
    }
}