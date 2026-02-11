namespace TildeSql.JsonNet.Tests.ChangeDetector.Fields {
    // -------------------------------
        // Field-based model types
        // -------------------------------

        public class PersonFields {
            private string? name;

            private int age;

            private bool isAdmin;

            private AddressFields? address;

            private bool? nullableBool;

            private int? nullableInt;

            private float? nullableFloat;

            private decimal? nullableDecimal;

            // Public properties exist only to make test setup ergonomic.
            // Serialization uses the private fields via the resolver above.
            public string? Name {
                get => this.name;
                set => this.name = value;
            }

            public int Age {
                get => this.age;
                set => this.age = value;
            }

            public bool IsAdmin {
                get => this.isAdmin;
                set => this.isAdmin = value;
            }

            public AddressFields? Address {
                get => this.address;
                set => this.address = value;
            }

            public bool? NullableBool {
                get => this.nullableBool;
                set => this.nullableBool = value;
            }

            public int? NullableInt {
                get => this.nullableInt;
                set => this.nullableInt = value;
            }

            public float? NullableFloat {
                get => this.nullableFloat;
                set => this.nullableFloat = value;
            }

            public decimal? NullableDecimal {
                get => this.nullableDecimal;
                set => this.nullableDecimal = value;
            }
        }
}
