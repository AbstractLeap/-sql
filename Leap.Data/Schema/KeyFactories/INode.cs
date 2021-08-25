namespace Leap.Data.Schema.KeyFactories {
    interface INode {
        object GetValue(object[] row);
    }
}