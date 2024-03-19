```
tests/
├── FakeLogger.cs
├── GlobalUsings.cs
├── tests.csproj
├── Controllers_Tests/
│   ├── Account/
│   │   ├── AuthRegistrationController_Test.cs
│   │   ├── RecoveryController_Test.cs
│   │   ├── SessionController_Test.cs
│   │   └── Edit/
│   │       ├── EmailController_Test.cs
│   │       ├── PasswordController_Test.cs
│   │       ├── UsernameController_Test.cs
│   │       └── _2FaController_Test.cs
│   └── Admin/
│       ├── ApiController_Test.cs
│       ├── AppController_Test.cs
│       ├── FileController_Test.cs
│       ├── KeyController_Test.cs
│       ├── LinkController_Test.cs
│       ├── MimeController_Test.cs
│       ├── NotificationController_Test.cs
│       ├── OfferController_Test.cs
│       ├── SendEmailController_Test.cs
│       ├── TokenController_Test.cs
│       └── UserController_Test.cs
├── Cryptography_Test/
│   ├── CypherKey_Test.cs
│   └── Cypher_Test.cs
├── Db_Tests/
│   ├── Redis_Tests/
│   │   └── RedisCache_Test.cs
│   └── Sql_Tests/
│       ├── Repository_Test.cs
│       └── Sorting_Test.cs
├── Helpers_Tests/
│   ├── FileManager_Test.cs
│   ├── ImplementationFinder_Test.cs
│   ├── UserData_Test.cs
│   └── Validation_Test.cs
├── Middlewares_Tests/
│   ├── Bearer_Test.cs
│   ├── ExceptionHandler_Test.cs
│   ├── Freeze_Service_Test.cs
│   ├── LogMiddleware_Test.cs
│   └── UserSession_Test.cs
├── Security_Tests/
│   ├── Generate_Test.cs
│   ├── PasswordManager_Test.cs
│   └── TokenService_Test.cs
└── Third_Party_Services_Tests/
    ├── ClamAv_Test.cs
    └── EmailSender_Test.cs
```
