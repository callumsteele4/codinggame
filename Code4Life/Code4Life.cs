using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

enum SampleLocation
{
    Cloud = -1,
    Player = 0,
    Opponent = 1
}

class Molecules
{
    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }
    public int D { get; set; }
    public int E { get; set; }
    
    public void AddMoleculeType(string moleculeType)
    {
        switch (moleculeType)
        {
            case "A": A++; break;
            case "B": B++; break;
            case "C": C++; break;
            case "D": D++; break;
            case "E": E++; break;
            default: break;
        }
    }

    public override bool Equals(object obj)
    {
        var molecules = obj as Molecules;

        if (molecules == null)
        {
            return false;
        }

        return A == molecules.A
               && B == molecules.B
               && C == molecules.C
               && D == molecules.D
               && E == molecules.E;
    }
    
    public override int GetHashCode()
    {
        return this.GetHashCode();
    }

    public static Molecules operator +(Molecules m1, Molecules m2)
    {
        return new Molecules
        {
            A = m1.A + m2.A,
            B = m1.B + m2.B,
            C = m1.C + m2.C,
            D = m1.D + m2.D,
            E = m1.E + m2.E
        };
    }
    
    public static Molecules operator -(Molecules m1, Molecules m2)
    {
        return new Molecules
        {
            A = Math.Max(m1.A - m2.A, 0),
            B = Math.Max(m1.B - m2.B, 0),
            C = Math.Max(m1.C - m2.C, 0),
            D = Math.Max(m1.D - m2.D, 0),
            E = Math.Max(m1.E - m2.E, 0)
        };
    }
    
    
    public static bool operator <=(Molecules m1, Molecules m2)
    {
        return m1.A <= m2.A
            && m1.B <= m2.B
            && m1.C <= m2.C
            && m1.D <= m2.D
            && m1.E <= m2.E;
    }

    public static bool operator >=(Molecules m1, Molecules m2)
    {
        return m1.A >= m2.A
            || m1.B >= m2.B
            || m1.C >= m2.C
            || m1.D >= m2.D
            || m1.E >= m2.E;
    }

    public static bool operator >(Molecules m1, Molecules m2)
    {
        return m1.A > m2.A
            || m1.B > m2.B
            || m1.C > m2.C
            || m1.D > m2.D
            || m1.E > m2.E;
    }

    public static bool operator <(Molecules m1, Molecules m2)
    {
        return m1.A < m2.A
            || m1.B < m2.B
            || m1.C < m2.C
            || m1.D < m2.D
            || m1.E < m2.E;
    }
    
    public Molecules Copy()
    {
        return new Molecules
        {
            A = A,
            B = B,
            C = C,
            D = D,
            E = E
        };
    }
    
    public int TotalMolecules()
    {
        return A + B + C + D + E;
    }
    
    public override string ToString()
    {
        return "\n" +
        $"  A: {A}\n" +
        $"  B: {B}\n" +
        $"  C: {C}\n" +
        $"  D: {D}\n" +
        $"  E: {E}\n";
    }
}

class Robot
{
    public string Module { get; set; }
    public int Eta { get; set; }
    public int Score { get; set; }
    public Molecules CarriedMolecules { get; set; }
    public Molecules MolecularExpertise { get; set; }

    public bool Travelling => Eta > 0;

    public Robot()
    {
        var input = Console.ReadLine();
        Console.Error.WriteLine($"Robot: {input}");
        string[] inputs = input.Split(' ');

        Module = inputs[0];
        Eta = int.Parse(inputs[1]);
        Score = int.Parse(inputs[2]);
        CarriedMolecules = new Molecules
        {
            A = int.Parse(inputs[3]),
            B = int.Parse(inputs[4]),
            C = int.Parse(inputs[5]),
            D = int.Parse(inputs[6]),
            E = int.Parse(inputs[7])
        };
        MolecularExpertise = new Molecules
        {
            A = int.Parse(inputs[8]),
            B = int.Parse(inputs[9]),
            C = int.Parse(inputs[10]),
            D = int.Parse(inputs[11]),
            E = int.Parse(inputs[12]),
        };
    }

    public Robot Copy()
    {
        return new Robot
        {
            Module = Module,
            Eta = Eta,
            Score = Score,
            CarriedMolecules = CarriedMolecules.Copy(),
            MolecularExpertise = MolecularExpertise.Copy()
        };
    }
}

