using api.Models;
using backend.DTOs;

namespace backend.Interfaces
{
    public interface IUserInterface
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
    }
}
