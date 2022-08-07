using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDbRepository.Extensions.IMongoQueryable
{
    public static class IMongoQueryableExtensions
    {
        public static IMongoQueryable<T> WhereText<T>(this IMongoQueryable<T> query, string search)
        {
            var filter = Builders<T>.Filter.Text(search);     
            return query.Where(_ => filter.Inject());
        }
    }
}
