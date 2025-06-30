namespace API.Entities
{
    public class AppUser
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public byte[] PasswardHash { get; set; }

        public byte[] PasswordSalt { get; set; }
    }
}
