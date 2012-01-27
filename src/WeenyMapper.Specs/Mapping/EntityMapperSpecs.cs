using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Mapping;
using WeenyMapper.Reflection;
using WeenyMapper.Specs.TestClasses.Conventions;

namespace WeenyMapper.Specs.Mapping
{
    [TestFixture]
    public class EntityMapperSpecs
    {
        private EntityMapper _mapper;
        private Dictionary<string, object> _values;

        [SetUp]
        public void SetUp()
        {
            _values = new Dictionary<string, object>();
            _mapper = new EntityMapper(new UpperCaseConvention());
        }

        [Test]
        public void Creates_new_instance_of_target_type_and_writes_values_to_properties_corresponding_to_key_string()
        {
            _values["ID"] = 123;
            _values["NAME"] = "a name";
            _values["TITLE"] = "a title";

            var child = _mapper.CreateInstance<Child>(_values);

            Assert.AreEqual(123, child.Id);
            Assert.AreEqual("a name", child.Name);
            Assert.AreEqual("a title", child.Title);
        }

        [Test]
        public void Creates_instance_of_type_for_entity_reference_property_with_values_from_type_name_prefixed_key_string()
        {
            _values["CHILD ID"] = 1;
            _values["CHILD NAME"] = "child name";
            _values["CHILD TITLE"] = "child title";
            _values["PARENT ID"] = 2;
            _values["PARENT NAME"] = "parent name";

            var parentProperty = Reflector<Child>.GetProperty(x => x.Parent);

            var child = _mapper.CreateInstance<Child>(_values, new [] { parentProperty });
            Assert.AreEqual(1, child.Id);
            Assert.AreEqual("child name", child.Name);
            Assert.AreEqual("child title", child.Title);
            Assert.AreEqual(2, child.Parent.Id);
            Assert.AreEqual("parent name", child.Parent.Name);
        }

        private class Parent
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            public IList<Child> Children { get; set; }
        }

        private class Child
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }

            public Parent Parent { get; set; }
        }
        
        private class GrandChild
        {
            public int Id { get; set; }
            public DateTime DateTime { get; set; }
        }
    }
}