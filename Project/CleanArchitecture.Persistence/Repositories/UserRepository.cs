using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Persistence.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User> GetByEmail(string email, CancellationToken cancellationToken)
        {
           var user = await Context.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
           return user ?? throw new KeyNotFoundException($"User with email '{email}' was not found.");
        }
    }
}
