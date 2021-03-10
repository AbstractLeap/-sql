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