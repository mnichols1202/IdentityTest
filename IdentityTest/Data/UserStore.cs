using IdentityTest.Model;
using IdentityTest.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityTest.Data
{
    public class UserStore : IUserStore<User>, IUserPasswordStore<User>, IUserEmailStore<User>
    {
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly string _connectionString;

        public UserStore(IConfiguration configuration, IPasswordHasher<User> passwordHasher)
        {
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            _connectionString = _configuration.GetConnectionString("GTCPWMSDb");
        }

        private IDbConnection CreateConnection()
        {

            var _connection = new SqlConnection(_connectionString);
            _connection.Open();
            return _connection;
        }

        // **User management methods**

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            using (var connection = CreateConnection())
            {
                await connection.InsertAsync(user);
                return IdentityResult.Success;
            }
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            using (var connection = CreateConnection())
            {
                await connection.UpdateAsync(user);
                return IdentityResult.Success;
            }
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            using (var connection = CreateConnection())
            {
                await connection.DeleteAsync(user);
                return IdentityResult.Success;
            }
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            using (var connection = CreateConnection())
            {
                return await connection.GetAsync<User>(int.Parse(userId));
            }
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            const string query = "SELECT * FROM [User] WHERE UserName = @UserName";
            using (var connection = CreateConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<User>(query, new { UserName = normalizedUserName });
            }
        }

        // **Password management using IPasswordHasher**

        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, passwordHash);
            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        // **User information for Identity framework**

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName.ToUpper()); // normalize username
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            user.UserName = normalizedName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> HashAndSavePasswordAsync(int userId, CancellationToken cancellationToken)
        {
            using (var connection = CreateConnection())
            {
                // Retrieve the user from the database
                var user = await connection.GetAsync<User>(userId);

                // If user does not exist, return an error
                if (user == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = $"User with ID {userId} not found." });
                }

                // Hash the existing plaintext password
                var hashedPassword = _passwordHasher.HashPassword(user, user.PasswordHash);

                // Update the PasswordHash field with the hashed password
                user.PasswordHash = hashedPassword;

                // Save changes to the database
                await connection.UpdateAsync(user);

                return IdentityResult.Success;
            }
        }

        // **IUserEmailStore implementation**

        public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            //user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult("");
            //return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
            //return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            //user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public async Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            //const string query = "SELECT * FROM [User] WHERE Email = @Email";
            //using (var connection = CreateConnection())
            //{
            return null;  
            //    return await connection.QuerySingleOrDefaultAsync<User>(query, new { Email = normalizedEmail });
            //}
        }

        public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(""); // Or your normalization logic
            //return Task.FromResult(user.Email.ToUpper()); // Or your normalization logic
        }

        public Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
        {
            // You might not need to store normalized email separately if you normalize on the fly
            // user.NormalizedEmail = normalizedEmail; 
            return Task.CompletedTask;
        }

        // **Dispose pattern**

        public void Dispose()
        {
            // No resources to dispose
        }
    }
}