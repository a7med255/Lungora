using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Lungora.Bl.Interfaces;
using Lungora.Dtos.AuthDtos;
using Lungora.Models;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Org.BouncyCastle.Asn1.Ocsp;
using Microsoft.AspNetCore.Authentication;
using Lungora.Dtos.ArticleDtos;
using Lungora.Services;
using Lungora.Bl.Repositories;

namespace Lungora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserService userService;
        private readonly IEmailService emailService;
        private readonly IImageService imageService;
        private readonly API_Resonse response;

        public AuthController(UserManager<ApplicationUser> userManager, IUserService userService,
            IEmailService emailService, IImageService imageService)
        {
            this.userManager = userManager;
            this.userService = userService;
            this.emailService = emailService;
            this.imageService = imageService;
            response = new API_Resonse();
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = new()
                {
                    "Not Validate"
                };
                return BadRequest(response);
            }
            if (registerDTO.Password != registerDTO.ConfirmPassword)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Passwords do not match");
                return BadRequest(response);
            }

            var res = await userService.CreateUserAsync(registerDTO);

            if (!res.IsAuthenticated)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add(res.Message);
                return BadRequest(response);
            }

            if (!string.IsNullOrEmpty(res.RefreshToken))
            {
                SetRefreshTokenInCookie(res.RefreshToken, res.RefreshTokenExpiration);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;
            response.Result = new {
                Token = res.Token,
                RefreshToken = res.RefreshToken,
                RefreshTokenExpiration = res.RefreshTokenExpiration,
            };
            return Ok(response);
        }
        [HttpPost("AddAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddAdmin([FromBody] RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = new()
                {
                    "Not Validate"
                };
                return BadRequest(response);
            }
            if (registerDTO.Password != registerDTO.ConfirmPassword)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Passwords do not match");
                return BadRequest(response);
            }

            var res = await userService.CreateAdminAsync(registerDTO);

            if (!res.IsAuthenticated)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add(res.Message);
                return BadRequest(response);
            }


            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;
            response.Result = new
            {
                Message = res.Message,
            };
            return Ok(response);
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {

            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = new()
                {
                    "Not Validate"
                };
                return BadRequest(response);
            }

            var res = await userService.GetTokenAsync(loginDTO);

            if (!res.IsAuthenticated)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                if (!string.IsNullOrEmpty(res.Message))
                {
                    response.Errors.Add(res.Message);
                }
                return BadRequest(response);
            }

            int refreshTokenValidity = loginDTO.RememberMe ? 30 : 7;
            if (!string.IsNullOrEmpty(res.RefreshToken) && loginDTO.RememberMe)
            {
                res.RefreshTokenExpiration = DateTime.UtcNow.AddDays(refreshTokenValidity);
                SetRefreshTokenInCookie(res.RefreshToken, res.RefreshTokenExpiration);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = new {
                Token = res.Token,
                RefreshToken = res.RefreshToken,
                RefreshTokenExpiration = res.RefreshTokenExpiration,
            };
            return Ok(response);
        }

        [HttpGet("GetAllUsersAsync")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var users = await userService.GetAllUsersAsync();

            if (users == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors = new List<string> { "No users found." };
                response.Result = string.Empty;

                return NotFound(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = new { Users = users };
            return Ok(response);
        }

        [HttpPut("EditUser/{UserId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string UserId, EditUser editUser)
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode=HttpStatusCode.BadRequest;
                response.Result = string.Empty;
                response.Errors= ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(response);
            }   

            var user = await userService.GetSingleAsync(u => u.Id == UserId);
            if (user == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors = new List<string> { "User not found." };
                response.Result = string.Empty;

                return NotFound(response);
            }

            if (!string.IsNullOrEmpty(editUser.Email))
            {
                var existingUserWithEmail = await userService.GetSingleAsync(u => u.Email == editUser.Email && u.Id != UserId);
                if (existingUserWithEmail != null)
                {
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.Conflict;
                    response.Errors = new List<string> { "This email is already in use by another user." };
                    response.Result = string.Empty;

                    return Conflict(response);
                }
            }

            string imageUrl = null;
            if (editUser.ImageUser != null && editUser.ImageUser.Length > 0)
            {
                imageUrl = await imageService.UploadOneImageAsync(editUser.ImageUser, "Users");
            }
            else
            {
                imageUrl = user.ImageUser;
            }
            user.Name = editUser.FullName ?? user.Name;
            user.Email = editUser.Email ?? user.Email;
            user.IsDeleted = (!editUser.IsActive) ?? user.IsDeleted;
            user.ImageUser = imageUrl;

            var updateResult = await userService.UpdateUserAsync(UserId,imageUrl, editUser);

            if (updateResult==null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Errors = new List<string> { "An error occurred while updating the user." };
                response.Result = string.Empty;

                return StatusCode(500, response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = new {Message = "User updated successfully." };
            return Ok(response);
        }
       
        [HttpDelete("delete")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Unauthorized access" });

                await userService.DeleteUserAsync(userId);
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = (new { message = "Account has been deactivated" });
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Errors.Add("An error occurred");
                return BadRequest(response);
            }
        }

        [HttpDelete("RemoveUser/{UserId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUser(string UserId)
        {
            try
            {
               
                await userService.RemoveAsync(a=>a.Id==UserId);
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = (new { message = "This Account Removed it" });
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess=false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Errors.Add("An error occurred");
                response.Result = string.Empty;
                return BadRequest(response);
            }
        }


        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
        {

            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            var user = await userManager.FindByEmailAsync(forgotPasswordDTO.Email);

            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("User is not found");
                return NotFound(response);
            }



            var confirmationCode = new Random().Next(1000, 9999).ToString();

            user.PasswordResetCode = confirmationCode;
            user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(5);
            await userManager.UpdateAsync(user);


            try
            {
                EmailMetadata emailMetadata = new EmailMetadata(
                    forgotPasswordDTO.Email,
                    "Password Reset Code",
                    $"Your password reset code is: {confirmationCode}"
                );

                if (emailService == null)
                {
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Errors.Add("Email service is not available");
                    return StatusCode(500, response);
                }

                await emailService.SendEmailAsync(emailMetadata);   

                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result =new { message= "Success for sending the code",expire="Expiration After 5 Min" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }
        }

        [HttpPost("VerifyResetCode")]
        public async Task<IActionResult> VerifyResetCode(VerifyResetCodeDTO verifyResetCodeDTO)
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            var user = await userManager.FindByEmailAsync(verifyResetCodeDTO.Email);

            if (user is null || user.PasswordResetCode != verifyResetCodeDTO.Code)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Invalid confirmation code");
                return BadRequest(response);
            }

            if (user.PasswordResetCodeExpiry < DateTime.UtcNow)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Time Out For Confirmation code");
                return BadRequest(response);
            }

            response.IsSuccess= true;
            response.StatusCode= HttpStatusCode.OK; 
            response.Result = new { message = "Code verified successfully" };
            return Ok(response);
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            var user = await userManager.FindByEmailAsync(resetPasswordDTO.Email);

            if (user is null || user.PasswordResetCode != resetPasswordDTO.Code)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Invalid confirmation code");
                return BadRequest(response);
            }

            if (user.PasswordResetCodeExpiry < DateTime.UtcNow)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Time Out For Confirmation code");
                return BadRequest(response);
            }

            if (resetPasswordDTO.NewPassword != resetPasswordDTO.ConfirmPassword)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Passwords do not match");
                return BadRequest(response);
            }

            var resetPassResult = await userManager.RemovePasswordAsync(user);

            if (!resetPassResult.Succeeded)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            var addPassResult = await userManager.AddPasswordAsync(user, resetPasswordDTO.NewPassword);

            if (!addPassResult.Succeeded)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }
            user.TokenVersion++;
            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiry = null;
            await userManager.UpdateAsync(user);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = new { message = "Password reset successfully" };
            return Ok(response);

        }
        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                response.Result=string.Empty;
                return BadRequest(response);
            }

            var user = await userManager.GetUserAsync(User);

            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not authenticated");
                response.Result = string.Empty;
                return Unauthorized(response);
            }

            var res = await userManager.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);

            if (!res.Succeeded)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = new List<string>();
                response.Result = string.Empty;
                foreach (var err in res.Errors) response.Errors.Add(err.Description);
                return BadRequest(response);
            }

            await userManager.UpdateAsync(user);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = new { message = "Password Changed successfully" };
            return Ok(response);
        }
        [HttpGet("GetDataUser")]
        [Authorize]
        public async Task<IActionResult> GetDataUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                response.Result = string.Empty;
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not Authenticated.");
                return Unauthorized(response);
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                response.Result = string.Empty;
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("User not found.");
                return NotFound(response);
            }

            var data = new GetDataUserDto
            {
                FullName=user.Name,
                ImageUser=user.ImageUser,
                Email = user.Email
            };
            response.Result= data;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        [HttpPost("EditInfo")]
        [Authorize]
        public async Task<IActionResult> UpdateUserInfo([FromForm] EditInfoDTO editInfoDTO)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not Authenticated.");
                response.Result = string.Empty;
                return Unauthorized(response);
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                response.Result = string.Empty;
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("User not found.");
                return NotFound(response);
            }

            string imageUrl = null;
            if (editInfoDTO.ImageUser != null && editInfoDTO.ImageUser.Length > 0)
            {
                imageUrl = await imageService.UploadOneImageAsync(editInfoDTO.ImageUser, "Users");
            }
            else
            {
                imageUrl=user.ImageUser;
            }

            if(editInfoDTO.FullName==null && editInfoDTO.ImageUser==null)
            {
                response.Result = string.Empty;
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }

            user.Name = editInfoDTO.FullName is null? user.Name: editInfoDTO.FullName;
            user.ImageUser = imageUrl;

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                response.Result = new { message = "Update User Successfully" };
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }

            response.IsSuccess = false;
            response.Result=string.Empty;
            response.StatusCode = HttpStatusCode.BadRequest;
            foreach (var error in result.Errors) response.Errors.Add(error.Description);
            return BadRequest(response);
        }

        [HttpPost("LogOutAll")]
        [Authorize]
        public async Task<IActionResult> LogOutAll()
        {
            var user = await userManager.GetUserAsync(User);

            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not authenticated");
                return Unauthorized(response);
            }

            user.TokenVersion++; // invalidate tokenVersion

            var res = await userManager.UpdateAsync(user);

            if (res.Succeeded)
            {
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = new { Message = "User logged out All devices successfully" };
                return Ok(response);
            }

            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors = new List<string>();
            foreach (var err in res.Errors) response.Errors.Add($"{err}");
            return BadRequest(response);

        }

        [HttpPost("LogOutSingle")]
        [Authorize]
        public async Task<IActionResult> LogOutSingle()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("Invalid user ID");
                return Unauthorized(response);
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User not found");
                return Unauthorized(response);
               
            }

            if (user.RefreshTokens == null || !user.RefreshTokens.Any(t => t.IsActive))
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("No active refresh tokens found");
                return BadRequest(response);
            }

            var activeToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            if (activeToken != null)
            {
                //activeToken.IsActive = false; 
                activeToken.RevokedOn = DateTime.UtcNow;
                await userManager.UpdateAsync(user);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = new { Message = "User logged out successfully" };
            return Ok(response);
        }

        [HttpPost("AddRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRole([FromBody] AddRoleDto addRoleDto)
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = new() 
                {
                    "Not Validate"
                };
                return BadRequest(response);
            }

            var res = await userService.AddRoleAsync(addRoleDto);

            if (!string.IsNullOrEmpty(res))
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add(res);
                return BadRequest(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = addRoleDto;
            return Ok(response);
        }

        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];


            if (string.IsNullOrEmpty(refreshToken))
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Refresh token is missing or invalid.");
                return BadRequest(response);
            }

            var result = await userService.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add(result.Message);
                response.Result = new
                {
                    Token = result.Token,
                    RefreshToken = result.RefreshToken,
                    RefreshTokenExpiration = result.RefreshTokenExpiration,
                };
                return BadRequest(response);
            }

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = new
            {
                Token = result.Token,
                RefreshToken = result.RefreshToken,
                RefreshTokenExpiration = result.RefreshTokenExpiration,
            };
            return Ok(response);
        }

        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto revokeTokenDto)
        {
            var token = revokeTokenDto.Token ?? Request.Cookies["refreshToken"]; //if null => request

            if (string.IsNullOrEmpty(token))
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Token is required!");
                return BadRequest(response);
            }

            var result = await userService.RevokeTokenAsync(token);

            if (!result)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Token is invalid!");
                return BadRequest(response);
            }
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = new { Message = "Token revoked successfully" };
            return Ok(response);
        }

        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken), "Refresh token cannot be null or empty.");
            }
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires,
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.None,
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

    }
}
