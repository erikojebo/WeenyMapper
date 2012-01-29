using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Mapping;
using WeenyMapper.Reflection;
using WeenyMapper.Specs.TestClasses.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs.Mapping
{
    [TestFixture]
    public class IdPropertyComparerSpecs
    {
        private IdPropertyComparer<Book> _comparer;

        [SetUp]
        public void SetUp()
        {
            _comparer = new IdPropertyComparer<Book>(new ConventionReader(new BookConvention()));            
        }

        [Test]
        public void Returns_the_hash_code_of_the_id_property_of_an_entity()
        {
            var actualHashCode = _comparer.GetHashCode(new Book { Isbn = "An ISBN" });

            Assert.AreEqual("An ISBN".GetHashCode(), actualHashCode);
        }

        [Test]
        public void Entities_with_the_same_id_are_equal()
        {
            Assert.IsTrue(_comparer.Equals(new Book { Isbn = "An ISBN" }, new Book { Isbn = "An ISBN" }));
        }

        [Test]
        public void Entities_with_different_ids_are_not_equal()
        {
            Assert.IsFalse(_comparer.Equals(new Book { Isbn = "An ISBN" }, new Book { Isbn = "Another ISBN" }));
        }
    }
}