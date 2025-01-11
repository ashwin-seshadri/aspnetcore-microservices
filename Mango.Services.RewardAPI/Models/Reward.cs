namespace Mango.Services.RewardAPI.Models
{
    public class Reward
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public int RewardActivity { get; set; }
        public int OrderId { get; set; }
    }
}
