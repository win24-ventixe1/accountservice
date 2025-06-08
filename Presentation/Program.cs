using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Presentation.Data;
using Presentation.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AccountSqlConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
}).AddEntityFrameworkStores<DataContext>()
  .AddDefaultTokenProviders();

// Använder "Bearer" (JWT) som inloggningsmetod.
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Kontroll rätt källa.
            ValidateAudience = true, // Kontroll rätt mottagare.
            ValidateLifetime = true, //Kontroll att token är i datum.
            ValidateIssuerSigningKey = true, // Kontroll att token är rätt signad.
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // Vem skickade token.
            ValidAudience = builder.Configuration["Jwt:Audience"], // Vem är mottagare av token.
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)) // Säkerhetsnyckel för kontroll av signeringen.
        };
    });

builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

await SeedTestUserAsync(app);

app.MapOpenApi();
app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

async Task SeedTestUserAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var email = "HansMattinLassei@domain.se";
    var password = "BytMig123!";

    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        var newUser = new IdentityUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(newUser, password);
        if (result.Succeeded)
            Console.WriteLine("Test user created.");
        else
            Console.WriteLine("Failed to create test user.");
    }
}
