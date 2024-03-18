# Start Docker container
---

## Configuration Settings:
**All subsequent data should be added to the user-secrets, it should not be stored in `appsetting.json`**:

|       Variable     |   Expected Key    |    Expected Key Format     |
|--------------------|-------------------|----------------------------|
|   Email Password   |  "EmailPassword"  |            Any             |
|    Email Adress    |      "Email"      |  "FileCryptWeb@email.com"  |

**Other necessary data such as: Connections to services, and Keys are already stored in the file `appsetting.json`**

```bash
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Postgres": "Server=postgres_db;Port=5432;User Id=postgres;Password=123;Database=filecrypt;",
    "Redis": "redis:6379,abortConnect=false",
    "Elasticsearch": "http://elasticsearch:9200"
  },
  "ClamServer": "clamav",
  "ClamPort": "3310",
  "JwtKey": "fix8Lph0KWGy10Bk9azhxJTzhTCnd2mp+QfgYBP/pqA=",
  "AppKey": "8ifrnDa8a9nabJDfjTrfXsgfVIhCYGrZbN5HdtX0dK8="
}
```
---
## Run the Container:
*Install Docker on your PC*

*Clone this repository on your PC*

**Next you will need a terminal**
-
*Specify the root folder of the repository*

```bash
cd C:\Users\Stewi\source\repos\air2921\FileCryptWeb
```

*And start our container*

```bash
C:\Users\User\source\repos\github\FileCryptWeb>docker-compose up
```
---
## Endpoints:

### You can test the API via `Swagger UI`

*It's available at: `https://localhost:8081/swagger/index.html`* in your browser

### You can monitor logs via `Kibana`

*It's available at: `http://localhost:5601/app/home`* in your browser

### You can manage the database via `PgAdmin GUI`

*It's available at: `http://localhost:5050`* in your browser

---

# PgAdmin

## PgAdmin Login
**You can log into PgAdmin which is listed in the environment**

**Login: `FileCrypt147@gmail.com`**

**Password: `FileCrypt123`**

## Server connect

**To connect to the server we also use the default values ​​from the environment**

**Host name/Address: `postgres_db`**

**Port: `5432`**

**Maintenance database: `filecrypt`**

**Username: `postgres`**

**Password: `123`**
