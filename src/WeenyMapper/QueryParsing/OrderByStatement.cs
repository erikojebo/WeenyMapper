using System;
using WeenyMapper.Conventions;
using WeenyMapper.Reflection;

namespace WeenyMapper.QueryParsing
{
    public enum OrderByDirection
    {
        Ascending,
        Descending
    }

    public class OrderByStatement
    {
        public OrderByStatement(string propertyName, OrderByDirection orderByDirection = OrderByDirection.Ascending)
        {
            Direction = orderByDirection;
            PropertyName = propertyName;
        }

        public static OrderByStatement Create(string propertyName, OrderByDirection orderByDirection, int orderingIndex)
        {
            return new OrderByStatement(propertyName, orderByDirection) { OrderIndex = orderingIndex };
        }

        public static OrderByStatement Create<T>(string propertyName, OrderByDirection orderByDirection, int orderingIndex)
        {
            return new OrderByStatement(propertyName, orderByDirection) { OrderIndex = orderingIndex, Type = typeof(T) };
        }

        public static OrderByStatement Create(string propertyName, OrderByDirection orderByDirection, string tableIdentifier)
        {
            return new OrderByStatement(propertyName, orderByDirection) { TableIdentifier = tableIdentifier };
        }

        public string TableIdentifier { get; set; }
        public Type Type { get; set; }
        public string PropertyName { get; private set; }
        public OrderByDirection Direction { get; private set; }
        public int OrderIndex { get; set; }

        public OrderByStatement Translate(IConventionReader convention, Type type)
        {
            return new OrderByStatement(convention.GetColumnName(PropertyName, type), Direction);
        }
        
        public OrderByStatement Translate(IConventionReader convention)
        {
            return new OrderByStatement(convention.GetColumnName(PropertyName, Type), Direction);
        }
    }
}