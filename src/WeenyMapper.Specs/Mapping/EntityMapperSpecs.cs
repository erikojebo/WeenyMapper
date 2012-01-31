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
        private Guid _guid1 = new Guid("00000000-0000-0000-0000-000000000001");
        private Guid _guid2 = new Guid("00000000-0000-0000-0000-000000000002");
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
            _row.Add("Id", _guid1);

            var instance = _mapper.CreateInstance<ClassWithoutRelations>(_row);

            Assert.AreEqual("a name", instance.Name);
            Assert.AreEqual(_guid1, instance.Id);
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
            _row.Add("MissingProperty", "a value");
            _mapper.CreateInstance<ClassWithoutRelations>(_row);
        }

        [Test]
        public void Creating_instance_with_multiple_values_with_column_name_according_to_convention_sets_corresponding_properties()
        {
            _mapper = new EntityMapper(new ConventionReader(new UpperCaseConvention()));

            _row.Add("NAME", "a name");
            _row.Add("ID", _guid1);

            var instance = _mapper.CreateInstance<ClassWithoutRelations>(_row);

            Assert.AreEqual("a name", instance.Name);
            Assert.AreEqual(_guid1, instance.Id);
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

        [Test]
        public void Creating_instance_with_parent_property_with_single_value_for_parent_sets_parent_property_to_instance_with_properties_set()
        {
            _row.Add("Child Id", 1);
            _row.Add("Parent Id", _guid1);

            var instance = _mapper.CreateInstanceGraph<Child>(_row, _childParentRelation);

            Assert.IsNotNull(instance.Parent);
            Assert.AreEqual(1, instance.Id);
            Assert.AreEqual(_guid1, instance.Parent.Id);
        }

        [Test]
        public void Creating_child_with_parent_adds_child_to_parents_child_collection()
        {
            _row.Add("Child Id", 1);
            _row.Add("Parent Id", _guid1);

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
            _row.Add("Parent Id", _guid1);

            var parent = _mapper.CreateInstanceGraph<Parent>(_row, _parentChildRelation);
            var child = parent.Children.FirstOrDefault();

            Assert.AreEqual(1, parent.Children.Count);
            Assert.AreSame(parent, child.Parent);
            Assert.AreEqual(1, child.Id);
            Assert.AreEqual(_guid1, parent.Id);
        }

        [Test]
        public void Creating_parent_with_null_values_for_child_leaves_child_collection_empty()
        {
            _row.Add("Child Id", DBNull.Value);
            _row.Add("Child Name", DBNull.Value);
            _row.Add("Parent Id", _guid1);

            var parent = _mapper.CreateInstanceGraph<Parent>(_row, _parentChildRelation);

            Assert.AreEqual(_guid1, parent.Id);
            Assert.AreEqual(0, parent.Children.Count);
        }

        [Test]
        public void Creating_child_with_null_values_for_parent_leaves_parent_as_null()
        {
            _row.Add("Child Id", 1);
            _row.Add("Parent Id", DBNull.Value);
            _row.Add("Parent Name", DBNull.Value);

            var child = _mapper.CreateInstanceGraph<Child>(_row, _childParentRelation);

            Assert.AreEqual(1, child.Id);
            Assert.IsNull(child.Parent);
        }

        [Test]
        public void Properties_and_table_names_are_matched_using_the_given_convention()
        {
            _row.Add("CHILD ID", 1);
            _row.Add("PARENT ID", _guid1);
            _row.Add("PARENT NAME", "parent name");

            _mapper = new EntityMapper(new ConventionReader(new UpperCaseConvention()));

            var instance = _mapper.CreateInstanceGraph<Child>(_row, _childParentRelation);

            Assert.IsNotNull(instance.Parent);
            Assert.AreEqual(1, instance.Id);
            Assert.AreEqual(_guid1, instance.Parent.Id);
            Assert.AreEqual("parent name", instance.Parent.Name);
        }

        [Test]
        public void Creating_child_instances_from_two_rows_with_two_children_referencing_same_parent_gives_two_children_referencing_same_parent_instance()
        {
            var resultSet = new ResultSet();

            resultSet.AddRow(new ColumnValue("Child Id", 1), new ColumnValue("Parent Id", _guid1));
            resultSet.AddRow(new ColumnValue("Child Id", 2), new ColumnValue("Parent Id", _guid1));

            var children = _mapper.CreateInstanceGraphs<Child>(resultSet, _childParentRelation);

            Assert.AreEqual(2, children.Count);

            var parent = children.First().Parent;

            Assert.AreEqual(1, children.First().Id);
            Assert.AreEqual(2, children.Last().Id);
            Assert.AreEqual(_guid1, parent.Id);
            
            Assert.AreSame(parent, children.Last().Parent);

            Assert.AreEqual(2, parent.Children.Count);
            CollectionAssert.Contains(parent.Children, children.First());
            CollectionAssert.Contains(parent.Children, children.Last());
        }
        
        [Test]
        public void Creating_instances_from_two_rows_with_single_parent_with_two_children_gives_single_parent_with_two_children()
        {
            var resultSet = new ResultSet();

            resultSet.AddRow(new ColumnValue("Child Id", 1), new ColumnValue("Parent Id", _guid1));
            resultSet.AddRow(new ColumnValue("Child Id", 2), new ColumnValue("Parent Id", _guid1));

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
            _row.Add("Child ParentId", _guid1);
            _row.Add("Parent Id", _guid1);

            var child = _mapper.CreateInstanceGraph<Child>(_row, _childParentRelation);

            Assert.AreEqual(1, child.Id);
            Assert.AreEqual(_guid1, child.Parent.Id);
        }

        [Test]
        public void Single_row_with_three_level_relationship_can_be_mapped_to_entity_hierarchy()
        {
            _row.Add("Parent Id", _guid1);
            _row.Add("Child Id", 1);
            _row.Add("GrandChild Id", 2);

            var parent = _mapper.CreateInstanceGraph<Parent>(_row, new [] { _parentChildRelation, _childGrandChildRelation });
            var child = parent.Children.FirstOrDefault();

            Assert.AreEqual(1, parent.Children.Count);
            Assert.AreEqual(1, child.GrandChildren.Count);

            Assert.AreEqual(_guid1, parent.Id);
            Assert.AreEqual(1, child.Id);
            Assert.AreEqual(2, child.GrandChildren.First().Id);            
        }

        [Test]
        public void Two_rows_sharing_no_entities_are_mapped_to_two_separate_hierachies()
        {
            var resultSet = new ResultSet();

            resultSet.AddRow(new ColumnValue("Parent Id", _guid1), new ColumnValue("Child Id", 1), new ColumnValue("GrandChild Id", 2));
            resultSet.AddRow(new ColumnValue("Parent Id", _guid2), new ColumnValue("Child Id", 3), new ColumnValue("GrandChild Id", 4));

            var parents = _mapper.CreateInstanceGraphs<Parent>(resultSet, new [] { _parentChildRelation, _childGrandChildRelation });

            Assert.AreEqual(2, parents.Count);

            var firstParent = parents.First();
            var secondParent = parents.Last();
            var firstChild = firstParent.Children.FirstOrDefault();
            var secondChild = secondParent.Children.FirstOrDefault();

            Assert.AreEqual(1, firstParent.Children.Count);
            Assert.AreEqual(1, firstChild.GrandChildren.Count);

            Assert.AreEqual(_guid1, firstParent.Id);
            Assert.AreEqual(1, firstChild.Id);
            Assert.AreEqual(2, firstChild.GrandChildren.First().Id);            
            
            Assert.AreEqual(_guid2, secondParent.Id);
            Assert.AreEqual(3, secondChild.Id);
            Assert.AreEqual(4, secondChild.GrandChildren.First().Id);            
        }

        [Test]
        public void Two_rows_sharing_some_entities_are_mapped_to_two_hierachies_sharing_instances()
        {
            var resultSet = new ResultSet();

            resultSet.AddRow(new ColumnValue("Parent Id", _guid1), new ColumnValue("Child Id", 1), new ColumnValue("GrandChild Id", 2));
            resultSet.AddRow(new ColumnValue("Parent Id", _guid2), new ColumnValue("Child Id", 3), new ColumnValue("GrandChild Id", 2));
            resultSet.AddRow(new ColumnValue("Parent Id", _guid1), new ColumnValue("Child Id", 2), new ColumnValue("GrandChild Id", 4));
            resultSet.AddRow(new ColumnValue("Parent Id", _guid1), new ColumnValue("Child Id", 2), new ColumnValue("GrandChild Id", 5));

            var parents = _mapper.CreateInstanceGraphs<Parent>(resultSet, new [] { _parentChildRelation, _childGrandChildRelation });

            Assert.AreEqual(2, parents.Count);

            var firstParent = parents.First();
            var secondParent = parents.Last();
            var firstParentChild1 = firstParent.Children.FirstOrDefault();
            var firstParentChild2 = firstParent.Children.LastOrDefault();
            var secondParentChild = secondParent.Children.FirstOrDefault();

            Assert.AreEqual(2, firstParent.Children.Count);
            Assert.AreEqual(1, firstParentChild1.GrandChildren.Count);
            Assert.AreEqual(2, firstParentChild2.GrandChildren.Count);

            Assert.AreEqual(_guid1, firstParent.Id);
            Assert.AreEqual(1, firstParentChild1.Id);
            Assert.AreEqual(2, firstParentChild1.GrandChildren.First().Id);            
            Assert.AreEqual(2, firstParentChild2.Id);
            Assert.AreEqual(4, firstParentChild2.GrandChildren.First().Id);            
            Assert.AreEqual(5, firstParentChild2.GrandChildren.Last().Id);            
            
            Assert.AreEqual(_guid2, secondParent.Id);
            Assert.AreEqual(3, secondParentChild.Id);
            Assert.AreEqual(2, secondParentChild.GrandChildren.First().Id);

            Assert.AreSame(firstParentChild1.GrandChildren.First(), secondParentChild.GrandChildren.First());
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