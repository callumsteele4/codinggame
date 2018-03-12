using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace GhostInTheCell
{
    public class Player2
    {
        private static IConsole _console;
        
        public Player2(IConsole console)
        {
            _console = console;
        }

        public static void Main()
        {
            var robot = new Robot(_console);
            robot.Begin();
        }
    }

    public class Robot
    {
        // ReSharper disable once InconsistentNaming
        // Naming to replace System.Console.
        private readonly IConsole Console;

        public Robot(IConsole console)
        {
            Console = console;
        }

        public void Begin()
        {
            var initialState = new State
            {
                Factories = InitialiseFactories()
            };

            Play(initialState);
        }

        public void Play(State state)
        {
            while (true)
            {
                state = UpdateState(state);

                var moves = CalculateMoves(state);

                Console.WriteLine(moves);
            }
            // ReSharper disable once FunctionNeverReturns
            // Game loop
        }

        public string CalculateMoves(State state)
        {
            state.Moves = new List<string>();
            // Mutate state as we make moves, so that future moves
            // are based on moves as they are decided.

            foreach (var factory in state.Factories.Where(f => f.Owner == Owner2.Own))
            {
                var numberOfAvailableCyborgs = GetNumberOfAvailableCyborgs(factory, state);
                if (numberOfAvailableCyborgs == 0)
                {
                    continue;
                }
            
                // TODO: Defense moves
                
                // Attack moves
//                var potentialTargetFactories = FindTargets(factory, state);
                
                foreach (var potentialTargetFactory in state.Factories
                    .Where(f => f.Id != factory.Id)
                    .Where(f => f.Production != 0)
                    .OrderBy(f => f.Distances[factory.Id]))
                {
                    var totalOwnCyborgsIncoming = state.Troops
                        .Where(t => t.Destination == potentialTargetFactory.Id)
                        .Where(t => t.Owner == Owner2.Own)
                        .Sum(t => t.NumberOfCyborgs);
                    var totalOpponentCyborgsIncoming = state.Troops
                        .Where(t => t.Destination == potentialTargetFactory.Id)
                        .Where(t => t.Owner == Owner2.Opponent)
                        .Sum(t => t.NumberOfCyborgs);
                    
                    if (potentialTargetFactory.Owner == Owner2.Neutral &&
                        potentialTargetFactory.NumberOfCyborgs + totalOpponentCyborgsIncoming < totalOwnCyborgsIncoming)
                    {
                        continue;
                    }

                    if (potentialTargetFactory.Owner == Owner2.Opponent &&
                        potentialTargetFactory.NumberOfCyborgs + totalOpponentCyborgsIncoming +
                        potentialTargetFactory.Production * potentialTargetFactory.Distances[factory.Id] <
                        totalOwnCyborgsIncoming)
                    {
                        continue;
                    }
                    
                    var attackCyborgsNeeded = totalOpponentCyborgsIncoming - totalOwnCyborgsIncoming;
                    if (potentialTargetFactory.Owner == Owner2.Opponent)
                    {
                        attackCyborgsNeeded += potentialTargetFactory.NumberOfCyborgs + 1 + 
                            (factory.Distances[potentialTargetFactory.Id] + 1) * potentialTargetFactory.Production;
                    } else if (potentialTargetFactory.Owner == Owner2.Neutral)
                    {
                        attackCyborgsNeeded += potentialTargetFactory.NumberOfCyborgs + 1;
                    } else if (potentialTargetFactory.Owner == Owner2.Own)
                    {
                        attackCyborgsNeeded -= potentialTargetFactory.NumberOfCyborgs;
                    }
                
                    if (numberOfAvailableCyborgs >= attackCyborgsNeeded && attackCyborgsNeeded > 0)
                    {
                        state.Moves.Add(
                            $"MOVE {factory.Id} {potentialTargetFactory.Id} {attackCyborgsNeeded}");
                        numberOfAvailableCyborgs -= attackCyborgsNeeded;
                        if (potentialTargetFactory.Owner == Owner2.Own)
                        {
                            potentialTargetFactory.NumberOfCyborgs += attackCyborgsNeeded;
                        }
                        else
                        {
                            potentialTargetFactory.NumberOfCyborgs -= attackCyborgsNeeded;   
                        }
                    }
                }

                // Build moves
                if (factory.Production < 3 && numberOfAvailableCyborgs >= 10)
                {
                    state.Moves.Add($"INC {factory.Id}");
                }
            }
            
            // Bomb moves
            if (state.BombsLeft != 0)
            {
                var potentialBombTarget = state.Factories
                    .Where(f => !state.Bombs.Any(b => b.Destination == f.Id && b.Owner == Owner2.Own))
                    .Where(f => f.Owner == Owner2.Opponent)
                    .Where(f => f.Production != 0)
                    .OrderByDescending(f => f.Production)
                    .ThenByDescending(f => f.NumberOfCyborgs)
                    .FirstOrDefault();
                if (potentialBombTarget != null)
                {
                    var bombSourceFactories = potentialBombTarget.Distances
                        .Where(fTarget => state.Factories
                            .Any(f => 
                                f.Id == fTarget.Key && f.Owner == Owner2.Own))
                        .OrderBy(f => f.Value)
                        .Select(f => f.Key)
                        .ToArray();
                    if (bombSourceFactories.Any())
                    {
                        var bombSourceFactoryId = bombSourceFactories.First();
                        state.Moves.Add($"BOMB {bombSourceFactoryId} {potentialBombTarget.Id}");
                        state.BombsLeft--;
                    }
                }
            }
            
            var temps = Console.ReadLine()
                .Split(' ')
                .Select(int.Parse)
                .OrderBy(t => Math.Abs(0 - t))
                .ThenByDescending(t => t)
                .FirstOrDefault();
            
            return !state.Moves.Any()
                ? "WAIT"
                : string.Join(";", state.Moves);
        }
        
        // TODO: Take into account existing own troops
        // TODO: Take into account existing opponent troops
        // TODO: Take into account moves decided so far this turn
        public List<Factory2> FindTargets(Factory2 factory, State state)
        {
            return state.Factories
                .Where(f => f.Owner != Owner2.Own)
                .Where(f => f.Production != 0)
                .Where(f => f.Distances.ContainsKey(factory.Id))
                .OrderBy(f => f.Distances[factory.Id])
                .ToList();
        }

        // TODO: Take into account own troops
        public int GetNumberOfAvailableCyborgs(Factory2 factory, State state)
        {
            var incomingEnemyTroops = state.Troops
                .Where(t => t.Owner == Owner2.Opponent)
                .Where(t => t.Destination == factory.Id)
                .ToList();

            if (!incomingEnemyTroops.Any())
            {
                return factory.NumberOfCyborgs;
            }

            var currentNumberOfCyborgsInFactory = factory.NumberOfCyborgs;
            var minimumNumberOfCyborgsLeftInFactory = factory.NumberOfCyborgs;
            var turnsTillFurthestEnemyTroopArrives = incomingEnemyTroops
                .Max(t => t.RemainingTravelTime);
            for (var i = 1; i <= turnsTillFurthestEnemyTroopArrives; i++)
            {
                currentNumberOfCyborgsInFactory += 
                        factory.Production
                    - 
                        incomingEnemyTroops
                            .Where(t => t.RemainingTravelTime == i)
                            .Sum(t => t.NumberOfCyborgs);
                
                if (currentNumberOfCyborgsInFactory <= 0)
                {
                    return 0;
                }
                if (currentNumberOfCyborgsInFactory < minimumNumberOfCyborgsLeftInFactory)
                {
                    minimumNumberOfCyborgsLeftInFactory = currentNumberOfCyborgsInFactory;
                }
            }

            return minimumNumberOfCyborgsLeftInFactory;
        }

        public State UpdateState(State state)
        {
            state.Troops = new List<Troop2>();
            state.Bombs = new List<Bomb2>();
            var numberOfEntites = int.Parse(Console.ReadLine());
            for (var i = 0; i < numberOfEntites; i++)
            {
                var entityInput = Console.ReadLine().Split(' ');

                var entityId = int.Parse(entityInput[0]);
                var entityType = entityInput[1];
                switch (entityType)
                {
                    case "FACTORY":
                        state.Factories[entityId] = new Factory2(entityId)
                        {
                            Owner = (Owner2) int.Parse(entityInput[2]),
                            NumberOfCyborgs = int.Parse(entityInput[3]),
                            Distances = state.Factories[entityId].Distances,
                            Production = int.Parse(entityInput[4]),
                            TurnsUntilProducing = int.Parse(entityInput[5])
                        };
                        break;
                    case "TROOP":
                        state.Troops.Add(new Troop2(entityId)
                        {
                            Owner = (Owner2) int.Parse(entityInput[2]),
                            Origin = int.Parse(entityInput[3]),
                            Destination = int.Parse(entityInput[4]),
                            NumberOfCyborgs = int.Parse(entityInput[5]),
                            RemainingTravelTime = int.Parse(entityInput[6])
                        });
                        break;
                    case "BOMB":
                        state.Bombs.Add(new Bomb2(entityId)
                        {
                            Owner = (Owner2) int.Parse(entityInput[2]),
                            Origin = int.Parse(entityInput[3]),
                            Destination = int.Parse(entityInput[4]),
                            TurnsUntilExplodes = int.Parse(entityInput[5])
                        });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return state;
        }

        public List<Factory2> InitialiseFactories()
        {
            var numberOfFactories = int.Parse(Console.ReadLine());
            var factories = Enumerable.Range(0, numberOfFactories)
                .Select(n => new Factory2(n))
                .ToList();

            var numberOfLinks = int.Parse(Console.ReadLine());
            for (var i = 0; i < numberOfLinks; i++)
            {
                var linkInput = Console.ReadLine().Split(' ');
                var factory1Id = int.Parse(linkInput[0]);
                var factory2Id = int.Parse(linkInput[1]);   
                var distance = int.Parse(linkInput[2]);

                factories[factory1Id].Distances.Add(factory2Id, distance);
                factories[factory2Id].Distances.Add(factory1Id, distance);
            }

            return factories;
        }
    }
    
    public interface IConsole
    {
        string ReadLine();
        string WriteLine(string value);
    }

    public class State
    {
        public List<Factory2> Factories { get; set; }
        public List<Troop2> Troops { get; set; }
        public List<Bomb2> Bombs { get; set; }
        public IEnumerable<Factory2> FactoriesWithAvailableCyborgs { get; set; }
        public IEnumerable<Factory2> FactoriesUnderThreat { get; set; }
        public List<string> Moves { get; set; }
        public int BombsLeft { get; set; } = 2;
    }

    public class Entity2
    {
        public int Id { get; set; }
        public Owner2 Owner { get; set; }
        public int NumberOfCyborgs { get; set; }

        public Entity2(int id)
        {
            Id = id;
        }
    }

    public class Factory2 : Entity2
    {
        public Dictionary<int, int> Distances { get; set; } = new Dictionary<int, int>();
        public int Production { get; set; }
        public int TurnsUntilProducing { get; set; }

        public Factory2(int id) : base(id)
        {
        }
    }

    public class Troop2 : Entity2
    {
        public int Origin { get; set; }
        public int Destination { get; set; }
        public int RemainingTravelTime { get; set; }

        public Troop2(int id) : base(id)
        {
        }
    }

    public class Bomb2 : Entity2
    {
        public int Origin { get; set; }
        public int Destination { get; set; }
        public int TurnsUntilExplodes { get; set; }

        public Bomb2(int id) : base(id)
        {
        }
    }

    public enum Owner2
    {
        Opponent = -1,
        Neutral = 0,
        Own = 1
    }
}