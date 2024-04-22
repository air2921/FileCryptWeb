using data_access.Ef;
using domain.Exceptions;
using domain.Models;
using domain.Specifications.By_Relation_Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace tests.Data_Access
{
    public class RepositoryTest
    {
        [Fact]
        public async Task GetAll_ReturnsAllEntities()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

            using var context = new FileCryptDbContext(options);
            var repository = new Repository<KeyStorageModel>(new Logger<Repository<KeyStorageModel>>(new LoggerFactory()), context);

            var entities = new List<KeyStorageModel>
            {
                new KeyStorageModel { storage_name = "Air_Storage", last_time_modified = DateTime.UtcNow,
                    access_code = "access_code_1", user_id = 1 },
                new KeyStorageModel { storage_name = "Zanfery_Storage", last_time_modified = DateTime.UtcNow.AddDays(-1),
                    access_code = "access_code_2", user_id = 2 },
                new KeyStorageModel { storage_name = "baby_mary_Storage", last_time_modified = DateTime.UtcNow.AddDays(-2),
                    access_code = "access_code_3", user_id = 3 }
            };

            await repository.AddRange(entities);

            var result = await repository.GetAll();

            Assert.Equal(entities.Count, result.Count());
            foreach (var entity in entities)
            {
                Assert.Contains(result, e =>
                    e.storage_id == entity.storage_id &&
                    e.storage_name == entity.storage_name &&
                    e.last_time_modified == entity.last_time_modified &&
                    e.access_code == entity.access_code &&
                    e.user_id == entity.user_id
                );
            }
        }

        [Fact]
        public async Task GetAll_WithSort_ReturnsAllMatchingEntities()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var repository = new Repository<KeyStorageModel>(new Logger<Repository<KeyStorageModel>>(new LoggerFactory()), context);

            var entities = new List<KeyStorageModel>
            {
                new KeyStorageModel { storage_name = "Air_Storage", last_time_modified = DateTime.UtcNow,
                    access_code = "access_code_1", user_id = 1 },
                new KeyStorageModel { storage_name = "Air_Storage", last_time_modified = DateTime.UtcNow,
                    access_code = "access_code_2", user_id = 1 },
                new KeyStorageModel { storage_name = "Zanfery_Storage", last_time_modified = DateTime.UtcNow.AddDays(-1),
                    access_code = "access_code_3", user_id = 2 },
                new KeyStorageModel { storage_name = "baby_mary_Storage", last_time_modified = DateTime.UtcNow.AddDays(-2),
                    access_code = "access_code_4", user_id = 3 }
            };

            await repository.AddRange(entities);

            var result = await repository.GetAll(new StoragesByRelationSpec(1));

            var entityCount = entities.Count(e => e.user_id.Equals(1));
            var resultCount = result.Count();

            Assert.Equal(entityCount, resultCount);
            foreach (var entity in result)
                Assert.Contains(result, e => e.user_id.Equals(1));
        }

        [Fact]
        public async Task GetAll_WithSort_ReturnsNoMatchingEntities()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var repository = new Repository<KeyStorageModel>(new Logger<Repository<KeyStorageModel>>(new LoggerFactory()), context);

            var entities = new List<KeyStorageModel>
            {
                new KeyStorageModel { storage_name = "Air_Storage", last_time_modified = DateTime.UtcNow,
                    access_code = "access_code_1", user_id = 1 },
                new KeyStorageModel { storage_name = "Zanfery_Storage", last_time_modified = DateTime.UtcNow.AddDays(-1),
                    access_code = "access_code_3", user_id = 2 },
                new KeyStorageModel { storage_name = "baby_mary_Storage", last_time_modified = DateTime.UtcNow.AddDays(-2),
                    access_code = "access_code_4", user_id = 3 }
            };

            await repository.AddRange(entities);

            var result = await repository.GetAll(new StoragesByRelationSpec(4));

            var entityCount = entities.Count(e => e.user_id.Equals(4));
            var resultCount = result.Count();

            Assert.Equal(entityCount, resultCount);
            Assert.Equal(new List<KeyStorageModel>() { }, result);
        }

        [Fact]
        public async Task GetAll_Throws_OperationCanceledException_OnTimeout()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(0.01)).Token;

            await Assert.ThrowsAsync<EntityException>(() => repository.GetAll(null, cancellationToken));
        }

        [Fact]
        public async Task GetAll_Throws_OperationCanceledException_OnCancellation()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<EntityException>(() => repository.GetAll(null, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task GetByFilter_ReturnsMatchingEntity()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var repository = new Repository<KeyStorageModel>(new Logger<Repository<KeyStorageModel>>(new LoggerFactory()), context);

            var entity = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 5
            };

            await repository.Add(entity);

            var result = await repository.GetByFilter(new StorageByIdAndRelationSpec(1, 5));

            Assert.NotNull(result);
            Assert.Equal(1, result.storage_id);
            Assert.Equal(5, result.user_id);
        }

        [Fact]
        public async Task GetByFilter_ReturnsNull_NoMatchingEntity()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var repository = new Repository<KeyStorageModel>(new Logger<Repository<KeyStorageModel>>(new LoggerFactory()), context);

            var entity = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await repository.Add(entity);

            var result = await repository.GetByFilter(new StorageByIdAndRelationSpec(5, 10));

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByFilter_Throws_OperationCanceledException_OnTimeout()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(0.01)).Token;

            await Assert.ThrowsAsync<EntityException>(() => repository.GetByFilter(null, cancellationToken));
        }

        [Fact]
        public async Task GetByFilter_Throws_OperationCanceledException_OnCancellation()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<EntityException>(() => repository.GetByFilter(null, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task GetById_ReturnsMatchIdEntity()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var repository = new Repository<KeyStorageModel>(new Logger<Repository<KeyStorageModel>>(new LoggerFactory()), context);

            var entity = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await repository.Add(entity);

            var result = await repository.GetById(1);

            Assert.NotNull(result);
            Assert.Equal(entity.storage_id, result.storage_id);
        }

        [Fact]
        public async Task GetById_ReturnsNull_NoMatchIdEntity()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var repository = new Repository<KeyStorageModel>(new Logger<Repository<KeyStorageModel>>(new LoggerFactory()), context);

            var entity = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await repository.Add(entity);

            var result = await repository.GetById(2);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetById_Throws_OperationCanceledException_OnTimeout()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(0.01)).Token;

            await Assert.ThrowsAsync<EntityException>(() => repository.GetById(1, cancellationToken));
        }

        [Fact]
        public async Task GetById_Throws_OperationCanceledException_OnCancellation()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<EntityException>(() => repository.GetById(1, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task AddEntity_SuccessAdded()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var repository = new Repository<KeyStorageModel>(new Logger<Repository<KeyStorageModel>>(new LoggerFactory()), context);

            var storageModel = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await repository.Add(storageModel);

            var result = await repository.GetById(1);

            Assert.NotNull(result);
            Assert.Equal(storageModel.storage_id, result.storage_id);
        }

        [Fact]
        public async Task AddEntity_Throws_OperationCanceledException_OnTimeout()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(0.01)).Token;

            var storageModel = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await Assert.ThrowsAsync<EntityException>(() => repository.Add(storageModel, null, cancellationToken));
        }

        [Fact]
        public async Task AddEntity_Throws_OperationCanceledException_OnCancellation()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var storageModel = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await Assert.ThrowsAsync<EntityException>(() => repository.Add(storageModel, null, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task AddRangeEntities_SuccessAdded()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var repository = new Repository<KeyStorageModel>(new Logger<Repository<KeyStorageModel>>(new LoggerFactory()), context);

            var entities = new List<KeyStorageModel>
            {
                new KeyStorageModel { storage_name = "Air_Storage", last_time_modified = DateTime.UtcNow,
                    access_code = "access_code_1", user_id = 1 },
                new KeyStorageModel { storage_name = "Zanfery_Storage", last_time_modified = DateTime.UtcNow.AddDays(-1),
                    access_code = "access_code_2", user_id = 2 },
                new KeyStorageModel { storage_name = "baby_mary_Storage", last_time_modified = DateTime.UtcNow.AddDays(-2),
                    access_code = "access_code_3", user_id = 3 }
            };

            await repository.AddRange(entities);

            var result = await repository.GetAll();

            Assert.Equal(entities.Count, result.Count());
            foreach (var entity in entities)
            {
                Assert.Contains(result, e =>
                    e.storage_id == entity.storage_id &&
                    e.storage_name == entity.storage_name &&
                    e.last_time_modified == entity.last_time_modified &&
                    e.access_code == entity.access_code &&
                    e.user_id == entity.user_id
                );
            }
        }

        [Fact]
        public async Task AddRangeEntities_Throws_OperationCanceledException_OnTimeout()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(0.01)).Token;

            var entities = new List<KeyStorageModel>
            {
                new KeyStorageModel { storage_id = 1, storage_name = "Air_Storage", last_time_modified = DateTime.UtcNow,
                    access_code = "access_code_1", user_id = 1 },
                new KeyStorageModel { storage_id = 2, storage_name = "Zanfery_Storage", last_time_modified = DateTime.UtcNow.AddDays(-1),
                    access_code = "access_code_2", user_id = 2 },
                new KeyStorageModel { storage_id = 3,  storage_name = "baby_mary_Storage", last_time_modified = DateTime.UtcNow.AddDays(-2),
                    access_code = "access_code_3", user_id = 3 }
            };

            await Assert.ThrowsAsync<EntityException>(() => repository.AddRange(entities, cancellationToken));
        }

        [Fact]
        public async Task AddRangeEntities_Throws_OperationCanceledException_OnCancellation()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var entities = new List<KeyStorageModel>
            {
                new KeyStorageModel { storage_id = 1, storage_name = "Air_Storage", last_time_modified = DateTime.UtcNow,
                    access_code = "access_code_1", user_id = 1 },
                new KeyStorageModel { storage_id = 2, storage_name = "Zanfery_Storage", last_time_modified = DateTime.UtcNow.AddDays(-1),
                    access_code = "access_code_2", user_id = 2 },
                new KeyStorageModel { storage_id = 3,  storage_name = "baby_mary_Storage", last_time_modified = DateTime.UtcNow.AddDays(-2),
                    access_code = "access_code_3", user_id = 3 }
            };

            await Assert.ThrowsAsync<EntityException>(() => repository.AddRange(entities, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task DeleteEntity_SuccessDeleted()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var repository = new Repository<KeyStorageModel>(new Logger<Repository<KeyStorageModel>>(new LoggerFactory()), context);

            var storageModel = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await repository.Add(storageModel);
            var entity = await repository.GetById(1);

            Assert.NotNull(entity);

            await repository.Delete(1);
            var deletedEntity = await repository.GetById(1);

            Assert.Null(deletedEntity);
        }

        [Fact]
        public async Task DeleteEntity_Throws_OperationCanceledException_OnTimeout()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(0.01)).Token;

            var storageModel = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await repository.Add(storageModel);

            await Assert.ThrowsAsync<EntityException>(() => repository.Delete(1, cancellationToken));
        }

        [Fact]
        public async Task DeleteEntity_Throws_OperationCanceledException_OnCancellation()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var storageModel = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await repository.Add(storageModel);

            await Assert.ThrowsAsync<EntityException>(() => repository.Delete(1, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task DeleteRangeEntities_SuccessDeleted()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var repository = new Repository<KeyStorageModel>(new Logger<Repository<KeyStorageModel>>(new LoggerFactory()), context);

            var entities = new List<KeyStorageModel>
            {
                new KeyStorageModel { storage_id = 1, storage_name = "Air_Storage", last_time_modified = DateTime.UtcNow,
                    access_code = "access_code_1", user_id = 1 },
                new KeyStorageModel { storage_id = 2, storage_name = "Zanfery_Storage", last_time_modified = DateTime.UtcNow.AddDays(-1),
                    access_code = "access_code_2", user_id = 2 },
                new KeyStorageModel { storage_id = 3,  storage_name = "baby_mary_Storage", last_time_modified = DateTime.UtcNow.AddDays(-2),
                    access_code = "access_code_3", user_id = 3 }
            };

            await repository.AddRange(entities);

            var result = await repository.GetAll();

            Assert.Equal(entities.Count, result.Count());
            foreach (var entity in entities)
            {
                Assert.Contains(result, e =>
                    e.storage_id == entity.storage_id &&
                    e.storage_name == entity.storage_name &&
                    e.last_time_modified == entity.last_time_modified &&
                    e.access_code == entity.access_code &&
                    e.user_id == entity.user_id
                );
            }

            await repository.DeleteMany(new List<int> { 1, 2, 3 });

            var entity1 = await repository.GetById(1);
            var entity2 = await repository.GetById(2);
            var entity3 = await repository.GetById(3);

            Assert.Null(entity1);
            Assert.Null(entity2);
            Assert.Null(entity3);
        }

        [Fact]
        public async Task DeleteRangeEntities_Throws_OperationCanceledException_OnTimeout()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(0.01)).Token;

            var entities = new List<KeyStorageModel>
            {
                new KeyStorageModel { storage_id = 1, storage_name = "Air_Storage", last_time_modified = DateTime.UtcNow,
                    access_code = "access_code_1", user_id = 1 },
                new KeyStorageModel { storage_id = 2, storage_name = "Zanfery_Storage", last_time_modified = DateTime.UtcNow.AddDays(-1),
                    access_code = "access_code_2", user_id = 2 },
                new KeyStorageModel { storage_id = 3,  storage_name = "baby_mary_Storage", last_time_modified = DateTime.UtcNow.AddDays(-2),
                    access_code = "access_code_3", user_id = 3 }
            };

            await repository.AddRange(entities);

            await Assert.ThrowsAsync<EntityException>(() => repository.DeleteMany(new List<int> { 1, 2, 3 }, cancellationToken));
        }

        [Fact]
        public async Task DeleteRangeEntities_Throws_OperationCanceledException_OnCancellation()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var entities = new List<KeyStorageModel>
            {
                new KeyStorageModel { storage_id = 1, storage_name = "Air_Storage", last_time_modified = DateTime.UtcNow,
                    access_code = "access_code_1", user_id = 1 },
                new KeyStorageModel { storage_id = 2, storage_name = "Zanfery_Storage", last_time_modified = DateTime.UtcNow.AddDays(-1),
                    access_code = "access_code_2", user_id = 2 },
                new KeyStorageModel { storage_id = 3,  storage_name = "baby_mary_Storage", last_time_modified = DateTime.UtcNow.AddDays(-2),
                    access_code = "access_code_3", user_id = 3 }
            };

            await repository.AddRange(entities);

            await Assert.ThrowsAsync<EntityException>(() => repository.DeleteMany(new List<int> { 1, 2, 3 }, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task DeleteByFilter_SuccessDeleted()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var repository = new Repository<KeyStorageModel>(new Logger<Repository<KeyStorageModel>>(new LoggerFactory()), context);

            var storageModel = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 5
            };

            await repository.Add(storageModel);
            var entity = await repository.GetById(1);

            Assert.NotNull(entity);

            await repository.DeleteByFilter(new StorageByIdAndRelationSpec(1, 5));
            var deletedEntity = await repository.GetById(1);

            Assert.Null(deletedEntity);
        }

        [Fact]
        public async Task DeleteByFilter_Throws_OperationCanceledException_OnTimeout()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(0.01)).Token;

            var storageModel = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await repository.Add(storageModel);
            var entity = await repository.GetById(1);

            Assert.NotNull(entity);

            await Assert.ThrowsAsync<EntityException>(() => repository.DeleteByFilter(null, cancellationToken));
        }

        [Fact]
        public async Task DeleteByFilter_Throws_OperationCanceledException_OnCancellation()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var storageModel = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await repository.Add(storageModel);
            var entity = await repository.GetById(1);

            Assert.NotNull(entity);

            await Assert.ThrowsAsync<EntityException>(() => repository.DeleteByFilter(null, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task Update_SuccessUpdated()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var repository = new Repository<KeyStorageModel>(new Logger<Repository<KeyStorageModel>>(new LoggerFactory()), context);

            var entity = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await repository.Add(entity);

            var result = await repository.GetById(1);

            Assert.NotNull(result);
            Assert.Equal(entity.storage_id, result.storage_id);

            entity.storage_name = "Air_Storage_New_Name";
            await repository.Update(entity);

            var newResult = await repository.GetById(1);

            Assert.NotNull(newResult);

            Assert.Equal("Air_Storage_New_Name", entity.storage_name);
        }

        [Fact]
        public async Task Update_Throws_OperationCanceledException_OnTimeout()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(0.01)).Token;

            var storageModel = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await repository.Add(storageModel);
            var entity = await repository.GetById(1);

            Assert.NotNull(entity);

            await Assert.ThrowsAsync<EntityException>(() => repository.Update(entity, cancellationToken));
        }

        [Fact]
        public async Task Update_Throws_OperationCanceledException_OnCancellation()
        {
            var options = new DbContextOptionsBuilder<FileCryptDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FileCryptDbContext(options);
            var logger = new FakeLogger<Repository<KeyStorageModel>>();
            var repository = new Repository<KeyStorageModel>(logger, context);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var storageModel = new KeyStorageModel
            {
                storage_id = 1,
                storage_name = "Air_Storage",
                last_time_modified = DateTime.UtcNow,
                access_code = "access_code_1",
                user_id = 1
            };

            await repository.Add(storageModel);
            var entity = await repository.GetById(1);

            Assert.NotNull(entity);

            await Assert.ThrowsAsync<EntityException>(() => repository.Update(entity, cancellationTokenSource.Token));
        }
    }
}
