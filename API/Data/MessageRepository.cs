using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext db;

        public MessageRepository(DataContext context)
        {
            db = context;
        }

        public void AddMessage(Message message)
        {
            db.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            db.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await db.Messages.FindAsync(id);
        }

        public Task<PagedList<MessageDto>> GetMessagesForUser()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MessageDto>> GetMessageThread(int currentUserId, int recipientId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await db.SaveChangesAsync() > 0;
        }
    }
}