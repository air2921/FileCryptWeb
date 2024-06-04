# ������� � Docker ����������
---

## ��������� ������������:
**��� ����������� ������ ������ ���� ��������� � ��������� ��������, �� ����� ������� �� � � `appsetting.json`**:

|     ����������     |  ��������� ����   |      ��������� ������      |
|--------------------|-------------------|----------------------------|
|   Email Password   |  "EmailPassword"  |           �����            |
|    Email Adress    |      "Email"      |  "FileCryptWeb@email.com"  |

**������ ����������� ������, ����� ���: ����������� � ��������, � ����� ��� �������� � ����� `appsetting.json`**

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
## ������ ����������:
*���������� Docker � ���� �� ���������*

*���������� ���� ����������� � ���� �� ���������*

**����� ����������� ��������**
-
*��������� �������� ����� �����������*

```bash
cd C:\Users\Stewi\source\repos\air2921\FileCryptWeb
```

*� ��������� ��� ���������*

```bash
C:\Users\User\source\repos\github\FileCryptWeb>docker-compose up
```
---
## ���������:

**�������������� API ����� ����� `Swagger UI`**

*�� �������� �� ����: `https://localhost:8081/swagger/index.html` � ����� ��������*

**���������� ���� ����� ����� `Kibana`**

*�� �������� �� ����: `http://localhost:5601/app/home` � ����� ��������*

**��������� ������� � �� ����� ����� GUI PgAdmin**

*�� �������� �� ���� `http://localhost:5050` � ����� ��������*

---

# PgAdmin

## ���� � PgAdmin
**�� ������ ����� � PgAdmin � ������� �������� ������� ������� � ���������**

**�����: `FileCrypt147@gmail.com`**

**������: `FileCrypt123`**

## ����������� � �������
**��� ����������� � ������� ����� ����� ������������ �������� �� ��������� �� ���������**

**���/����� �������: `postgres_db`**

**����: `5432`**

**��������� ���� ������: `filecrypt`**

**��� ������������: `postgres`**

**������: `123`**

---

![Screenshot_2](https://github.com/air2921/FileCryptWeb/assets/92780383/db653ac8-8363-4d42-b468-92d54481c9d4)