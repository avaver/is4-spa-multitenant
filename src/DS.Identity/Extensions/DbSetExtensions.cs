using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.Identity.Extensions
{
    public static class DbSetExtensions
    {
        public  static void Recreate<TEntity>(this DbSet<TEntity> dbEntities, IEnumerable<TEntity> localEntities, Func<TEntity, TEntity, bool> sameEntities) where TEntity : class
        {
            foreach (var localEntity in localEntities)
            {
                var dbEntity = dbEntities.AsEnumerable().SingleOrDefault(dbe => sameEntities(dbe, localEntity));
                if (dbEntity != null)
                {
                    dbEntities.Remove(dbEntity);
                }
                dbEntities.Add(localEntity);
            }
        }
    }
}
