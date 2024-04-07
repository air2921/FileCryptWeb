using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications
{
    public class UserByEmailSpec : Specification<UserModel>
    {
        public UserByEmailSpec(string email)
        {
            Query.Where(x => x.email.Equals(email));
        }
    }
}