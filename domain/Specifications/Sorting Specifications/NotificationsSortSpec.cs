﻿using Ardalis.Specification;
using domain.Models;

namespace domain.Specifications.Sorting_Specifications
{
    public class NotificationsSortSpec : Specification<NotificationModel>
    {
        public NotificationsSortSpec(int? userId, int skip, int count, bool byDesc, int? priority, bool? isChecked)
        {
            UserId = userId;
            SkipCount = skip;
            Count = count;
            ByDesc = byDesc;
            Priority = priority;
            IsChecked = isChecked;

            if (byDesc)
                Query.OrderByDescending(n => n.send_time);
            else
                Query.OrderBy(n => n.send_time);

            if (userId.HasValue)
                Query.Where(n => n.user_id.Equals(userId.Value));

            if (priority.HasValue)
                Query.Where(n => n.priority.Equals(priority));

            if (isChecked.HasValue)
                Query.Where(o => o.is_checked.Equals(isChecked));

            Query.Skip(skip).Take(count);
        }

        public int? UserId { get; private set; }
        public int SkipCount { get; private set; }
        public int Count { get; private set; }
        public bool ByDesc { get; private set; }
        public int? Priority { get; private set; }
        public bool? IsChecked { get; private set;}
    }
}
