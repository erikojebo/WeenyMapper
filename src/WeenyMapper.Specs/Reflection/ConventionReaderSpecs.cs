using System;
using System.Reflection;
using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Reflection;

namespace WeenyMapper.Specs.Reflection
{
    [TestFixture]
    public class ConventionReaderSpecs
    {
        private ConventionReader _reader;

        [SetUp]
        public void SetUp()
        {
            var convention = new TestingConvention();
            _reader = new ConventionReader(convention);
        }

        [Test]
        public void GetIdProperty_returns_property_that_matches_id_property_convention()
        {
            var actualProperty = _reader.GetIdProperty(typeof(Product));

            var expectedProperty = Reflector<Product>.GetProperty(x => x.ProductId);

            Assert.AreEqual(expectedProperty, actualProperty);
        }

        class Product
        {
            public int ProductId { get; set; }
            public decimal Price { get; set; }

            public bool IsDirty { get; set; }
        }

        public class TestingConvention : DefaultConvention
        {
            public override bool IsIdProperty(PropertyInfo propertyInfo)
            {
                // CartId is the primary key of the shopping cart table, but it does
                // not conform to our convention. So we need to special case it.
                if (propertyInfo.Name == "CartId")
                    return true;

                // Product => ProductId
                return propertyInfo.Name == propertyInfo.DeclaringType.Name + "Id";
            }

            public override bool HasIdentityId(Type entityType)
            {
                return true;
            }

            public override string GetColumnName(PropertyInfo propertyInfo)
            {
                // For example C_PRICE
                return "C_" + propertyInfo.Name.ToUpper();
            }

            public override string GetTableName(Type entityType)
            {
                // For example T_PRODUCTS
                return "T_" + entityType.Name + "s";
            }

            public override bool ShouldMapProperty(PropertyInfo propertyInfo)
            {
                // Do not try to read/write any properties called IsDirty since they do not
                // exist in the database
                if (propertyInfo.Name == "IsDirty")
                    return false;

                return base.ShouldMapProperty(propertyInfo);
            }
        }
    }
}