namespace Leap.Data.Tests.JsonNet {
    using Leap.Data.JsonNet;

    using Xunit;

    public class SerializationTests {
        [Fact]
        public void ConstructorNotCalled() {
            var fieldsSerializer = new JsonNetFieldSerializer();
            var foo = (Foo)fieldsSerializer.Deserialize(typeof(Foo), "{id:1}");
            Assert.Equal(1, foo.Id);
            Assert.False(Foo.Called);
        }

        [Fact]
        public void NonInitedField() {
            var fieldsSerializer = new JsonNetFieldSerializer();
            var newField = (NewField)fieldsSerializer.Deserialize(typeof(NewField), "{id:1}");
            Assert.Equal(1, newField.Id);
            Assert.Null(newField.Foo);
            Assert.Equal(0, newField.Bar);
        }

        class NewField {
            private readonly int id;

            private long bar;

            private string foo;

            public NewField(int id) {
                this.id = id;
            }

            public string Foo => this.foo;

            public long Bar => this.bar;

            public int Id => this.id;
        }

        class Foo {
            private readonly int id;

            public static bool Called;

            public Foo(int id) {
                this.id = id;
                Called  = true;
            }

            public int Id => this.id;
        }
    }
}