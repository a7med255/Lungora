using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Lungora.Bl.Interfaces;
using Microsoft.EntityFrameworkCore;
using Lungora.Dtos.AuthDtos;
using Lungora.Models;
using Lungora.JWT;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics.SymbolStore;
using Lungora.Bl;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Lungora.Bl.Repositories;
using System.Net;

namespace Lungora.Services
{
    public class UserService : Repository<ApplicationUser>  ,IUserService
    {
        private readonly LungoraContext context;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;
        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
              IConfiguration configuration, LungoraContext context) : base(context)
        {
            this.context = context;
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.configuration = configuration;

        }
        public async Task<AuthModel> CreateUserAsync(RegisterDTO model)
        {
            if (await userManager.FindByEmailAsync(model.Email) is not null)
            {
                return new AuthModel
                {
                    Message = "Email is already registerd!"
                };
            }


            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.Email,
                Name=model.Name,
                ImageUser= "https://res.cloudinary.com/deoayl2hl/image/upload/v1742340954/Users/f446ff10-d23b-42ed-bb90-be18f88d9f01_2025_03_19_profile_avatar_brm2oi.jpg",
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Empty;

                foreach (var error in result.Errors)
                {
                    errors += $"{error.Description},";
                }

                return new AuthModel { Message = errors };
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

             await userManager.AddToRoleAsync(user, "User");

            var jwtSecurityToken = await CreateJwtToken(user);

            var refreshToken =  GenerateRefreshToken();
            user.RefreshTokens?.Add(refreshToken);
            await userManager.UpdateAsync(user);


            return new AuthModel
            {
                Email = user.Email,
                //ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Username = user.UserName,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.ExpiresOn
            };
        }
        public async Task<AuthModel> CreateAdminAsync(RegisterDTO model)
        {

            var existingUser = await userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return new AuthModel
                {
                    Message = "This email is already registered."
                };
            }

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                Name = model.Name,
                ImageUser = "https://res.cloudinary.com/deoayl2hl/image/upload/v1742340954/Users/f446ff10-d23b-42ed-bb90-be18f88d9f01_2025_03_19_profile_avatar_brm2oi.jpg"
            };

            var creationResult = await userManager.CreateAsync(user, model.Password);
            if (!creationResult.Succeeded)
            {
                var errorMessages = string.Join(", ", creationResult.Errors.Select(e => e.Description));
                return new AuthModel { Message = errorMessages };
            }

            var roleExists = await roleManager.RoleExistsAsync("Admin");
            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            await userManager.AddToRoleAsync(user, "Admin");

            return new AuthModel
            {
                Email = user.Email,
                IsAuthenticated = true,
                Roles = new List<string> { "Admin" },
                Username = user.UserName,
                Message = "Admin user created successfully."
            };
        }

        public async Task<List<GetAllUsers>> GetAllUsersAsync()
        {
            var users = await context.Users
                .ToListAsync();

            var result = new List<GetAllUsers>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user); // Get list of role names

                result.Add(new GetAllUsers
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.Name,
                    ImageUser = user.ImageUser,
                    CreatedDate = user.CreatedDate,
                    IsActive = !(user.IsDeleted),
                    Roles = roles.ToList()
                });
            }

            return result;
        }
        //name-email-Active-date-rolement

        public async Task<ApplicationUser?> UpdateUserAsync(string userId,string imageUrl , EditUser updatedData)
        {
            var existingUser = await userManager.FindByIdAsync(userId);
            if (existingUser == null)
            {
                return null;
            }

            // تحديث الحقول
            existingUser.Name = updatedData.FullName ?? existingUser.Name;
            existingUser.Email = updatedData.Email ?? existingUser.Email;
            existingUser.UserName = updatedData.Email ?? existingUser.UserName;
            existingUser.ImageUser = imageUrl;
            existingUser.IsDeleted = !(updatedData.IsActive)??existingUser.IsDeleted;

            var updateResult = await userManager.UpdateAsync(existingUser);
            if (!updateResult.Succeeded)
            {
                return null;
            }

            var currentRoles = await userManager.GetRolesAsync(existingUser);
            var removeRolesResult = await userManager.RemoveFromRolesAsync(existingUser, currentRoles);
            var addRolesResult = await userManager.AddToRolesAsync(existingUser, updatedData.Roles);

            if (!addRolesResult.Succeeded)
            {
                return null;
            }


           return existingUser;
        }


        public async Task<AuthModel> GetTokenAsync(LoginDTO model)
        {
            var authModel = new AuthModel();

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user is null || !await userManager.CheckPasswordAsync(user, model.Password) || user.IsDeleted==true)
            {
                authModel.Message = "Email or Password is incorrect!";
                return authModel;
            }

            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Email = user.Email;
            authModel.Username = user.UserName;
            //authModel.ExpiresOn = jwtSecurityToken.ValidTo;
            authModel.Roles = rolesList.ToList();

            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(a => a.IsActive && a.ExpiresOn > DateTime.Now);

            if (activeRefreshToken != null)
            {
                authModel.RefreshToken = activeRefreshToken.Token;
                authModel.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
            }
            else
            {
                var refreshToken = GenerateRefreshToken();
                authModel.RefreshToken = refreshToken.Token;
                authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;

                user.RefreshTokens.Add(refreshToken);
                await userManager.UpdateAsync(user);
            }

            return authModel;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsDeleted = true; // Soft Delete
            context.Users.Update(user);
            await context.SaveChangesAsync();
            return true;
        }   

        public async Task<ApplicationUser> FindUserAsync(string email, string password)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user is null || !await userManager.CheckPasswordAsync(user, password))
                return null;

            return user;
        }

        public async Task<string> AddRoleAsync(AddRoleDto model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);

            if (user is null || !await roleManager.RoleExistsAsync(model.Role))
                return "Invalid User ID or Role";

            if (await userManager.IsInRoleAsync(user, model.Role))
                return "User already assigned to this role";

            var result = await userManager.AddToRoleAsync(user, model.Role);

            if (result.Succeeded)
                return string.Empty;

            return "Something went wrong";
        }
        public async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await userManager.GetClaimsAsync(user);
            var roles = await userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("TokenVersion", user.TokenVersion.ToString()),
                new Claim("FullName", user.Name.ToString()),
            }.Union(userClaims)
            .Union(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:Issuer"],
                audience: configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(configuration["JWT:DurationInMinutes"])),
                signingCredentials: creds
            );
            return token;

        }
        public async Task<AuthModel> RefreshTokenAsync(string token)
        {
            var authModel = new AuthModel();

            var user = await userManager.Users.SingleOrDefaultAsync(a => a.RefreshTokens.Any(t => t.Token == token));


            if (user == null)
            {
                authModel.Message = "Invaild Token";
                return authModel;
            }

            var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == token);

            if (refreshToken == null || !refreshToken.IsActive || refreshToken.ExpiresOn <= DateTime.Now)
            {
                authModel.Message = "Invalid or Expired Token";
                return authModel;
            }

            refreshToken.RevokedOn = DateTime.Now;

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);

            var userId = await userManager.FindByIdAsync(user.Id);
            await userManager.UpdateAsync(userId);
            var newJwt = await CreateJwtToken(userId);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(newJwt);
            authModel.Email = user.Email;
            authModel.Username = user.Email;

            var rolesList = await userManager.GetRolesAsync(userId);
            authModel.Roles = rolesList.ToList();
            authModel.RefreshToken = newRefreshToken.Token;
            authModel.RefreshTokenExpiration = newRefreshToken.ExpiresOn;

            return authModel;
        }
        public async Task<bool> RevokeTokenAsync(string token)
        {
            var user = await userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
                return false;

            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

            if (!refreshToken.IsActive)
                return false;

            refreshToken.RevokedOn = DateTime.Now;

            await userManager.UpdateAsync(user);


            return true;
        }


        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];

            using var generator = RandomNumberGenerator.Create();

            generator.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = WebEncoders.Base64UrlEncode(randomNumber),
                ExpiresOn = DateTime.Now.AddDays(7),
                CreatedOn = DateTime.Now
            };
        }


    }
}
