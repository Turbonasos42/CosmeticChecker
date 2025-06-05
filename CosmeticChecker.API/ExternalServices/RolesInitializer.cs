using DatabaseModels;
using DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace CosmeticChecker.API.ExternalServices
{
    public class RolesInitializer : IRolesInitializer
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RolesInitializer> _logger;

        public RolesInitializer(
            AppDbContext context,
            IConfiguration configuration,
            ILogger<RolesInitializer> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InitializeRolesAsync()
        {
            try
            {
                // Проверяем, есть ли уже роли Admin, Moderator и DefaultUser
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole == null)
                {
                    adminRole = new Role { Name = "Admin" };
                    _context.Roles.Add(adminRole);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Admin role created successfully");
                }

                var moderatorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Moderator");
                if (moderatorRole == null)
                {
                    moderatorRole = new Role { Name = "Moderator" };
                    _context.Roles.Add(moderatorRole);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Moderator role created successfully");
                }

                var defaultUserRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "DefaultUser");
                if (defaultUserRole == null)
                {
                    defaultUserRole = new Role { Name = "DefaultUser" };
                    _context.Roles.Add(defaultUserRole);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("DefaultUser role created successfully");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while initializing roles");
            }
        }
    }
}
