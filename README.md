# FileCryptWeb

## SPA web application on ASP.NET Core + Reactjs

## Configuration Settings:

All values must be added to the secret store. The expected configuration settings are shown below:

| Variable         | Expected Key                                 | Expected Value Format                                                              |
|------------------|----------------------------------------------|------------------------------------------------------------------------------------|
| Secret Key       | "SecretKey"                                  | String 128 bits long                                                               |
| E-Password       | "EmailPassword"                              | Any                                                                                |
| E-Address        | "Email"                                      | "FileCryptWeb@email.com"                                                           |
| App Key          | "FileCryptKey"                               | 256-bit byte array encoded in Base64String                                         |
| Redis            | "ConnectionStrings:RedisConnection"          | "localhost:6379,abortConnect=false"                                                |
| Elastic          | "ConnectionStrings:ElasticsearchConnection"  | "http://localhost:9200"                                                            |
| PostgreSQL       | "ConnectionStrings:PostgresConnection"       | "Host=YourHost;Port=5432;Username=Username;Password=YourPassword;Database=YourDB;" |

## Additional setup

 **ClamAV**: You need to install and run ClamAV, and configure it on **`localhost`** and port **`3310`**

   ClamAV configuration files are located in the **`/config`**

 **Redis**: You need to run the Redis server on any platform, select your host and port, in connection string

## Database Migration with Entity Framework Core (EF Core)

 **Create a Migration**: To create a new database migration, use the following command. Replace `YourMigrationName` with a meaningful name for your migration:

  ```bash
   dotnet ef migrations add YourMigrationName
  
   dotnet ef database update
  ```
