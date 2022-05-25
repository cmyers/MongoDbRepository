﻿using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace MongoDbRepository.Interfaces
{
    public interface IMongoDbRepository<T>
    {
        Task AddDocumentAsync(T document, CancellationToken cancellationToken = default(CancellationToken));
        Task<T> DeleteDocumentAsync(ObjectId id, CancellationToken cancellationToken = default(CancellationToken));
        Task<DeleteResult> DeleteDocumentsAsync(Expression<Func<T, bool>> linqExpression, CancellationToken cancellationToken = default(CancellationToken));
        Task<T> GetDocumentByIdAsync(ObjectId id, CancellationToken cancellationToken = default(CancellationToken));
        IMongoQueryable<T> GetDocuments();
        IMongoQueryable<T> GetDocuments(Expression<Func<T, bool>> linqExpression);
        Task<T> UpdateDocumentAsync(T document, CancellationToken cancellationToken = default(CancellationToken));
        Task<T> UpdateDocumentFieldAsync<I>(ObjectId id, string fieldToUpdate, I value, CancellationToken cancellationToken = default(CancellationToken));
        Task<IAsyncCursor<T>> GetDocumentsByFieldAsync<I>(string fieldName, I fieldValue, CancellationToken cancellationToken = default(CancellationToken));
    }
}