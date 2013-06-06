using System;
using System.Reflection;

namespace WeenyMapper.Conventions
{
    public interface IConvention
    {
        /// <summary>
        /// Returns the column name for a given property
        /// Default: Same column name as property name
        /// </summary>
        /// <param name="propertyInfo">The PropertyInfo for which to determine the underlying database column name</param>
        /// <returns>The column name for a given property</returns>
        string GetColumnName(PropertyInfo propertyInfo);

        /// <summary>
        /// Returns the table name for a given class
        /// Default: Same table name as class name
        /// </summary>
        /// <param name="entityType">The type of the entity for which to determine the underlying database table name</param>
        /// <returns>The table name for a given class</returns>
        string GetTableName(Type entityType);

        /// <summary>
        /// Returns the foreign key column name for a navigation property from a child entity to a parent (many to one)
        /// Default: Property name + Id, so the property "BlogPost.Blog" returns the column name "BlogId"        
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

        /// <summary>
        /// Returns true if the given property represents the primary key column in the database table
        /// Default: True if the columns is called "Id" false otherwise
        /// </summary>
        /// <param name="propertyInfo">The PropertyInfo for which is should be determined if WeenyMapper should attempt to map it against a column in the database or not</param>
        /// <returns>True if the given property represents the primary key column in the database table</returns>
        bool IsIdProperty(PropertyInfo propertyInfo);

        /// <summary>
        /// Returns true if the property should be included in queries/statements. If false, the property is ignored entirely by WeenyMapper.
        /// Default: Non-static properties with public getter and setter are mapped, all other properties are ignored
        /// </summary>
        /// <param name="propertyInfo">The PropertyInfo for which is should be determined if WeenyMapper should attempt to map it against a column in the database or not</param>
        /// <returns>True if the property should be included in queries/statements. If false, the property is ignored entirely by WeenyMapper.</returns>
        bool ShouldMapProperty(PropertyInfo propertyInfo);

        /// <summary>
        /// Returns true if the given class represents a database table with an identity primary key column, false otherwise
        /// Default: True if the property representing the primary key is an integer property, false otherwise
        /// </summary>
        /// <param name="entityType">The type for which is should be determined if the underlying database table has an identity primary key column</param>
        /// <returns>True if the given class represents a database table with an identity primary key column, false otherwise</returns>
        bool HasIdentityId(Type entityType);
    }
}