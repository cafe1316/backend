using Cafe1316.Application.Interfaces;
using Cafe1316.Application.DTOs;
using Cafe1316.Application.Mappings;
using Cafe1316.Domain.Entities;
using Google.Apis.Auth;

namespace Cafe1316.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(string idToken)
    {
        // 1. 验证 Google ID Token
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

        // 2. 查找或创建用户
        var user = await _userRepository.GetByEmailAsync(payload.Email);
        if (user == null)
        {
            user = new User
            {
                Email = payload.Email,
                DisplayName = payload.Name,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName
            };

            // 如果 Google 提供了头像，创建 Profile
            if (!string.IsNullOrEmpty(payload.Picture))
            {
                user.Profile = new UserProfile
                {
                    AvatarUrl = payload.Picture
                };
            }

            user = await _userRepository.AddAsync(user);
        }
        else
        {
            // 4. 用户已存在，更新头像
            if (!string.IsNullOrEmpty(payload.Picture))
            {
                if (user.Profile == null)
                {
                    user.Profile = new UserProfile { UserId = user.Id };
                }
                user.Profile.AvatarUrl = payload.Picture;
                await _userRepository.UpdateAsync(user);
            }
        }

        // 5. 生成我们的 JWT Token
        var jwtToken = _jwtService.GenerateToken(user);

        // 6. 返回AuthResponseDto
        return new AuthResponseDto
        {
            Token = jwtToken,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.DisplayName ?? user.Email,
                Email = user.Email,
                AvatarUrl = user.Profile?.AvatarUrl
            }
        };
    }

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
    {
        // 以后实现
        throw new NotImplementedException();
    }
}