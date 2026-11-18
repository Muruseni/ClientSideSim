using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
namespace LifeManagementApp.Services;
public class WeatherService(HttpClient client)
{
    public async Task<(string Summary, string Alert)> GetAsync(double latitude, double longitude)
    {
        // Open-Meteo defaults to km/h; request m/s to match the 5 m/s threshold.
        var url = FormattableString.Invariant($"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,wind_speed_10m&wind_speed_unit=ms");
        var data = await client.GetFromJsonAsync<WeatherResponse>(url);
        if (data?.Current is null || data.Current.Temperature is null || data.Current.Wind is null) throw new InvalidDataException("Missing weather data.");
        var wind = data.Current.Wind.Value;
        return (string.Format(CultureInfo.InvariantCulture, "{0:0.0} ?C  ?  Wind {1:0.0} m/s", data.Current.Temperature, wind), wind > 5 ? "?? Kitesurfing weather!" : "");
    }
    public class WeatherResponse { [JsonPropertyName("current")] public CurrentWeather? Current { get; set; } }
    public class CurrentWeather { [JsonPropertyName("temperature_2m")] public double? Temperature { get; set; } [JsonPropertyName("wind_speed_10m")] public double? Wind { get; set; } }
}
