using CosmeticChecker.API.ExternalServices;
using CosmeticChecker.API.Services;
using DatabaseContext;
using DatabaseModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Repositories;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();  // �������� ����������� ���������� �����������
builder.Logging.AddConsole();      // �������� ���������� ���������

// ��������� Kestrel ��� ������������� SSL-�����������
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(443, listenOptions =>
    {
        listenOptions.UseHttps("localhost.pfx", "123");
    });
});


// ������������ CORS (���������� ��� ���������)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://62.60.234.22:5173", "http://localhost:5173", "http://localhost:3001", "http://localhost:3000" ) // ��������� ����� �������� (����� ���������� ���������� ����������)
              .AllowAnyMethod()  // ��������� ����� HTTP ������ (GET, POST, PUT � �.�.)
              .AllowAnyHeader() // ��������� ����� ���������
              .AllowCredentials();
    });
});

// ������������ ��������
builder.Services.AddControllers();

// ��������� Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CosmeticChecker API", Version = "v1" });

    // ��������� JWT � Swagger (���� �����)
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new()
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// ���� ������
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFavoritesService, FavoritesService>(); // ����������� �������
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>(); // ����������� �����������
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IProductReviewService, ProductReviewService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IReviewModerationService, ReviewModerationService>();
builder.Services.AddScoped<IIngredientExtService, IngredientExtService>(); // ����������� ������� ��� ������������
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IIngredientService, IngredientService>();
builder.Services.AddScoped<ICompositionSearchService, CompositionSearchService>();
builder.Services.AddScoped<IProductAnaloguesService, ProductAnaloguesService>();
builder.Services.AddScoped<IImageAddingService, ImageAddingService>();

// ���������� ������� ��� ��������� ��������� ���������
builder.Services.AddScoped<ProductRatingService>();

// �������
builder.Services.AddScoped<IRolesInitializer, RolesInitializer>();

// ��������� �������������� � Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.LoginPath = "/api/auth/login";
        options.AccessDeniedPath = "/api/auth/forbidden";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
    });

// ���������� �����������
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// ���������� �������������� � JWT (���� �����)
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.RequireHttpsMetadata = true;
//        options.SaveToken = true;
//        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//        };
//    });

// ���������� IHttpContextAccessor ��� ������ � ����������
builder.Services.AddHttpContextAccessor();

// ������ ����������
var app = builder.Build();

// ���������� �������� � ������ ����������
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();  // ��������� ��������

        var initializer = scope.ServiceProvider.GetRequiredService<IRolesInitializer>();
        await initializer.InitializeRolesAsync();  // �������������� ����

        // ��������� �������� ��������� ��������� ����� ������������� ���� ������
        var productRatingService = scope.ServiceProvider.GetRequiredService<ProductRatingService>();
        //await productRatingService.RecalculateAndSetSafetyRatingsAsync(); // ����� ��� ��������� � ��������� ��������� ���������

        // ��������� ����������� �� JSON ����� � ���� ������
        //var ingredientService = scope.ServiceProvider.GetRequiredService<IIngredientService>();
        //string jsonFilePath = "C:\\Users\\gg902\\Desktop\\descriptions .json";  // ������� ���� � ������ JSON �����
        //await ingredientService.AddIngredientsFromJsonAsync(jsonFilePath);  // ��������� ����������� � ���� ������
        //await ingredientService.LinkAllIngredientsToProductsAsync();
        //await ingredientService.AddIngredientToProductAsync(1, 1); // ������ ���������� ����������� � ��������
        //var productAnaloguesService = scope.ServiceProvider.GetRequiredService<IProductAnaloguesService>();
        //await productAnaloguesService.FindAndSetProductAnaloguesAsync(); // ���� ����� ����� ��������� ���� Analogues

        //var imageAddingService = scope.ServiceProvider.GetRequiredService<IImageAddingService>();
        //string imageFilePath = "C:\\Users\\gg902\\Downloads\\Telegram Desktop\\products_with_images3.json"; // ������� ���������� ���� � ������ �����
        //await imageAddingService.UpdateProductImageUrlsFromJsonAsync(imageFilePath);// ������ ������ ��� ���������� ImageURL ���������

    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "������ ��� ������������� ��");
    }
}

// �������� middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CosmeticChecker API v1");
        c.RoutePrefix = "swagger";
    });
}

// ��������� CORS
app.UseCors("AllowFrontend"); // ��������� �������� CORS

// ��������� HTTPS ���������
app.UseHttpsRedirection();

// �������������� � �����������
app.UseAuthentication();
app.UseAuthorization();

// ���������� ������������
app.MapControllers();

// ������ ����������
await app.RunAsync();
