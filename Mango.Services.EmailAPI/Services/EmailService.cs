using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Services.EmailAPI.Services
{
    public class EmailService : IEmailService
    {
        private DbContextOptions<AppDbContext> _dbOptions;
        public EmailService(DbContextOptions<AppDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public async Task EmailCartAndLog(CartDto cartDto)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine("<br/>Cart Email Requested ");
            message.AppendLine($"<br/>Total {cartDto.CartHeader.Total}");
            message.AppendLine("<br/>");
            message.AppendLine("<ul>");
            foreach (var detail in cartDto.CartDetails)
            {
                message.AppendLine($"<li> Product: {detail.Product.Name} Quantity: {detail.Count} Price: {string.Format("{0:c}",detail.Product.Price)} </li>");
            }
            message.AppendLine("</ul>");
            await LogAndEmail(message.ToString(), cartDto.CartHeader.Email);    
        }

        public async Task LogOrderPlaced(RewardDto rewardDto)
        {
            string message = $"New Order Placed. <br/> Order Id: {rewardDto.OrderId}";
            await LogAndEmail(message, "admin@bob.com");
        }

        public async Task RegisterUserEmailAndLog(string emailAddress)
        {
            var message = $"User registration successful. <br/> Email: {emailAddress}";
            await LogAndEmail(message, "admin@bob.com");
        }

        private async Task<bool> LogAndEmail(string message, string email)
        {
            try
            {
                EmailLog emailLog = new EmailLog
                {
                    Email = email,
                    EmailSentAt = DateTime.Now,
                    Message = message
                };

                await using var db = new AppDbContext(_dbOptions);
                await db.EmailLogs.AddAsync(emailLog);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
