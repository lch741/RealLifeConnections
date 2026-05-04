using backend.DTOs;
using backend.Models;
using backend.DTO.Matching;

namespace backend.Mapper
{
    public static class ProfileMapper
    {
        public static UserProfileDto ToUserProfile(AppUser user, List<InterestCategory> categories)
        {
            var latestStatus = user.Verifications
                .OrderByDescending(verification => verification.CreatedAt)
                .Select(verification => verification.Status)
                .FirstOrDefault() ?? "pending";

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Bio = user.Bio,
                Region = user.Region,
                Suburb = user.Suburb,
                AvatarUrl = user.ProfileImageUrl,
                IsVerified = user.IsVerified,
                VerificationStatus = latestStatus,
                CanMatch = user.IsVerified,
                Gender = user.Gender.ToString(),
                Age = user.Age,
                Culture = user.Culture,
                Personality = ToPersonalityDto(user),
                InterestSelections = InterestMapper.ToInterestResults(user.Interests, categories)
            };
        }

        public static AppUser ApplyProfileUpdate(AppUser user, UpdateProfileDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.UserName))
            {
                user.UserName = dto.UserName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Region))
            {
                user.Region = dto.Region.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Suburb))
            {
                user.Suburb = dto.Suburb.Trim();
            }

            user.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim();
            
            if (!string.IsNullOrWhiteSpace(dto.Gender))
            {
                if (Enum.TryParse<Gender>(dto.Gender, true, out var parsedGender))
                {
                    user.Gender = parsedGender;
                }
                else
                {
                    user.Gender = Gender.NotToTell;
                }
            }

            if (dto.Age.HasValue)
            {
                user.Age = dto.Age;
            }

            user.Culture = string.IsNullOrWhiteSpace(dto.Culture) ? null : dto.Culture.Trim();

            // Apply personality traits
            if (dto.Personality != null)
            {
                ApplyPersonalityTraits(user, dto.Personality);
            }

            // Apply preferred distance
            if (dto.PreferredDistanceKm.HasValue)
            {
                user.PreferredDistanceKm = dto.PreferredDistanceKm;
            }

            return user;
        }

        public static AppUser ApplyRegistrationData(AppUser user, RegisterUserDto dto)
        {
            user.Region = dto.Region.Trim();
            user.Suburb = dto.Suburb.Trim();
            user.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim();
            
            if (!string.IsNullOrWhiteSpace(dto.Gender))
            {
                if (Enum.TryParse<Gender>(dto.Gender, true, out var parsedGender))
                {
                    user.Gender = parsedGender;
                }
                else
                {
                    user.Gender = Gender.NotToTell;
                }
            }

            if (dto.Age.HasValue)
            {
                user.Age = dto.Age;
            }

            user.Culture = string.IsNullOrWhiteSpace(dto.Culture) ? null : dto.Culture.Trim();
            user.ProfileImageUrl = dto.ProfileImageUrl;

            // Apply personality traits
            if (dto.Personality != null)
            {
                ApplyPersonalityTraits(user, dto.Personality);
            }

            // Apply preferred distance
            if (dto.PreferredDistanceKm.HasValue)
            {
                user.PreferredDistanceKm = dto.PreferredDistanceKm;
            }

            return user;
        }

        public static PersonalityDto ToPersonalityDto(AppUser user)
        {
            return new PersonalityDto
            {
                ChillToEnergetic = user.ChillToEnergetic,
                TalkativeToQuiet = user.TalkativeToQuiet,
                PlannerToSpontaneous = user.PlannerToSpontaneous,
                IntrovertToExtrovert = user.IntrovertToExtrovert,
                PreferredDaysOfWeek = user.PreferredDaysOfWeek,
                PreferredTimeOfDay = user.PreferredTimeOfDay,
                PreferredDistanceKm = user.PreferredDistanceKm
            };
        }

        public static void ApplyPersonalityTraits(AppUser user, PersonalityDto dto)
        {
            if (dto.ChillToEnergetic.HasValue)
            {
                user.ChillToEnergetic = dto.ChillToEnergetic;
            }

            if (dto.TalkativeToQuiet.HasValue)
            {
                user.TalkativeToQuiet = dto.TalkativeToQuiet;
            }

            if (dto.PlannerToSpontaneous.HasValue)
            {
                user.PlannerToSpontaneous = dto.PlannerToSpontaneous;
            }

            if (dto.IntrovertToExtrovert.HasValue)
            {
                user.IntrovertToExtrovert = dto.IntrovertToExtrovert;
            }

            if (!string.IsNullOrWhiteSpace(dto.PreferredDaysOfWeek))
            {
                user.PreferredDaysOfWeek = dto.PreferredDaysOfWeek.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.PreferredTimeOfDay))
            {
                user.PreferredTimeOfDay = dto.PreferredTimeOfDay.Trim();
            }
        }
    }
}