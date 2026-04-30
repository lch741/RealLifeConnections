using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Interfaces;

namespace backend.Service
{
    public class MockAzureFaceService : IAzureFaceService
{
    private readonly IConfiguration _configuration;

    public MockAzureFaceService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<AzureFaceVerificationResultDto> VerifyFacesAsync(string avatarUrl, string liveCaptureUrl)
    {
        // 开发环境：固定通过，生产环境：调真实 Azure
        var environment = _configuration["ASPNETCORE_ENVIRONMENT"];
        
        if (environment == "Development")
        {
            await Task.Delay(500); // 模拟延迟
            return new AzureFaceVerificationResultDto
            {
                IsIdentical = true,
                Confidence = 0.95,
                Provider = "MockAzureFace"
            };
        }

        // 生产走真实实现
        throw new NotImplementedException("Use real AzureFaceService in production");
    }

    public Task<AzureFaceVerificationResultDto> VerifyFacesAsync(string avatarUrl, byte[] liveCaptureImageBytes)
    {
        return VerifyFacesAsync(avatarUrl, "live-capture-upload");
    }

    public Task<AzureFaceVerificationResultDto> VerifyFacesAsync(byte[] avatarImageBytes, byte[] liveCaptureImageBytes)
    {
        return VerifyFacesAsync("avatar-upload", "live-capture-upload");
    }
}   
}