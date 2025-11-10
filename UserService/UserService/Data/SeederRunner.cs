namespace UserService.Data
{
    public static class SeederRunner
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
            var permissionSeeder = scope.ServiceProvider.GetRequiredService<PermissionSeeder>();

            await roleSeeder.SeedRolesAsync();
            await permissionSeeder.SeedAsync();

            await roleSeeder.SeedAdminAsync("admin@company.com", "Admin@123");
        }
    }

}
