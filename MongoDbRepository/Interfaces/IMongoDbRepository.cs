using MongoDB.Driver;
using System.Linq.Expressions;

namespace MongoDbRepository.Interfaces
{
    public interface IMongoDbRepository<T>
    {
        Task AddDocumentAsync(T document);
        Task<DeleteResult> DeleteDocumentByIdAsync(string id);
        Task<T> GetDocumentByIdAsync(string id);
        Task<List<T>> GetDocumentsAsync();
        Task<List<T>> GetDocumentsAsync(Expression<Func<T, bool>> linqExpression);
        Task<T> UpdateDocumentAsync(T document);
        Task<UpdateResult> UpdateDocumentPropertyByFieldAsync<J>(string filterField, string filterValue, string fieldToUpdate, J value);
        IFindFluent<T, T> GetDocumentsByField(string fieldName, string fieldValue);
    }
}