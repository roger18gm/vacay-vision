using Microsoft.EntityFrameworkCore;

namespace juveApp.Data
{
    public static class DbInitializer
    {
        /// <summary>
        /// Clear all user data to start fresh with proper BCrypt hashes
        /// </summary>
        public static async Task ClearUserDataAsync(AppDbContext context)
        {
            try
            {
                // Delete all records from dependent tables first
                // (In order of foreign key dependencies)

                // Delete user-dependent data (comments, widgets, requests, feedback, etc.)
                // This would depend on your actual schema

                // Finally delete users
                await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE users CASCADE;");

                Console.WriteLine("✓ User data cleared successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Warning: Could not clear user data: {ex.Message}");
                // Don't throw - let the app continue
            }
        }
    }
}
