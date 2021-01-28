namespace Leap.Data.Tests {
    using Leap.Data.Humanizer;
    using Leap.Data.Schema;

    class TestSchema {
        public static ISchema Get() {
            return new SchemaBuilder().AddTypes(typeof(Blog)).UseHumanizerPluralization().Build();
        }
    }
}