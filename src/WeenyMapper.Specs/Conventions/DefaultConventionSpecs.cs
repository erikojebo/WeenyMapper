using System;
using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

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
            var columnName = _defaultConvention.GetTableName(typeof(User));

            Assert.AreEqual("User", columnName);
        }

        [Test]
        public void Property_called_Id_is_id_property()
        {
            var idProperty = typeof(User).GetProperty("Id");
            var isIdProperty = _defaultConvention.IsIdProperty(idProperty);

            Assert.IsTrue(isIdProperty);
        }

        [Test]
        public void Property_with_name_other_than_Id_is_not_id_property()
        {
            var usernameProperty = typeof(User).GetProperty("Username");
            var passwordProperty = typeof(User).GetProperty("Password");

            Assert.IsFalse(_defaultConvention.IsIdProperty(usernameProperty));
            Assert.IsFalse(_defaultConvention.IsIdProperty(passwordProperty));
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

        [Test]
        public void Class_with_int_id_has_identity_id()
        {
            Assert.IsTrue(_defaultConvention.HasIdentityId(typeof(IntIdEntity)));
        }

        [Test]
        public void Class_with_guid_id_does_not_have_identity_id()
        {
            Assert.IsFalse(_defaultConvention.HasIdentityId(typeof(GuidIdEntity)));
        }

        private bool ShouldMapEntityProperty(string name)
        {
            var property = typeof(Entity).GetProperty(name);
            return _defaultConvention.ShouldMapProperty(property);
        }

        private class IntIdEntity
        {
            public int Id { get; set; }
        }

        private class GuidIdEntity
        {
            public Guid Id { get; set; }
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