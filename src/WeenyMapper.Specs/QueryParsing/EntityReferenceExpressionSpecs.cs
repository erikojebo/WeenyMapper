using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Specs.TestClasses.Conventions;
using WeenyMapper.Specs.TestClasses.Entities;

namespace WeenyMapper.Specs.QueryParsing
{
    [TestFixture]
    public class EntityReferenceExpressionSpecs
    {
        [Test]
        public void Translate_returns_property_expression_with_foreign_key_name_according_to_convention_for_reference_property_as_property_name()
        {
            var blogProperty = Reflector<BlogPost>.GetProperty(x => x.Blog);
            var blogIdProperty = Reflector<Blog>.GetProperty(x => x.Id);

            var entityReferenceExpression = new EntityReferenceExpression(blogProperty, blogIdProperty);

            var expectedExpression = new PropertyExpression("BLOGID");

            Assert.AreEqual(expectedExpression, entityReferenceExpression.Translate(new UpperCaseConvention()));
        }
    }
}