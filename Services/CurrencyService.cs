using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GLMS.Services;

public interface ICurrencyService
{
    Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default);
    decimal ConvertUsdToZar(decimal usd, decimal rate);
}

public sealed class CurrencyService(HttpClient httpClient) : ICurrencyService
{
    public async Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("v6/latest/USD", cancellationToken);
        response.EnsureSuccessStatusCode();
        ExchangeRateResponse? quote;
        try
        {
            quote = await response.Content.ReadFromJsonAsync<ExchangeRateResponse>(cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("The currency provider returned an invalid response.", ex);
        }
        if (quote?.Rates?.TryGetValue("ZAR", out var rate) != true || rate <= 0)
            throw new InvalidOperationException("The USD/ZAR exchange rate was not available.");
        return rate;
    }

    public decimal ConvertUsdToZar(decimal usd, decimal rate)
    {
        if (usd < 0) throw new ArgumentOutOfRangeException(nameof(usd));
        if (rate <= 0) throw new ArgumentOutOfRangeException(nameof(rate));
        return decimal.Round(usd * rate, 2, MidpointRounding.AwayFromZero);
    }

    private sealed class ExchangeRateResponse
    {
        [JsonPropertyName("rates")]
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}
