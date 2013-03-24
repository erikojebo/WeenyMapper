using System;
using System.Reflection;

namespace WeenyMapper.Conventions
{
    public interface IConvention
    {
        string GetColumnName(PropertyInfo propertyInfo);
        string GetTableName(Type entityType);

        /// <summary>
        /// Gets the name of the database column for the foreign key, when executing a join query.
        /// </summary>
        /// <remarks>
        /// This method is called when a join is made which includes a child->parent navigation property.
        /// </remarks>
        /// <example>
        /// Given a blogPost.User which corresponds to [BlogPost].[UserId] in the database,
        /// "UserId" should be returned by this method when given the PropertyInfo for the User property
        /// on the BlogPost class.
        /// </example>
        /// <param name="propertyInfo">The navigation property used in the join</param>
        /// <returns>The name of the actual column name used in the foreign key in the database</returns>
        string GetManyToOneForeignKeyColumnName(PropertyInfo propertyInfo);

        bool IsIdProperty(PropertyInfo propertyInfo);
        bool ShouldMapProperty(PropertyInfo propertyInfo);
        bool HasIdentityId(Type entityType);
    }
}