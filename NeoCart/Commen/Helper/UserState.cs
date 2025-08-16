namespace NeoCart.Commen.Helper
{
    public class UserState
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();

        public UserState() { }
    }
}
