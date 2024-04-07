﻿using Ardalis.Specification;
using webapi.Models;

namespace webapi.DB.Ef.Specifications.By_Relation_Specifications
{
    public class StoragesByRelationSpec : Specification<KeyStorageModel>
    {
        public StoragesByRelationSpec(int userId)
        {
            Query.Where(x => x.user_id.Equals(userId));
        }
    }
}
