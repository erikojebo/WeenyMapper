using NUnit.Framework;

namespace WeenyMapper.Specs
{
    [TestFixture]
    public class InMemoryRepositoryAcceptanceSpecs : RepositoryAcceptanceSpecs
    {
        protected override void PerformSetUp()
        {
            Repository = new InMemoryRepository();
        }
    }
}