using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Exceptions;
using WeenyMapper.Mapping;
using WeenyMapper.Reflection;
using WeenyMapper.Specs.TestClasses.Conventions;

namespace WeenyMapper.Specs.Mapping
{
    [TestFixture]
    public class EntityMapperSpecs
    {
        private EntityMapper _mapper;
        private List<ColumnValue> _columnValues;
        private Guid _guid = new Guid("00000000-0000-0000-0000-000000000001");
        private ObjectRelation _parentChildRelation;

        [SetUp]
        public void SetUp()
        {
            _columnValues = new List<ColumnValue>();
            _mapper = new EntityMapper(new ConventionReader(new DefaultConvention()));
            _parentChildRelation = ObjectRelation.Create<Parent, Child>(x => x.Children, x => x.Parent);
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
            AddValue("Id", _guid);

            var instance = _mapper.CreateInstance<ClassWithoutRelations>(_columnValues);

            Assert.AreEqual("a name", instance.Name);
            Assert.AreEqual(_guid, instance.Id);
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

        [Test]
        public void Creating_instance_with_multiple_values_with_column_name_according_to_convention_sets_corresponding_properties()
        {
            _mapper = new EntityMapper(new ConventionReader(new UpperCaseConvention()));

            AddValue("NAME", "a name");
            AddValue("ID", _guid);

            var instance = _mapper.CreateInstance<ClassWithoutRelations>(_columnValues);

            Assert.AreEqual("a name", instance.Name);
            Assert.AreEqual(_guid, instance.Id);
        }

        [Test]
        public void Creating_instance_with_column_names_prefixed_by_table_name_sets_corresponding_properties()
        {
            AddValue("Child Id", 2);
            AddValue("Child Name", "child name");

            var instance = _mapper.CreateInstance<Child>(_columnValues);

            Assert.AreEqual(2, instance.Id);
            Assert.AreEqual("child name", instance.Name);
        }

        [Test]
        public void Creating_instance_with_parent_property_without_any_values_for_parent_leaves_parent_property_as_null()
        {
            AddValue("Child Id", 2);

            var instance = _mapper.CreateInstance<Child>(_columnValues, _parentChildRelation);

            Assert.IsNull(instance.Parent);
        }

        [Test]
        public void Creating_instance_with_parent_property_with_single_value_for_parent_sets_parent_property_to_instance_with_properties_set()
        {
            AddValue("Child Id", 1);
            AddValue("Parent Id", _guid);

            var instance = _mapper.CreateInstance<Child>(_columnValues, _parentChildRelation);

            Assert.IsNotNull(instance.Parent);
            Assert.AreEqual(1, instance.Id);
            Assert.AreEqual(_guid, instance.Parent.Id);
        }

        [Test]
        public void Creating_child_with_parent_adds_child_to_parents_child_collection()
        {
            AddValue("Child Id", 1);
            AddValue("Parent Id", _guid);

            var child = _mapper.CreateInstance<Child>(_columnValues, _parentChildRelation);
            var parent = child.Parent;

            Assert.IsNotNull(parent);
            Assert.AreEqual(1, parent.Children.Count);
            Assert.AreSame(child, parent.Children.First());
        }

        [Test]
        public void Properties_and_table_names_are_matched_using_the_given_convention()
        {
            AddValue("CHILD ID", 1);
            AddValue("PARENT ID", _guid);
            AddValue("PARENT NAME", "parent name");
            _mapper = new EntityMapper(new ConventionReader(new UpperCaseConvention()));
            var instance = _mapper.CreateInstance<Child>(_columnValues, _parentChildRelation);

            Assert.IsNotNull(instance.Parent);
            Assert.AreEqual(1, instance.Id);
            Assert.AreEqual(_guid, instance.Parent.Id);
            Assert.AreEqual("parent name", instance.Parent.Name);

        }

        private void AddValue(string name, object value)
        {
            _columnValues.Add(new ColumnValue(name, value));
        }

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
            public Parent()
            {
                Children = new List<Child>();
            }

            public Guid Id { get; set; }
            public string Name { get; set; }

            public IList<Child> Children { get; set; }
        }

        private class Child
        {
            public Child()
            {
                GrandChildren = new List<GrandChild>();
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }

            public Parent Parent { get; set; }
            public IList<GrandChild> GrandChildren { get; set; }
        }

        private class GrandChild
        {
            public int Id { get; set; }
            public DateTime DateTime { get; set; }
        }
    }

}