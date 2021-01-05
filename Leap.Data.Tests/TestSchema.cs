namespace Leap.Data.Tests {
    using Leap.Data.Schema;

    class TestSchema {
        public static ISchema Get() {
            return new SchemaBuilder().AddTypes(typeof(Blog)).Build();

            //var mockSchema = new Mock<ISchema>();
            //var idColumn = new Column(typeof(Guid), "Id");
            //mockSchema.Setup(s => s.GetTable<Blog>())
            //          .Returns(
            //              new Table (mockSchema.Object)
            //              {
            //                  Name    = "Blogs",
            //                  Schema  = "dbo",
            //                  KeyType = typeof(BlogId),
            //                  Columns = new List<Column>() {
            //                      idColumn,
            //                      new Column(typeof(string), SpecialColumns.Document),
            //                      new Column(typeof(string), SpecialColumns.DocumentType)
            //                  },
            //                  KeyColumns = new List<Column> {
            //                      idColumn
            //                  }
            //              });
        }
    }
}