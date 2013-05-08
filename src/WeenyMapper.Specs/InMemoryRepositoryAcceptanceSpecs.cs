using NUnit.Framework;
using WeenyMapper.Conventions;

namespace WeenyMapper.Specs
{
    [TestFixture]
    public class InMemoryRepositoryAcceptanceSpecs : RepositoryAcceptanceSpecs
    {
        protected override void PerformSetUp()
        {
            Repository = new InMemoryRepository();
        }

        protected override Repository CreateRepository(IConvention convention)
        {
            return new InMemoryRepository { Convention = convention };
        }
    }
}