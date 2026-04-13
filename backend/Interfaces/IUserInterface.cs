using api.Models;
using backend.DTOs;

namespace backend.Interfaces
{
    public interface IUserInterface
    {
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UserNameExistsAsync(string userName);
        Task<AppUser?> GetUserByEmailAsync(string email);
        Task<List<InterestCategory>> GetInterestCategoriesAsync();
        Task<AppUser> CreateUserAsync(AppUser user, IEnumerable<RegisterInterestSelectionDto> interests);
    }
}
