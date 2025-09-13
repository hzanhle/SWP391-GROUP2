namespace UserService.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public Role()
        {
        }
        public Role(int id, string name)
        {
            RoleId = id;
            RoleName = name;
        }
    }
}
