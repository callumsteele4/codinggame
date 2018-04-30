    using System;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;

    class Location
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Radius { get; set; }

        public Location(int x, int y, int radius)
        {
            X = x;
            Y = y;
            Radius = radius;
        }

        public double GetDistance(Location location)
        {
            var xDiff = X - location.X;
            var yDiff = Y - location.Y;

            return Math.Sqrt(Math.Pow(xDiff, 2) + Math.Pow(yDiff, 2));
        }

        public bool InRangeOfAny(IEnumerable<Site> sites)
        {
            return sites.Any(s => GetDistance(s.Location) <= s.Param2);
        }
    }

    enum StructureType
    {
        None = -1,
        Mine = 0,
        Tower = 1,
        Barracks = 2
    }

    enum Owner
    {
        None = -1,
        Friendly = 0,
        Enemy = 1
    }

    enum CreepType
    {
        Queen = -1,
        Knight = 0,
        Archer = 1
    }

    class Site
    {
        public int Id { get; set; }
        public int Gold { get; set; }
        public int MaxMineSize { get; set; }
        public Location Location { get; set; }
        public StructureType StructureType { get; set; }
        public Owner Owner { get; set; }
        public int Param1 { get; set; }
        public int Param2 { get; set; }

        public Site(int id, int x, int y, int radius)
        {
            Id = id;
            Location = new Location(x, y, radius);
        }
    }

    class Unit
    {
        public Location Location { get; set; }
        public Owner Owner { get; set; }
        public CreepType CreepType { get; set; }
        public int Health { get; set; }

        public Unit(int x, int y, Owner owner, CreepType creepType, int health)
        {
            Location = new Location(x, y, 0);
            Owner = owner;
            CreepType = creepType;
            Health = health;
        }
    }

    class Player
    {
        static void Main(string[] args)
        {
            var sites = new Site[int.Parse(Console.ReadLine())];
            for (int i = 0; i < sites.Length; i++)
            {
                var inputs = Console.ReadLine().Split(' ');
                sites[i] = new Site(
                    int.Parse(inputs[0]),
                    int.Parse(inputs[1]),
                    int.Parse(inputs[2]),
                    int.Parse(inputs[3]));
            }

            // game loop
            while (true)
            {
                var inputs = Console.ReadLine().Split(' ');
                var gold = int.Parse(inputs[0]);
                var touchedSite = sites
                    .SingleOrDefault(s => s.Id == int.Parse(inputs[1]));

                for (int i = 0; i < sites.Length; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    var site = sites.Single(s => s.Id == int.Parse(inputs[0]));
                    site.Gold = int.Parse(inputs[1]);
                    site.MaxMineSize = int.Parse(inputs[2]);
                    site.StructureType = (StructureType) int.Parse(inputs[3]);
                    site.Owner = (Owner) int.Parse(inputs[4]);
                    site.Param1 = int.Parse(inputs[5]);
                    site.Param2 = int.Parse(inputs[6]);
                }

                var units = new Unit[int.Parse(Console.ReadLine())];
                for (int i = 0; i < units.Length; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    units[i] = new Unit(
                        int.Parse(inputs[0]),
                        int.Parse(inputs[1]),
                        (Owner) int.Parse(inputs[2]),
                        (CreepType) int.Parse(inputs[3]),
                        int.Parse(inputs[4])
                    );
                }

                var queen = units
                    .Single(unit =>
                        unit.Owner == Owner.Friendly &&
                        unit.CreepType == CreepType.Queen);
                var goldBeingMined = sites
                    .Where(s => s.Owner == Owner.Friendly)
                    .Where(s => s.StructureType == StructureType.Mine)
                    .Sum(s => s.Param1);
                var knightBarracks = sites
                    .Where(s => s.Owner == Owner.Friendly)
                    .Where(s => s.StructureType == StructureType.Barracks)
                    .Where(s => ((CreepType) s.Param2) == CreepType.Knight);
                var archerBarracks = sites
                    .Where(s => s.Owner == Owner.Friendly)
                    .Where(s => s.StructureType == StructureType.Barracks)
                    .Where(s => ((CreepType) s.Param2) == CreepType.Archer);
                var towers = sites
                    .Where(s => s.Owner == Owner.Friendly)
                    .Where(s => s.StructureType == StructureType.Tower);
                var enemyQueen = units
                    .Single(unit =>
                        unit.Owner == Owner.Enemy &&
                        unit.CreepType == CreepType.Queen);
                var enemyKnights = units
                    .Where(u => u.Owner == Owner.Enemy)
                    .Where(u => u.CreepType == CreepType.Knight);
                var enemyTowers = sites
                    .Where(s => s.Owner == Owner.Enemy)
                    .Where(s => s.StructureType == StructureType.Tower);

                if (goldBeingMined < 7 || knightBarracks.Count() == 0 || archerBarracks.Count() == 0 || towers.Count() < 2)
                {
                    if (touchedSite?.Owner == Owner.None)
                    {
                        if (goldBeingMined < 4 &&
                            touchedSite.Gold > 0 &&
                            !enemyKnights.Any(u => u.Location.GetDistance(touchedSite.Location) < 200))
                            Console.WriteLine($"BUILD {touchedSite.Id} MINE");
                        else if (archerBarracks.Count() == 0)
                            Console.WriteLine($"BUILD {touchedSite.Id} BARRACKS-ARCHER");
                        else if (knightBarracks.Count() == 0)
                            Console.WriteLine($"BUILD {touchedSite.Id} BARRACKS-KNIGHT");
                        else if (goldBeingMined < 7 &&
                            touchedSite.Gold > 0 &&
                            !enemyKnights.Any(u => u.Location.GetDistance(touchedSite.Location) < 200))
                            Console.WriteLine($"BUILD {touchedSite.Id} MINE");
                        else
                            Console.WriteLine($"BUILD {touchedSite.Id} TOWER");
                    }
                    else if (touchedSite?.Owner == Owner.Friendly &&
                            touchedSite.StructureType == StructureType.Mine &&
                            touchedSite.MaxMineSize > touchedSite.Param1)
                    {
                        Console.WriteLine($"BUILD {touchedSite.Id} MINE");
                    }
                    else
                    {
                        var nearestUnclaimedSite = sites
                            .Where(s => s.Owner == Owner.None)
                            .Where(s => !s.Location.InRangeOfAny(enemyTowers))
                            .OrderBy(s => queen.Location.GetDistance(s.Location))
                            .FirstOrDefault();
                        if (nearestUnclaimedSite != null)
                            Console.WriteLine(
                                $"MOVE {nearestUnclaimedSite.Location.X} {nearestUnclaimedSite.Location.Y}");
                        else
                            Console.WriteLine("WAIT");
                    }
                }
                else if (touchedSite?.Owner == Owner.Friendly &&
                        touchedSite.StructureType == StructureType.Tower &&
                        touchedSite.Param1 <= 600)
                {
                    Console.WriteLine($"BUILD {touchedSite.Id} TOWER");
                }
                else if (towers.Any(t => t.Param1 <= 600))
                {
                    var nearestLowHpTower = towers
                        .Where(t => t.Param1 <= 600)
                        .OrderBy(t => queen.Location.GetDistance(t.Location))
                        .First();
                    Console.WriteLine($"MOVE {nearestLowHpTower.Location.X} {nearestLowHpTower.Location.Y}");
                }
                else
                {
                    var furthestTowerLocation = towers
                        .OrderByDescending(t => t.Location.GetDistance(enemyQueen.Location))
                        .First().Location;

                    Console.WriteLine($"MOVE {furthestTowerLocation.X} {furthestTowerLocation.Y}");
                }

                var barracks = sites
                    .Where(s => s.Owner == Owner.Friendly)
                    .Where(s => s.StructureType == StructureType.Barracks)
                    .Where(s => s.Param1 == 0)
                    .OrderBy(s => s.Location.GetDistance(enemyQueen.Location));

                var archersExist = units
                    .Any(u =>
                        u.Owner == Owner.Friendly &&
                        u.CreepType == CreepType.Archer);

                if (gold < 80)
                {
                    Console.WriteLine("TRAIN");
                }
                else if (!archersExist && archerBarracks.Any() && gold < 100)
                {
                    Console.WriteLine("TRAIN");
                }
                else if (!archersExist && archerBarracks.Any())
                {
                    var nearestArcherBarracksId = archerBarracks
                        .OrderBy(s => s.Location.GetDistance(enemyQueen.Location))
                        .First().Id;
                    Console.WriteLine($"TRAIN {nearestArcherBarracksId}");
                }
                else if (knightBarracks.Any() && gold >= 80)
                {
                    var nearestKnightBarracksId = knightBarracks
                        .OrderBy(s => s.Location.GetDistance(enemyQueen.Location))
                        .First().Id;
                    Console.WriteLine($"TRAIN {nearestKnightBarracksId}");
                }
                else
                    Console.WriteLine("TRAIN");
            }
        }
    }