using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace GLMS.Services;

public sealed class AzureBlobAgreementStorage(IConfiguration configuration) : IAgreementStorage
{
    private readonly BlobContainerClient _container = new BlobContainerClient(
        configuration["Storage:BlobConnectionString"],
        configuration["Storage:BlobContainer"] ?? "signed-agreements");

    public async Task<string> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        await _container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        var key = $"{Guid.NewGuid():N}.pdf";
        await _container.GetBlobClient(key).UploadAsync(content, overwrite: false, cancellationToken);
        return key;
    }

    public async Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        if (Path.GetFileName(key) != key) throw new InvalidDataException("Invalid agreement key.");
        var response = await _container.GetBlobClient(key).DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }
}
