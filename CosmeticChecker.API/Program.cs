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

builder.Logging.ClearProviders();  // Очистить стандартные провайдеры логирования
builder.Logging.AddConsole();      // Добавить консольный провайдер

// Настройка Kestrel для использования SSL-сертификата
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(443, listenOptions =>
    {
        listenOptions.UseHttps("localhost.pfx", "123");
    });
});


// Конфигурация CORS (разрешение для фронтенда)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://62.60.234.22:5173", "http://localhost:5173", "http://localhost:3001", "http://localhost:3000" ) // Разрешаем любой источник (можно ограничить конкретным источником)
              .AllowAnyMethod()  // Разрешаем любые HTTP методы (GET, POST, PUT и т.д.)
              .AllowAnyHeader() // Разрешаем любые заголовки
              .AllowCredentials();
    });
});

// Конфигурация сервисов
builder.Services.AddControllers();

// Настройка Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CosmeticChecker API", Version = "v1" });

    // Настройка JWT в Swagger (если нужно)
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

// База данных
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFavoritesService, FavoritesService>(); // Регистрация сервиса
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>(); // Регистрация репозитория
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IProductReviewService, ProductReviewService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IReviewModerationService, ReviewModerationService>();
builder.Services.AddScoped<IIngredientExtService, IngredientExtService>(); // Регистрация сервиса для ингредиентов
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IIngredientService, IngredientService>();
builder.Services.AddScoped<ICompositionSearchService, CompositionSearchService>();
builder.Services.AddScoped<IProductAnaloguesService, ProductAnaloguesService>();
builder.Services.AddScoped<IImageAddingService, ImageAddingService>();

// Добавление сервиса для пересчета рейтингов продуктов
builder.Services.AddScoped<ProductRatingService>();

// Сервисы
builder.Services.AddScoped<IRolesInitializer, RolesInitializer>();

// Настройка аутентификации с Cookies
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

// Добавление авторизации
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// Добавление аутентификации с JWT (если нужно)
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

// Добавление IHttpContextAccessor для работы с контекстом
builder.Services.AddHttpContextAccessor();

// Строим приложение
var app = builder.Build();

// Применение миграций и запуск приложения
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();  // Применяем миграции

        var initializer = scope.ServiceProvider.GetRequiredService<IRolesInitializer>();
        await initializer.InitializeRolesAsync();  // Инициализируем роли

        // Запускаем пересчет рейтингов продуктов после инициализации базы данных
        var productRatingService = scope.ServiceProvider.GetRequiredService<ProductRatingService>();
        //await productRatingService.RecalculateAndSetSafetyRatingsAsync(); // Метод для пересчета и установки рейтингов продуктов

        // Загружаем ингредиенты из JSON файла в базу данных
        //var ingredientService = scope.ServiceProvider.GetRequiredService<IIngredientService>();
        //string jsonFilePath = "C:\\Users\\gg902\\Desktop\\descriptions .json";  // Укажите путь к вашему JSON файлу
        //await ingredientService.AddIngredientsFromJsonAsync(jsonFilePath);  // Загружаем ингредиенты в базу данных
        //await ingredientService.LinkAllIngredientsToProductsAsync();
        //await ingredientService.AddIngredientToProductAsync(1, 1); // Пример добавления ингредиента к продукту
        //var productAnaloguesService = scope.ServiceProvider.GetRequiredService<IProductAnaloguesService>();
        //await productAnaloguesService.FindAndSetProductAnaloguesAsync(); // Этот метод будет заполнять поле Analogues

        //var imageAddingService = scope.ServiceProvider.GetRequiredService<IImageAddingService>();
        //string imageFilePath = "C:\\Users\\gg902\\Downloads\\Telegram Desktop\\products_with_images3.json"; // Укажите правильный путь к вашему файлу
        //await imageAddingService.UpdateProductImageUrlsFromJsonAsync(imageFilePath);// Запуск метода для обновления ImageURL продуктов

    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при инициализации БД");
    }
}

// Конвейер middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CosmeticChecker API v1");
        c.RoutePrefix = "swagger";
    });
}

// Включение CORS
app.UseCors("AllowFrontend"); // Применяем политику CORS

// Включение HTTPS редиректа
app.UseHttpsRedirection();

// Аутентификация и авторизация
app.UseAuthentication();
app.UseAuthorization();

// Применение контроллеров
app.MapControllers();

// Запуск приложения
await app.RunAsync();
