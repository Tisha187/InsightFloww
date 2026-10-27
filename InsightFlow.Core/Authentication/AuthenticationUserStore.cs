using System;
using System.Collections.Generic;
using InsightFlow.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace InsightFlow.Core.Authentication
{
    public sealed class ApplicationUserStore :
     IUserStore<AspNetUser>,
     IUserPasswordStore<AspNetUser>,
     IUserEmailStore<AspNetUser>,
     IUserSecurityStampStore<AspNetUser>,
     IUserLockoutStore<AspNetUser>
    
    {
        private readonly InsightFlowAiContext _context;

        public ApplicationUserStore(InsightFlowAiContext context)
        {
            _context = context;
        }

        // Create a user
        public async Task<IdentityResult> CreateAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(user.Id))
                user.Id = Guid.NewGuid().ToString();

            user.ConcurrencyStamp ??= Guid.NewGuid().ToString();

            _context.AspNetUsers.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return IdentityResult.Success;
        }

        // Update a user
        public async Task<IdentityResult> UpdateAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            _context.AspNetUsers.Update(user);

            await _context.SaveChangesAsync(cancellationToken);

            return IdentityResult.Success;
        }

        // Delete a user
        public async Task<IdentityResult> DeleteAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _context.AspNetUsers.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);

            return IdentityResult.Success;
        }

        public void Dispose()
        {
            // The DbContext is managed by dependency injection.
        }

        // Find users
        public Task<AspNetUser?> FindByIdAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            return _context.AspNetUsers
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public Task<AspNetUser?> FindByNameAsync(
            string normalizedUserName,
            CancellationToken cancellationToken)
        {
            return _context.AspNetUsers
                .FirstOrDefaultAsync(
                    u => u.NormalizedUserName == normalizedUserName,
                    cancellationToken);
        }

        public Task<string?> GetUserIdAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
            => Task.FromResult<string?>(user.Id);

        public Task<string?> GetUserNameAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
            => Task.FromResult(user.UserName);

        public Task SetUserNameAsync(
            AspNetUser user,
            string? userName,
            CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedUserNameAsync(
         AspNetUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.NormalizedUserName);

        public Task SetNormalizedUserNameAsync(
            AspNetUser user,
            string? normalizedName,
            CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        // Password hash operations
        public Task SetPasswordHashAsync(
            AspNetUser user,
            string? passwordHash,
            CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string?> GetPasswordHashAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
            => Task.FromResult(user.PasswordHash);

        public Task<bool> HasPasswordAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
            => Task.FromResult(user.PasswordHash is not null);

        // Email operations
        public Task SetEmailAsync(
            AspNetUser user,
            string? email,
            CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string?> GetEmailAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
            => Task.FromResult(user.Email);

        public Task<bool> GetEmailConfirmedAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
            => Task.FromResult(user.EmailConfirmed);

        public Task SetEmailConfirmedAsync(
            AspNetUser user,
            bool confirmed,
            CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task<AspNetUser?> FindByEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken)
        {
            return _context.AspNetUsers
                .FirstOrDefaultAsync(
                    u => u.NormalizedEmail == normalizedEmail,
                    cancellationToken);
        }

        public Task<string?> GetNormalizedEmailAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
            => Task.FromResult(user.NormalizedEmail);

        public Task SetNormalizedEmailAsync(
            AspNetUser user,
            string? normalizedEmail,
            CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task SetSecurityStampAsync(
            AspNetUser user,
            string? stamp,
            CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public Task<string?> GetSecurityStampAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }


        public Task<DateTimeOffset?> GetLockoutEndDateAsync(
    AspNetUser user,
    CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEnd);
        }

        public Task SetLockoutEndDateAsync(
            AspNetUser user,
            DateTimeOffset? lockoutEnd,
            CancellationToken cancellationToken)
        {
            user.LockoutEnd = lockoutEnd;
            return Task.CompletedTask;
        }

        public Task<int> IncrementAccessFailedCountAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
        {
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
        {
            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        public Task<int> GetAccessFailedCountAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(
            AspNetUser user,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(
            AspNetUser user,
            bool enabled,
            CancellationToken cancellationToken)
        {
            user.LockoutEnabled = enabled;
            return Task.CompletedTask;
        }
    }
}
