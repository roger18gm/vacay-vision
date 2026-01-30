using Microsoft.EntityFrameworkCore;
using juveApp.Data;
using juveApp.Models;
using juveApp.Models.ViewModels;

namespace juveApp.Services
{
    public class VacationService
    {
        private readonly AppDbContext _context;

        public VacationService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all vacations for a specific user
        /// </summary>
        public async Task<List<Vacation>> GetUserVacationsAsync(int userId)
        {
            return await _context.Vacations
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get a single vacation by ID with User navigation loaded
        /// </summary>
        public async Task<Vacation?> GetVacationByIdAsync(int vacationId)
        {
            return await _context.Vacations
                .Include(v => v.User)
                .Include(v => v.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(v => v.VacationId == vacationId);
        }

        /// <summary>
        /// Create a new vacation
        /// </summary>
        public async Task<Vacation> CreateVacationAsync(CreateVacationViewModel model, int userId)
        {
            var vacation = new Vacation
            {
                UserId = userId,
                Title = model.Title,
                Destination = model.Destination,
                Description = model.Description,
                ImageUrl = model.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vacations.Add(vacation);
            await _context.SaveChangesAsync();

            return vacation;
        }

        /// <summary>
        /// Update an existing vacation (with ownership verification)
        /// </summary>
        public async Task<bool> UpdateVacationAsync(EditVacationViewModel model, int userId)
        {
            var vacation = await _context.Vacations
                .FirstOrDefaultAsync(v => v.VacationId == model.VacationId && v.UserId == userId);

            if (vacation == null)
                return false;

            vacation.Title = model.Title;
            vacation.Destination = model.Destination;
            vacation.Description = model.Description;
            vacation.ImageUrl = model.ImageUrl;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Delete a vacation (with ownership verification)
        /// </summary>
        public async Task<bool> DeleteVacationAsync(int vacationId, int userId)
        {
            var vacation = await _context.Vacations
                .FirstOrDefaultAsync(v => v.VacationId == vacationId && v.UserId == userId);

            if (vacation == null)
                return false;

            _context.Vacations.Remove(vacation);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Check if a vacation exists
        /// </summary>
        public async Task<bool> VacationExistsAsync(int vacationId)
        {
            return await _context.Vacations.AnyAsync(v => v.VacationId == vacationId);
        }

        /// <summary>
        /// Check if a user owns a specific vacation
        /// </summary>
        public async Task<bool> UserOwnsVacationAsync(int vacationId, int userId)
        {
            return await _context.Vacations
                .AnyAsync(v => v.VacationId == vacationId && v.UserId == userId);
        }
    }
}
