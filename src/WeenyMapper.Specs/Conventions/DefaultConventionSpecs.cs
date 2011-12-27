using NUnit.Framework;
using WeenyMapper.Conventions;

namespace WeenyMapper.Specs.Conventions
{
    [TestFixture]
    public class DefaultConventionSpecs
    {
        private DefaultConvention _defaultConvention;

        [SetUp]
        public void SetUp()
        {
            _defaultConvention = new DefaultConvention();
        }

        [Test]
        public void Default_convention_for_column_names_is_property_name()
        {
            var columnName = _defaultConvention.GetColumnName("Username");

            Assert.AreEqual("Username", columnName);
        }

        [Test]
        public void Default_convention_for_table_names_is_class_name()
        {
            var columnName = _defaultConvention.GetTableName("User");

            Assert.AreEqual("User", columnName);
        }

        [Test]
        public void Property_called_Id_is_id_property()
        {
            var isIdProperty = _defaultConvention.IsIdProperty("Id");

            Assert.IsTrue(isIdProperty);
        }

        [Test]
        public void Property_with_name_other_than_Id_is_not_id_property()
        {
            Assert.IsFalse(_defaultConvention.IsIdProperty("Username"));
            Assert.IsFalse(_defaultConvention.IsIdProperty("Password"));
        }
    }
}