using BCrypt.Net;
using juveApp.Data;
using juveApp.Models;
using juveApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace juveApp.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Authenticate user by email/password
        /// </summary>
        public async Task<User?> AuthenticateUserAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            try
            {
                // Verify password hash
                bool isValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
                return isValid ? user : null;
            }
            catch (InvalidOperationException)
            {
                // Invalid password hash format in database
                return null;
            }
        }

        /// <summary>
        /// Create new user with hashed password
        /// </summary>
        public async Task<User> CreateUserAsync(SignupViewModel model)
        {
            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                var user = new User
                {
                    Email = model.Email,
                    Username = model.Username,
                    Password = hashedPassword,
                    RoleId = 0 // Default role (user)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create user account", ex);
            }
        }

        /// <summary>
        /// Validate signup data
        /// </summary>
        public async Task<List<string>> ValidateSignupAsync(SignupViewModel model)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(model.Email) || !model.Email.Contains("@"))
                errors.Add("Valid email address is required");

            if (string.IsNullOrEmpty(model.Username) || model.Username.Length < 8)
                errors.Add("Username must be at least 8 characters long");

            if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 8)
                errors.Add("Password must be at least 8 characters long");

            if (model.Password != model.ConfirmPassword)
                errors.Add("Passwords do not match");

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                errors.Add("An account with this email already exists");

            return errors;
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}
