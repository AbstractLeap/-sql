namespace TildeSql.Schema.KeyFactories {
    interface INode {
        object GetValue(object[] row);
    }
}