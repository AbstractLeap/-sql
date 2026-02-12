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


        public Guid? NullableGuid { get; set; }
        public DateTime? NullableDate { get; set; }
        public DateTimeOffset? NullableDateOffset { get; set; }
        public TimeSpan? NullableTimeSpan { get; set; }

    }
    public enum Status {
        Unknown = 0,
        Active = 1,
        Suspended = 2
    }

    public class WithPrimitives {
        public Guid? Id { get; set; }
        public DateTime? When { get; set; }
        public DateTimeOffset? WhenOffset { get; set; }
        public TimeSpan? Duration { get; set; }
        public Uri? Link { get; set; }
        public Status? State { get; set; }
        public byte[]? Data { get; set; }
    }

    public class Wrapper {
        public int[]? Values { get; set; }
    }

}
