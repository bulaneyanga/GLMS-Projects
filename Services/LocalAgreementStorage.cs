namespace GLMS.Services;

public sealed class LocalAgreementStorage(IWebHostEnvironment environment, IConfiguration configuration) : IAgreementStorage
{
    private readonly string _root = Path.GetFullPath(Path.Combine(environment.ContentRootPath,
        configuration["Storage:LocalPath"] ?? "App_Data/agreements"));

    public async Task<string> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_root);
        var key = $"{Guid.NewGuid():N}.pdf";
        await using var output = File.Create(Path.Combine(_root, key));
        await content.CopyToAsync(output, cancellationToken);
        return key;
    }

    public Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        if (Path.GetFileName(key) != key) throw new InvalidDataException("Invalid agreement key.");
        Stream stream = File.OpenRead(Path.Combine(_root, key));
        return Task.FromResult(stream);
    }
}
