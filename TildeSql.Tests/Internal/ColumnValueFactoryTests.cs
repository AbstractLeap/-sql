namespace TildeSql.Tests.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using TildeSql.Internal.ColumnValueFactories;

    using Xunit;

    public class ColumnValueFactoryTests {

        [Fact]
        public void OptimisticConcurrencyColumnValueFactoryUsesCombGuidInternally() {
            var factory = new OptimisticConcurrencyColumnValueFactory();

            var now = DateTime.UtcNow;
            var comb = factory.GetValue<object, object, Guid>(null, null);
            Assert.True(RT.Comb.Provider.Sql.GetTimestamp(comb) - now < TimeSpan.FromMilliseconds(100));
        }

    }
}
