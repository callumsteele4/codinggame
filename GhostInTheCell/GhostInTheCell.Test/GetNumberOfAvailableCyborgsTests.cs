using System.Collections.Generic;
using Xunit;

namespace GhostInTheCell.Test
{
    public class GetNumberOfAvailableCyborgsTests
    {
        private const int Factory1Id = 0;
        private const int Troop1Id = 1;
//        private const int Troop2Id = 2;
        private readonly Robot _robot;

        public GetNumberOfAvailableCyborgsTests()
        {
            _robot = new Robot(null);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        public void GetNumberOfAvailableCyborgs_no_troops(int numberOfCyborgsInFactory, int expectedAvailableCyborgs)
        {
            var factory1 = new Factory2(Factory1Id)
            {
                NumberOfCyborgs = numberOfCyborgsInFactory
            };
            var state = new State
            {
                Factories = new List<Factory2>
                {
                    factory1
                },
                Troops = new List<Troop2>()
            };
            
            Assert.Equal(
                expectedAvailableCyborgs,
                _robot.GetNumberOfAvailableCyborgs(factory1, state));
        }

        [Theory]
        // Cyborgs in factory
        [InlineData(0, 0, 1, 1, 0)]
        [InlineData(1, 0, 1, 1, 0)]
        [InlineData(2, 0, 1, 1, 1)]
        // Production and turns until arriving
        [InlineData(0, 1, 2, 1, 0)]
        [InlineData(0, 1, 2, 2, 0)]
        [InlineData(0, 2, 2, 1, 0)]
        [InlineData(0, 2, 2, 2, 0)]
        // Cyborgs in factory, production, and turns until arriving
        [InlineData(1, 1, 1, 1, 1)]
        [InlineData(1, 1, 2, 1, 0)]
        [InlineData(1, 1, 3, 1, 0)]
        [InlineData(1, 1, 2, 2, 1)]
        [InlineData(1, 1, 2, 3, 1)]
        [InlineData(2, 1, 2, 1, 1)]
        [InlineData(2, 2, 2, 1, 2)]
        [InlineData(2, 3, 2, 1, 2)]
        [InlineData(3, 1, 2, 1, 2)]
        [InlineData(3, 2, 2, 1, 3)]
        [InlineData(3, 3, 2, 1, 3)]
        public void GetNumberOfAvailableCyborgs_single_enemy_troop(
            int numberOfCyborgsInFactory,
            int factoryProduction,
            int numberOfCyborgsInEnemyTroop,
            int enemyTroopRemainingTravelTime,
            int expectedAvailableCyborgs)
        {
            var factory1 = new Factory2(Factory1Id)
            {
                NumberOfCyborgs = numberOfCyborgsInFactory,
                Production = factoryProduction
            };
            var state = new State
            {
                Factories = new List<Factory2>
                {
                    factory1
                },
                Troops = new List<Troop2>
                {
                    new Troop2(Troop1Id)
                    {
                        Destination = Factory1Id,
                        NumberOfCyborgs = numberOfCyborgsInEnemyTroop,
                        Owner = Owner2.Opponent,
                        RemainingTravelTime = enemyTroopRemainingTravelTime
                    }
                }
            };
            
            Assert.Equal(
                expectedAvailableCyborgs,
                _robot.GetNumberOfAvailableCyborgs(factory1, state));
        }
        
        [Theory]
        [InlineData(0, 1, 0)]
        [InlineData(1, 1, 1)]
        public void GetNumberOfAvailableCyborgs_single_own_troop(
            int numberOfCyborgsInFactory, int numberOfCyborgsInOwnTroop, int expectedAvailableCyborgs)
        {
            var factory1 = new Factory2(Factory1Id)
            {
                NumberOfCyborgs = numberOfCyborgsInFactory
            };
            var state = new State
            {
                Factories = new List<Factory2>
                {
                    factory1
                },
                Troops = new List<Troop2>
                {
                    new Troop2(Troop1Id)
                    {
                        Destination = Factory1Id,
                        NumberOfCyborgs = numberOfCyborgsInOwnTroop,
                        Owner = Owner2.Own
                    }
                }
            };
            
            Assert.Equal(
                expectedAvailableCyborgs,
                _robot.GetNumberOfAvailableCyborgs(factory1, state));
        }
        
//        [Theory]
//        [InlineData(0, 1, 1, -1)]
//        [InlineData(1, 1, 1, 0)]
//        [InlineData(2, 1, 1, 1)]
//        public void GetNumberOfAvailableCyborgs_single_enemy_and_own_troop(
//            int numberOfCyborgsInFactory,
//            int numberOfCyborgsInEnemyTroop,
//            int numberOfCyborgsInOwnTroop,
//            int expectedAvailableCyborgs)
//        {
//            var factory1 = new Factory2(Factory1Id)
//            {
//                NumberOfCyborgs = numberOfCyborgsInFactory
//            };
//            var state = new State
//            {
//                Factories = new[]
//                {
//                    factory1
//                },
//                Troops = new List<Troop2>
//                {
//                    new Troop2(Troop1Id)
//                    {
//                        Destination = Factory1Id,
//                        NumberOfCyborgs = numberOfCyborgsInEnemyTroop,
//                        Owner = Owner2.Opponent
//                    },
//                    new Troop2(Troop2Id)
//                    {
//                        Destination = Factory1Id,
//                        NumberOfCyborgs = numberOfCyborgsInOwnTroop,
//                        Owner = Owner2.Own
//                    }
//                }
//            };
//            
//            Assert.Equal(
//                expectedAvailableCyborgs,
//                _robot.GetNumberOfAvailableCyborgs(factory1, state));
//        }
    }
}