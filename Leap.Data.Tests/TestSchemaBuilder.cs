namespace Leap.Data.Tests {
    using System;
    using System.Collections.Generic;

    using Leap.Data.Configuration;
    using Leap.Data.Humanizer;
    using Leap.Data.JsonNet;
    using Leap.Data.Schema;
    using Leap.Data.Schema.Conventions;
    using Leap.Data.SqlServer;
    using Leap.Data.Tests.TestDomain.Blog;
    using Leap.Data.Tests.TestDomain.GenericType;
    using Leap.Data.Tests.TestDomain.Identity;
    using Leap.Data.Tests.TestDomain.InheritanceFail;
    using Leap.Data.Tests.TestDomain.InheritancePlay;
    using Leap.Data.Tests.TestDomain.MultiFieldKeyType;
    using Leap.Data.Tests.TestDomain.MultipleFutures;
    using Leap.Data.Tests.TestDomain.MultiTableType;
    using Leap.Data.Tests.TestDomain.TupleKeyType;
    using Leap.Data.Tests.TestDomain.TypedSerialization;

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
        public const string SqlServerConnectionString = "Server=.;Database=leap-data;Trusted_Connection=True;";

        public static ISessionFactory Build(ISchema schema) {
            var sessionFactory = new Configuration(schema).UseSqlServer(SqlServerConnectionString).UseJsonNetFieldSerialization().BuildSessionFactory();
            return sessionFactory;
        }
    }
}