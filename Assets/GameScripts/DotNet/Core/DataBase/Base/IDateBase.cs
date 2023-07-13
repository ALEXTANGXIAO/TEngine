#if TENGINE_NET
using System.Linq.Expressions;
#pragma warning disable CS8625

namespace TEngine.Core.DataBase;

public interface IDateBase
{
    IDateBase Initialize(string connectionString, string dbName);
    FTask<long> Count<T>(string collection = null) where T : Entity;
    FTask<long> Count<T>(Expression<Func<T, bool>> filter, string collection = null) where T : Entity;
    FTask<bool> Exist<T>(string collection = null) where T : Entity;
    FTask<bool> Exist<T>(Expression<Func<T, bool>> filter, string collection = null) where T : Entity;
    FTask<T> Query<T>(long id, string collection = null) where T : Entity;
    FTask<(int count, List<T> dates)> QueryCountAndDatesByPage<T>(Expression<Func<T, bool>> filter, int pageIndex, int pageSize, string collection = null) where T : Entity;
    FTask<(int count, List<T> dates)> QueryCountAndDatesByPage<T>(Expression<Func<T, bool>> filter, int pageIndex, int pageSize, string[] cols, string collection = null) where T : Entity;
    FTask<List<T>> QueryByPage<T>(Expression<Func<T, bool>> filter, int pageIndex, int pageSize, string collection = null) where T : Entity;
    FTask<List<T>> QueryByPage<T>(Expression<Func<T, bool>> filter, int pageIndex, int pageSize, string[] cols, string collection = null) where T : Entity;
    FTask<List<T>> QueryByPageOrderBy<T>(Expression<Func<T, bool>> filter, int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, bool isAsc = true, string collection = null) where T : Entity;
    FTask<T> First<T>(Expression<Func<T, bool>> filter, string collection = null) where T : Entity;
    FTask<T> First<T>(string json, string[] cols, string collection = null) where T : Entity;
    FTask<List<T>> QueryOrderBy<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> orderByExpression, bool isAsc = true, string collection = null) where T : Entity;
    FTask<List<T>> Query<T>(Expression<Func<T, bool>> filter, string collection = null) where T : Entity;
    FTask Query(long id, List<string> collectionNames, List<Entity> result);
    FTask<List<T>> QueryJson<T>(string json, string collection = null) where T : Entity;
    FTask<List<T>> QueryJson<T>(string json, string[] cols, string collection = null) where T : Entity;
    FTask<List<T>> QueryJson<T>(long taskId, string json, string collection = null) where T : Entity;
    FTask<List<T>> Query<T>(Expression<Func<T, bool>> filter, string[] cols, string collection = null) where T : class;
    FTask Save<T>(T entity, string collection = null) where T : Entity, new();
    FTask Save(long id, List<Entity> entities);
    FTask Save<T>(object transactionSession, T entity, string collection = null) where T : Entity;
    FTask Insert<T>(T entity, string collection = null) where T : Entity, new();
    FTask InsertBatch<T>(IEnumerable<T> list, string collection = null) where T : Entity, new();
    FTask InsertBatch<T>(object transactionSession, IEnumerable<T> list, string collection = null) where T : Entity, new();
    FTask<long> Remove<T>(object transactionSession, long id, string collection = null) where T : Entity, new();
    FTask<long> Remove<T>(long id, string collection = null) where T : Entity, new();
    FTask<long> Remove<T>(long id,object transactionSession, Expression<Func<T, bool>> filter, string collection = null) where T : Entity, new();
    FTask<long> Remove<T>(long id, Expression<Func<T, bool>> filter, string collection = null) where T : Entity, new();
    FTask<long> Sum<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> sumExpression, string collection = null) where T : Entity;
    FTask CreateIndex<T>(string collection, params object[] keys) where T : Entity;
    FTask CreateIndex<T>(params object[] keys) where T : Entity;
    FTask CreateDB<T>() where T : Entity;
    FTask CreateDB(Type type);
}
#endif