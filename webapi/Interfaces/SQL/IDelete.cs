﻿namespace webapi.Interfaces.SQL
{
    public interface IDelete<TModel>
    {
        Task DeleteById(int id);
    }
}
