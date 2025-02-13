using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class ReCaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ReCaptchaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<bool> VerifyTokenAsync(string token)
    {
        var secretKey = _configuration["GoogleReCaptcha:SecretKey"];
        var response = await _httpClient.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}", null);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RecaptchaResponse>(json);
        return result.Success && result.Score >= 0.5; // Adjust threshold if needed
    }
}

public class RecaptchaResponse
{
    public bool Success { get; set; }
    public double Score { get; set; }
}
