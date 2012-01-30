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
        private Row _row;
        private Guid _guid = new Guid("00000000-0000-0000-0000-000000000001");
        private ObjectRelation _parentChildRelation;
        private ObjectRelation _childParentRelation;
        private ObjectRelation _childGrandChildRelation;

        [SetUp]
        public void SetUp()
        {
            _row = new Row();
            _mapper = new EntityMapper(new ConventionReader(new DefaultConvention()));
            _parentChildRelation = ObjectRelation.Create<Parent, Child>(x => x.Children, x => x.Parent, typeof(Parent));
            _childParentRelation = ObjectRelation.Create<Parent, Child>(x => x.Children, x => x.Parent, typeof(Child));
            _childGrandChildRelation = ObjectRelation.Create<Child, GrandChild>(x => x.GrandChildren, x => x.Child, typeof(Child));
        }

        [Test]
        public void Creating_instance_of_type_without_values_creates_empty_instance()
        {
            var instance = _mapper.CreateInstance<ClassWithoutRelations>(_row);

            Assert.AreEqual(null, instance.Name);
            Assert.AreEqual(Guid.Empty, instance.Id);
        }

        [Test]
        public void Creating_instance_with_single_value_with_same_column_name_as_property_name_sets_property()
        {
            _row.Add("Name", "a name");

            var instance = _mapper.CreateInstance<ClassWithoutRelations>(_row);

            Assert.AreEqual("a name", instance.Name);
        }

        [Test]
        public void Creating_instance_with_multiple_values_with_same_column_name_as_property_name_sets_properties()
        {
            _row.Add("Name", "a name");
            _row.Add("Id", _guid);

            var instance = _mapper.CreateInstance<ClassWithoutRelations>(_row);

            Assert.AreEqual("a name", instance.Name);
            Assert.AreEqual(_guid, instance.Id);
        }

        [ExpectedException(typeof(MissingDefaultConstructorException))]
        [Test]
        public void Trying_to_create_instance_without_default_constructor_throws_exception()
        {
            _mapper.CreateInstance<ClassWithoutDefaultConstructor>(_row);
        }

        [ExpectedException(typeof(MissingPropertyException))]
        [Test]
        public void Trying_to_create_instance_with_values_that_do_not_have_corresponding_properties_throws_exception()
        {
            _row.Add("MissingProperty", null);
            _mapper.CreateInstance<ClassWithoutRelations>(_row);
        }

        [Test]
        public void Creating_instance_with_multiple_values_with_column_name_according_to_convention_sets_corresponding_properties()
        {
            _mapper = new EntityMapper(new ConventionReader(new UpperCaseConvention()));

            _row.Add("NAME", "a name");
            _row.Add("ID", _guid);

            var instance = _mapper.CreateInstance<ClassWithoutRelations>(_row);

            Assert.AreEqual("a name", instance.Name);
            Assert.AreEqual(_guid, instance.Id);
        }

        [Test]
        public void Creating_instance_with_column_names_prefixed_by_table_name_sets_corresponding_properties()
        {
            _row.Add("Child Id", 2);
            _row.Add("Child Name", "child name");

            var instance = _mapper.CreateInstance<Child>(_row);

            Assert.AreEqual(2, instance.Id);
            Assert.AreEqual("child name", instance.Name);
        }

        [Ignore("Not that important, since it will not happen...")]
        [Test]
        public void Creating_instance_with_parent_property_without_any_values_for_parent_leaves_parent_property_as_null()
        {
            _row.Add("Child Id", 2);

            var instance = _mapper.CreateInstanceGraph<Child>(_row, _childParentRelation);

            Assert.IsNull(instance.Parent);
        }

        [Test]
        public void Creating_instance_with_parent_property_with_single_value_for_parent_sets_parent_property_to_instance_with_properties_set()
        {
            _row.Add("Child Id", 1);
            _row.Add("Parent Id", _guid);

            var instance = _mapper.CreateInstanceGraph<Child>(_row, _childParentRelation);

            Assert.IsNotNull(instance.Parent);
            Assert.AreEqual(1, instance.Id);
            Assert.AreEqual(_guid, instance.Parent.Id);
        }

        [Test]
        public void Creating_child_with_parent_adds_child_to_parents_child_collection()
        {
            _row.Add("Child Id", 1);
            _row.Add("Parent Id", _guid);

            var child = _mapper.CreateInstanceGraph<Child>(_row, _childParentRelation);
            var parent = child.Parent;

            Assert.IsNotNull(parent);
            Assert.AreEqual(1, parent.Children.Count);
            Assert.AreSame(child, parent.Children.First());
        }

        [Test]
        public void Creating_parent_from_row_with_both_parent_and_child_returns_parent_with_initialized_child_added_to_collection()
        {
            _row.Add("Child Id", 1);
            _row.Add("Parent Id", _guid);

            var parent = _mapper.CreateInstanceGraph<Parent>(_row, _parentChildRelation);
            var child = parent.Children.FirstOrDefault();

            Assert.AreEqual(1, parent.Children.Count);
            Assert.AreSame(parent, child.Parent);
            Assert.AreEqual(1, child.Id);
            Assert.AreEqual(_guid, parent.Id);
        }

        [Test]
        public void Properties_and_table_names_are_matched_using_the_given_convention()
        {
            _row.Add("CHILD ID", 1);
            _row.Add("PARENT ID", _guid);
            _row.Add("PARENT NAME", "parent name");

            _mapper = new EntityMapper(new ConventionReader(new UpperCaseConvention()));

            var instance = _mapper.CreateInstanceGraph<Child>(_row, _childParentRelation);

            Assert.IsNotNull(instance.Parent);
            Assert.AreEqual(1, instance.Id);
            Assert.AreEqual(_guid, instance.Parent.Id);
            Assert.AreEqual("parent name", instance.Parent.Name);
        }

        [Test]
        public void Creating_child_instances_from_two_rows_with_two_children_referencing_same_parent_gives_two_children_referencing_same_parent_instance()
        {
            var resultSet = new ResultSet();

            resultSet.AddRow(new ColumnValue("Child Id", 1), new ColumnValue("Parent Id", _guid));
            resultSet.AddRow(new ColumnValue("Child Id", 2), new ColumnValue("Parent Id", _guid));

            var children = _mapper.CreateInstanceGraphs<Child>(resultSet, _childParentRelation);

            Assert.AreEqual(2, children.Count);

            var parent = children.First().Parent;

            Assert.AreEqual(1, children.First().Id);
            Assert.AreEqual(2, children.Last().Id);
            Assert.AreEqual(_guid, parent.Id);
            
            Assert.AreSame(parent, children.Last().Parent);

            Assert.AreEqual(2, parent.Children.Count);
            CollectionAssert.Contains(parent.Children, children.First());
            CollectionAssert.Contains(parent.Children, children.Last());
        }
        
        [Test]
        public void Creating_instances_from_two_rows_with_single_parent_with_two_children_gives_single_parent_with_two_children()
        {
            var resultSet = new ResultSet();

            resultSet.AddRow(new ColumnValue("Child Id", 1), new ColumnValue("Parent Id", _guid));
            resultSet.AddRow(new ColumnValue("Child Id", 2), new ColumnValue("Parent Id", _guid));

            var parents = _mapper.CreateInstanceGraphs<Parent>(resultSet, _parentChildRelation);

            Assert.AreEqual(1, parents.Count);

            var parent = parents.First();
            var children = parent.Children;

            Assert.AreEqual(1, children.First().Id);
            Assert.AreEqual(2, children.Last().Id);
            Assert.AreSame(parent, children.First().Parent);
            Assert.AreSame(parent, children.Last().Parent);
        }

        [Test]
        public void Does_not_attempt_to_write_foreign_key_value_to_property_if_property_does_not_exist()
        {
            _row.Add("Child Id", 1);
            _row.Add("Child ParentId", _guid);
            _row.Add("Parent Id", _guid);

            var child = _mapper.CreateInstanceGraph<Child>(_row, _childParentRelation);

            Assert.AreEqual(1, child.Id);
            Assert.AreEqual(_guid, child.Parent.Id);
        }

        [Test]
        public void Single_row_with_three_level_relationship_can_be_mapped_to_entity_hierarchy()
        {
            _row.Add("Parent Id", _guid);
            _row.Add("Child Id", 1);
            _row.Add("GrandChild Id", 2);

            var parent = _mapper.CreateInstanceGraph<Parent>(_row, new [] { _parentChildRelation, _childGrandChildRelation });
            var child = parent.Children.FirstOrDefault();

            Assert.AreEqual(1, parent.Children.Count);
            Assert.AreEqual(1, child.GrandChildren.Count);

            Assert.AreEqual(_guid, parent.Id);
            Assert.AreEqual(1, child.Id);
            Assert.AreEqual(2, child.GrandChildren.First().Id);            
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

            public Child Child { get; set; }
        }
    }

}