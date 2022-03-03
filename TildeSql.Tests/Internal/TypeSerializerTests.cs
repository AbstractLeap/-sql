namespace TildeSql.Tests.Internal {
    using TildeSql.Internal;

    using Xunit;

    public class TypeSerializerTests {
        [Fact]
        public void SimpleTypeRoundTrips() {
            var typeSerializer = new TypeSerializer(new[] { typeof(Simple) });
            var serialized = typeSerializer.Serialize(typeof(Simple));
            Assert.Equal(nameof(Simple), serialized);
            var deserialized = typeSerializer.Deserialize(serialized);
            Assert.Equal(typeof(Simple), deserialized);
        }

        [Fact]
        public void MultipleTypeRoundTrips() {
            var typeSerializer = new TypeSerializer(new[] { typeof(Animal), typeof(Dog) });
            var serialized = typeSerializer.Serialize(typeof(Animal));
            Assert.Equal(nameof(Animal), serialized);
            var deserialized = typeSerializer.Deserialize(serialized);
            Assert.Equal(typeof(Animal), deserialized);
        }

        [Fact]
        public void NonGenericBaseRoundTrips() {
            var typeSerializer = new TypeSerializer(new[] { typeof(NonGenericBase), typeof(GenericWithNonGenericBase<>) });

            var baseSerialized = typeSerializer.Serialize(typeof(NonGenericBase));
            Assert.Equal(nameof(NonGenericBase), baseSerialized);
            var baseDeserialized = typeSerializer.Deserialize(baseSerialized);
            Assert.Equal(typeof(NonGenericBase), baseDeserialized);

            var genericSerialized = typeSerializer.Serialize(typeof(GenericWithNonGenericBase<>).MakeGenericType(typeof(Simple)));
            Assert.Equal($"{typeof(GenericWithNonGenericBase<>).Name}[{typeof(Simple).AssemblyQualifiedName}]", genericSerialized);
            var genericDeserialized = typeSerializer.Deserialize(genericSerialized);
            Assert.Equal(typeof(GenericWithNonGenericBase<>).MakeGenericType(typeof(Simple)), genericDeserialized);
        }

        [Fact]
        public void MultiGenericRoundTrips() {
            var typeSerializer = new TypeSerializer(new[] { typeof(Generic1<>), typeof(Generic2<>) });

            var baseSerialized = typeSerializer.Serialize(typeof(Generic1<>).MakeGenericType(typeof(Simple)));
            Assert.Equal($"{typeof(Generic1<>).Name}[{typeof(Simple).AssemblyQualifiedName}]", baseSerialized);
            var baseDeserialized = typeSerializer.Deserialize(baseSerialized);
            Assert.Equal(typeof(Generic1<>).MakeGenericType(typeof(Simple)), baseDeserialized);

            var genericSerialized = typeSerializer.Serialize(typeof(Generic2<>).MakeGenericType(typeof(Simple)));
            Assert.Equal($"{typeof(Generic2<>).Name}[{typeof(Simple).AssemblyQualifiedName}]", genericSerialized);
            var genericDeserialized = typeSerializer.Deserialize(genericSerialized);
            Assert.Equal(typeof(Generic2<>).MakeGenericType(typeof(Simple)), genericDeserialized);
        }

        class Simple { }

        class Animal { }

        class Dog : Animal { }

        class NonGenericBase { }

        class GenericWithNonGenericBase<T> : NonGenericBase { }

        class Generic1<T> { }

        class Generic2<T> : Generic1<T> { }
    }
}