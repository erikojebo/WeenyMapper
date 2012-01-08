using System.Collections.Generic;
using NUnit.Framework;
using WeenyMapper.Extensions;

namespace WeenyMapper.Specs.Extensions
{
    [TestFixture]
    public class EnumerableExtensionsSpecs
    {
        private List<Element> _collection1;
        private List<Element> _collection2;

        [SetUp]
        public void SetUp()
        {
            _collection1 = new List<Element>();
            _collection2 = new List<Element>();
        }

        [Test]
        public void Empty_collections_are_element_equal()
        {
            Assert.IsTrue(_collection1.ElementEquals(_collection2));
        }

        [Test]
        public void Collection_with_element_is_not_element_equal_to_empty_collection()
        {
            _collection1.Add(new Element(1));

            Assert.IsFalse(_collection1.ElementEquals(_collection2));
        }
        
        [Test]
        public void Empty_collection_is_not_element_equal_to_collection_with_element()
        {
            _collection2.Add(new Element(1));

            Assert.IsFalse(_collection1.ElementEquals(_collection2));
        }

        [Test]
        public void Collections_with_single_equal_item_are_element_equal()
        {
            _collection1.Add(new Element(1));
            _collection2.Add(new Element(1));

            Assert.IsTrue(_collection1.ElementEquals(_collection2));
        }

        [Test]
        public void Collections_with_single_non_equal_item_are_not_element_equal()
        {
            _collection1.Add(new Element(1));
            _collection2.Add(new Element(2));

            Assert.IsFalse(_collection1.ElementEquals(_collection2));
        }

        [Test]
        public void Collections_with_one_equal_and_one_non_equal_element_are_not_element_equal()
        {
            _collection1.Add(new Element(1));
            _collection1.Add(new Element(2));
            _collection2.Add(new Element(1));
            _collection2.Add(new Element(3));

            Assert.IsFalse(_collection1.ElementEquals(_collection2));
        }

        [Test]
        public void Collections_with_non_equal_followed_by_equal_element_are_not_element_equal()
        {
            _collection1.Add(new Element(2));
            _collection1.Add(new Element(1));
            _collection2.Add(new Element(3));
            _collection2.Add(new Element(1));

            Assert.IsFalse(_collection1.ElementEquals(_collection2));
        }

        [Test]
        public void Collection_with_null_element_is_not_element_equal_to_collection_with_non_null_element()
        {
            _collection1.Add(null);
            _collection2.Add(new Element(1));

            Assert.IsFalse(_collection1.ElementEquals(_collection2));
        }

        [Test]
        public void Collections_with_equal_elements_in_different_order_are_not_element_equal()
        {
            _collection1.Add(new Element(1));
            _collection1.Add(new Element(2));
            _collection2.Add(new Element(2));
            _collection2.Add(new Element(1));

            Assert.IsFalse(_collection1.ElementEquals(_collection2));
        }

        private class Element
        {
            private readonly int _id;

            public Element(int id)
            {
                _id = id;
            }

            public override int GetHashCode()
            {
                return _id;
            }

            public override bool Equals(object obj)
            {
                return _id == ((Element)obj)._id;
            }
        }
    }
}