namespace TildeSql.Tests {
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Threading.Tasks;

    using Microsoft.Data.SqlClient;

    using TildeSql.Tests.TestDomain.Blog;

    using Xunit;
    using Xunit.Abstractions;

    public class PoolExhaustion {
        private readonly ITestOutputHelper output;

        public PoolExhaustion(ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        public async Task ItDisposes() {
            var cStr = "Server=.;Database=tildesql;Trusted_Connection=True;TrustServerCertificate=Yes;Max Pool Size=10";
            var sf = TestSessionFactoryBuilder.Build(
                TestSchemaBuilder.Build(), cStr
                , configuration => configuration.ConnectionFactoryFactory = new ConFacFac(cStr, this.output));
            for (int i = 0; i < 15; i++) {
                await using var session = sf.StartSession();
                var blog = await session.Get<Blog>().SingleAsync(new BlogId());
                this.output.WriteLine($"{(i + 1)} run");
            }
            
            Assert.True(true);
            for (int i = 0; i < 15; i++) {
                await using var s = sf.StartSession();
                var bF = s.Get<Blog>().SingleFuture(new BlogId());
                var bf2 = s.Get<Blog>().SingleFuture(new BlogId());
                var bf3 = s.Get<Blog>().SingleFuture(new BlogId());
                var b = await bF.SingleAsync(); // don't get the bf2
                this.output.WriteLine($"{(i + 1)} run");
            }
        }

        class ConnFactory : IConnectionFactory {
            private readonly string conString;

            private readonly ITestOutputHelper outputHelper;

            private readonly List<SqlConnection> conns;

            public ConnFactory(string conString, ITestOutputHelper outputHelper) {
                this.conString    = conString;
                this.outputHelper = outputHelper;
                this.conns        = new List<SqlConnection>();
            }

            public DbConnection Get() {
                var con = new SqlConnection(this.conString);
                this.conns.Add(con);
                this.outputHelper.WriteLine($"Conn: {this.conns.Count}");
                con.Disposed    += (sender, args) => this.outputHelper.WriteLine($"{this.Conn((SqlConnection)sender)} disposed");
                con.StateChange += (sender, args) => this.outputHelper.WriteLine($"{this.Conn((SqlConnection)sender)}: {args.OriginalState} -> {args.CurrentState}");
                return con;
            }

            private int Conn(SqlConnection conn) {
                for (int i = 0; i < this.conns.Count; i++) {
                    if (ReferenceEquals(this.conns[i], conn)) {
                        return i + 1;
                    }
                }

                return -1;
            }
        }

        class ConFacFac : IConnectionFactoryFactory {
            private readonly ConnFactory conFac;

            public ConFacFac(string conString, ITestOutputHelper outputHelper) {
                this.conFac = new ConnFactory(conString, outputHelper);
            }

            public IConnectionFactory Get() {
                return this.conFac;
            }
        }
    }
}