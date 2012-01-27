using System;
using System.Collections.Generic;
using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Exceptions;
using WeenyMapper.Mapping;

namespace WeenyMapper.Specs.Mapping
{
    [TestFixture]
    public class EntityMapperSpecs
    {
        private EntityMapper _mapper;
        private List<ColumnValue> _columnValues;

        [SetUp]
        public void SetUp()
        {
            _columnValues = new List<ColumnValue>();
            _mapper = new EntityMapper(new DefaultConvention());
        }

        [Test]
        public void Creating_instance_of_type_without_values_creates_empty_instance()
        {
            var instance = _mapper.CreateInstance<ClassWithoutRelations>(_columnValues);

            Assert.AreEqual(null, instance.Name);
            Assert.AreEqual(Guid.Empty, instance.Id);
        }

        [Test]
        public void Creating_instance_with_single_value_with_same_column_name_as_property_name_sets_property()
        {
            AddValue("Name", "a name");

            var instance = _mapper.CreateInstance<ClassWithoutRelations>(_columnValues);

            Assert.AreEqual("a name", instance.Name);
        }

        [Test]
        public void Creating_instance_with_multiple_values_with_same_column_name_as_property_name_sets_properties()
        {
            AddValue("Name", "a name");
            AddValue("Id", new Guid("30F3B29F-A2C7-4D5A-ADE0-691B38F32453"));

            var instance = _mapper.CreateInstance<ClassWithoutRelations>(_columnValues);

            Assert.AreEqual("a name", instance.Name);
            Assert.AreEqual(new Guid("30F3B29F-A2C7-4D5A-ADE0-691B38F32453"), instance.Id);
        }

        [ExpectedException(typeof(MissingDefaultConstructorException))]
        [Test]
        public void Trying_to_create_instance_without_default_constructor_throws_exception()
        {
            _mapper.CreateInstance<ClassWithoutDefaultConstructor>(_columnValues);
        }

        [ExpectedException(typeof(MissingPropertyException))]
        [Test]
        public void Trying_to_create_instance_with_values_that_do_not_have_corresponding_properties_throws_exception()
        {
            AddValue("MissingProperty", null);
            _mapper.CreateInstance<ClassWithoutRelations>(_columnValues);
        }

        private void AddValue(string name, object value)
        {
            _columnValues.Add(new ColumnValue(name, value));
        }

        //[Test]
        //public void Creates_new_instance_of_target_type_and_writes_values_to_properties_corresponding_to_key_string()
        //{
        //    _values["ID"] = 123;
        //    _values["NAME"] = "a name";
        //    _values["TITLE"] = "a title";

        //    var child = _mapper.CreateInstance<Child>(_values);

        //    Assert.AreEqual(123, child.Id);
        //    Assert.AreEqual("a name", child.Name);
        //    Assert.AreEqual("a title", child.Title);
        //}

        //[Test]
        //public void Creates_instance_of_type_for_entity_reference_property_with_values_from_type_name_prefixed_key_string()
        //{
        //    _values["CHILD ID"] = 1;
        //    _values["CHILD NAME"] = "child name";
        //    _values["CHILD TITLE"] = "child title";
        //    _values["PARENT ID"] = 2;
        //    _values["PARENT NAME"] = "parent name";

        //    var parentProperty = Reflector<Child>.GetProperty(x => x.Parent);

        //    var child = _mapper.CreateInstance<Child>(_values, new [] { parentProperty });
        //    Assert.AreEqual(1, child.Id);
        //    Assert.AreEqual("child name", child.Name);
        //    Assert.AreEqual("child title", child.Title);
        //    Assert.AreEqual(2, child.Parent.Id);
        //    Assert.AreEqual("parent name", child.Parent.Name);
        //}

        private class ClassWithoutDefaultConstructor
        {
            private ClassWithoutDefaultConstructor() {}
        }

        private class ClassWithoutRelations
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
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