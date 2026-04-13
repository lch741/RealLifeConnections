using backend.DTOs;

namespace backend.Interfaces
{
    public interface IAzureFaceService
    {
        Task<AzureFaceVerificationResultDto> VerifyFacesAsync(string avatarUrl, string liveCaptureUrl);
    }
}
