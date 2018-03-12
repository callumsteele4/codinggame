using System;
using System.Collections.Generic;
using System.Linq;

namespace GhostInTheCell
{
    internal class Player
    {
        private static int _bombsLeft = 2;
        private static readonly List<Factory> Factories = new List<Factory>();
        private static List<Troop> _troops;
        
        private static void Main()
        {
            InitialiseFactories();

            // game loop
            while (true)
            {
                PopulateEntities();
                
                /* TODO: Add a time till taken
                    We only want to send Cyborgs to nodes that we want to take
                    or nodes that are going to be taken from us.
                    
                    We can represent this with an OwnershipEvent list on each factory.
                    
                    Each turn, we will extrapolate all future changes in ownership of factories, taking
                    into account their production, and all troops heading in their direction.
                    
                    If this is applied as a function to a factory, we can extrapolate what our future moves
                    could do to the ownership of a node, and thus decide whether to send troops to it.
                */ 
                PredictFuture();

                // Any valid action, such as "WAIT" or "MOVE source destination cyborgs"
                Console.WriteLine(
                    GetMove());
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static string GetMove()
        {
            var moves = Factories
                .Where(f => f.Owner == Owner.Mine)
                .Select(GetFactoryMove)
                .Where(move => !string.IsNullOrWhiteSpace(move))
                .ToList();
            
            // Decide whether to fire a bomb
            if (_bombsLeft != 0)
            {
                var potentialBombTarget = Factories
                    .FirstOrDefault(f => 
                        f.FutureOwner == Owner.Opponent && f.Production == 3);
                if (potentialBombTarget != null)
                {
                    var bombSourceFactories = potentialBombTarget.Distances
                        .Where(fTarget => Factories
                            .Exists(f => 
                                f.Id == fTarget.Key && f.Owner == Owner.Mine))
                        .OrderBy(f => f.Value)
                        .Select(f => f.Key)
                        .ToArray();
                    if (bombSourceFactories.Any())
                    {
                        var bombSourceFactoryId = bombSourceFactories.First();
                        moves.Add($"BOMB {bombSourceFactoryId} {potentialBombTarget.Id}");
                        _bombsLeft--;
                    }
                }
            }

            return moves.Any()
                ? string.Join(";", moves)
                : "WAIT";
        }

        private static string GetFactoryMove(Factory sourceFactory)
        {
            var availableCyborgs = sourceFactory.Cyborgs;
            
            // Dedicate required number of Cyborgs to defence
            availableCyborgs -= CalculateDefendersNeeded(sourceFactory.Id);
            
            // Check there are any Cyborgs left
            if (availableCyborgs <= 0)
            {
                return "";
            }
            
            // Check if there are any nearby allied nodes that need any help
            
            // Move against attack targets
            var potentialTargetFactories = FindPotentialTargetFactories(sourceFactory);

            var moves = new List<string>();
            foreach (var potentialTargetFactory in potentialTargetFactories)
            {
                var attackCyborgsNeeded = potentialTargetFactory.Cyborgs + 1;
                if (potentialTargetFactory.Owner == Owner.Opponent)
                {
                    attackCyborgsNeeded += 
                        (sourceFactory.Distances[potentialTargetFactory.Id] + 1) * potentialTargetFactory.Production;
                }
                
                if (availableCyborgs >= attackCyborgsNeeded)
                {
                    moves.Add(
                        $"MOVE {sourceFactory.Id} {potentialTargetFactory.Id} {attackCyborgsNeeded}");
                    availableCyborgs -= attackCyborgsNeeded;
                    potentialTargetFactory.FutureOwner = Owner.Mine;
                }
            }
            
            // Check if we should upgrade the factory
            if (sourceFactory.Production < 3 && availableCyborgs >= 10)
            {
                return $"INC {sourceFactory.Id}";
            }

            return !moves.Any()
                ? ""
                : string.Join(";", moves);
        }

        private static int CalculateDefendersNeeded(int sourceFactoryId)
        {
            return _troops
                .Where(t => t.Owner == Owner.Opponent)
                .Where(t => t.Destination == sourceFactoryId)
                .Sum(t => t.Cyborgs);
        }

        private static IEnumerable<Factory> FindPotentialTargetFactories(Factory sourceFactory)
        {
            return Factories
                .Where(f => f.FutureOwner != Owner.Mine)
                .Where(f => f != sourceFactory)
                .Where(f => f.Owner != Owner.Mine)
                .Where(f => f.Production != 0)
                .Where(f => f.Distances.ContainsKey(sourceFactory.Id))
                .OrderBy(f => f.Distances[sourceFactory.Id])
                .ToArray();
        }

        private static void PredictFuture()
        {
            foreach (var factory in Factories)
            {
                var totalIncomingOpponentCyborgs = _troops
                    .Where(t => t.Owner == Owner.Opponent)
                    .Where(t => t.Destination == factory.Id)
                    .Sum(t => t.Cyborgs);
                var totalIncomingOwnCyborgs = _troops
                    .Where(t => t.Owner == Owner.Mine)
                    .Where(t => t.Destination == factory.Id)
                    .Sum(t => t.Cyborgs);
                var factoryCyborgs = factory.Cyborgs + factory.Production;

                Owner futureOwner;
                switch (factory.Owner)
                {
                    case Owner.Mine:
                    {
                        futureOwner = totalIncomingOpponentCyborgs > (factoryCyborgs + totalIncomingOwnCyborgs)
                        ? Owner.Opponent
                        : Owner.Mine;
                    }
                        break;
                    case Owner.Opponent:
                    {
                        futureOwner = totalIncomingOwnCyborgs < (factoryCyborgs + totalIncomingOpponentCyborgs)
                            ? Owner.Opponent
                            : Owner.Mine;
                    }
                        break;
                    case Owner.Neutral:
                    {
                        if (totalIncomingOwnCyborgs == 0 && totalIncomingOpponentCyborgs == 0)
                        {
                            futureOwner = Owner.Neutral;
                        }
                        else
                        {
                            futureOwner = totalIncomingOwnCyborgs < totalIncomingOpponentCyborgs
                                ? Owner.Opponent
                                : Owner.Mine;
                        }
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                factory.FutureOwner = futureOwner;
                
                Console.Error.WriteLine($"Factory: {factory.Id} has future owner: {factory.FutureOwner}");
            }
        }

        private static void PopulateEntities()
        {
            _troops = new List<Troop>();
            
            var entityCount = int.Parse(Console.ReadLine());
            for (var i = 0; i < entityCount; i++)
            {
                var inputs = Console.ReadLine().Split(' ');

                var id = int.Parse(inputs[0]);
                var owner = (Owner) int.Parse(inputs[2]);

                var entityType = (EntityType) Enum.Parse(typeof(EntityType), inputs[1]);
                switch (entityType)
                {
                    case EntityType.FACTORY:
                        var factory = Factories
                            .Single(f => f.Id == id);
                        factory.Owner = owner;
                        factory.Cyborgs = int.Parse(inputs[3]);
                        factory.Production = int.Parse(inputs[4]);
                        break;
                    case EntityType.TROOP:
                        var troop = new Troop(id)
                        {
                            Owner = owner,
                            Origin = int.Parse(inputs[3]),
                            Destination = int.Parse(inputs[4]),
                            Cyborgs = int.Parse(inputs[5]),
                            RemainingTravelTime = int.Parse(inputs[6])
                        };
                        _troops.Add(troop);
                        break;
                    case EntityType.BOMB:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void InitialiseFactories()
        {
            Console.ReadLine();
//            var factoryCount = int.Parse(Console.ReadLine()); // the number of factories
            var linkCount = int.Parse(Console.ReadLine()); // the number of links between factories
            for (var i = 0; i < linkCount; i++)
            {
                var inputs = Console.ReadLine().Split(' ');
                var factory1Id = int.Parse(inputs[0]);
                var factory2Id = int.Parse(inputs[1]);
                var distance = int.Parse(inputs[2]);

                if (!Factories.Exists(f => f.Id == factory1Id))
                {
                    Factories.Add(new Factory(factory1Id));
                }
                if (!Factories.Exists(f => f.Id == factory2Id))
                {
                    Factories.Add(new Factory(factory2Id));
                }

                Factories
                    .Single(f => f.Id == factory1Id).Distances
                    .Add(factory2Id, distance);
                Factories
                    .Single(f => f.Id == factory2Id).Distances
                    .Add(factory1Id, distance);
            }
        }
    }

    public class Link
    {
        public int Distance { get; set; }
    }

    public class Entity
    {
        public int Id { get; set; }
        public Owner Owner { get; set; }
        public int Cyborgs { get; set; }
        
        public Entity(int id)
        {
            Id = id;
        }
    }

    public class Factory : Entity
    {
        public Owner FutureOwner { get; set; }
        public int Production { get; set; }
        public Dictionary<int, int> Distances { get; set; } = new Dictionary<int, int>();

        public Factory(int id) : base(id)
        {
        }
    }

    public class Troop : Entity
    {
        public int Origin { get; set; }
        public int Destination { get; set; }
        public int RemainingTravelTime { get; set; }

        public Troop(int id) : base(id)
        {
        }
    }

    public enum Owner
    {
        Opponent = -1,
        Neutral = 0,
        Mine = 1
    }

    public enum EntityType
    {
        FACTORY,
        TROOP,
        BOMB
    }
}