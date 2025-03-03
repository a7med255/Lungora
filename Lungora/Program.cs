using Lungora.Bl;
using Lungora.Bl.Interfaces;
using Lungora.JWT;
using Lungora.Models;
using Lungora.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LungoraContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));//to get connect sql in app.json
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
    options =>
    {
        //options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.User.RequireUniqueEmail = true;
    }).AddEntityFrameworkStores<LungoraContext>().AddDefaultTokenProviders();

builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));


builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();



// Add services to the container.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
        ClockSkew = TimeSpan.Zero,
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            // Token validation logic
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var userId = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var tokenVersionClaim = context.Principal.FindFirst("TokenVersion")?.Value;

            if (!int.TryParse(tokenVersionClaim, out var tokenVersion))
            {
                context.Fail("Invalid token version.");
                return;
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null || user.TokenVersion != tokenVersion)
            {
                context.Fail("Token is no longer valid.");
                return;
            }

            // Check if the token has been revoked (single logout)
            var tokenService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
            var jti = context.Principal.FindFirstValue(JwtRegisteredClaimNames.Jti);

            if (string.IsNullOrEmpty(jti))
            {
                context.Fail("Invalid token.");
                return;
            }

        }
    };
});
builder.Services.AddAuthorization();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Lungora",
        Version = "v1",
        Description = "API for Lungora with JWT Authentication",
        Contact = new OpenApiContact
        {
            Name = "Ahmed Mansour",
            Email = "a7med.mans25@gmail.com",
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT token in the format: Bearer YOUR_TOKEN_HERE"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

});




var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
