using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext db;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            db = context;
            _mapper = mapper;
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

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = db.Messages.OrderByDescending(m => m.MessageSent)
                                   .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.Recipient.Username == messageParams.Username),
                "Outbox" => query.Where(u => u.Sender.Username == messageParams.Username),
                _ => query.Where(u => u.Recipient.Username == messageParams.Username && u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber,
                                                           messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await db.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m => m.Recipient.Username == currentUsername && m.Sender.Username == recipientUsername
                        || m.Recipient.Username == recipientUsername && m.Sender.Username == currentUsername)
                .OrderBy(m => m.MessageSent)
                .ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null
                                             && m.Recipient.Username == currentUsername)
                                         .ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.Now;
                }

                await db.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
            
        }

        public async Task<bool> SaveAllAsync()
        {
            return await db.SaveChangesAsync() > 0;
        }
    }
}