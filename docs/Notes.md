# Loading entities

## Request to load entity of type TEntity with key X of type TKey

Load(X);  
* check session ID Map, if exists, return  
* ask session factory cache, if record exists  
    * hydrate instance
    * add to session ID Map
* execute query, if row exists
    * create record
    * hydrate instance
    * add to session ID map
    * add to session factory cache
* return null

## Request to query entities

Query()
* execute query
* for each row
    * create record
    * check session ID Map, if exists, add to result set
    * if not,
        * hydrate instance
        * add to session ID map
        * add to session factory cache
        * add to result set



Session considered single threaded

Inheritance
* Configuration dictates which table a particular type is persisted to
* support `session.Get<IThing>()`
* IdentityMap is based on the table that things are stored in


Second level cache can use strongly typed IDs instead of columns