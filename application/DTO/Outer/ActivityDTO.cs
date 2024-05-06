using domain.Models;

namespace application.DTO.Outer
{
    public class ActivityDTO
    {
        public string Date { get; set; }
        public int ActivityCount { get; set; }

        public ActivityModel[] Activities { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is null || obj is not ActivityDTO other)
                return false;

            return Date == other.Date && ActivityCount == other.ActivityCount && Activities == other.Activities;
        }

        public override int GetHashCode() => HashCode.Combine(Date, ActivityCount, Activities);
    }
}
