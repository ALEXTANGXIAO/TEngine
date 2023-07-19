#if TENGINE_NET
using System.Linq.Expressions;
using TEngine.Core;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TEngine.Core.DataBase;

public sealed class MongoDataBase : IDateBase
{
    private string _dbName;
    private string _connectionString;
    private MongoClient _mongoClient;
    private IMongoDatabase _mongoDatabase;
    private readonly HashSet<string> _collections = new HashSet<string>();
    private readonly CoroutineLockQueueType _mongoDataBaseLock = new CoroutineLockQueueType("MongoDataBaseLock");

    public IDateBase Initialize(string connectionString, string dbName)
    {
        _dbName = dbName;
        _connectionString = connectionString;
        _mongoClient = new MongoClient(connectionString);
        _mongoDatabase = _mongoClient.GetDatabase(dbName);
        // 记录所有集合名
        _collections.UnionWith(_mongoDatabase.ListCollectionNames().ToList());
        return this;
    }

    #region Other

    public async FTask<long> Sum<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> sumExpression, string collection = null) where T : Entity
    {
        var member = (MemberExpression) ((UnaryExpression) sumExpression.Body).Operand;

        var projection =
            new BsonDocument("_id", "null").Add("Result", new BsonDocument("$sum", $"${member.Member.Name}"));

        var data = await GetCollection<T>(collection).Aggregate().Match(filter).Group(projection)
            .FirstOrDefaultAsync();

        return data == null ? 0 : Convert.ToInt64(data["Result"]);
    }

    #endregion

    #region GetCollection

    private IMongoCollection<T> GetCollection<T>(string collection = null)
    {
        return _mongoDatabase.GetCollection<T>(collection ?? typeof(T).Name);
    }

    private IMongoCollection<Entity> GetCollection(string name)
    {
        return _mongoDatabase.GetCollection<Entity>(name);
    }

    #endregion

    #region Count

    public async FTask<long> Count<T>(string collection = null) where T : Entity
    {
        return await GetCollection<T>(collection).CountDocumentsAsync(d => true);
    }

    public async FTask<long> Count<T>(Expression<Func<T, bool>> filter, string collection = null) where T : Entity
    {
        return await GetCollection<T>(collection).CountDocumentsAsync(filter);
    }

    #endregion

    #region Exist

    public async FTask<bool> Exist<T>(string collection = null) where T : Entity
    {
        return await Count<T>(collection) > 0;
    }

    public async FTask<bool> Exist<T>(Expression<Func<T, bool>> filter, string collection = null) where T : Entity
    {
        return await Count(filter, collection) > 0;
    }

    #endregion

    #region Query

    public async FTask<T> Query<T>(long id, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(id))
        {
            var cursor = await GetCollection<T>(collection).FindAsync(d => d.Id == id);
            var v = await cursor.FirstOrDefaultAsync();
            return v;
        }
    }

    public async FTask<(int count, List<T> dates)> QueryCountAndDatesByPage<T>(Expression<Func<T, bool>> filter, int pageIndex, int pageSize, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            var count = await Count(filter);
            var dates = await QueryByPage(filter, pageIndex, pageSize, collection);
            return ((int)count, dates);
        }
    }

    public async FTask<(int count, List<T> dates)> QueryCountAndDatesByPage<T>(Expression<Func<T, bool>> filter, int pageIndex, int pageSize, string[] cols, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            var count = await Count(filter);

            var dates = await QueryByPage(filter, pageIndex, pageSize, cols, collection);

            return ((int) count, dates);
        }
    }

    public async FTask<List<T>> QueryByPage<T>(Expression<Func<T, bool>> filter, int pageIndex, int pageSize, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            return await GetCollection<T>(collection).Find(filter).Skip((pageIndex - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }
    }

    public async FTask<List<T>> QueryByPage<T>(Expression<Func<T, bool>> filter, int pageIndex, int pageSize, string[] cols, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            var projection = Builders<T>.Projection.Include("");

            foreach (var col in cols)
            {
                projection = projection.Include(col);
            }

            return await GetCollection<T>(collection).Find(filter).Project<T>(projection)
                .Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
        }
    }

    public async FTask<List<T>> QueryByPageOrderBy<T>(Expression<Func<T, bool>> filter, int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, bool isAsc = true, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            if (isAsc)
            {
                return await GetCollection<T>(collection).Find(filter).SortBy(orderByExpression)
                    .Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
            }

            return await GetCollection<T>(collection).Find(filter).SortByDescending(orderByExpression)
                .Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
        }
    }

    public async FTask<T> First<T>(Expression<Func<T, bool>> filter, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            var cursor = await GetCollection<T>(collection).FindAsync(filter);

            return await cursor.FirstOrDefaultAsync();
        }
    }

    public async FTask<T> First<T>(string json, string[] cols, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            var projection = Builders<T>.Projection.Include("");
            
            foreach (var col in cols)
            {
                projection = projection.Include(col);
            }

            var options = new FindOptions<T, T> {Projection = projection};

            FilterDefinition<T> filterDefinition = new JsonFilterDefinition<T>(json);

            var cursor = await GetCollection<T>(collection).FindAsync(filterDefinition, options);

            return await cursor.FirstOrDefaultAsync();
        }
    }
    
    public async FTask<T> Last<T>(Expression<Func<T, bool>> filter, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            var cursor = await GetCollection<T>(collection).FindAsync(filter);

            var list = await cursor.ToListAsync();

            return list.LastOrDefault();
        }
    }

    public async FTask<List<T>> QueryOrderBy<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> orderByExpression, bool isAsc = true, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            if (isAsc)
            {
                return await GetCollection<T>(collection).Find(filter).SortBy(orderByExpression).ToListAsync();
            }

            return await GetCollection<T>(collection).Find(filter).SortByDescending(orderByExpression)
                .ToListAsync();
        }
    }

    public async FTask<List<T>> Query<T>(Expression<Func<T, bool>> filter, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            var cursor = await GetCollection<T>(collection).FindAsync(filter);
            var v = await cursor.ToListAsync();
            return v;
        }
    }

    public async FTask Query(long id, List<string> collectionNames, List<Entity> result)
    {
        using (await _mongoDataBaseLock.Lock(id))
        {
            if (collectionNames == null || collectionNames.Count == 0)
            {
                return;
            }

            foreach (var collectionName in collectionNames)
            {
                var cursor = await GetCollection(collectionName).FindAsync(d => d.Id == id);

                var e = await cursor.FirstOrDefaultAsync();

                if (e == null)
                {
                    continue;
                }

                result.Add(e);
            }
        }
    }

    public async FTask<List<T>> QueryJson<T>(string json, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            FilterDefinition<T> filterDefinition = new JsonFilterDefinition<T>(json);
            var cursor = await GetCollection<T>(collection).FindAsync(filterDefinition);
            var v = await cursor.ToListAsync();
            return v;
        }
    }

    public async FTask<List<T>> QueryJson<T>(string json, string[] cols, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            var projection = Builders<T>.Projection.Include("");
            
            foreach (var col in cols)
            {
                projection = projection.Include(col);
            }

            var options = new FindOptions<T, T> {Projection = projection};

            FilterDefinition<T> filterDefinition = new JsonFilterDefinition<T>(json);

            var cursor = await GetCollection<T>(collection).FindAsync(filterDefinition, options);
            var v = await cursor.ToListAsync();
            return v;
        }
    }

    public async FTask<List<T>> QueryJson<T>(long taskId, string json, string collection = null) where T : Entity
    {
        using (await _mongoDataBaseLock.Lock(taskId))
        {
            FilterDefinition<T> filterDefinition = new JsonFilterDefinition<T>(json);
            var cursor = await GetCollection<T>(collection).FindAsync(filterDefinition);
            var v = await cursor.ToListAsync();
            return v;
        }
    }

    public async FTask<List<T>> Query<T>(Expression<Func<T, bool>> filter, string[] cols, string collection = null) where T : class
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            var projection = Builders<T>.Projection.Include(cols[0]);

            for (var i = 1; i < cols.Length; i++)
            {
                projection = projection.Include(cols[i]);
            }

            return await GetCollection<T>(collection).Find(filter).Project<T>(projection).ToListAsync();
        }
    }

    #endregion

    #region Save

    public async FTask Save<T>(object transactionSession, T entity, string collection = null) where T : Entity
    {
        if (entity == null)
        {
            Log.Error($"save entity is null: {typeof(T).Name}");
            return;
        }

        var clone = MongoHelper.Instance.Clone(entity);
        
        using (await _mongoDataBaseLock.Lock(clone.Id))
        {
            await GetCollection(collection ?? clone.GetType().Name).ReplaceOneAsync(
                (IClientSessionHandle) transactionSession, d => d.Id == clone.Id, clone,
                new ReplaceOptions {IsUpsert = true});
        }
    }

    public async FTask Save<T>(T entity, string collection = null) where T : Entity, new()
    {
        if (entity == null)
        {
            Log.Error($"save entity is null: {typeof(T).Name}");

            return;
        }

        var clone = MongoHelper.Instance.Clone(entity);

        using (await _mongoDataBaseLock.Lock(clone.Id))
        {
            await GetCollection(collection ?? clone.GetType().Name).ReplaceOneAsync(d => d.Id == clone.Id, clone,
                new ReplaceOptions {IsUpsert = true});
        }
    }

    private async FTask SaveBase<T>(T entity, string collection = null) where T : Entity
    {
        if (entity == null)
        {
            Log.Error($"save entity is null: {typeof(T).Name}");

            return;
        }

        var clone = MongoHelper.Instance.Clone(entity);

        using (await _mongoDataBaseLock.Lock(clone.Id))
        {
            await GetCollection(collection ?? clone.GetType().Name).ReplaceOneAsync(d => d.Id == clone.Id, clone, new ReplaceOptions {IsUpsert = true});
        }
    }

    public async FTask Save(long id, List<Entity> entities)
    {
        if (entities == null)
        {
            Log.Error("save entity is null");
            return;
        }

        var clones = MongoHelper.Instance.Clone(entities);

        using (await _mongoDataBaseLock.Lock(id))
        {
            foreach (Entity clone in clones)
            {
                try
                {
                    await GetCollection(clone.GetType().Name).ReplaceOneAsync(d => d.Id == clone.Id, clone,
                        new ReplaceOptions {IsUpsert = true});
                }
                catch (Exception e)
                {
                    Log.Error($"Save List Entity Error: {clone.GetType().Name} {clone}\n{e}");
                }
            }
        }
    }

    #endregion

    #region Insert

    public FTask Insert<T>(T entity, string collection = null) where T : Entity, new()
    {
        return Save(entity);
    }

    public async FTask InsertBatch<T>(IEnumerable<T> list, string collection = null) where T : Entity, new()
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            await GetCollection<T>(collection ?? typeof(T).Name).InsertManyAsync(list);
        }
    }

    public async FTask InsertBatch<T>(object transactionSession, IEnumerable<T> list, string collection = null) where T : Entity, new()
    {
        using (await _mongoDataBaseLock.Lock(RandomHelper.RandInt64()))
        {
            await GetCollection<T>(collection ?? typeof(T).Name).InsertManyAsync((IClientSessionHandle) transactionSession, list);
        }
    }

    #endregion

    #region Remove

    public async FTask<long> Remove<T>(object transactionSession, long id, string collection = null) where T : Entity, new()
    {
        using (await _mongoDataBaseLock.Lock(id))
        {
            var result = await GetCollection<T>(collection).DeleteOneAsync((IClientSessionHandle) transactionSession, d => d.Id == id);
            return result.DeletedCount;
        }
    }

    public async FTask<long> Remove<T>(long id, string collection = null) where T : Entity, new()
    {
        using (await _mongoDataBaseLock.Lock(id))
        {
            var result = await GetCollection<T>(collection).DeleteOneAsync(d => d.Id == id);
            return result.DeletedCount;
        }
    }

    public async FTask<long> Remove<T>(long id, object transactionSession, Expression<Func<T, bool>> filter, string collection = null) where T : Entity, new()
    {
        using (await _mongoDataBaseLock.Lock(id))
        {
            var result = await GetCollection<T>(collection).DeleteManyAsync((IClientSessionHandle) transactionSession, filter);
            return result.DeletedCount;
        }
    }

    public async FTask<long> Remove<T>(long id, Expression<Func<T, bool>> filter, string collection = null) where T : Entity, new()
    {
        using (await _mongoDataBaseLock.Lock(id))
        {
            var result = await GetCollection<T>(collection).DeleteManyAsync(filter);
            return result.DeletedCount;
        }
    }

    #endregion

    #region Index

    /// <summary>
    /// 创建数据库索引
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="keys"></param>
    /// <typeparam name="T"></typeparam>
    /// <code>
    /// 使用例子(可多个):
    /// 1 : Builders.IndexKeys.Ascending(d=>d.Id)
    /// 2 : Builders.IndexKeys.Descending(d=>d.Id).Ascending(d=>d.Name)
    /// 3 : Builders.IndexKeys.Descending(d=>d.Id),Builders.IndexKeys.Descending(d=>d.Name)
    /// </code>
    public async FTask CreateIndex<T>(string collection, params object[] keys) where T : Entity
    {
        if (keys == null || keys.Length <= 0)
        {
            return;
        }
        
        var indexModels = new List<CreateIndexModel<T>>();

        foreach (object key in keys)
        {
            IndexKeysDefinition<T> indexKeysDefinition = (IndexKeysDefinition<T>) key;
            
            indexModels.Add(new CreateIndexModel<T>(indexKeysDefinition));
        }

        await GetCollection<T>(collection).Indexes.CreateManyAsync(indexModels);
    }
    
    public async FTask CreateIndex<T>(params object[] keys) where T : Entity
    {
        if (keys == null)
        {
            return;
        }

        List<CreateIndexModel<T>> indexModels = new List<CreateIndexModel<T>>();

        foreach (object key in keys)
        {
            IndexKeysDefinition<T> indexKeysDefinition = (IndexKeysDefinition<T>) key;
            
            indexModels.Add(new CreateIndexModel<T>(indexKeysDefinition));
        }

        await GetCollection<T>().Indexes.CreateManyAsync(indexModels);
    }

    #endregion

    #region CreateDB

    public async FTask CreateDB<T>() where T : Entity
    {
        // 已经存在数据库表
        string name = typeof(T).Name;

        if (_collections.Contains(name))
        {
            return;
        }
        
        await _mongoDatabase.CreateCollectionAsync(name);
        
        _collections.Add(name);
    }
   
    public async FTask CreateDB(Type type)
    {
        string name = type.Name;
        
        if (_collections.Contains(name))
        {
            return;
        }
        
        await _mongoDatabase.CreateCollectionAsync(name);

        _collections.Add(name);
    }

    #endregion
}
#endif