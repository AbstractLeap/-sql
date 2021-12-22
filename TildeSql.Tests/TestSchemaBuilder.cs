namespace TildeSql.Tests {
    using System;
    using System.Collections.Generic;

    using TildeSql.JsonNet;

    using TildeSql.SqlServer;
    using TildeSql.Configuration;
    using TildeSql.Humanizer;
    using TildeSql.Schema;
    using TildeSql.Schema.Conventions;
    using TildeSql.Tests.TestDomain.Blog;
    using TildeSql.Tests.TestDomain.GenericType;
    using TildeSql.Tests.TestDomain.Identity;
    using TildeSql.Tests.TestDomain.InheritanceFail;
    using TildeSql.Tests.TestDomain.InheritancePlay;
    using TildeSql.Tests.TestDomain.MultiFieldKeyType;
    using TildeSql.Tests.TestDomain.MultipleFutures;
    using TildeSql.Tests.TestDomain.MultiTableType;
    using TildeSql.Tests.TestDomain.TupleKeyType;
    using TildeSql.Tests.TestDomain.TypedSerialization;

    class TestSchemaBuilder {
        public static ISchema Build() {
            var schemaBuilder = new SchemaBuilder().AddTypes(typeof(Blog))
                                                   .AddTypes("Entities", typeof(Entity<>))
                                                   .AddTypes("EntityWithIdentities", typeof(EntityWithIdentity))
                                                   .AddTypes("MeetingRequests", typeof(MeetingRequest), typeof(LessonRequest), typeof(IntroductionRequest))
                                                   .AddTypes(typeof(IAnimal), typeof(Animal), typeof(Dog), typeof(Terrier), typeof(Poodle), typeof(Cat))
                                                   .AddTypes(typeof(MultiFieldIdEntity))
                                                   .AddTypes(typeof(MultiNonPrimitiveIdFieldIdEntity))
                                                   .AddTypes(typeof(Orange), typeof(Apple))
                                                   .AddTypes(MultiTableType.NotApprovedCollectionName, typeof(Person))
                                                   .AddTypes(MultiTableType.ApprovedCollectionName, typeof(Person))
                                                   .AddTypes(MultiTableType.ArchivedCollectionName, typeof(Person))
                                                   .AddTypes(typeof(TestDomain.PlayExtraColumns.Person))
                                                   .AddTypes(typeof(TupleKeyTypeThing))
                                                   .AddTypes(typeof(GenericContainer));
            schemaBuilder.Setup<TestDomain.PlayExtraColumns.Person>().AddComputedColumn<string>("Email", "$.email.address", true, true);
            schemaBuilder.Setup<TestDomain.PlayExtraColumns.Person>()
                         .AddProjectionColumn("Fullname", person => (person.Name.GivenNames ?? string.Empty) + " " + (person.Name.Surname ?? string.Empty));

            return schemaBuilder.UseSqlServerConvention().UseConvention(new NameConvention()).UseConvention(new ComputedConvention()).Build();
        }

        class ComputedConvention : IKeyComputedSchemaConvention {
            public bool IsKeyComputed(string collectionName, IEnumerable<Type> entityTypes) {
                return collectionName == "EntityWithIdentities";
            }
        }

        class NameConvention : ICollectionNamingSchemaConvention {
            private readonly PluralizationConvention defaultConvention;

            public NameConvention() {
                this.defaultConvention = new PluralizationConvention();
            }

            public string GetCollectionName(Type type) {
                return type.Namespace == typeof(IAnimal).Namespace ? "Animals" : this.defaultConvention.GetCollectionName(type);
            }
        }
    }

    class TestSessionFactoryBuilder {
        public const string SqlServerConnectionString = "Server=.;Database=tildesql;Trusted_Connection=True;";

        public static ISessionFactory Build(ISchema schema) {
            var sessionFactory = new Configuration(schema).UseSqlServer(SqlServerConnectionString).UseJsonNetFieldSerialization().BuildSessionFactory();
            return sessionFactory;
        }
    }
}