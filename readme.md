

![~Sql logo](https://raw.githubusercontent.com/AbstractLeap/docs/logo.webp)

# ~Sql
> A .Net Data library for a DDD-inspired Document DB over Sql

~Sql is a .Net library designed to support a [DDD](https://en.wikipedia.org/wiki/Domain-driven_design) domain model that uses a relational database (namely SQL Server) to provide a transactional document database.

It's goal is to enable storage of a [domain model](https://martinfowler.com/eaaCatalog/domainModel.html) that uses the full expressiveness of C# in a persistence agnostic way.

## Installing

To get started, add the following packages from Nuget:

```
dotnet add package TildeSql
dotnet add package TildeSql.SqlServer
dotnet add package TildeSql.JsonNet
```

## Developing

If you want to build the solution (and run the tests) you will need Sql Server and dotnet, that's it!

```shell
git clone https://github.com/AbstractLeap/tildesql.git
cd tildesql/
```


## Features

What can it do?
* Supports DDD tactical patterns (Entity, Value Object, Service, Aggregate, Repository, Domain Events)
* Removes/minimises primitive obsession
* Complete persistence ignorance -> Automatic dirty tracking
* Migrations
* Unit of Work/Identity Map
* In-memory and distributed caching
* Strongly typed and multiple primary keys
* Batched queries through "Futures"
* Supports persisting generic types
* Multiple tables per type
* Optimistic concurrency
* Inheritance
* Computed and projected columns
* Async everywhere

## A Few Examples

The following code illustrates the basic operations in ~Sql:
```
var blogId1 = new BlogId(); // strongly typed key
var blogId2 = new BlogId();
await using var session = this.GetSession(); // async everywhere
var entitiesEnumerable = session.Get<Blog>().MultipleFuture(new [] { blogId1, blogId2 }); // batch up multiple query
var entity = await session.Get<Blog>().SingleAsync(blogId1);
await foreach (var asyncEntity in entitiesEnumerable) { } // async enumeration

var blog = new Blog("My blog");
session.Add(blog);
session.Delete(blog);

var queryEntities = await session.Get<Blog>()
				.Where("AuthorId = @AuthorId", new { AuthorId = authorId.Id })
				.ToListAsync();

entity.Title = "Foo"; // automatic dirty checking
await session.SaveChangesAsync();
```

The following example illustrates how an inheritance hierarchy could be stored in a single table with the identity map ensuring that a query for a super type still returns the original (more specifically typed) instance for the same session:

```
var poodle = new Poodle("Paul");
session.Add(poodle);

var dog = await session.Get<Dog>().SingleAsync(poodle.Id);
Assert.Same(poodle, dog);
```
This example shows the storage of a generic type:
```
var thing = new Entity<Foo>(new Foo { Name = "Foofoo" });
insertSession.Add(thing);
await insertSession.SaveChangesAsync();

var selectSession = sf.StartSession();
var personAgain = await selectSession.Get<Entity<Foo>>().SingleAsync(thing.Id);
Assert.Equal("Foofoo", personAgain.Thing.Name);
```

## Contributing

If you'd like to contribute, please fork the repository and use a feature
branch. Pull requests are warmly welcome.

You may want to open an issue in the repository first :-)

## Links

Even though this information can be found inside the project on machine-readable
format like in a .json file, it's good to include a summary of most useful
links to humans using your project. You can include links like:

- Project homepage: https://your.github.com/awesome-project/
- Repository: https://github.com/your/awesome-project/
- Issue tracker: https://github.com/your/awesome-project/issues
  - In case of sensitive bugs like security vulnerabilities, please contact
    my@email.com directly instead of using issue tracker. We value your effort
    to improve the security and privacy of this project!
- Related projects:
  - Your other project: https://github.com/your/other-project/
  - Someone else's project: https://github.com/someones/awesome-project/


## Licensing

"The code in this project is licensed under MIT license."