class Sample
{
    public int Id { get; set; }
    public SampleLocation SampleLocation { get; set; }
    public int Health { get; set; }
    public Molecules RequiredMolecules { get; set; }
    public int Rank { get; set; }
    public string ExpertiseGain { get; set; }
    
    public bool IsDiagnosed()
    {
        return Health != -1
            && RequiredMolecules.A != -1
            && RequiredMolecules.B != -1
            && RequiredMolecules.C != -1
            && RequiredMolecules.D != -1
            && RequiredMolecules.E != -1;
    }

    public bool HasMoleculesRequired(Molecules playerMolecules)
    {
        return playerMolecules >= RequiredMolecules;
    }

    public Sample Copy()
    {
        return new Sample
        {
            Id = Id,
            SampleLocation = SampleLocation,
            Health = Health,
            RequiredMolecules = RequiredMolecules.Copy(),
            Rank = Rank,
            ExpertiseGain = ExpertiseGain
        };
    }
}

class Move
{
    public string Action { get; set; }
    public string Module { get; set; }
    public int SampleRank { get; set; }
    public string SampleId { get; set; }
    public string MoleculeType { get; set; }

    private string _actionParameter =>
        SampleId != null
            ? SampleId
            : MoleculeType != null
                ? MoleculeType
                : $"{SampleRank}";

    public override string ToString()
    {
        switch (Action)
        {
            case "GOTO":
                return $"{Action} {Module}";
            case "CONNECT":
                return $"{Action} {_actionParameter}";
            default:
                return "Invalid move object.";
        }
    }
}

class Simulation
{
    private Robot _player;
    private Robot _opponent;
    private Molecules _availableMolecules;
    private Sample[] _samples;

    private IEnumerable<Sample> _playerSamples => _samples.Where(s => s.SampleLocation == SampleLocation.Player);
    private IEnumerable<Sample> _cloudSamples => _samples.Where(s => s.SampleLocation == SampleLocation.Cloud);

    public Simulation(Robot player, Robot opponent, Molecules availableMolecules, Sample[] samples)
    {
        _player = player;
        _opponent = opponent;
        _availableMolecules = availableMolecules;
        _samples = samples;
    }

    // public Simulation MakeMove(Move playerMove)
    // {
    //     return new Simulation(
    //         _player.Copy(),
    //         _opponent.Copy(),
    //         _availableMolecules.Copy(),
    //         _samples.Select(s => s.Copy()).ToArray());
    // }

    public Move[] GetPossibleMoves()
    {
        if (_player.Travelling)
            return new Move[] {};

        return GetMovesFromModule(_player.Module);
    }

