using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications
{
    public class UserByEmailSpecification : Specification<UserModel>
    {
        public UserByEmailSpecification(string email)
        {
            Query.Where(x => x.email.Equals(email));
        }
    }
}