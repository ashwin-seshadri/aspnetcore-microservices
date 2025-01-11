using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Models.Dto;
using Mango.Services.RewardAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.RewardAPI.Services
{
    public class RewardService : IRewardService
    {
        private DbContextOptions<AppDbContext> _dbOptions;
        public RewardService(DbContextOptions<AppDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public async Task UpdateRewards(RewardDto rewardDto)
        {
            try
            {
                Reward reward = new Reward
                {
                    UserId = rewardDto.UserId,
                    Date = DateTime.Now,
                    RewardActivity = rewardDto.RewardActivity,
                    OrderId = rewardDto.OrderId
                };

                await using var db = new AppDbContext(_dbOptions);
                await db.Rewards.AddAsync(reward);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"----- Error in Rewards Service: {ex.Message}");
            }
        }
    }
}
