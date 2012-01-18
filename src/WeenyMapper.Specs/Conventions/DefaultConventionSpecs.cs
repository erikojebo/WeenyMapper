using System;
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

        [Test]
        public void Public_read_write_properties_are_mapped_by_default()
        {
            var shouldMapProperty = ShouldMapEntityProperty("PublicReadWriteProperty");
            Assert.IsTrue(shouldMapProperty);
        }

        [Test]
        public void Public_property_without_setter_is_not_mapped_by_default()
        {
            var shouldMapProperty = ShouldMapEntityProperty("PublicPropertyWithoutSetter");
            Assert.IsFalse(shouldMapProperty);
        }

        [Test]
        public void Public_property_with_private_setter_is_not_mapped_by_default()
        {
            var shouldMapProperty = ShouldMapEntityProperty("PublicPropertyWithPrivateSetter");
            Assert.IsFalse(shouldMapProperty);
        }

        [Test]
        public void Static_properties_are_not_mapped_by_default()
        {
            var shouldMapProperty = ShouldMapEntityProperty("PublicStaticProperty");
            Assert.IsFalse(shouldMapProperty);
        }

        [Test]
        public void Property_with_backing_field_is_mapped_by_default()
        {
            var shouldMapProperty = ShouldMapEntityProperty("ReadWritePropertyWithBackingField");
            Assert.IsTrue(shouldMapProperty);
        }

        private bool ShouldMapEntityProperty(string name)
        {
            var property = typeof(Entity).GetProperty(name);
            return _defaultConvention.ShouldMapProperty(property);
        }

        private class Entity
        {
            private string _propertyWithBackingField;

            public string PublicReadWriteProperty { get; set; }
            public string PublicPropertyWithPrivateSetter { get; private set; }
            public static string PublicStaticProperty { get; set; }

            public string ReadWritePropertyWithBackingField
            {
                get { return _propertyWithBackingField; }
                set { _propertyWithBackingField = value; }
            }

            public string PublicPropertyWithoutSetter
            {
                get { return ""; }
            }
        }
    }
}