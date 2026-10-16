namespace GLMS.Services;

public interface IAgreementStorage
{
    Task<string> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default);
}
