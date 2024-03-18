```
project/
│
├── .dockerignore
├── .gitattributes
├── .gitignore
├── docker-compose.dcproj
├── docker-compose.override.yml
├── docker-compose.yml
├── FileCryptWeb.sln
├── LICENSE.txt
├── README.md
├── .github/
│   └── workflows/
│       ├── build.yml
│       ├── notify.yml
│       └── tests.yml
├── docs/
│   ├── Readme_RU.md
│   └── Readme_US.md
├── reactapp/
│   ├── .eslintrc.cjs
│   ├── .gitignore
│   ├── aspnetcore-https.js
│   ├── index.html
│   ├── nuget.config
│   ├── package-lock.json
│   ├── package.json
│   ├── reactapp.esproj
│   ├── README.md
│   ├── tsconfig.json
│   ├── vite.config.js
│   ├── public/
│   │   └── vite.svg
│   └── src/
│       ├── App.jsx
│       ├── index.css
│       ├── main.jsx
│       ├── assets/
│       │   └── react.svg
│       ├── components/
│       │   ├── Layout/
│       │   │   ├── Layout.css
│       │   │   └── Layout.tsx
│       │   ├── List/
│       │   │   ├── ApiList/
│       │   │   │   ├── ApiList.tsx
│       │   │   │   ├── ApiListProps.tsx
│       │   │   │   └── ApiProps.tsx
│       │   │   ├── FileList/
│       │   │   │   ├── FileList.tsx
│       │   │   │   ├── FileListProps.tsx
│       │   │   │   └── FileProps.tsx
│       │   │   ├── Notifications/
│       │   │   │   ├── NotificationList.tsx
│       │   │   │   ├── NotificationListProps.tsx
│       │   │   │   └── NotificationProps.tsx
│       │   │   ├── OfferList/
│       │   │   │   ├── OfferList.tsx
│       │   │   │   ├── OfferListProps.tsx
│       │   │   │   └── OfferProps.tsx
│       │   │   └── UserList/
│       │   │       ├── UserList.tsx
│       │   │       ├── UserListProps.tsx
│       │   │       └── UserProps.tsx
│       │   ├── Modal/
│       │   │   ├── Modal.css
│       │   │   ├── Modal.tsx
│       │   │   └── ModalProps.tsx
│       │   ├── UseAuth/
│       │   │   └── UseAuth.tsx
│       │   ├── User/
│       │   │   ├── UserData.tsx
│       │   │   ├── UserDataProps.tsx
│       │   │   ├── UserKeys.tsx
│       │   │   └── UserKeysProps.tsx
│       │   └── UseResize/
│       │       └── useResize.tsx
│       ├── pages/
│       │   ├── auth/
│       │   │   ├── login/
│       │   │   │   ├── Login.css
│       │   │   │   └── Login.tsx
│       │   │   ├── recovery/
│       │   │   │   ├── CreateRecovery.tsx
│       │   │   │   ├── RecoveryAccount.css
│       │   │   │   └── RecoveryAccount.tsx
│       │   │   └── registration/
│       │   │       ├── Register.tsx
│       │   │       └── Registration.css
│       │   ├── list/
│       │   │   ├── api/
│       │   │   │   └── Api.tsx
│       │   │   ├── files/
│       │   │   │   └── Files.tsx
│       │   │   ├── notifications/
│       │   │   │   └── Notifications.tsx
│       │   │   ├── offers/
│       │   │   │   └── Offers.tsx
│       │   │   └── storages/
│       │   │       └── Storages.tsx
│       │   ├── no_logic/
│       │   │   ├── About.tsx
│       │   │   ├── Home.tsx
│       │   │   ├── Policy.tsx
│       │   │   └── ErrorPage/
│       │   │       ├── ErrorPage.css
│       │   │       ├── ErrorPage.tsx
│       │   │       └── ErrorPageProps.tsx
│       │   └── user/
│       │       ├── Settings.tsx
│       │       └── profile/
│       │           └── User.tsx
│       └── utils/
│           ├── api/
│           │   ├── AxiosRequest.tsx
│           │   └── RequestProps.tsx
│           └── helpers/
│               ├── date/
│               │   ├── Date.tsx
│               │   └── DateProps.tsx
│               ├── icon/
│               │   ├── Font.tsx
│               │   └── FontProps.tsx
│               └── message/
│                   ├── Message.tsx
│                   └── MessageProps.tsx
├── tests/
│   ├── FakeLogger.cs
│   ├── GlobalUsings.cs
│   ├── tests.csproj
│   ├── Controllers_Tests/
│   │   ├── Account/
│   │   │   ├── AuthRegistrationController_Test.cs
│   │   │   ├── RecoveryController_Test.cs
│   │   │   ├── SessionController_Test.cs
│   │   │   └── Edit/
│   │   │       ├── EmailController_Test.cs
│   │   │       ├── PasswordController_Test.cs
│   │   │       ├── UsernameController_Test.cs
│   │   │       └── _2FaController_Test.cs
│   │   └── Admin/
│   │       ├── ApiController_Test.cs
│   │       ├── AppController_Test.cs
│   │       ├── FileController_Test.cs
│   │       ├── KeyController_Test.cs
│   │       ├── LinkController_Test.cs
│   │       ├── MimeController_Test.cs
│   │       ├── NotificationController_Test.cs
│   │       ├── OfferController_Test.cs
│   │       ├── SendEmailController_Test.cs
│   │       ├── TokenController_Test.cs
│   │       └── UserController_Test.cs
│   ├── Cryptography_Tests/
│   │   ├── CypherKey_Test.cs
│   │   └── Cypher_Test.cs
│   ├── Db_Tests/
│   │   ├── Redis_Tests/
│   │   │   └── RedisCache_Test.cs
│   │   └── Sql_Tests/
│   │       ├── Repository_Test.cs
│   │       └── Sorting_Test.cs
│   ├── Helpers_Tests/
│   │   ├── FileManager_Test.cs
│   │   ├── ImplementationFinder_Test.cs
│   │   ├── UserData_Test.cs
│   │   └── Validation_Test.cs
│   ├── Middlewares_Tests/
│   │   ├── Bearer_Test.cs
│   │   ├── ExceptionHandler_Test.cs
│   │   ├── Freeze_Service_Test.cs
│   │   ├── LogMiddleware_Test.cs
│   │   └── UserSession_Test.cs
│   ├── Security_Tests/
│   │   ├── Generate_Test.cs
│   │   ├── PasswordManager_Test.cs
│   │   └── TokenService_Test.cs
│   └── Third_Party_Services_Tests/
│       ├── ClamAv_Test.cs
│       └── EmailSender_Test.cs
└── webapi/
    ├── appsettings.Development.json
    ├── appsettings.json
    ├── Dockerfile
    ├── Program.cs
    ├── Startup.cs
    ├── webapi.csproj
    ├── webapi.csproj.user
    ├── Attributes/
    │   ├── AuxiliaryObjectAttribute.cs
    │   ├── HelperAttribute.cs
    │   └── ImplementationKeyAttribute.cs
    ├── Controllers/
    │   ├── Account/
    │   │   ├── AuthRegistrationController.cs
    │   │   ├── AuthSessionController.cs
    │   │   ├── RecoveryController.cs
    │   │   └── Edit/
    │   │       ├── 2FaController.cs
    │   │       ├── EmailController.cs
    │   │       ├── PasswordController.cs
    │   │       └── UsernameController.cs
    │   ├── Admin/
    │   │   ├── Admin_ApiController.cs
    │   │   ├── Admin_FileController.cs
    │   │   ├── Admin_KeyController.cs
    │   │   ├── Admin_LinkController.cs
    │   │   ├── Admin_MimeController.cs
    │   │   ├── Admin_NotificationController.cs
    │   │   ├── Admin_OfferController.cs
    │   │   ├── Admin_TokenController.cs
    │   │   ├── Admin_UserController.cs
    │   │   ├── AppController.cs
    │   │   └── SendEmailController.cs
    │   ├── Core/
    │   │   ├── ApiController.cs
    │   │   ├── CryptographyController.cs
    │   │   ├── FileController.cs
    │   │   ├── KeysController.cs
    │   │   ├── KeyStorageController.cs
    │   │   ├── NotificationController.cs
    │   │   ├── OfferController.cs
    │   │   ├── UserController.cs
    │   │   └── CryptographyUtils/
    │   │       ├── CryptographyHelper.cs
    │   │       └── FileService.cs
    │   └── Public API/
    │       └── CryptographyController.cs
    ├── Cryptography/
    │   ├── AesCreator.cs
    │   ├── CryptographyResult.cs
    │   ├── Cypher.cs
    │   └── CypherKey.cs
    ├── CFileCryptWeb/
    ├── DB/
    │   ├── FileCryptDbContext.cs
    │   ├── Repository.cs
    │   ├── Sorting.cs
    │   └── RedisDb/
    │       ├── RedisCache.cs
    │       ├── RedisDbContext.cs
    │       └── RedisKeys.cs
    ├── DTO/
    │   ├── AuthDTO.cs
    │   ├── EmailDto.cs
    │   ├── KeyDTO.cs
    │   ├── NotifyDTO.cs
    │   ├── PasswordDto.cs
    │   ├── RegisterDTO.cs
    │   ├── StorageDTO.cs
    │   └── UpdateStorageDTO.cs
    ├── Exceptions/
    │   ├── EntityNotCreatedException.cs
    │   ├── EntityNotDeletedException.cs
    │   ├── EntityNotUpdatedException.cs
    │   ├── InvalidConfigurationException.cs
    │   ├── InvalidRouteException.cs
    │   └── SmtpClientException.cs
    ├── Helpers/
    │   ├── FileManager.cs
    │   ├── ImmutableData.cs
    │   ├── ImplementationFinder.cs
    │   ├── UserData.cs
    │   └── Validation.cs
    ├── Interfaces/
    │   ├── IRepository.cs
    │   ├── Controllers/
    │   │   ├── ICryptographyControllerBase.cs
    │   │   ├── ICryptographyParamsProvider.cs
    │   │   └── IFileService.cs
    │   ├── Cryptography/
    │   │   ├── IAes.cs
    │   │   ├── ICypher.cs
    │   │   └── ICypherKey.cs
    │   ├── Redis/
    │   │   ├── IRedisCache.cs
    │   │   ├── IRedisDbContext.cs
    │   │   └── IRedisKeys.cs
    │   └── Services/
    │       ├── IEmailSender.cs
    │       ├── IFileManager.cs
    │       ├── IGenerate.cs
    │       ├── IGetSize.cs
    │       ├── IImplementationFinder.cs
    │       ├── IPasswordManager.cs
    │       ├── ITokenService.cs
    │       ├── IUserInfo.cs
    │       ├── IValidation.cs
    │       └── IVirusCheck.cs
    ├── Localization/
    │   ├── EmailMessage.cs
    │   ├── Message.cs
    │   └── NotificationMessage.cs
    ├── Mapper/
    │   ├── KeyDtoToKeyStorageItemModelMapping.cs
    │   ├── NotifyDtoToNotificationModelMapping.cs
    │   └── StorageDtoToKeyStorageModelMapping.cs
    ├── Middlewares/
    │   ├── BearerMiddleware.cs
    │   ├── ExceptionHandleMiddleware.cs
    │   ├── Freeze Service Middleware.cs
    │   ├── LogMiddleware.cs
    │   ├── UserSessionMiddleware.cs
    │   └── XSRF Protection Middleware.cs
    ├── Migrations/
    │   ├── 20240317155330_Init.cs
    │   ├── 20240317155330_Init.Designer.cs
    │   └── FileCryptDbContextModelSnapshot.cs
    ├── Models/
    │   ├── ApiModel.cs
    │   ├── FileMimeModel.cs
    │   ├── FileModel.cs
    │   ├── KeyModel.cs
    │   ├── KeyStorageItemModel.cs
    │   ├── KeyStorageModel.cs
    │   ├── LinkModel.cs
    │   ├── NotificationModel.cs
    │   ├── OfferModel.cs
    │   ├── TokenModel.cs
    │   └── UserModel.cs
    ├── Security/
    │   ├── Generate.cs
    │   ├── PasswordManager.cs
    │   └── TokenService.cs
    ├── Startup Extensions/
    │   ├── AppConfigurationCheck.cs
    │   ├── AppServices.cs
    │   └── DependencyContainer.cs
    └── Third Party Services/
        ├── ClamAV.cs
        └── EmailSender.cs
```
