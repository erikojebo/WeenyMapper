using NUnit.Framework;
using WeenyMapper.Conventions;
using WeenyMapper.Specs.TestClasses.Conventions;

namespace WeenyMapper.Specs
{
    [TestFixture]
    public class RepositorySpecs
    {
        private Repository _repository;

        [SetUp]
        public void SetUp()
        {
            _repository = new Repository();
        }

        [Test]
        public void A_repository_without_an_explicit_convention_uses_the_default_convention()
        {
            var expectedConvention = new BookConvention();
            Repository.DefaultConvention = expectedConvention;

            var repositoryWithoutExplicitConvention = new Repository();

            Assert.AreSame(expectedConvention, repositoryWithoutExplicitConvention.Convention);
        }

        [Test]
        public void Setting_the_default_convention_after_creating_a_repository_without_specifying_an_explicit_convention_uses_the_new_default_convention()
        {
            var repositoryWithoutExplicitConvention = new Repository();

            var expectedConvention = new BookConvention();

            Repository.DefaultConvention = expectedConvention;

            Assert.AreSame(expectedConvention, repositoryWithoutExplicitConvention.Convention);
        }

        [Test]
        public void Setting_an_explicit_convention_overrides_the_default_convention()
        {
            Repository.DefaultConvention = new DefaultConvention();

            var expectedConvention = new BookConvention();
            
            _repository.Convention = expectedConvention;

            Assert.AreEqual(expectedConvention, _repository.Convention);
            
        }

        [Test]
        public void A_repository_without_an_explicit_connection_string_uses_the_default_connection_string()
        {
            Repository.DefaultConnectionString = "default connection string";

            var repositoryWithoutExplicitConnectionString = new Repository();

            Assert.AreEqual("default connection string", repositoryWithoutExplicitConnectionString.ConnectionString);
            
        }

        [Test]
        public void Setting_the_default_connection_string_after_creating_a_repository_without_specifying_an_explicit_connection_string_uses_the_new_default_connection_string()
        {
            var repositoryWithoutExplicitConnectionString = new Repository();

            Repository.DefaultConnectionString = "default connection string";

            Assert.AreEqual("default connection string", repositoryWithoutExplicitConnectionString.ConnectionString);
        }

        [Test]
        public void Setting_an_explicit_connection_string_overrides_the_default_connection_string()
        {
            Repository.DefaultConnectionString = "default connection string";
            
            _repository.ConnectionString = "explicit connection string";

            Assert.AreEqual("explicit connection string", _repository.ConnectionString);
            
        }
    }
}