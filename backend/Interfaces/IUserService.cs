using System.Security.Claims;
using backend.DTO.Matching;
using backend.DTOs;
using backend.Helper;

namespace backend.Interfaces
{
    public interface IUserService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto);
        Task<AuthResponseDto> LoginAsync(LoginUserDto dto);
        Task<UserProfileDto> GetProfileAsync(ClaimsPrincipal principal);
        Task<UserProfileDto> UpdateProfileAsync(ClaimsPrincipal principal, UpdateProfileDto dto);
        Task<UserProfileDto> SaveAvatarAsync(ClaimsPrincipal principal, SaveAvatarDto dto);
        Task<FaceVerificationResponseDto> VerifyFaceAsync(ClaimsPrincipal principal, FaceVerificationRequestDto dto);
        Task<List<MatchCandidateDto>> GetMatchesAsync(ClaimsPrincipal principal);
        Task<List<SearchingCandidateDto>> SearchMatchesAsync(ClaimsPrincipal principal, UserQueryObject queryObject);
    }
}
