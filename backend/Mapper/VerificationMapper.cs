using backend.DTOs;

namespace backend.Mapper
{
    public static class VerificationMapper
    {
        public static FaceVerificationResponseDto ToFaceVerificationResponse(AzureFaceVerificationResultDto azureResult)
        {
            return new FaceVerificationResponseDto
            {
                Status = azureResult.IsIdentical ? "approved" : "rejected",
                Message = azureResult.IsIdentical
                    ? "Face verification passed. Matching is now unlocked."
                    : "Face verification failed. Matching remains locked.",
                IsVerified = azureResult.IsIdentical,
                Confidence = azureResult.Confidence
            };
        }
    }
}