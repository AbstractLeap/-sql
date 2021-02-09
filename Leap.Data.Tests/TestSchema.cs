namespace Leap.Data.Tests {
    using Leap.Data.Humanizer;
    using Leap.Data.Schema;
    using Leap.Data.SqlServer;

    class TestSchema {
        public static ISchema Get() {
            return new SchemaBuilder().AddTypes(typeof(Blog)).UseSqlServerConvention().UseHumanizerPluralization().Build();
        }
    }
}