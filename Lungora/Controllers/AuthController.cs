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

namespace Lungora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserService userService;
        private readonly IEmailService emailService;
        private readonly API_Resonse response;

        public AuthController(UserManager<ApplicationUser> userManager, IUserService userService,
            IEmailService emailService)
        {
            this.userManager = userManager;
            this.userService = userService;
            this.emailService = emailService;
            response = new API_Resonse();
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody]RegisterDTO registerDTO)
        {
            if(!ModelState.IsValid)
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
                Token = res.Token ,
                RefreshTokenExpiration = res.RefreshTokenExpiration,
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
                res.RefreshTokenExpiration = DateTime.Now.AddDays(refreshTokenValidity);
                SetRefreshTokenInCookie(res.RefreshToken, res.RefreshTokenExpiration);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = new {
                Token = res.Token,
                RefreshTokenExpiration = res.RefreshTokenExpiration,
            };
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
                response.IsSuccess=false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Errors.Add("An error occurred");
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
            user.PasswordResetCodeExpiry = DateTime.Now.AddMinutes(5);
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

            if (user.PasswordResetCodeExpiry < DateTime.Now)
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

            if (user.PasswordResetCodeExpiry < DateTime.Now)
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
                return BadRequest(response);
            }

            var user = await userManager.GetUserAsync(User);

            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not authenticated");
                return Unauthorized(response);
            }

            var res = await userManager.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);

            if (!res.Succeeded)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = new List<string>();
                foreach (var err in res.Errors) response.Errors.Add(err.Description);
                return BadRequest(response);
            }

            user.TokenVersion++;
            await userManager.UpdateAsync(user);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
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
                activeToken.RevokedOn = DateTime.Now;
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
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

    }
}
