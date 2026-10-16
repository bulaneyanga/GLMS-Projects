using GLMS.Services;
using System.Net;
using System.Text;
using Xunit;

namespace GLMS.Tests;

public class CurrencyServiceTests
{
    [Theory]
    [InlineData(100, 18.5, 1850)]
    [InlineData(12.34, 18.1234, 223.64)]
    [InlineData(0, 18.5, 0)]
    public void ConvertUsdToZar_MultipliesAndRoundsToCents(decimal usd, decimal rate, decimal expected)
    {
        var service = new CurrencyService(new HttpClient());
        Assert.Equal(expected, service.ConvertUsdToZar(usd, rate));
    }

    [Fact]
    public void ConvertUsdToZar_RejectsNonPositiveRate()
    {
        var service = new CurrencyService(new HttpClient());
        Assert.Throws<ArgumentOutOfRangeException>(() => service.ConvertUsdToZar(10, 0));
    }

    [Fact]
    public void ConvertUsdToZar_RejectsNegativeRate()
    {
        var service = new CurrencyService(new HttpClient());
        Assert.Throws<ArgumentOutOfRangeException>(() => service.ConvertUsdToZar(10, -1));
    }

    [Fact]
    public void ConvertUsdToZar_RejectsNegativeAmount()
    {
        var service = new CurrencyService(new HttpClient());
        Assert.Throws<ArgumentOutOfRangeException>(() => service.ConvertUsdToZar(-1, 18.5m));
    }

    [Fact]
    public async Task GetUsdToZarRateAsync_ParsesRateFromHttpResponse()
    {
        using var http = CreateHttpClient(HttpStatusCode.OK, "{\"rates\":{\"ZAR\":18.25}}");
        var service = new CurrencyService(http);

        Assert.Equal(18.25m, await service.GetUsdToZarRateAsync());
    }

    [Fact]
    public async Task GetUsdToZarRateAsync_RejectsMalformedJson()
    {
        using var http = CreateHttpClient(HttpStatusCode.OK, "{invalid json");
        var service = new CurrencyService(http);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetUsdToZarRateAsync());
    }

    [Fact]
    public async Task GetUsdToZarRateAsync_RejectsHttpFailure()
    {
        using var http = CreateHttpClient(HttpStatusCode.ServiceUnavailable, "{}");
        var service = new CurrencyService(http);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetUsdToZarRateAsync());
    }

    private sealed class StubHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
    }

        private static HttpClient CreateHttpClient(HttpStatusCode statusCode, string body) =>
            new(new StubHandler(statusCode, body))
            {
                BaseAddress = new Uri("https://open.er-api.com/")
            };
}
