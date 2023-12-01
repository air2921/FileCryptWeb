# FileCryptWeb

## SPA web application on ASP.NET Core + Reactjs

## Configuration Settings:

All values must be added to the secret store. The expected configuration settings are shown below:

| Variable           | Expected Key                           | Expected Value Format                                                              |
|--------------------|--------------------------------------- |------------------------------------------------------------------------------------|
| secretKey          | "SecretKey"                            | String 128 bits long                                                                                |
| emailPassword      | "EmailPassword"                        | Any                                                                                |
| emailAdress        | "Email"                                | "FileCryptWeb@email.com"                                                           |
| fileCryptKey       | "FileCryptKey"                         | 256-bit byte array encoded in Base64String                                         |
| redisServer        | "ConnectionStrings:RedisConnection"    | "localhost:6379,abortConnect=false"                                                |
| mongoDb            | "ConnectionStrings:MongoDbConnection"  | "mongodb://localhost:27017"                                                        |
| postgres           | "ConnectionStrings:PostgresConnection" | "Host=YourHost;Port=5432;Username=Username;Password=YourPassword;Database=YourDB;" |

## Additional setup

 **ClamAV**: You need to install and run ClamAV, and configure it on "localhost" and port 3310

   ClamAV configuration files are located in the **`/config`**

 **Redis**: You need to run the Redis server on **`localhost`**, and port **`6379`**

## Database Migration with Entity Framework Core (EF Core)

 **Create a Migration**: To create a new database migration, use the following command. Replace `YourMigrationName` with a meaningful name for your migration:

  ```bash
   dotnet ef migrations add YourMigrationName
  
   dotnet ef database update
  ```
