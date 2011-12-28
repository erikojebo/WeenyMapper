using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Extensions;

namespace WeenyMapper.Specs.Extensions
{
    [TestFixture]
    public class DictionaryExtensionsSpecs
    {
        [Test]
        public void Transforming_keys_for_empty_dictionary_returns_empty_dictionary()
        {
            var dictionary = new Dictionary<string, int>();

            var transformed = dictionary.TransformKeys(x => x.ToUpper());

            CollectionAssert.IsEmpty(transformed);
        }

        [Test]
        public void Transforming_keys_does_not_modify_original_dictionary()
        {
            var original = new Dictionary<string, int>
                {
                    { "Key 1", 1 }
                };

            original.TransformKeys(x => x.ToUpper());

            Assert.AreEqual("Key 1", original.First().Key);
            Assert.AreEqual(1, original.First().Value);
        }

        [Test]
        public void Transforming_keys_for_non_empty_dictionary_returns_dictionary_with_transformed_keys()
        {
            var dictionary = new Dictionary<string, int>
                {
                    { "Key 1", 1 },
                    { "Key 2", 2 },
                };
            
            var transformed = dictionary.TransformKeys(x => x.ToUpper());

            Assert.AreEqual(2, transformed.Count);

            Assert.AreEqual("KEY 1", transformed.First().Key);
            Assert.AreEqual(1, transformed.First().Value);

            Assert.AreEqual("KEY 2", transformed.Last().Key);
            Assert.AreEqual(2, transformed.Last().Value);
        }
    }
}