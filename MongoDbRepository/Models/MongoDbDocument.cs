using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDbRepository.Models
{
    public abstract class MongoDbDocument
    {
        [BsonId]
        public ObjectId Id { get; init; } = ObjectId.GenerateNewId();
    }
}
