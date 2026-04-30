using backend.DTOs;

namespace backend.Interfaces
{
    public interface IAzureFaceService
    {
        Task<AzureFaceVerificationResultDto> VerifyFacesAsync(string avatarUrl, string liveCaptureUrl);
        Task<AzureFaceVerificationResultDto> VerifyFacesAsync(string avatarUrl, byte[] liveCaptureImageBytes);
        Task<AzureFaceVerificationResultDto> VerifyFacesAsync(byte[] avatarImageBytes, byte[] liveCaptureImageBytes);
    }
}
