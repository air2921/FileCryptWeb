using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications
{
    public class UserByEmailSpec : Specification<UserModel>
    {
        public UserByEmailSpec(string email)
        {
            Email = email;

            Query.Where(x => x.email.Equals(email));
        }

        public string Email { get; private set; }
    }
}