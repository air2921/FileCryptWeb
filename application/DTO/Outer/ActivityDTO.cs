namespace application.DTO.Outer
{
    public class ActivityDTO
    {
        public string Date { get; set; }
        public int ActivityCount { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is null || obj is not ActivityDTO other)
                return false;

            return Date == other.Date && ActivityCount == other.ActivityCount;
        }

        public override int GetHashCode() => HashCode.Combine(Date, ActivityCount);
    }
}
