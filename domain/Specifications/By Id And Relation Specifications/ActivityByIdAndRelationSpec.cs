using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.By_Id_And_Relation_Specifications
{
    public class ActivityByIdAndRelationSpec : Specification<ActivityModel>
    {
        public ActivityByIdAndRelationSpec(int activityId, int userId)
        {
            ActivityId = activityId;
            UserId = userId;

            Query.Where(a => a.action_id.Equals(activityId) && a.user_id.Equals(userId));
        }

        public int ActivityId { get; private set; }
        public int UserId { get; private set; }
    }
}
