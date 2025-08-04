using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using Microsoft.AspNetCore.Http;

namespace Common.Application.Services;

public class FileStorageService : IFileStorageService
{
    public async Task<Result<string>> SaveFileAsync(
        IFormFile file, 
        string directory, 
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var fullPath = Path.Combine(directory, fileName);

        await using var outputStream = new FileStream(fullPath, FileMode.Create);
        await using var inputStream = file.OpenReadStream();

        if (inputStream.Length == 0)
        {
            return Result<string>.Failure("File stream is empty");
        }

        await inputStream.CopyToAsync(outputStream, cancellationToken);

        return fileName;
    }

    public async Task<Result> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return Result.Failure("File does not exist");
        }

        await Task.Run(() => File.Delete(filePath), cancellationToken);
        return Result.Success();
    }
}