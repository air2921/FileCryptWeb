├── .dockerignore
├── .editorconfig
├── .gitattributes
├── .gitignore
├── docker-compose.dcproj
├── docker-compose.override.yml
├── docker-compose.yml
├── FileCryptWeb.sln
├── LICENSE.txt
├── .github/
│   └── workflows/
│       ├── build.yml
│       ├── notify.yml
│       └── tests.yml
├── application/
│   ├── application.csproj
│   ├── ServiceRegistration.cs
│   ├── Abstractions/
│   │   ├── Endpoints/
│   │   │   ├── Account/
│   │   │   │   ├── I2FaService.cs
│   │   │   │   ├── IAvatarService.cs
│   │   │   │   ├── IEmailService.cs
│   │   │   │   ├── IPasswordService.cs
│   │   │   │   ├── IRecoveryService.cs
│   │   │   │   ├── IRegistrationService.cs
│   │   │   │   ├── ISessionService.cs
│   │   │   │   └── IUsernameService.cs
│   │   │   ├── Admin/
│   │   │   │   ├── IAdminFileService.cs
│   │   │   │   ├── IAdminLinkService.cs
│   │   │   │   ├── IAdminMimeService.cs
│   │   │   │   ├── IAdminNotificationService.cs
│   │   │   │   ├── IAdminOfferService.cs
│   │   │   │   ├── IAdminTokenService.cs
│   │   │   │   └── IAdminUserService.cs
│   │   │   └── Core/
│   │   │       ├── IActivityService.cs
│   │   │       ├── ICryptographyService.cs
│   │   │       ├── IFileService.cs
│   │   │       ├── INotificationService.cs
│   │   │       ├── IOfferService.cs
│   │   │       ├── IStorageItemService.cs
│   │   │       ├── IStorageService.cs
│   │   │       └── IUserService.cs
│   │   ├── Inner/
│   │   │   ├── ICryptographyHelper.cs
│   │   │   ├── IFileHelper.cs
│   │   │   └── ITokenComparator.cs
│   │   └── TP_Services/
│   │       ├── ICypher.cs
│   │       ├── ICypherKey.cs
│   │       ├── IEmailSender.cs
│   │       ├── IFileManager.cs
│   │       ├── IGenerate.cs
│   │       ├── IGetSize.cs
│   │       ├── IHashUtility.cs
│   │       ├── IS3Manager.cs
│   │       └── IVirusCheck.cs
│   ├── Cache Handlers/
│   │   ├── Activity.cs
│   │   ├── Files.cs
│   │   ├── ICacheHandler.cs
│   │   ├── Notifications.cs
│   │   ├── Offers.cs
│   │   ├── StorageItems.cs
│   │   ├── Storages.cs
│   │   └── Users.cs
│   ├── DTO/
│   │   ├── Inner/
│   │   │   ├── AcceptOfferDTO.cs
│   │   │   ├── CreateOfferDTO.cs
│   │   │   ├── CryptographyDTO.cs
│   │   │   ├── CryptographyResult.cs
│   │   │   ├── EmailDto.cs
│   │   │   ├── JwtDTO.cs
│   │   │   ├── UserContextDTO.cs
│   │   │   └── UserDTO.cs
│   │   └── Outer/
│   │       ├── ActivityDTO.cs
│   │       ├── AvatarDTO.cs
│   │       ├── CredentialsDTO.cs
│   │       ├── CypherFileDTO.cs
│   │       ├── LoginDTO.cs
│   │       ├── PasswordDTO.cs
│   │       ├── RecoveryDTO.cs
│   │       ├── RegisterDTO.cs
│   │       └── StorageDTO.cs
│   ├── Helper Services/
│   │   ├── IDataManagement.cs
│   │   ├── ITransaction.cs
│   │   ├── IValidator.cs
│   │   ├── Account/
│   │   │   ├── RecoveryHelper.cs
│   │   │   ├── RegistrationHelper.cs
│   │   │   ├── SessionHelper.cs
│   │   │   └── Edit/
│   │   │       ├── 2FaHelper.cs
│   │   │       ├── EmailHelper.cs
│   │   │       └── PasswordHelper.cs
│   │   ├── Admin/
│   │   │   ├── TokenService.cs
│   │   │   └── UserService.cs
│   │   └── Core/
│   │       ├── CryptographyHelper.cs
│   │       ├── FileHelper.cs
│   │       ├── KeyStorageHelper.cs
│   │       ├── KeyStorageItemHelper.cs
│   │       └── OfferHelper.cs
│   ├── Helpers/
│   │   ├── ImmutableData.cs
│   │   ├── TokenComparator.cs
│   │   └── Localization/
│   │       ├── EmailMessage.cs
│   │       ├── Message.cs
│   │       └── NotificationMessage.cs
│   ├── Master Services/
│   │   ├── Response.cs
│   │   ├── Account/
│   │   │   ├── RecoveryService.cs
│   │   │   ├── RegistrationService.cs
│   │   │   ├── SessionService.cs
│   │   │   └── Edit/
│   │   │       ├── 2FaService.cs
│   │   │       ├── AvatarService.cs
│   │   │       ├── EmailService.cs
│   │   │       ├── PasswordService.cs
│   │   │       └── UsernameService.cs
│   │   ├── Admin/
│   │   │   ├── Admin_FileService.cs
│   │   │   ├── Admin_LinkService.cs
│   │   │   ├── Admin_MimeService.cs
│   │   │   ├── Admin_NotificationService.cs
│   │   │   ├── Admin_OfferService.cs
│   │   │   ├── Admin_TokenService.cs
│   │   │   └── Admin_UserService.cs
│   │   └── Core/
│   │       ├── ActivityService.cs
│   │       ├── CryptographyService.cs
│   │       ├── FilesService.cs
│   │       ├── NotificationsService.cs
│   │       ├── OfferService.cs
│   │       ├── StorageItemsService.cs
│   │       ├── StoragesService.cs
│   │       └── UsersService.cs
├── data/
│   ├── data access.csproj
│   ├── ServiceRegistration.cs
│   ├── Ef/
│   │   ├── DatabaseTransaction.cs
│   │   ├── FileCryptDbContext.cs
│   │   └── Repository.cs
│   └── Redis/
│       ├── IRedisDbContext.cs
│       ├── RedisCache.cs
│       └── RedisDbContext.cs
├── docs/
│   └── docker/
│       ├── Start-En.md
│       └── Start-Ru.md
├── domain/
│   ├── domain.csproj
│   ├── Abstractions/
│   │   └── Data Access/
│   │       ├── IDatabaseTransaction.cs
│   │       ├── IRedisCache.cs
│   │       └── IRepository.cs
│   ├── Exceptions/
│   │   ├── EntityException.cs
│   │   ├── S3ClientException.cs
│   │   └── SmtpClientException.cs
│   ├── Models/
│   │   ├── ActivityModel.cs
│   │   ├── FileModel.cs
│   │   ├── KeyStorageItemModel.cs
│   │   ├── KeyStorageModel.cs
│   │   ├── LinkModel.cs
│   │   ├── MimeModel.cs
│   │   ├── NotificationModel.cs
│   │   ├── OfferModel.cs
│   │   ├── TokenModel.cs
│   │   └── UserModel.cs
│   └── Specifications/
│       ├── RecoveryTokenByTokenSpec.cs
│       ├── RefreshTokenByTokenSpec.cs
│       ├── UserByEmailSpec.cs
│       ├── UsersByUsernameSpec.cs
│       ├── By Id And Relation Specifications/
│       │   ├── ActivityByIdAndRelationSpec.cs
│       │   ├── FileByIdAndRelationSpec.cs
│       │   ├── NotificationByIdAndByRelationSpec.cs
│       │   ├── OfferByIdAndRelationSpec.cs
│       │   ├── StorageByIdAndRelationSpec.cs
│       │   └── StorageKeyByIdAndRelationSpec.cs
│       ├── By Relation Specifications/
│       │   ├── RefreshTokenByTokenAndExpiresSpec.cs
│       │   ├── RefreshTokensByRelationSpec.cs
│       │   ├── StorageKeysByRelationSpec.cs
│       │   └── StoragesByRelationSpec.cs
│       └── Sorting Specifications/
│           ├── ActivitySortSpec.cs
│           ├── FilesSortSpec.cs
│           ├── LinksSortSpec.cs
│           ├── MimesSortSpec.cs
│           ├── NotificationsSortSpec.cs
│           ├── OffersSortSpec.cs
│           ├── StorageItemsSortSpec.cs
│           └── StoragesSortSpec.cs
├── services/
│   ├── ServiceRegistration.cs
│   ├── services.csproj
│   ├── ClamAv/
│   │   └── ClamAV.cs
│   ├── Cryptography/
│   │   ├── AesCreator.cs
│   │   ├── Cypher.cs
│   │   ├── CypherKey.cs
│   │   ├── Abstractions/
│   │   │   └── IAes.cs
│   │   └── Helpers/
│   │       ├── FileManager.cs
│   │       └── Security/
│   │           ├── Generate.cs
│   │           └── HashUtility.cs
│   ├── S3/
│   │   ├── S3Client.cs
│   │   ├── S3ClientDTO.cs
│   │   ├── S3Manager.cs
│   │   └── Abstractions/
│   │       └── IS3ClientProvider.cs
│   └── Sender/
│       └── EmailSender.cs
├── tests/
│   ├── FakeLogger.cs
│   ├── tests.csproj
│   ├── Api/
│   │   └── UserInfoTest.cs
│   ├── Data Access/
│   │   └── RepositoryTest.cs
│   └── Services/
│       └── ClamAvTest.cs
└── webapi/
    ├── appsettings.Development.json
    ├── appsettings.json
    ├── Dockerfile
    ├── Program.cs
    ├── Startup.cs
    ├── webapi.csproj
    ├── webapi.csproj.user
    ├── Controllers/
    │   ├── Account/
    │   │   ├── Auth/
    │   │   │   ├── RecoveryController.cs
    │   │   │   ├── RegisterController.cs
    │   │   │   └── SessionController.cs
    │   │   └── Edit/
    │   │       ├── AvatarController.cs
    │   │       ├── EmailController.cs
    │   │       ├── PasswordController.cs
    │   │       ├── TwoFaController.cs
    │   │       └── UsernameController.cs
    │   ├── Admin/
    │   │   ├── _FileController.cs
    │   │   ├── _LinkController.cs
    │   │   ├── _MimeController.cs
    │   │   ├── _NotificationController.cs
    │   │   ├── _OfferController.cs
    │   │   ├── _TokenController.cs
    │   │   └── _UserController.cs
    │   └── Core/
    │       ├── ActivityController.cs
    │       ├── FileController.cs
    │       ├── KeyStorageController.cs
    │       ├── NotificationController.cs
    │       ├── OfferController.cs
    │       └── UserController.cs
    ├── Exceptions/
    │   └── InvalidConfigurationException.cs
    ├── Helpers/
    │   ├── UserData.cs
    │   └── Abstractions/
    │       └── IUserInfo.cs
    ├── Middlewares/
    │   ├── BearerMiddleware.cs
    │   ├── ExceptionHandleMiddleware.cs
    │   ├── LogMiddleware.cs
    │   ├── UserSessionMiddleware.cs
    │   └── XSRF Protection Middleware.cs
    └── Startup Extensions/
        ├── AppConfigurationCheck.cs
        └── AppServices.cs
