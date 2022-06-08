set /p apikey=Enter nuget api key: 
dotnet pack -o .
nuget setapikey %apikey%
FOR %%y IN (.\*.nupkg) DO call nuget push %%y -source https://api.nuget.org/v3/index.json
FOR %%y IN (.\*.nupkg) DO del "%%y"
FOR %%y IN (.\*.snupkg) DO del "%%y"