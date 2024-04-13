using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications
{
    public class UsersByUsernameSpec : Specification<UserModel>
    {
        public UsersByUsernameSpec(string username)
        {
            Username = username;

            Query.Where(x => x.username.Equals(username));
        }

        public string Username { get; private set; }
    }
}
