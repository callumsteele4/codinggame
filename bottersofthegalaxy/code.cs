using System;
using System.Collections.Generic;
using System.Linq;

enum EntityType
{
    Unit,
    Hero,
    Tower,
    Groot
}

enum HeroType
{
    Deadpool,
    Valkyrie,
    Doctor_Strange,
    Hulk,
    Ironman
}

enum LocationType
{
    Spawn,
    Bush
}

class Location
{
    public int X { get; set; }
    public int Y { get; set; }

    public Location(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int DistanceFrom(Location location)
    {
        return (int) Math.Floor(
            Math.Sqrt(
                Math.Pow(Math.Abs(X - location.X), 2)
                +
                Math.Pow(Math.Abs(Y - location.Y), 2)));
    }

    public bool LocationIsInRange(Location location, int range)
    {
        return DistanceFrom(location) < range;
    }
    
    public static bool operator== (Location location1, Location location2)
    {
        return location1.X == location2.X && location1.Y == location2.Y;
    }

    public static bool operator!= (Location location1, Location location2)
    {
        return location1.X != location2.X || location1.Y != location2.Y;
    }
    
    public override bool Equals(object obj)
    {
        var location = obj as Location;

        if (location == null)
        {
            return false;
        }

        return X == location.X
            && Y == location.Y;
    }
    
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

class TypedLocation : Location
{
    public LocationType Type { get; set; }

    public TypedLocation(int x, int y) : base(x, y) {}
}

class Entity
{
    public bool Deniable => (((double)Health + (double) Shield) / (double)MaxHealth) <= 0.4;

    public int Id { get; set; }
    public int Team { get; set; }
    public EntityType Type { get; set; }
    public Location Location { get; set; }
    public int AttackRange { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Shield { get; set; }
    public int AttackDamage { get; set; }
    public int MovementSpeed { get; set; }
    public int ItemsOwned { get; set; }

    public bool Killed { get; set; }
    public bool PriorityTarget { get; set; }

    public bool CanBeLastHit(int attackDamage)
    {
        return Health + Shield <= attackDamage;
    }
}

class Hero : Entity
{
    public HeroType HeroType { get; set; }
    public int Skill1Cooldown { get; set; }
    public int Skill2Cooldown { get; set; }
    public int Skill3Cooldown { get; set; }
    public int Mana { get; set; }

    public bool HasItemSpace => ItemsOwned < 3;
    public bool HasLowHealth => (((double)Health + (double) Shield) / (double)MaxHealth) <= 0.7;
    
    public void MoveToSafePosition(IEnumerable<Entity> playerUnits)
    {
        var foremostUnit = Team == 0
            ? playerUnits.OrderByDescending(e => e.Location.X).First()
            : playerUnits.OrderBy(e => e.Location.X).First();

        var destinationX = Team == 0
            ? foremostUnit.Location.X - 70
            : foremostUnit.Location.X + 70;

        Console.WriteLine($"MOVE {destinationX} {foremostUnit.Location.Y}");
    }

    public bool BehindFrontLine(IEnumerable<Entity> playerUnits)
    {
        return Team == 0
            ? Location.X < playerUnits.Max(u => u.Location.X)
            : Location.X > playerUnits.Min(u => u.Location.X);
    }

    public void MoveTo(Location location)
    {
        Console.WriteLine($"MOVE {location.X} {location.Y}");
    }

    public void LastHit(IEnumerable<Entity> lastHittableUnits)
    {
        foreach (var lastHittableUnit in lastHittableUnits)
            Console.Error.WriteLine($"Current Hero: {HeroType} Id: {lastHittableUnit.Id} Health: {lastHittableUnit.Health} Team: {lastHittableUnit.Team}");
        var bestOptionToLastHit = lastHittableUnits.Any(u => u.PriorityTarget)
            ? lastHittableUnits.Single(u => u.PriorityTarget)
            : lastHittableUnits
                .OrderBy(u => u.Team == Team)
                .ThenBy(u => u.Location.DistanceFrom(Location))
                .ThenBy(u => u.Health + u.Shield)
                .First();

        Console.Error.WriteLine($"Id: {bestOptionToLastHit.Id} Health: {bestOptionToLastHit.Health} Team: {bestOptionToLastHit.Team}");

        bestOptionToLastHit.Killed = (bestOptionToLastHit.Health + bestOptionToLastHit.Shield) <= AttackDamage;
        bestOptionToLastHit.PriorityTarget = !bestOptionToLastHit.Killed;

        Console.Error.WriteLine($"Was killed: {bestOptionToLastHit.Killed} Is priority: {bestOptionToLastHit.PriorityTarget}");
        Attack(bestOptionToLastHit.Id);
    }

    public void Attack(int entityId)
    {
        Console.WriteLine($"ATTACK {entityId}");
    }
    
    public void AttackNearest(EntityType entityType)
    {
        Console.WriteLine($"ATTACK_NEAREST {entityType.ToString().ToUpper()}");
    }

    public void Cast(string skill, Location location)
    {
        Console.WriteLine($"{skill} {location.X} {location.Y}");
    }

    public void Cast(string skill, int unitId)
    {
        Console.WriteLine($"{skill} {unitId}");
    }

    public void BuyItem(string itemName)
    {
        Console.WriteLine($"BUY {itemName}");
    }

    public void Wait()
    {
        Console.WriteLine("WAIT");
    }

    public bool CanAttackUnitThisTurn(Entity unit)
    {
        var maxPossibleDistanceFromUnit = unit.Location.DistanceFrom(Location) + unit.MovementSpeed;
        var maximumAttackDistanceInSingleTurn = MovementSpeed * 0.8 + AttackRange;
        
        // TODO: Make this more accurate, units will only ever move to the right or left, and we know which way.
        return maxPossibleDistanceFromUnit <= maximumAttackDistanceInSingleTurn;
    }
}

class Item
{
    public string Name { get; set; }
    public int Cost { get; set; }
    public int Damage { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Mana { get; set; }
    public int MaxMana { get; set; }
    public int MoveSpeed { get; set; }
    public int ManaRegeneration { get; set; }
    public bool IsPotion { get; set; }
}

class Player
{
    static TypedLocation[] GetBushAndSpawnPoints()
    {
        int bushAndSpawnPointCount = int.Parse(Console.ReadLine()); // useful from wood1, represents the number of bushes and the number of places where neutral units can spawn
        var typedLocations = new TypedLocation[bushAndSpawnPointCount];
        for (int i = 0; i < bushAndSpawnPointCount; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var locationType = (LocationType) Enum.Parse(typeof(LocationType), inputs[0], true);
            typedLocations[i] = new TypedLocation(int.Parse(inputs[1]), int.Parse(inputs[2]))
            {
                Type = locationType
            };
            // int radius = int.Parse(inputs[3]);
        }
        return typedLocations;
    }

    static Item[] GetItems()
    {
        int itemCount = int.Parse(Console.ReadLine()); // useful from wood2
        var items = new Item[itemCount];
        for (int i = 0; i < itemCount; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            items[i] = new Item
            {
                Name = inputs[0], // contains keywords such as BRONZE, SILVER and BLADE, BOOTS connected by "_" to help you sort easier
                Cost = int.Parse(inputs[1]), // BRONZE items have lowest cost, the most expensive items are LEGENDARY
                Damage = int.Parse(inputs[2]), // keyword BLADE is present if the most important item stat is damage
                Health = int.Parse(inputs[3]),
                MaxHealth = int.Parse(inputs[4]),
                Mana = int.Parse(inputs[5]),
                MaxMana = int.Parse(inputs[6]),
                MoveSpeed = int.Parse(inputs[7]), // keyword BOOTS is present if the most important item stat is moveSpeed
                ManaRegeneration =  int.Parse(inputs[8]),
                IsPotion = int.Parse(inputs[9]) != 0 // 0 if it's not instantly consumed
            };
            Console.Error.WriteLine($"Name: {items[i].Name} Cost: {items[i].Cost} Damage: {items[i].Damage} Health: {items[i].Health} MaxHealth: {items[i].MaxHealth} Mana: {items[i].Mana} MaxMana: {items[i].MaxMana} MoveSpeed: {items[i].MoveSpeed} ManaRegeneration: {items[i].ManaRegeneration} IsPotion: {items[i].IsPotion}");
        }
        return items;
    }

    static Entity[] GetEntities()
    {
        int entityCount = int.Parse(Console.ReadLine());
        var entities = new Entity[entityCount];
        for (int i = 0; i < entityCount; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var entityType = (EntityType) Enum.Parse(typeof(EntityType), inputs[2], true);
            if (entityType == EntityType.Hero)
                entities[i] = new Hero
                {
                    Id = int.Parse(inputs[0]),
                    Team = int.Parse(inputs[1]),
                    Type = entityType,
                    Location = new Location(int.Parse(inputs[3]), int.Parse(inputs[4])),
                    AttackRange = int.Parse(inputs[5]),
                    Health = int.Parse(inputs[6]),
                    MaxHealth = int.Parse(inputs[7]),
                    Shield = int.Parse(inputs[8]),
                    AttackDamage = int.Parse(inputs[9]),
                    MovementSpeed = int.Parse(inputs[10]),
                    Skill1Cooldown = int.Parse(inputs[13]),
                    Skill2Cooldown = int.Parse(inputs[14]),
                    Skill3Cooldown = int.Parse(inputs[15]),
                    Mana = int.Parse(inputs[16]),
                    HeroType = (HeroType) Enum.Parse(typeof(HeroType), inputs[19], true),
                    ItemsOwned = int.Parse(inputs[21])
                };
            else
                entities[i] = new Entity
                {
                    Id = int.Parse(inputs[0]),
                    Team = int.Parse(inputs[1]),
                    Type = entityType,
                    Location = new Location(int.Parse(inputs[3]), int.Parse(inputs[4])),
                    AttackRange = int.Parse(inputs[5]),
                    Health = int.Parse(inputs[6]),
                    MaxHealth = int.Parse(inputs[7]),
                    Shield = int.Parse(inputs[8]),
                    AttackDamage = int.Parse(inputs[9]),
                    MovementSpeed = int.Parse(inputs[10]),
                    ItemsOwned = int.Parse(inputs[21])
                };
                // int stunDuration = int.Parse(inputs[11]); // useful in bronze
                // int goldValue = int.Parse(inputs[12]);
                // int maxMana = int.Parse(inputs[17]);
                // int manaRegeneration = int.Parse(inputs[18]);
                // int isVisible = int.Parse(inputs[20]); // 0 if it isn't
                // int itemsOwned = int.Parse(inputs[21]); // useful from wood1
        }
        return entities;
    }

    static void Main(string[] args)
    {
        var playerTeam = int.Parse(Console.ReadLine());
        var bushAndSpawnPoints = GetBushAndSpawnPoints();
        var items = GetItems();
        var healthPotions = items.Where(i => i.IsPotion && i.Health > 0);

        var roundCount = 0;
        while (true)
        {
            var gold = int.Parse(Console.ReadLine());
            var enemyGold = int.Parse(Console.ReadLine());
            var roundType = int.Parse(Console.ReadLine()); // a positive value will show the number of heroes that await a command

            var availableDamageItemsToBuy = items
                .Where(i => i.Cost <= gold - healthPotions.Min(hp => hp.Cost) &&  i.Damage > 0);
            var availableMp5ItemsToBuy = items
                .Where(i => i.Cost <= gold - healthPotions.Min(hp => hp.Cost) &&  i.ManaRegeneration > 1);

            var entities = GetEntities();

            if (roundType < 0)
            {
                if (roundCount == 0)
                    Console.WriteLine("IRONMAN");
                else if (roundCount == 1)
                    Console.WriteLine("DOCTOR_STRANGE");
            }
            else
            {
                var playerEntities = entities.Where(e => e.Team == playerTeam);
                var playerUnits = playerEntities.Where(e => e.Type == EntityType.Unit);
                var playerTower = playerEntities.Single(e => e.Type == EntityType.Tower);
                var playerTowerLocation = playerEntities.Single(e => e.Type == EntityType.Tower).Location;
                var playerHeroes = playerEntities.Where(e => e.Type == EntityType.Hero).Select(h => (Hero) h);

                var closestBushToPlayerTower = bushAndSpawnPoints
                    .Where(p => p.Type == LocationType.Bush)
                    .OrderBy(p => p.DistanceFrom(playerTowerLocation))
                    .FirstOrDefault();
                
                // TODO: Explore an alternative approach to looping through heroes, might make coordination easier
                foreach (var playerHero in playerHeroes)
                {
                    var otherPlayerHero = playerHeroes.SingleOrDefault(h => h != playerHero);
                    if (!playerUnits.Any())
                    {
                        if (!Object.ReferenceEquals(null, closestBushToPlayerTower))
                        {
                            if (playerHero.Location != closestBushToPlayerTower)
                            {
                                playerHero.MoveTo(closestBushToPlayerTower);
                            }
                            else
                            {
                                playerHero.Wait();
                            }
                        }
                        else
                        {
                            if (playerHero.Location != playerTowerLocation)
                            {
                                playerHero.MoveTo(playerTowerLocation);
                            }
                            else
                            {
                                playerHero.AttackNearest(EntityType.Unit);
                            }
                        }
                    }
                    else
                    {
                        if (!playerHero.BehindFrontLine(playerUnits))
                            playerHero.MoveToSafePosition(playerUnits);
                        else
                        {
                            if (playerHero.HasLowHealth
                                && gold >= healthPotions.Min(hp => hp.Cost)
                                && (otherPlayerHero == null || playerHero.Health + playerHero.Shield <= otherPlayerHero.Health + otherPlayerHero.Shield))
                            {
                                var bestHealthPotionCanAfford = healthPotions
                                    .Where(hp => hp.Cost <= gold)
                                    .OrderByDescending(hp => hp.Cost)
                                    .First();
                                playerHero.BuyItem(bestHealthPotionCanAfford.Name);
                                gold -= bestHealthPotionCanAfford.Cost;
                            }
                            else if (
                                playerHero.HeroType == HeroType.Doctor_Strange &&
                                playerHero.Skill1Cooldown == 0 && 
                                playerHero.Mana >= 50 &&
                                (((playerHero.Health + playerHero.Shield) < playerHero.MaxHealth) || (otherPlayerHero != null && (otherPlayerHero.Health + otherPlayerHero.Shield) < otherPlayerHero.MaxHealth && otherPlayerHero.Location.LocationIsInRange(playerHero.Location, 250))))
                            {
                                var lowestHealthHero = otherPlayerHero == null || (playerHero.Health + playerHero.Shield) < (otherPlayerHero.Health + otherPlayerHero.Shield) ? playerHero : otherPlayerHero;
                                playerHero.Cast("AOEHEAL", lowestHealthHero.Location);
                            }
                            else if (
                                playerHero.HeroType == HeroType.Doctor_Strange &&
                                playerHero.Skill2Cooldown == 0 &&
                                playerHero.Mana >= 40 &&
                                (playerHero.HasLowHealth || (otherPlayerHero != null && otherPlayerHero.HasLowHealth && otherPlayerHero.Location.LocationIsInRange(playerHero.Location, 500))))
                            {
                                var lowestHealthHero = otherPlayerHero == null || (playerHero.Health + playerHero.Shield) < (otherPlayerHero.Health + otherPlayerHero.Shield) ? playerHero : otherPlayerHero;
                                playerHero.Cast("SHIELD", lowestHealthHero.Id);
                            }
                            // TODO: Sell item if both heroes have low health
                            else
                            {
                                var opponentEntities = entities
                                    .Where(e => e.Team != playerTeam);
                                var opponentHeroes = opponentEntities
                                    .Where(e => e.Type == EntityType.Hero)
                                    .Select(h => (Hero) h);
                                var opponentUnits = opponentEntities
                                    .Where(e => e.Type == EntityType.Unit);
                                var opponentTowerLocation = opponentEntities
                                    .Single(e => e.Type == EntityType.Tower).Location;
                                var attackableUnits = opponentUnits
                                    .Concat(playerUnits.Where(u => u.Deniable))
                                    .Where(u => !u.Killed && playerHero.CanAttackUnitThisTurn(u) && !u.Location.LocationIsInRange(opponentTowerLocation, 200));
                                var lastHittableUnits = attackableUnits
                                    .Where(u => u.CanBeLastHit(playerHero.AttackDamage) || u.PriorityTarget);
                                var coordinatedLastHittableUnits = attackableUnits
                                    .Where(u => otherPlayerHero != null && otherPlayerHero.CanAttackUnitThisTurn(u) && u.CanBeLastHit(playerHero.AttackDamage + otherPlayerHero.AttackDamage));
                                if (lastHittableUnits.Any())
                                    playerHero.LastHit(lastHittableUnits);
                                else if (playerHero.HeroType != HeroType.Doctor_Strange && coordinatedLastHittableUnits.Any())
                                    playerHero.LastHit(coordinatedLastHittableUnits);
                                else if (playerHero.HeroType == HeroType.Ironman &&
                                    playerHero.Skill2Cooldown == 0 &&
                                    playerHero.Mana >= 60 &&
                                    opponentHeroes.Any(h => h.Location.LocationIsInRange(playerHero.Location, 900)))
                                {
                                    var bestOpponentHeroInRange = opponentHeroes
                                        .Where(h =>  h.Location.LocationIsInRange(playerHero.Location, 900))
                                        .OrderByDescending(h => h.Location.DistanceFrom(playerHero.Location))
                                        .First();
                                    playerHero.Cast("FIREBALL", bestOpponentHeroInRange.Location);
                                }
                                else if (
                                    playerHero.HeroType == HeroType.Doctor_Strange &&
                                    playerHero.Skill3Cooldown == 0 &&
                                    playerHero.Mana >= 40 &&
                                    (opponentUnits.Count(u => u.Location.LocationIsInRange(playerHero.Location, (int) Math.Floor(u.AttackRange + u.MovementSpeed * 0.75))) < 3 || playerHero.Location.LocationIsInRange(playerTowerLocation, 400)) &&
                                    opponentHeroes.Any(h => h.Location.LocationIsInRange(playerHero.Location, 400)))
                                {
                                    var opponentHeroInRange = opponentHeroes
                                        .Where(h => h.Location.LocationIsInRange(playerHero.Location, 400))
                                        .OrderBy(h => h.Health)
                                        .First();
                                    playerHero.Cast("PULL", opponentHeroInRange.Id);
                                }
                                else if (opponentUnits.Any() &&
                                            (opponentHeroes.Any(h => !h.BehindFrontLine(opponentUnits)) ||
                                            opponentHeroes.Any(h => h.Location.LocationIsInRange(playerHero.Location, 100))))
                                {
                                    Hero bestOpponentHeroTarget;
                                    if (opponentHeroes.Count(h => !h.BehindFrontLine(opponentUnits)) == 1)
                                    {
                                        bestOpponentHeroTarget = opponentHeroes
                                            .Single(h => !h.BehindFrontLine(opponentHeroes));
                                    }
                                    else if (opponentHeroes.Count(h => !h.BehindFrontLine(opponentUnits)) ==  2)
                                    {
                                        bestOpponentHeroTarget = opponentHeroes
                                            .OrderBy(h => h.Health + h.Shield)
                                            .First();
                                    }
                                    else
                                    {
                                        bestOpponentHeroTarget = opponentHeroes
                                            .Where(h => h.Location.LocationIsInRange(playerHero.Location, 125))
                                            .OrderBy(h => h.Health + h.Shield)
                                            .First();
                                    }
                                    playerHero.Attack(bestOpponentHeroTarget.Id);
                                }
                                else if (availableDamageItemsToBuy.Any() && playerHero.HasItemSpace && playerHero.HeroType == HeroType.Ironman)
                                {
                                    var bestItemForDamage = availableDamageItemsToBuy
                                        .OrderByDescending(i => i.Damage / i.Cost)
                                        .First();
                                    playerHero.BuyItem(bestItemForDamage.Name);
                                    gold -= bestItemForDamage.Cost;
                                }
                                else if (availableMp5ItemsToBuy.Any() && playerHero.HasItemSpace && playerHero.HeroType == HeroType.Doctor_Strange)
                                {
                                    var bestItemForMp5 = availableMp5ItemsToBuy
                                        .OrderByDescending(i => i.ManaRegeneration / i.Cost)
                                        .First();
                                    playerHero.BuyItem(bestItemForMp5.Name);
                                    gold -= bestItemForMp5.Cost;
                                }
                                else if (opponentUnits.Any(u => u.Location.LocationIsInRange(playerHero.Location, playerHero.AttackRange / 2) && !u.Killed))
                                {
                                    var lowestHealthInRangeOpponentUnit = opponentUnits
                                        .Where(u => u.Location.LocationIsInRange(playerHero.Location, playerHero.AttackRange / 2) && !u.Killed)
                                        .OrderBy(u => u.Health + u.Shield)
                                        .First();
                                    playerHero.Attack(lowestHealthInRangeOpponentUnit.Id);
                                }
                                else
                                    playerHero.MoveToSafePosition(playerUnits);
                            }
                        }
                    }
                }
            }
            roundCount++;
        }
    }
}