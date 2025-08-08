using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext db;
        public UserRepository(DataContext context)
        {
            this.db = context;
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await db.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await db.Users.Include(p => p.Photos).SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await db.Users.Include(p => p.Photos).ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await db.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            db.Entry(user).State = EntityState.Modified;
        }
    }
}