using System;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace GhostInTheCell.Test
{
    public class UpdateStateTests
    {
        private const string FactoryEntityType = "FACTORY";
        private const string TroopEntityType = "TROOP";
        private const string BombEntityType = "BOMB";

        private readonly Mock<IConsole> _consoleMock;
        private readonly Robot _robot;

        public UpdateStateTests()
        {
            _consoleMock = new Mock<IConsole>();
            _robot = new Robot(_consoleMock.Object);
        }

        [Fact]
        public void UpdateState_unknown_entity_type_throws()
        {
            const int entityCount = 1;
            const int entityId = 1;
            const string unknownEntityType = "UNKNOWN";
            const Owner2 entityOwner = Owner2.Own;
            var previousState = new State();

            _consoleMock
                .SetupSequence(c => c.ReadLine())
                .Returns($"{entityCount}")
                .Returns($"{entityId} " +
                         $"{unknownEntityType} " +
                         $"{(int) entityOwner}");

            Assert.Throws<ArgumentOutOfRangeException>(() => _robot.UpdateState(previousState));
        }
        
        [Fact]
        public void UpdateState_updates_factory()
        {
            const int entityCount = 1;
            const int factory1Id = 0;
            const int factory2Id = 1;
            const int distance = 1;
            const Owner2 factoryOwner = Owner2.Own;
            const int numberOfCyborgs = 10;
            const int production = 1;
            const int turnsUntilProducing = 0;
            var previousFactory = new Factory2(factory1Id)
            {
                Distances = new Dictionary<int, int>()
                {
                    {factory2Id, distance}
                }
            };
            var previousState = new State
            {
                Factories = new List<Factory2>
                {
                    previousFactory
                }
            };
            
            _consoleMock
                .SetupSequence(c => c.ReadLine())
                .Returns($"{entityCount}")
                .Returns($"{factory1Id} " +
                         $"{FactoryEntityType} " +
                         $"{(int) factoryOwner} " +
                         $"{numberOfCyborgs} " +
                         $"{production} " +
                         $"{turnsUntilProducing}");

            var state = _robot.UpdateState(previousState);

            var factory1 = state.Factories[factory1Id];
            Assert.Equal(factory1Id, factory1.Id);
            Assert.Equal(factoryOwner, factory1.Owner);
            Assert.Equal(numberOfCyborgs, factory1.NumberOfCyborgs);
            Assert.Equal(previousFactory.Distances, factory1.Distances);
            Assert.Equal(production, factory1.Production);
            Assert.Equal(turnsUntilProducing, factory1.TurnsUntilProducing);
        }

        [Fact]
        public void UpdateState_adds_troop()
        {
            const int entityCount = 1;
            const int troop1Id = 0;
            const Owner2 troopOwner = Owner2.Own;
            const int originFactoryId = 0;
            const int destinationFactoryId = 1;
            const int numberOfCyborgs = 10;
            const int remainingTravelTime = 2;
            var previousState = new State();

            _consoleMock
                .SetupSequence(c => c.ReadLine())
                .Returns($"{entityCount}")
                .Returns($"{troop1Id} " +
                         $"{TroopEntityType} " +
                         $"{(int) troopOwner} " +
                         $"{originFactoryId} " +
                         $"{destinationFactoryId} " +
                         $"{numberOfCyborgs} " +
                         $"{remainingTravelTime}");

            var state = _robot.UpdateState(previousState);

            var troop = Assert.Single(state.Troops);
            Assert.NotNull(troop);
            Assert.Equal(troop1Id, troop.Id);
            Assert.Equal(troopOwner, troop.Owner);
            Assert.Equal(originFactoryId, troop.Origin);
            Assert.Equal(destinationFactoryId, troop.Destination);
            Assert.Equal(numberOfCyborgs, troop.NumberOfCyborgs);
            Assert.Equal(remainingTravelTime, troop.RemainingTravelTime);
        }

        [Fact]
        public void UpdateState_adds_bomb()
        {
            const int entityCount = 1;
            const int bomb1Id = 0;
            const Owner2 bombOwner = Owner2.Own;
            const int bombOriginFactoryId = 0;
            const int bombDestinationFactoryId = 1;
            const int turnsUntilExplodes = 5;
            var previousState = new State();
            
            _consoleMock
                .SetupSequence(c => c.ReadLine())
                .Returns($"{entityCount}")
                .Returns($"{bomb1Id} " +
                         $"{BombEntityType} " +
                         $"{(int) bombOwner} " +
                         $"{bombOriginFactoryId} " +
                         $"{bombDestinationFactoryId} " +
                         $"{turnsUntilExplodes}");

            var state = _robot.UpdateState(previousState);

            var bomb = Assert.Single(state.Bombs);
            Assert.NotNull(bomb);
            Assert.Equal(bomb1Id, bomb.Id);
            Assert.Equal(bombOwner, bomb.Owner);
            Assert.Equal(bombOriginFactoryId, bomb.Origin);
            Assert.Equal(bombDestinationFactoryId, bomb.Destination);
            Assert.Equal(turnsUntilExplodes, bomb.TurnsUntilExplodes);
        }
    }
}