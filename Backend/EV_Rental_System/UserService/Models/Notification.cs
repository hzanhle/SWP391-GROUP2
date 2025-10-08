namespace UserService.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public User User { get; set; }
    }
}