    private Move[] GetMovesFromModule(string module)
    {
        var noMoves = new Move[] {};

        switch (module)
        {
            case "START_POS":
                return new Move[]
                {
                    new Move { Action = "GOTO", Module = "SAMPLES" },
                    new Move { Action = "GOTO", Module = "DIAGNOSIS" },
                    new Move { Action = "GOTO", Module = "MOLECULES" },
                    new Move { Action = "GOTO", Module = "LABORATORY" }
                };
            case "SAMPLES":
                var samplePickupMoves = _playerSamples.Count() < 3
                    ? new Move[]
                    {
                        new Move { Action = "CONNECT", SampleRank = 1 },
                        new Move { Action = "CONNECT", SampleRank = 2 },
                        new Move { Action = "CONNECT", SampleRank = 3 }
                    }
                    : noMoves;
                return new Move[]
                {
                    new Move { Action = "GOTO", Module = "DIAGNOSIS" },
                    new Move { Action = "GOTO", Module = "MOLECULES" },
                    new Move { Action = "GOTO", Module = "LABORATORY" }
                }
                .Concat(samplePickupMoves)
                .ToArray();
            case "DIAGNOSIS":
                var playerSampleMoves = _playerSamples.Select(s => new Move { Action = "CONNECT", SampleId = $"{s.Id}" });
                var cloudSampleMoves = _playerSamples.Count() < 3
                    ? _cloudSamples.Select(s => new Move { Action = "CONNECT", SampleId = $"{s.Id}" })
                    : noMoves;
                return new Move[]
                {
                    new Move { Action = "GOTO", Module = "SAMPLES" },
                    new Move { Action = "GOTO", Module = "MOLECULES" },
                    new Move { Action = "GOTO", Module = "LABORATORY" }
                }
                .Concat(playerSampleMoves)
                .Concat(cloudSampleMoves)
                .ToArray();
            case "MOLECULES":
                var moleculePickupMoves = _player.CarriedMolecules.TotalMolecules() < 10
                    ? new Move[]
                    {
                        new Move { Action = "CONNECT", MoleculeType = "A" },
                        new Move { Action = "CONNECT", MoleculeType = "B" },
                        new Move { Action = "CONNECT", MoleculeType = "C" },
                        new Move { Action = "CONNECT", MoleculeType = "D" },
                        new Move { Action = "CONNECT", MoleculeType = "E" }
                    }
                    : noMoves;
                return new Move[]
                {
                    new Move { Action = "GOTO", Module = "SAMPLES" },
                    new Move { Action = "GOTO", Module = "DIAGNOSIS" },
                    new Move { Action = "GOTO", Module = "LABORATORY" }
                }
                .Concat(moleculePickupMoves)
                .ToArray();
            case "LABORATORY":
                var playerSampleDropoffMoves = _playerSamples
                    .Where(s => s.HasMoleculesRequired(_player.CarriedMolecules + _player.MolecularExpertise))
                    .Select(s => new Move { Action = "CONNECT", SampleId = $"{s.Id}" });
                return new Move[]
                {
                    new Move { Action = "GOTO", Module = "SAMPLES" },
                    new Move { Action = "GOTO", Module = "DIAGNOSIS" },
                    new Move { Action = "GOTO", Module = "MOLECULES" }
                }
                .Concat(playerSampleDropoffMoves)
                .ToArray();
            default:
                return noMoves;
        }
    }
}

class SimulationOrchestrator
{
    private Simulation CurrentSimulationSnapshot;

    public SimulationOrchestrator(Simulation currentSimulationSnapshot)
    {
        CurrentSimulationSnapshot = currentSimulationSnapshot;
    }

    public string CalculateBestMove()
    {
        var possibleMoves = CurrentSimulationSnapshot.GetPossibleMoves();

        if (!possibleMoves.Any())
            return "TRAVELLING";

        return possibleMoves[new Random().Next(possibleMoves.Count() - 1)].ToString();
    }
}

class Player
{
    static Sample[] GetSamples()
    {
        var sampleCount = int.Parse(Console.ReadLine());
        Sample[] samples = new Sample[sampleCount];

        string[] inputs;
        for (int i = 0; i < sampleCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            Console.Error.WriteLine($"Sample {i}: {String.Join(" ", inputs)}");

            samples[i] = new Sample
            {
                Id = int.Parse(inputs[0]),
                SampleLocation = (SampleLocation)int.Parse(inputs[1]),
                Rank = int.Parse(inputs[2]),
                ExpertiseGain = inputs[3],
                Health = int.Parse(inputs[4]),
                RequiredMolecules = new Molecules
                {
                    A = int.Parse(inputs[5]),
                    B = int.Parse(inputs[6]),
                    C = int.Parse(inputs[7]),
                    D = int.Parse(inputs[8]),
                    E = int.Parse(inputs[9])
                }
            };
        }
        return samples;
    }

    static Molecules GetAvailableMolecules()
    {
        string[] inputs = Console.ReadLine().Split(' ');
        return new Molecules
        {
            A = int.Parse(inputs[0]),
            B = int.Parse(inputs[1]),
            C = int.Parse(inputs[2]),
            D = int.Parse(inputs[3]),
            E = int.Parse(inputs[4])
        };
    }

    // Not yet weighted
    static void GetProjectStuff()
    {
        int projectCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < projectCount; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            // int a = int.Parse(inputs[0]);
            // int b = int.Parse(inputs[1]);
            // int c = int.Parse(inputs[2]);
            // int d = int.Parse(inputs[3]);
            // int e = int.Parse(inputs[4]);
        }
    }

    static void Main(string[] args)
    {
        string[] inputs;
        GetProjectStuff();

        // game loop
        while (true)
        {
            var simulationOrchestrator = new SimulationOrchestrator(new Simulation(new Robot(), new Robot(), GetAvailableMolecules(), GetSamples()));
            Console.WriteLine(simulationOrchestrator.CalculateBestMove());
        }
    }
}