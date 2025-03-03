using Lungora.Dtos.AuthDtos;
using Lungora.Models;
using System.IdentityModel.Tokens.Jwt;

namespace Lungora.Bl.Interfaces
{
    public interface IUserService 
    {
        Task<AuthModel> CreateUserAsync(RegisterDTO registerDTO);
        Task<AuthModel> GetTokenAsync(LoginDTO model);
        Task<bool> DeleteUserAsync(string userId);
        Task<string> AddRoleAsync(AddRoleDto model);
        Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user);
        Task<AuthModel> RefreshTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);

    }
}
