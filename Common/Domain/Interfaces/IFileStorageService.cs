using Common.Domain.Models.Results;
using Microsoft.AspNetCore.Http;

namespace Common.Domain.Interfaces;

public interface IFileStorageService
{
    Task<Result<string>> SaveFileAsync(
        IFormFile file, 
        string directory, 
        CancellationToken cancellationToken = default);

    Task<Result> DeleteFileAsync(
        string filePath, 
        CancellationToken cancellationToken = default);
}