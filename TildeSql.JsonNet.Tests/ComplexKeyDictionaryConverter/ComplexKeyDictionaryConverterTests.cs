namespace TildeSql.JsonNet.Tests.ComplexKeyDictionaryConverter {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Xunit;

    public class ComplexKeyDictionaryConverterTests {
        private static JsonNetFieldSerializer GetSerializer() {
            return new JsonNetFieldSerializer();
        }

        [Fact]
        public void MapsIDictionariesCorrectly() {
            var json = """{"id":2,"map":[{"Key":{"id":99},"Value":{"name":"Foo"}},{"Key":{"id":67},"Value":{"name":"haha funny number"}}],"lookup":null}""";
            var obj = new HasComplexKeyDictionary {
                Id = 2,
                Map = new Dictionary<IdLike, SomeValue> {
                    { new IdLike { Id = 99 }, new SomeValue { Name = "Foo" } },
                    { new IdLike { Id = 67 }, new SomeValue { Name = "haha funny number" } }
                }
            };

            Assert.Equal(json, GetSerializer().Serialize(obj));
        }

        [Fact]
        public void MapsConcreteDictionariesCorrectly() {
            var json = """{"id":1,"map":null,"lookup":[{"Key":{"idOne":"34b65a14-8b68-4169-9234-836ebbb5fff2","idTwo":"e752d992-caed-44a1-b5f1-2f2a0d5c5a83"},"Value":{"x":2,"y":2}},{"Key":{"idOne":"00000000-0000-0000-0000-000000000000","idTwo":"00000000-0000-0000-0000-000000000001"},"Value":{"x":3,"y":3}}]}""";
            var obj = new HasComplexKeyDictionary {
                Id = 1,
                Lookup = new Dictionary<CompoundId, SomeOtherValue> {
                    { new CompoundId { 
                        IdOne = Guid.Parse("34b65a14-8b68-4169-9234-836ebbb5fff2"), 
                        IdTwo = Guid.Parse("e752d992-caed-44a1-b5f1-2f2a0d5c5a83") },
                        new SomeOtherValue { X = 2, Y = 2 }
                    },
                    { new CompoundId { 
                        IdOne = Guid.Parse("00000000-0000-0000-0000-000000000000"), 
                        IdTwo = Guid.Parse("00000000-0000-0000-0000-000000000001") },
                        new SomeOtherValue { X = 3, Y = 3 }
                    }
                }
            };

            Assert.Equal(json, GetSerializer().Serialize(obj));
        }
    }
}
