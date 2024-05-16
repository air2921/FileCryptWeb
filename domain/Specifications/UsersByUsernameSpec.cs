using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications
{
    public class UsersByUsernameSpec : Specification<UserModel>
    {
        public UsersByUsernameSpec(string username, int skip, int count)
        {
            Username = username;
            SkipCount = skip;
            Count = count;

            Query.Where(x => x.username.Contains(username));
            Query.OrderBy(x => Math.Abs(x.username.IndexOf(username) - x.username.Length));

            Query.Skip(skip).Take(count);
        }

        public string Username { get; private set; }
        public int SkipCount { get; private set; }
        public int Count { get; private set; }
    }
}
