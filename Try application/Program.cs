using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Try_application.Database.Entities;
using Try_application.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    WebRootPath = "wwwroot",
    Args = args
});

// 1️⃣ Database
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

// 4️⃣ JWT
var jwtKey = builder.Configuration["JwtSettings:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    Console.WriteLine("⚠️ JWT Key missing; using fallback.");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? "DefaultKeyReplaceMe"));

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

// 5️⃣ Swagger + Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Migration + Table Check
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDBContext>();
        Console.WriteLine("Applying migrations...");
        context.Database.Migrate();
        Console.WriteLine("✅ Migrations applied.");

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
        logger.LogError(ex, "❌ Error in migrations.");
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

// ✅ SEED ADMIN + STAFF
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "admin@gmail.com";
    string adminPassword = "Admin@123";
    string adminRole = "Admin";

    string staffEmail = "staff@gmail.com";
    string staffPassword = "Staff@123";
    string staffRole = "Staff";

    // --- CREATE ADMIN ROLE ---
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
        Console.WriteLine($"✅ Role '{adminRole}' created.");
    }

    // --- CREATE STAFF ROLE ---
    if (!await roleManager.RoleExistsAsync(staffRole))
    {
        await roleManager.CreateAsync(new IdentityRole(staffRole));
        Console.WriteLine($"✅ Role '{staffRole}' created.");
    }

    // --- CREATE ADMIN USER ---
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
            Console.WriteLine("❌ Failed to create admin:");
            foreach (var error in result.Errors)
                Console.WriteLine($"- {error.Description}");
        }
    }
    else
    {
        Console.WriteLine("✅ Admin user already exists.");
    }

    // --- CREATE STAFF USER ---
    var staffUser = await userManager.FindByEmailAsync(staffEmail);
    if (staffUser == null)
    {
        var user = new User
        {
            UserName = staffEmail,
            Email = staffEmail,
            EmailConfirmed = true,
            FullName = "Staff User"
        };
        var result = await userManager.CreateAsync(user, staffPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, staffRole);
            Console.WriteLine("✅ Staff user created.");
        }
        else
        {
            Console.WriteLine("❌ Failed to create staff:");
            foreach (var error in result.Errors)
                Console.WriteLine($"- {error.Description}");
        }
    }
    else
    {
        Console.WriteLine("✅ Staff user already exists.");
    }
}

app.Run();
