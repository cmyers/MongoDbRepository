using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDbRepository.Attributes;
using MongoDbRepository.Interfaces;
using MongoDbRepository.Models;
using System.Linq.Expressions;

namespace MongoDbRepository
{
    public class MongoDbRepository<T> : IMongoDbRepository<T> where T : MongoDbDocument
    {
        private readonly IMongoCollection<T> _collection;

        public MongoDbRepository(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<T>(collectionName);

            InitialiseCustom();
        }

        private void InitialiseCustom()
        {
            var uniqueStringIndexProperties = typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(MongoDbUnique))).ToList();

            if (uniqueStringIndexProperties.Any())
            {
                foreach (var propertyInfo in uniqueStringIndexProperties)
                {
                    var options = new CreateIndexOptions { Unique = true };
                    var propertyInfoName = propertyInfo.Name;
                    var field = new StringFieldDefinition<T>(propertyInfoName);
                    var indexDefinition = new IndexKeysDefinitionBuilder<T>().Ascending(field);
                    var indexModel = new CreateIndexModel<T>(indexDefinition, options);
                    _collection.Indexes.CreateOne(indexModel);
                }
            } 
        }

        public Task AddDocumentAsync(T document)
        {
            return _collection.InsertOneAsync(document);
        }

        public Task<DeleteResult> DeleteDocumentByIdAsync(string id)
        {
            return _collection.DeleteOneAsync(id);
        }

        public async Task<T> GetDocumentByIdAsync(string id)
        {
            var results =  await _collection.FindAsync(x => x.Id.Equals(id));
            return results.SingleOrDefault();
        }

        public Task<List<T>> GetDocumentsAsync(Expression<Func<T, bool>> linqExpression)
        {
            return _collection.AsQueryable().Where(linqExpression).ToListAsync();
        }

        public Task<T> UpdateDocumentAsync(T document)
        {
            var filter = Builders<T>.Filter.Where(x => x.Id.Equals(document.Id));
            var options = new FindOneAndReplaceOptions<T, T>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            return _collection.FindOneAndReplaceAsync(filter, document, options);
        }

        public Task<UpdateResult> UpdateDocumentPropertyByFieldAsync<J>(string filterField, string filterValue, string fieldToUpdate, J value)
        {
            var filter = Builders<T>.Filter.Eq(filterField, filterValue);
            var update = Builders<T>.Update.Set(fieldToUpdate, value);
            return _collection.UpdateOneAsync(filter, update);
        }

        public IFindFluent<T, T> GetDocumentsByField(string fieldName, string fieldValue)
        {
            var filter = Builders<T>.Filter.Eq(fieldName, fieldValue);
            return _collection.Find(filter);
        }
    }
}
