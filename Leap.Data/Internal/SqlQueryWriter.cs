namespace Leap.Data.Internal {
    using System.Data.Common;

    using Leap.Data.Queries;

    internal class SqlQueryWriter {
        public void Write(IQuery query, DbCommand command) {
            // TODO implement
            command.CommandText += "select cast('77b55913-d2b6-488d-8860-3e8e70cb5146' as uniqueidentifier) as id, '{ \"BlogId\": { \"Id\":\"77b55913-d2b6-488d-8860-3e8e70cb5146\"}, \"Title\":\"Foo\" }' as document, 'Leap.Data.Tests.Blog, Leap.Data.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' as type";
        }
    }
}