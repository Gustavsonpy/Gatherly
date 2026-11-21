using System.Text;
using API.Data;
using API.Interfaces;
using API.Interfaces.Category;
using API.Interfaces.Comment;
using API.Interfaces.Event;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using API.Interfaces.Participant;
using API.Interfaces.Report;
using API.Interfaces.Rate;
using API.Interfaces.Admin;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile(
        "appsettings.Local.json",
        optional: true,
        reloadOnChange: true
    );
}

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("JWT Key não configurada.");

if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new InvalidOperationException("JWT Issuer não configurado.");

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var fieldErrors = context.ModelState
                .Where(e => e.Value!.Errors.Count > 0)
                .ToDictionary(
                    e => char.ToLowerInvariant(e.Key[0]) + e.Key[1..],
                    e => e.Value!.Errors.Select(x => x.ErrorMessage).ToList());

            return new BadRequestObjectResult(new
            {
                errors = fieldErrors.SelectMany(f => f.Value).ToList(),
                fieldErrors
            });
        };
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

const string FrontendCorsPolicy = "FrontendCorsPolicy";

var allowedOrigins = new List<string>
{
    "http://localhost:4200"
};

var frontendOrigin = builder.Configuration["Cors:FrontendOrigin"];

if (!string.IsNullOrWhiteSpace(frontendOrigin))
{
    allowedOrigins.Add(frontendOrigin);
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IUserReportService, UserReportService>();
builder.Services.AddScoped<IUserReportRepository, UserReportRepository>();

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventService, EventService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IImageStorage, LocalImageStorage>();

builder.Services.AddScoped<IEventParticipantRepository, EventParticipantRepository>();

builder.Services.AddScoped<ReportReasonService>();

builder.Services.AddScoped<IUserRatingRepository, UserRatingRepository>();
builder.Services.AddScoped<IUserRatingService, UserRatingService>();

builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
        
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (identity is null || !Guid.TryParse(userIdClaim, out var userId))
                {
                    context.Fail("Token inválido.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var user = await db.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.IsAdmin, u.IsSuspended })
                    .FirstOrDefaultAsync();

                if (user is null || user.IsSuspended)
                {
                    context.Fail("Usuário inexistente ou suspenso.");
                    return;
                }

                foreach (var roleClaim in identity.FindAll(identity.RoleClaimType).ToList())
                    identity.RemoveClaim(roleClaim);

                if (user.IsAdmin)
                    identity.AddClaim(new Claim(identity.RoleClaimType, API.Controllers.AdminController.AdminRole));
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok("Healthy"));
app.MapGet("/health/db", async (AppDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();

    return canConnect
        ? Results.Ok("Database connected")
        : Results.Problem("Database unavailable");
});

app.MapControllers();

app.Run();