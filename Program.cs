using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace csharp_practice27;

public class ExchangeRate
{
    [JsonPropertyName("r030")]
    public int R030 { get; set; }
    
    [JsonPropertyName("txt")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }
    
    [JsonPropertyName("cc")]
    public string Code { get; set; } = string.Empty;
    
    [JsonPropertyName("exchangedate")]
    public string ExchangeDate { get; set; } = string.Empty;
}

internal class Program
{
    private const string API_URL = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";
    
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        
        List<ExchangeRate>? rates = await LoadRatesAsync();
        if (rates == null || rates.Count == 0)
        {
            Console.WriteLine("No exchange rates found :(");
            return;
        }
        
        Console.WriteLine($"NBU rates for {rates[0].ExchangeDate}");
        Console.WriteLine("Converter from UAH to any currency");
        Console.WriteLine("To exit, enter word => EXIT");
        while (true)
        {
            Console.Write("Currency code (for example USD, EUR, PLN): ");
            string? code = Console.ReadLine()?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(code))
                continue;
            if (code == "EXIT")
                break;
            
            ExchangeRate? rate = rates.FirstOrDefault(r => r.Code == code);
            if (rate == null)
            {
                Console.WriteLine($"Currency code {code} not found. Try again");
                continue;
            }
            Console.Write("Sum in UAH: ");
            string? input = Console.ReadLine()?.Trim().Replace(',', '.');
            if (!decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount) ||amount < 0)
            {
                Console.WriteLine($"Amount {input} is not a valid non-negative amount. Try again");
                continue;
            }

            decimal result = amount / rate.Rate;
            Console.WriteLine($"{amount:N2} UAH = {result:N2} {rate.Code} ({rate.Name}), rate: {rate.Rate} UAH for 1 {rate.Code}\n");
        }
    }
    
    private static async Task<List<ExchangeRate>?> LoadRatesAsync()
    {
        using HttpClient client = new HttpClient();

        try
        {
            HttpResponseMessage response = await client.GetAsync(API_URL);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API error: {(int)response.StatusCode} {response.ReasonPhrase}");
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ExchangeRate>>(json);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"No answer from API: {ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Deserialisation error: {ex.Message}");
            return null;
        }
    }
}