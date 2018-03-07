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

class Location
{
    public int X { get; set; }
    public int Y { get; set; }

    public Location(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public static bool operator== (Location location1, Location location2)
    {
        return location1.X == location2.X
            && location1.Y == location2.Y;
    }

    public static bool operator!= (Location location1, Location location2)
    {
        return location1.X != location2.X
            || location1.Y != location2.Y;
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

class Entity
{
    public bool Deniable => ((double)Health / (double)MaxHealth) <= 0.4;

    public int Id { get; set; }
    public int Team { get; set; }
    public EntityType Type { get; set; }
    public Location Location { get; set; }
    public int AttackRange { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int AttackDamage { get; set; }
    public int ItemsOwned { get; set; }

    public bool CanLastHit(int attackDamage)
    {
        return Health <= attackDamage;
    }
}

class Hero : Entity
{
    public HeroType HeroType { get; set; }
    public int Skill1Cooldown { get; set; }
    public int Skill2Cooldown { get; set; }
    public int Skill3Cooldown { get; set; }

    public bool HasItemSpace => ItemsOwned < 3;
    public bool HasLowHealth => Deniable;
    
    public void MoveToSafePosition(IEnumerable<Entity> playerUnits)
    {
        var foremostUnit = Team == 0
            ? playerUnits.OrderByDescending(e => e.Location.X).First()
            : playerUnits.OrderBy(e => e.Location.X).First();

        var destinationX = Team == 0
            ? foremostUnit.Location.X - 50
            : foremostUnit.Location.X + 50;

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

    public void AttemptToLastHit(IEnumerable<Entity> playerUnits, IEnumerable<Entity> opponentUnits)
    {
        var deniableUnits = playerUnits
            .Where(u => u.Deniable && u.CanLastHit(AttackDamage))
            .OrderBy(u => u.Health);
        var lastHittableUnits = opponentUnits
            .Where( u => u.CanLastHit(AttackDamage))
            .OrderByDescending(u => u.Health);
        if (deniableUnits.Any())
            Console.WriteLine($"ATTACK {deniableUnits.First().Id}");
        else if (lastHittableUnits.Any())
            Console.WriteLine($"ATTACK {lastHittableUnits.First().Id}");
        else
            // TODO: Improve this logic - coordinate attacks between heroes
            AttackNearest(EntityType.Unit);
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
        Console.WriteLine("JUST CHILLING");
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
    static void GetBushAndSpawnPoints()
    {
        int bushAndSpawnPointCount = int.Parse(Console.ReadLine()); // useful from wood1, represents the number of bushes and the number of places where neutral units can spawn
        for (int i = 0; i < bushAndSpawnPointCount; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            string entityType = inputs[0]; // BUSH, from wood1 it can also be SPAWN
            int x = int.Parse(inputs[1]);
            int y = int.Parse(inputs[2]);
            int radius = int.Parse(inputs[3]);
        }
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
                    AttackDamage = int.Parse(inputs[9]),
                    Skill1Cooldown = int.Parse(inputs[13]),
                    Skill2Cooldown = int.Parse(inputs[14]),
                    Skill3Cooldown = int.Parse(inputs[15]),
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
                    AttackDamage = int.Parse(inputs[9]),
                    ItemsOwned = int.Parse(inputs[21])
                };
                // int shield = int.Parse(inputs[8]); // useful in bronze
                // int movementSpeed = int.Parse(inputs[10]);
                // int stunDuration = int.Parse(inputs[11]); // useful in bronze
                // int goldValue = int.Parse(inputs[12]);
                // int mana = int.Parse(inputs[16]);
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
        GetBushAndSpawnPoints();
        var items = GetItems();
        var healthPotions = items.Where(i => i.IsPotion && i.Health > 0);

        var roundCount = 0;
        while (true)
        {
            var gold = int.Parse(Console.ReadLine());
            var enemyGold = int.Parse(Console.ReadLine());
            var roundType = int.Parse(Console.ReadLine()); // a positive value will show the number of heroes that await a command

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
                var playerTowerLocation = playerEntities.Single(e => e.Type == EntityType.Tower).Location;
                var playerHeroes = playerEntities.Where(e => e.Type == EntityType.Hero).Select(h => (Hero) h);

                // TODO: Explore an alternative approach to looping through heroes, might make coordination easier
                foreach (var playerHero in playerHeroes)
                {
                    var otherPlayerHero = playerHeroes.SingleOrDefault(h => h != playerHero);
                    if (!playerUnits.Any())
                    {
                        if (playerHero.Location != playerTowerLocation)
                            playerHero.MoveTo(playerTowerLocation);
                        else
                            playerHero.AttackNearest(EntityType.Unit);
                    }
                    else
                    {
                        if (!playerHero.BehindFrontLine(playerUnits))
                            playerHero.MoveToSafePosition(playerUnits);
                        else
                        {
                            if (playerHero.HasLowHealth
                                && gold >= healthPotions.Min(hp => hp.Cost)
                                && (otherPlayerHero == null || playerHero.Health <= otherPlayerHero.Health))
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
                                (playerHero.Skill1Cooldown == 0 || playerHero.Skill2Cooldown == 0) &&
                                (playerHero.HasLowHealth || (otherPlayerHero != null && otherPlayerHero.HasLowHealth)))
                            {
                                var lowestHealthHero = playerHero.Health < otherPlayerHero.Health ? playerHero : otherPlayerHero;
                                if (playerHero.Skill1Cooldown == 0)
                                    playerHero.Cast("AOEHEAL", lowestHealthHero.Location);
                                else
                                    playerHero.Cast("SHIELD", lowestHealthHero.Id);
                            }
                            else if (playerHero.HasLowHealth && otherPlayerHero != null && !otherPlayerHero.HasLowHealth)
                                playerHero.MoveTo(playerTowerLocation);
                            // TODO: Sell item if both heroes have low health
                            else
                            {
                                var availableDamageItemsToBuy = items
                                    .Where(i => i.Cost <= gold - healthPotions.Min(hp => hp.Cost) && i.Damage > 0);
                                if (availableDamageItemsToBuy.Any() && playerHero.HasItemSpace)
                                {
                                    var bestItemForDamage = availableDamageItemsToBuy
                                        .OrderByDescending(i => i.Damage)
                                        .First();
                                    playerHero.BuyItem(bestItemForDamage.Name);
                                    gold -= bestItemForDamage.Cost;
                                }
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
                                    if (opponentUnits.Any() && opponentHeroes.Any(h => !h.BehindFrontLine(opponentUnits)))
                                        // TODO: Add some logic here to decide which hero to attack (closest / lowest health / etc)
                                        playerHero.AttackNearest(EntityType.Hero);
                                    else if (opponentUnits.Any() && opponentUnits.Any(u => Math.Abs(u.Location.X - opponentTowerLocation.X) > 150))
                                        playerHero.AttemptToLastHit(playerUnits, opponentUnits);
                                    else
                                        playerHero.Wait();
                                    // TODO: Special attacks if off-cooldown
                                }
                            }
                        }
                    }
                }
            }
            roundCount++;
        }
    }
}