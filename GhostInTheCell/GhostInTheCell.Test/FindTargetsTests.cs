using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GhostInTheCell.Test
{
    public class FindTargetsTests
    {
        private const int Factory1Id = 0;
        private readonly Factory2 _originFactory;

        private readonly State _state;
        private const int Factory2Id = 1;

        private readonly Robot _robot;

        public FindTargetsTests()
        {
            _robot = new Robot(null);
            _originFactory = new Factory2(Factory1Id)
            {
                Owner = Owner2.Own
            };
            _state = new State
            {
                Factories = new List<Factory2>
                {
                    _originFactory
                }
            };
        }
        
        [Fact]
        public void FindTargets_no_factories_returns_no_targets()
        {
            var targets = _robot.FindTargets(_originFactory, _state);

            Assert.Empty(targets);
        }

        [Theory]
        [InlineData(Owner2.Opponent)]
        [InlineData(Owner2.Neutral)]
        [InlineData(Owner2.Own)]
        public void FindTargets_single_factory_no_link_returns_no_target(Owner2 owner)
        {
            var factory = new Factory2(Factory2Id)
            {
                Owner = owner,
                Production = 1,
                Distances = new Dictionary<int, int>()
            };
            _state.Factories.Add(factory);

            var targets = _robot.FindTargets(_originFactory, _state);

            Assert.Empty(targets);
        }

        [Theory]
        [InlineData(Owner2.Opponent)]
        [InlineData(Owner2.Neutral)]
        public void FindTargets_single_factory_link_returns_target(Owner2 owner)
        {
            var factory = new Factory2(Factory2Id)
            {
                Owner = owner,
                Production = 1,
                Distances = new Dictionary<int, int>
                {
                    {Factory1Id, 1}
                }
            };
            _state.Factories.Add(factory);

            var targets = _robot.FindTargets(_originFactory, _state);

            var target = Assert.Single(targets);
            Assert.Equal(factory, target);
        }

        [Fact]
        public void FindTargets_single_own_factory_with_link_returns_no_target()
        {
            var ownFactory = new Factory2(Factory2Id)
            {
                Owner = Owner2.Own,
                Production = 1,
                Distances = new Dictionary<int, int>
                {
                    {Factory1Id, 1}
                }
            };
            _state.Factories.Add(ownFactory);

            var targets = _robot.FindTargets(_originFactory, _state);

            Assert.Empty(targets);
        }

        [Fact]
        public void FindTargets_no_production_factory_returns_no_target()
        {
            var factory = new Factory2(Factory2Id)
            {
                Owner = Owner2.Neutral,
                Production = 0,
                Distances = new Dictionary<int, int>
                {
                    {Factory1Id, 1}
                }
            };
            _state.Factories.Add(factory);

            var targets = _robot.FindTargets(_originFactory, _state);

            Assert.Empty(targets);
        }

        [Fact]
        public void FindTargets_orders_factories_by_distance()
        {
            var expectedCloseFactory = new Factory2(Factory2Id)
            {
                Owner = Owner2.Neutral,
                Production = 1,
                Distances = new Dictionary<int, int>
                {
                    {Factory1Id, 1}
                }
            };
            const int factory3Id = 2;
            var expectedFarFactory = new Factory2(factory3Id)
            {
                Owner = Owner2.Neutral,
                Production = 1,
                Distances = new Dictionary<int, int>
                {
                    {Factory1Id, 2}
                }
            };
            _state.Factories.Add(expectedFarFactory);
            _state.Factories.Add(expectedCloseFactory);

            var targets = _robot.FindTargets(_originFactory, _state);

            var closeFactory = targets[0];
            Assert.Equal(expectedCloseFactory, closeFactory);

            var farFactory = targets[1];
            Assert.Equal(expectedFarFactory, farFactory);
        }
    }
}