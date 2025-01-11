using Mango.Services.RewardAPI.Models.Dto;

namespace Mango.Services.RewardAPI.Services.Interfaces
{
    public interface IRewardService
    {
        Task UpdateRewards(RewardDto rewardDto);
    }
}
