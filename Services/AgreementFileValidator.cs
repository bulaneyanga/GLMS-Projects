namespace GLMS.Services;

public static class AgreementFileValidator
{
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();

    public static async Task ValidatePdfAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Only PDF agreement files are allowed.");

        var header = new byte[PdfSignature.Length];
        var read = await content.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        if (read != header.Length || !header.AsSpan().SequenceEqual(PdfSignature))
            throw new InvalidDataException("The uploaded file is not a valid PDF document.");
    }
}
