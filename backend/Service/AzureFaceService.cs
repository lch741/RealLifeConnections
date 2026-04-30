using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using backend.DTOs;
using backend.Interfaces;

namespace backend.Service
{
    public class AzureFaceService : IAzureFaceService
    {
        private const string JsonMediaType = "application/json";
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

            var avatarFaceId = await DetectFaceIdByUrlAsync(endpoint, apiKey, avatarUrl);
            var liveFaceId = await DetectFaceIdByUrlAsync(endpoint, apiKey, liveCaptureUrl);

            return await VerifyByFaceIdsAsync(endpoint, apiKey, avatarFaceId, liveFaceId);
        }

        public async Task<AzureFaceVerificationResultDto> VerifyFacesAsync(string avatarUrl, byte[] liveCaptureImageBytes)
        {
            var endpoint = _configuration["AzureFace:Endpoint"]
                ?? throw new InvalidOperationException("AzureFace:Endpoint is missing.");
            var apiKey = _configuration["AzureFace:ApiKey"]
                ?? throw new InvalidOperationException("AzureFace:ApiKey is missing.");

            var avatarFaceId = await DetectFaceIdByUrlAsync(endpoint, apiKey, avatarUrl);
            var liveFaceId = await DetectFaceIdByBytesAsync(endpoint, apiKey, liveCaptureImageBytes);

            return await VerifyByFaceIdsAsync(endpoint, apiKey, avatarFaceId, liveFaceId);
        }

        public async Task<AzureFaceVerificationResultDto> VerifyFacesAsync(byte[] avatarImageBytes, byte[] liveCaptureImageBytes)
        {
            var endpoint = _configuration["AzureFace:Endpoint"]
                ?? throw new InvalidOperationException("AzureFace:Endpoint is missing.");
            var apiKey = _configuration["AzureFace:ApiKey"]
                ?? throw new InvalidOperationException("AzureFace:ApiKey is missing.");

            var avatarFaceId = await DetectFaceIdByBytesAsync(endpoint, apiKey, avatarImageBytes);
            var liveFaceId = await DetectFaceIdByBytesAsync(endpoint, apiKey, liveCaptureImageBytes);

            return await VerifyByFaceIdsAsync(endpoint, apiKey, avatarFaceId, liveFaceId);
        }

        private async Task<AzureFaceVerificationResultDto> VerifyByFaceIdsAsync(
            string endpoint,
            string apiKey,
            string avatarFaceId,
            string liveFaceId)
        {

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{endpoint.TrimEnd('/')}/face/v1.0/verify");

            request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));
            request.Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    faceId1 = avatarFaceId,
                    faceId2 = liveFaceId
                }),
                Encoding.UTF8,
                JsonMediaType);

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

        private async Task<string> DetectFaceIdByUrlAsync(string endpoint, string apiKey, string imageUrl)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{endpoint.TrimEnd('/')}/face/v1.2/detect?returnFaceId=true&recognitionModel=recognition_04&detectionModel=detection_03");

            request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));
            request.Content = new StringContent(
                JsonSerializer.Serialize(new { url = imageUrl }),
                Encoding.UTF8,
                JsonMediaType);

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

        private async Task<string> DetectFaceIdByBytesAsync(string endpoint, string apiKey, byte[] imageBytes)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{endpoint.TrimEnd('/')}/face/v1.2/detect?returnFaceId=true&recognitionModel=recognition_04&detectionModel=detection_03");

            request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));
            request.Content = new ByteArrayContent(imageBytes);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

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
