namespace TildeSql.JsonNet.Tests.ChangeDetector.Properties {
    public class Person {
        public string? Name { get; set; }
        public int Age { get; set; }
        public bool IsAdmin { get; set; }

        public bool? NullableBool { get; set; }

        public int? NullableInt { get; set; }
        public float? NullableFloat { get; set; }
        public decimal? NullableDecimal { get; set; }

        public Address? Address { get; set; }
    }
}
