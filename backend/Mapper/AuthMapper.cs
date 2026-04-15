using backend.DTOs;
using api.Models;

namespace backend.Mapper
{
    public static class AuthMapper
    {
        public static AuthResponseDto ToAuthResponse(
            AppUser user,
            List<InterestCategory> categories,
            string message,
            string token)
        {
            return new AuthResponseDto
            {
                Message = message,
                Token = token,
                User = ToAuthUser(user, categories)
            };
        }

        public static AuthUserDto ToAuthUser(AppUser user, List<InterestCategory> categories)
        {
            return new AuthUserDto
            {
                Email = user.Email,
                UserName = user.UserName,
                Bio = user.Bio,
                City = user.City,
                ProfileImageUrl = user.ProfileImageUrl,
                InterestSelections = InterestMapper.ToInterestResults(user.Interests, categories)
            };
        }
    }
}