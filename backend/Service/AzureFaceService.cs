using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using backend.DTOs;
using backend.Interfaces;

namespace backend.Service
{
    public class AzureFaceService : IAzureFaceService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AzureFaceService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AzureFaceVerificationResultDto> VerifyFacesAsync(string avatarUrl, string liveCaptureUrl)
        {
            var endpoint = _configuration["AzureFace:Endpoint"]
                ?? throw new InvalidOperationException("AzureFace:Endpoint is missing.");
            var apiKey = _configuration["AzureFace:ApiKey"]
                ?? throw new InvalidOperationException("AzureFace:ApiKey is missing.");

            var avatarFaceId = await DetectFaceIdAsync(endpoint, apiKey, avatarUrl);
            var liveFaceId = await DetectFaceIdAsync(endpoint, apiKey, liveCaptureUrl);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{endpoint.TrimEnd('/')}/face/v1.0/verify");

            request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    faceId1 = avatarFaceId,
                    faceId2 = liveFaceId
                }),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Azure Face verify failed: {response.StatusCode} {payload}");
            }

            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            return new AzureFaceVerificationResultDto
            {
                IsIdentical = root.GetProperty("isIdentical").GetBoolean(),
                Confidence = root.TryGetProperty("confidence", out var confidence)
                    ? confidence.GetDouble()
                    : 0,
                Provider = "AzureFace"
            };
        }

        private async Task<string> DetectFaceIdAsync(string endpoint, string apiKey, string imageUrl)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{endpoint.TrimEnd('/')}/face/v1.2/detect?returnFaceId=true&recognitionModel=recognition_04&detectionModel=detection_03");

            request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(
                JsonSerializer.Serialize(new { url = imageUrl }),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Azure Face detect failed: {response.StatusCode} {payload}");
            }

            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("No face detected in the provided image.");
            }

            if (document.RootElement.GetArrayLength() > 1)
            {
                throw new InvalidOperationException("Only one face is allowed per verification image.");
            }

            return document.RootElement[0].GetProperty("faceId").GetString()
                ?? throw new InvalidOperationException("Azure Face did not return faceId.");
        }
    }
}
