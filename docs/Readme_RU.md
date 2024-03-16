# Запуска в Docker контейнере
---

## Настройки конфигурации:
**Все последующие данные должны быть добавлены в хранилище секретов, не стоит хранить их в в `appsetting.json`**:

|     Переменная     |  Ожидаемый ключ   |      Ожидаемый формат      |
|--------------------|-------------------|----------------------------|
|   Email Password   |  "EmailPassword"  |           Любой            |
|    Email Adress    |      "Email"      |  "FileCryptWeb@email.com"  |

**Другие необходимые данные, такие как: Подключения к сервисам, и Ключи уже хранится в файле `appsetting.json`**

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
## Запуск контейнера:
*Установите Docker к себе на компьютер*

*Клонируйте этот репозиторий к себе на компьютер*

**Далее понабодится терминал**
-
*Указываем корневую папку репозитория*

```bash
cd C:\Users\Stewi\source\repos\air2921\FileCryptWeb
```

*И поднимаем наш контейнер*

```bash
C:\Users\User\source\repos\github\FileCryptWeb>docker-compose down
```
---
## Эндпоинты:

**Протестировать API можно через `Swagger UI`**

*Он доступен по пути: `https://localhost:8081/swagger/index.html`* в вашем браузере

**Мониторий логи можно через `Kibana`**

*Он доступен по пути: `http://localhost:5601/app/home`* в вашем браузере
