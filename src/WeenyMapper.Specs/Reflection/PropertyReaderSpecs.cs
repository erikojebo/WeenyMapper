using NUnit.Framework;
using WeenyMapper.Reflection;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs.Reflection
{
    [TestFixture]
    public class PropertyReaderSpecs
    {
        [Test]
        public void Can_get_name_of_property_from_expression()
        {
            var propertyName = PropertyMetadataReader<User>.GetPropertyName(x => x.Password);

            Assert.AreEqual("Password", propertyName);
        }
    }
}