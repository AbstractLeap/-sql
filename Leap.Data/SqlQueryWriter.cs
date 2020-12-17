namespace Leap.Data {
    using System;
    using System.Data.Common;

    internal class SqlQueryWriter {
        public void Write(IQuery query, DbCommand command) {
            // TODO implement
            command.CommandText = "select newid() as id, '{ \"Title\":\"Foo\" }' as document, 'Leap.Data.Tests.Blog, Leap.Data.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' as type";
        }
    }
}