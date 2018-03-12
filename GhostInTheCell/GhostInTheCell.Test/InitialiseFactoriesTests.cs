using Moq;
using Xunit;

namespace GhostInTheCell.Test
{
    public class InitialiseFactoriesTests
    {
        private const int Factory1Id = 0;
        private const int Factory2Id = 1;
        private const int Factory3Id = 2;

        private readonly Mock<IConsole> _consoleMock;
        private readonly Robot _robot;

        public InitialiseFactoriesTests()
        {
            _consoleMock = new Mock<IConsole>();
            _robot = new Robot(_consoleMock.Object);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(100)]
        public void InitialiseFactories_factories_no_links(int factoryCount)
        {
            const int linkCount = 0;
            
            _consoleMock
                .SetupSequence(c => c.ReadLine())
                .Returns($"{factoryCount}")
                .Returns($"{linkCount}");

            var factories = _robot.InitialiseFactories();

            Assert.Equal(factoryCount, factories.Count);
        }

        [Fact]
        public void InitialiseFactories_two_factories_with_link()
        {
            const int factoryCount = 2;
            const int linkCount = 1;
            const int distance = 10;
            
            _consoleMock
                .SetupSequence(c => c.ReadLine())
                .Returns($"{factoryCount}")
                .Returns($"{linkCount}")
                .Returns($"{Factory1Id} {Factory2Id} {distance}");

            var factories = _robot.InitialiseFactories();
            
            var factory1Link = Assert.Single(factories[Factory1Id].Distances);
            Assert.Equal(Factory2Id, factory1Link.Key);
            Assert.Equal(distance, factory1Link.Value);

            var factory2Link = Assert.Single(factories[Factory2Id].Distances);
            Assert.Equal(Factory1Id, factory2Link.Key);
            Assert.Equal(distance, factory2Link.Value);
        }

        [Fact]
        public void InitialiseFactories_factories_with_factory_with_multiple_links()
        {
            const int factoryCount = 3;
            const int linkCount = 2;
            const int link1Distance = 10;
            const int link2Distance = 20;

            _consoleMock
                .SetupSequence(c => c.ReadLine())
                .Returns($"{factoryCount}")
                .Returns($"{linkCount}")
                .Returns($"{Factory1Id} {Factory2Id} {link1Distance}")
                .Returns($"{Factory2Id} {Factory3Id} {link2Distance}");

            var factories = _robot.InitialiseFactories();

            var factory2Links = factories[Factory2Id].Distances;
            Assert.Equal(linkCount, factory2Links.Count);
            Assert.Equal(link1Distance, factory2Links[Factory1Id]);
            Assert.Equal(link2Distance, factory2Links[Factory3Id]);
        }
    }
}