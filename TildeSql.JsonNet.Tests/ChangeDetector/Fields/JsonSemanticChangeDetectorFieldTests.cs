namespace TildeSql.JsonNet.Tests.ChangeDetector.Fields {
    using Xunit;

    public class JsonSemanticChangeDetectorFieldTests {
        private static JsonSemanticChangeDetector GetDetector() {
            return new JsonSemanticChangeDetector(JsonNetFieldSerializer.GetSettings());
        }

        // ---------------------------------------------------------------
        // BASELINE TESTS
        // ---------------------------------------------------------------

        [Fact]
        public void HasChanged_SimpleEquality() {
            var json = @"{ ""name"": ""Mark"", ""age"": 42 }";
            var obj = new PersonFields { Name = "Mark", Age = 42 };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void HasChanged_IgnoresPropertyOrder() {
            var json = @"{ ""age"": 42, ""name"": ""Mark"" }";
            var obj = new PersonFields { Name = "Mark", Age = 42 };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void HasChanged_NestedEquality() {
            var json = @"{
                ""name"": ""Mark"",
                ""address"": { ""line1"": ""123 Road"", ""postcode"": ""AB1 2CD"" }
            }";

            var obj = new PersonFields {
                Name    = "Mark",
                Address = new AddressFields { Line1 = "123 Road", Postcode = "AB1 2CD" }
            };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        // ---------------------------------------------------------------
        // NULL / MISSING / DEFAULT HANDLING
        // ---------------------------------------------------------------

        [Fact]
        public void MissingProperty_EqualsNull() {
            var json = @"{ ""name"": null }";
            var obj = new PersonFields { Name = null };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void MissingNestedObject_EqualsNull() {
            var json = @"{ }";
            var obj = new PersonFields { Address = null };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void DefaultInt_EqualsNull() {
            var json = @"{ ""age"": null }";
            var obj = new PersonFields { Age = 0 };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void DefaultBool_EqualsNull() {
            var json = @"{ ""isAdmin"": null }";
            var obj = new PersonFields { IsAdmin = false };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void DefaultString_EqualsNull() {
            var json = @"{ ""name"": null }";
            var obj = new PersonFields { Name = "" };

            // empty string is NOT treated as null
            Assert.True(GetDetector().HasChanged(json, obj));
        }

        // ---------------------------------------------------------------
        // NULLABLE TESTS
        // ---------------------------------------------------------------

        [Fact]
        public void NullableBool_False_DoesNotEqualNull() {
            var json = @"{ ""nullableBool"": null }";
            var obj = new PersonFields { NullableBool = false };

            Assert.True(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableBool_True_EqualsTrueJson() {
            var json = @"{ ""nullableBool"": true }";
            var obj = new PersonFields { NullableBool = true };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableBool_Null_EqualsMissingProperty() {
            var json = @"{ }";
            var obj = new PersonFields { NullableBool = null };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableInt_Zero_DoesNotEqualNull() {
            var json = @"{ ""nullableInt"": null }";
            var obj = new PersonFields { NullableInt = 0 };

            Assert.True(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableInt_Null_EqualsMissingProperty() {
            var json = @"{ }";
            var obj = new PersonFields { NullableInt = null };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableInt_NumericSemantics_OneEqualsOnePointZero() {
            var json = @"{ ""nullableInt"": 1.0 }";
            var obj = new PersonFields { NullableInt = 1 };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableFloat_ZeroPointZero_DoesNotEqualNull() {
            var json = @"{ ""nullableFloat"": null }";
            var obj = new PersonFields { NullableFloat = 0.0f };

            Assert.True(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableFloat_Null_EqualsMissingProperty() {
            var json = @"{ }";
            var obj = new PersonFields { NullableFloat = null };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableDecimal_Zero_DoesNotEqualNull() {
            var json = @"{ ""nullableDecimal"": null }";
            var obj = new PersonFields { NullableDecimal = 0m };

            Assert.True(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableDecimal_Null_EqualsMissingProperty() {
            var json = @"{ }";
            var obj = new PersonFields { NullableDecimal = null };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void NullableDecimal_NumericSemantics() {
            var json = @"{ ""nullableDecimal"": 1.00 }";
            var obj = new PersonFields { NullableDecimal = 1m };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        // ---------------------------------------------------------------
        // NUMERIC SEMANTICS
        // ---------------------------------------------------------------

        [Fact]
        public void Numbers_OneEqualsOnePointZero() {
            var json = @"{ ""age"": 1.0 }";
            var obj = new PersonFields { Age = 1 };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void Numbers_DifferentValues_NotEqual() {
            var json = @"{ ""age"": 2 }";
            var obj = new PersonFields { Age = 1 };

            Assert.True(GetDetector().HasChanged(json, obj));
        }

        // ---------------------------------------------------------------
        // ARRAY TESTS
        // ---------------------------------------------------------------

        [Fact]
        public void Arrays_EqualOrderRequired() {
            var json = @"{ ""values"": [1,2,3] }";
            var obj = new WrapperFields { Values = new[] { 1, 2, 3 } };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void Arrays_DifferentOrder_NotEqual() {
            var json = @"{ ""values"": [1,3,2] }";
            var obj = new WrapperFields { Values = new[] { 1, 2, 3 } };

            Assert.True(GetDetector().HasChanged(json, obj));
        }

        // ---------------------------------------------------------------
        // CONTRACT / RESOLVER-BASED HANDLING
        // ---------------------------------------------------------------

        [Fact]
        public void Contract_RenamedFieldNameHonouredByResolver() {
            // Field is named _renamed -> JSON key "renamed"
            var json = @"{ ""renamed"": ""abc"" }";
            var obj = new RenamedFields { Name = "abc" };

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void Contract_IgnoredField_IsIgnored() {
            var json = @"{ ""name"": ""abc"" }";
            var obj = new IgnoredFields { Name = "abc" };
            obj.SetSecret("should-not-appear");

            Assert.False(GetDetector().HasChanged(json, obj));
        }

        // ---------------------------------------------------------------
        // NEGATIVE TESTS
        // ---------------------------------------------------------------

        [Fact]
        public void DifferentStrings_NotEqual() {
            var json = @"{ ""name"": ""Mark"" }";
            var obj = new PersonFields { Name = "Sam" };

            Assert.True(GetDetector().HasChanged(json, obj));
        }

        [Fact]
        public void MissingProperty_WhenNotDefault_NotEqual() {
            var json = @"{ }";
            var obj = new PersonFields { Name = "not-default" };

            Assert.True(GetDetector().HasChanged(json, obj));
        }
    }
}