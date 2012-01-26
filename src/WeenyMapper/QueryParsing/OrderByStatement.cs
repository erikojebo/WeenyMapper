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

        public static OrderByStatement Create(string propertyName, OrderByDirection orderByDirection)
        {
            return new OrderByStatement(propertyName, orderByDirection);
        }

        public string PropertyName { get; private set; }
        public OrderByDirection Direction { get; private set; }

        public OrderByStatement Translate(IConventionReader convention, Type type)
        {
            return new OrderByStatement(convention.GetColumnName(PropertyName, type), Direction);
        }
    }
}