using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Try_application.Database.Entities;
using Try_application.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

// ✅ Set WebRootPath explicitly
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    WebRootPath = "wwwroot",
    Args = args
});

// 1️⃣ Database connection
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2️⃣ Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();

// 3️⃣ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// 4️⃣ JWT auth
var jwtKey = builder.Configuration["JwtSettings:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    Console.WriteLine("⚠️ JWT Key is missing in config; using fallback key!");
}
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? "DefaultHardCodedKeyChangeThis"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key
    };
});

// 5️⃣ Swagger + controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Migration and missing table check
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDBContext>();
        Console.WriteLine("Applying database migrations...");
        context.Database.Migrate();
        Console.WriteLine("✅ Database migrations applied.");

        // Check if CartItems table exists; if not, create it
        var sql = @"
            CREATE TABLE IF NOT EXISTS ""CartItems"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""UserId"" TEXT NULL,
                ""ProductId"" INTEGER NOT NULL,
                ""Quantity"" INTEGER NOT NULL,
                ""UnitPrice"" DECIMAL(18,2) NOT NULL,
                ""DateAdded"" TIMESTAMP NOT NULL,
                CONSTRAINT ""FK_CartItems_Products_ProductId"" FOREIGN KEY (""ProductId"") 
                    REFERENCES ""Products"" (""Id"") ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS ""IX_CartItems_ProductId"" ON ""CartItems"" (""ProductId"");
        ";
        context.Database.ExecuteSqlRaw(sql);
        Console.WriteLine("✅ CartItems table ensured.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error applying migrations or initializing tables.");
        Console.WriteLine($"❌ Migration error: {ex.Message}");
    }
}

// ✅ Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseCors("AllowReactApp");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ✅ Seed admin
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "admin@gmail.com";
    string adminPassword = "Admin@123";
    string adminRole = "Admin";

    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
        Console.WriteLine($"✅ Role '{adminRole}' created.");
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var user = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FullName = "Admin User"
        };

        var result = await userManager.CreateAsync(user, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, adminRole);
            Console.WriteLine("✅ Admin user created.");
        }
        else
        {
            Console.WriteLine("❌ Failed to create admin user:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine("✅ Admin user already exists.");
    }
}

app.Run();
