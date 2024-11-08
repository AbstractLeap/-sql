namespace TildeSql.Tests {
    using Xunit;

    public class ChangeTrackerTests {
        [Theory]
        [InlineData(null, null, true)]
        [InlineData("123", "123", true)]
        [InlineData("123", null, false)]
        [InlineData(null, "123", false)]
        [InlineData("\\/", "/", true)]
        [InlineData("/", "\\/", true)]
        [InlineData("/", "\\", false)]
        [InlineData("\\", "/", false)]
        [InlineData("12\\/3", "12/3", true)]
        [InlineData("12/3", "12\\/3", true)]
        [InlineData("12\\\\3", "12\\3", false)]
        [InlineData("12\\3", "12\\\\3", false)]
        [InlineData("12\\/", "12\\", false)]
        [InlineData("12\\", "12\\/", false)]
        [InlineData("12/", "12\\/", true)]
        [InlineData("12\\/", "12/", true)]
        [InlineData("\\/123", "/123", true)]
        [InlineData(
            "{\"mechanism\":{\"line1\":\"Flat 1\",\"line2\":\"83\\/87 Bobs Road\tfar\",\"city\":\"London\",\"postalCode\":\"SW16 1AB\",\"country\":null}}",
            "{\"mechanism\":{\"line1\":\"Flat 1\",\"line2\":\"83/87 Bobs Road\tfar\",\"city\":\"London\",\"postalCode\":\"SW16 1AB\",\"country\":null}}",
            true)]
        public void SolidusIgnored(string left, string right, bool equals) {
            Assert.Equal(equals, JsonSolidusEscapeIgnoringStringComparator.StringEquals(left, right));
        }
    }
}