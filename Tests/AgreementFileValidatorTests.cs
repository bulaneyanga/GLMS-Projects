using GLMS.Services;
using Xunit;

namespace GLMS.Tests;

public class AgreementFileValidatorTests
{
    [Theory]
    [InlineData("malware.exe")]
    [InlineData("document.docx")]
    public async Task ValidatePdfAsync_RejectsRestrictedExtensions(string fileName)
    {
        await using var content = new MemoryStream("not a PDF"u8.ToArray());
        await Assert.ThrowsAsync<InvalidDataException>(() => AgreementFileValidator.ValidatePdfAsync(fileName, content));
    }

    [Fact]
    public async Task ValidatePdfAsync_AcceptsPdfSignature()
    {
        await using var content = new MemoryStream("%PDF-1.7 sample"u8.ToArray());
        await AgreementFileValidator.ValidatePdfAsync("agreement.pdf", content);
    }

    [Fact]
    public async Task ValidatePdfAsync_RejectsPdfExtensionWithInvalidSignature()
    {
        await using var content = new MemoryStream("not a PDF"u8.ToArray());
        await Assert.ThrowsAsync<InvalidDataException>(() => AgreementFileValidator.ValidatePdfAsync("agreement.pdf", content));
    }

    [Fact]
    public async Task ValidatePdfAsync_RejectsTruncatedSignature()
    {
        await using var content = new MemoryStream("%PD"u8.ToArray());
        await Assert.ThrowsAsync<InvalidDataException>(() => AgreementFileValidator.ValidatePdfAsync("agreement.pdf", content));
    }
}
