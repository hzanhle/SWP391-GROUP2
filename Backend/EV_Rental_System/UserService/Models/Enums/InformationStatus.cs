namespace UserService.Models.Enums
{
    public enum Status
    {
        Pending,
        Approved,
        Rejected
    }

    public static class StatusInformation
    {
        public const string Pending = "Chờ xác thực";
        public const string Approved = "Đã xác nhận";
        public const string Rejected = "Từ chối";
    }
}
