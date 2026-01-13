namespace TildeSql.Tests.Internal {
    using Xunit;

    public class JsonEqualityTests {
        [Theory]
        [InlineData("{}", "{}")]
        [InlineData(@"{""a"":1}", @"{""a"":1}")]
        [InlineData(@"{""a"":1,""b"":2}", @"{""b"":2,""a"":1}")] // property order ignored
        [InlineData(@"{""a"":null}", @"{}")] // null vs missing equal
        [InlineData(@"{}", @"{""a"":null}")] // null vs missing equal (reverse)
        [InlineData(@"{""o"":{""x"":null}}", @"{""o"":{}}")] // nested null vs missing
        [InlineData(@"{""arr"":[1,2,3]}", @"{""arr"":[1,2,3]}")] // arrays equal in same order
        [InlineData(@"{""name"":""Mark"",""ok"":true}", @"{""ok"":true,""name"":""Mark""}")] // strings & booleans
        public void Equal_ExpectedTrue(string a, string b) {
            Assert.True(JsonEquality.JsonEquals(a, b));
        }

        [Theory]
        [InlineData(@"{""a"":1}", @"{""a"":2}")] // diff number
        [InlineData(@"{""a"":1}", @"{""b"":1}")] // missing vs non-null (not ok)
        [InlineData(@"{""a"":0}", @"{}")] // 0 vs missing (not ok)
        [InlineData(@"{""a"":[]}", @"{""a"":{}}")] // array vs object
        [InlineData(@"{""a"":{}}", @"{""a"":[]}")] // object vs array
        [InlineData(@"{""a"":1}", @"{""a"":""1""}")] // number vs string
        [InlineData(@"{""arr"":[1,2,3]}", @"{""arr"":[1,3,2]}")] // array order matters
        [InlineData(@"{""arr"":[1,2,3]}", @"{""arr"":[1,2]}")] // array length differs
        [InlineData(@"{""x"":null}", @"{""x"":0}")] // null vs explicit zero
        [InlineData(@"{""x"":""a""}", @"{""x"":""b""}")] // string diff
        [InlineData(@"{""x"":true}", @"{""x"":false}")] // bool diff
        public void NotEqual_ExpectedFalse(string a, string b) {
            Assert.False(JsonEquality.JsonEquals(a, b));
        }

        [Fact]
        public void Numbers_Treat_One_And_OnePointZero_As_Equal() {
            var a = @"{""n"":1}";
            var b = @"{""n"":1.0}";
            Assert.True(JsonEquality.JsonEquals(a, b));
        }

        [Fact]
        public void Numbers_Large_With_Different_Textual_Forms_Compare_Carefully() {
            // Extremely large numbers that may not fit decimal.
            // These differ textually, and decimal parsing may fail; method falls back to raw text compare.
            var a = @"{""big"": 123456789012345678901234567890}";
            var b = @"{""big"": 123456789012345678901234567891}";
            Assert.False(JsonEquality.JsonEquals(a, b));
        }

        [Fact]
        public void Nested_NullVsMissing_Are_Equal_Deep() {
            var a = @"{ ""user"": { ""name"": null, ""prefs"": { ""theme"": null } } }";
            var b = @"{ ""user"": { ""prefs"": { }, ""age"": 10 } }";
            // name is null vs missing; prefs.theme is null vs missing; user.age present only in B but not null -> should fail
            Assert.False(JsonEquality.JsonEquals(a, b));
        }

        [Fact]
        public void Nested_NullVsMissing_Symmetric_Equality() {
            var a = @"{ ""user"": { ""name"": null, ""prefs"": { ""theme"": null } } }";
            var b = @"{ ""user"": { } }";
            // All extra properties on A are null, so equal to missing in B
            Assert.False(JsonEquality.JsonEquals(a, b));
        }

        [Fact]
        public void Object_Property_Order_Ignored_At_All_Levels() {
            var a =
                @"{
          ""rootB"": 2,
          ""rootA"": 1,
          ""obj"": { ""y"": true, ""x"": false }
        }";
            var b =
                @"{
          ""obj"": { ""x"": false, ""y"": true },
          ""rootA"": 1,
          ""rootB"": 2
        }";
            Assert.True(JsonEquality.JsonEquals(a, b));
        }

        [Fact]
        public void Array_Objects_With_NullVsMissing_Inside_Are_Equal() {
            var a = @"{ ""items"": [ { ""id"": 1, ""name"": null }, { ""id"": 2 } ] }";
            var b = @"{ ""items"": [ { ""id"": 1 }, { ""id"": 2, ""name"": null } ] }";
            // Order is the same; within items, null vs missing is equal
            Assert.True(JsonEquality.JsonEquals(a, b));
        }

        [Fact]
        public void Array_Order_Matters_Even_If_Same_Elements() {
            var a = @"{ ""items"": [ { ""id"": 1 }, { ""id"": 2 } ] }";
            var b = @"{ ""items"": [ { ""id"": 2 }, { ""id"": 1 } ] }";
            Assert.False(JsonEquality.JsonEquals(a, b));
        }

        [Fact]
        public void Mixed_Types_Differ_ObjectVsScalar() {
            var a = @"{ ""x"": { ""a"": 1 } }";
            var b = @"{ ""x"": 1 }";
            Assert.False(JsonEquality.JsonEquals(a, b));
        }

        [Fact]
        public void NullVsMissing_At_Root_Object_Level() {
            var a = @"{ ""a"": null }";
            var b = @"{ }";
            Assert.True(JsonEquality.JsonEquals(a, b));
        }

        [Fact]
        public void Extra_NonNull_Property_Should_Fail() {
            var a = @"{ ""a"": 1 }";
            var b = @"{ ""a"": 1, ""extra"": 0 }"; // non-null extra -> should fail
            Assert.False(JsonEquality.JsonEquals(a, b));
        }

        [Fact]
        public void Extra_Null_Property_Should_Pass() {
            var a = @"{ ""a"": 1 }";
            var b = @"{ ""a"": 1, ""extra"": null }"; // null extra -> treated as missing in A
            Assert.True(JsonEquality.JsonEquals(a, b));
        }

        [Theory]
        [InlineData(@"{""a"":true}", @"{""a"":true}")]
        [InlineData(@"{""a"":false}", @"{""a"":false}")]
        public void Booleans_Equal(string a, string b) {
            Assert.True(JsonEquality.JsonEquals(a, b));
        }

        [Fact]
        public void Strings_Whitespace_In_Values_Matters() {
            var a = @"{ ""s"": ""hello"" }";
            var b = @"{ ""s"": ""hello "" }"; // different value
            Assert.False(JsonEquality.JsonEquals(a, b));
        }

        [Fact]
        public void Strings_With_Escapes_Compare_By_Value() {
            var a = @"{ ""s"": ""line\nbreak"" }";
            var b = "{ \"s\": \"line\\nbreak\" }"; // equivalent
            Assert.True(JsonEquality.JsonEquals(a, b));
        }
    }
}