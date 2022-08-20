using MongoDB.Bson;
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
            var uniqueStringIndexProperties = typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(MongoDbUniqueAttribute))).ToList();

            foreach (var propertyInfo in uniqueStringIndexProperties)
            {
                var options = new CreateIndexOptions { Unique = true };
                var propertyInfoName = propertyInfo.Name;
                var field = new StringFieldDefinition<T>(propertyInfoName);
                var indexDefinition = new IndexKeysDefinitionBuilder<T>().Ascending(field);
                var indexModel = new CreateIndexModel<T>(indexDefinition, options);
                _collection.Indexes.CreateOne(indexModel);
            }

            _collection.Indexes.CreateOne(new CreateIndexModel<T>(Builders<T>.IndexKeys.Text("$**")));
        }

        public IMongoCollection<T> Collection
        {
            get { return _collection; } 
        }

        public Task AddDocumentAsync(T document, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        }

        public Task AddDocumentsAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _collection.InsertManyAsync(documents, cancellationToken: cancellationToken);
        }

        public Task<T> DeleteDocumentByIdAsync(ObjectId id, CancellationToken cancellationToken = default)
        {

            var filter = Builders<T>.Filter.Where(x => x.Id.Equals(id));
            return _collection.FindOneAndDeleteAsync(filter, cancellationToken: cancellationToken);
        }

        public Task<DeleteResult> DeleteDocumentsAsync(Expression<Func<T, bool>> linqExpression, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<T>.Filter.Where(linqExpression);
            return _collection.DeleteManyAsync(filter, cancellationToken: cancellationToken);
        }

        public async Task<T> GetDocumentByIdAsync(ObjectId id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var results = await _collection.FindAsync(x => x.Id.Equals(id), cancellationToken: cancellationToken);
            return results.SingleOrDefault();
        }

        public IMongoQueryable<T> GetDocuments()
        {
            return _collection.AsQueryable();
        }

        public Task<IAsyncCursor<T>> GetDocumentsAsync(FilterDefinition<T> filterDefinition)
        {
            return _collection.FindAsync(filterDefinition);
        }

        public Task<IAsyncCursor<T>> GetDocumentsAsync(FilterDefinition<T> filterDefinition, PipelineDefinition<T, T> pipeline)
        {
            pipeline = pipeline.Match(filterDefinition);    
            return _collection.AggregateAsync(pipeline);
          
        }

        public IMongoQueryable<T> GetDocuments(Expression<Func<T, bool>> linqExpression)
        {
            return _collection.AsQueryable().Where(linqExpression);
        }

        public Task<T> UpdateDocumentAsync(T document, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<T>.Filter.Where(x => x.Id.Equals(document.Id));
            var options = new FindOneAndReplaceOptions<T, T>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            return _collection.FindOneAndReplaceAsync(filter, document, options, cancellationToken);
        }

        public Task<UpdateResult> UpdateDocumentsFieldAsync<I>(Expression<Func<T, bool>> linqExpression, string fieldToUpdate, I value, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<T>.Filter.Where(linqExpression);
            var update = Builders<T>.Update.Set(fieldToUpdate, value);
            return _collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
        }

        public Task<T> UpdateDocumentFieldByIdAsync<I>(ObjectId id, string fieldToUpdate, I value, CancellationToken cancellationToken = default)
        {
            var filter = Builders<T>.Filter.Where(x => x.Id.Equals(id));
            var update = Builders<T>.Update.Set(fieldToUpdate, value);
            return _collection.FindOneAndUpdateAsync(filter, update, cancellationToken: cancellationToken);
        }

        public Task<IAsyncCursor<T>> GetDocumentsByFieldAsync<I>(string fieldName, I fieldValue, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filter = Builders<T>.Filter.Eq(fieldName, fieldValue);
            return _collection.FindAsync(filter, cancellationToken: cancellationToken);
        }
    }
}
