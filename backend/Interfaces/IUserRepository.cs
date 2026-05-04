using backend.Models;
using backend.DTO.Matching;
using backend.DTOs;
using backend.Helper;

namespace backend.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UserNameExistsAsync(string userName);
        Task<AppUser?> GetUserByEmailAsync(string email);
        Task<AppUser?> GetUserByIdAsync(int userId);
        Task<List<InterestCategory>> GetInterestCategoriesAsync();
        Task<AppUser> CreateUserAsync(AppUser user, IEnumerable<RegisterInterestSelectionDto> interests);
        Task<AppUser> UpdateProfileAsync(AppUser user, UpdateProfileDto dto);
        Task<AppUser> SaveAvatarUrlAsync(AppUser user, string avatarUrl);
        Task<Verification> AddVerificationAsync(Verification verification);
        Task<List<AppUser>> GetVerifiedMatchCandidatesAsync(int userId);
        Task<List<AppUser>> SearchCandidatesAsync(int userId, UserQueryObject queryObject);
    }
}
