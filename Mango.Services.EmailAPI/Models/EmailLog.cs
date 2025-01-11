namespace Mango.Services.EmailAPI.Models
{
    public class EmailLog
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime? EmailSentAt { get; set; }
    }
}
