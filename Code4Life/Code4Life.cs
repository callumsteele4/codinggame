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

    public void Move(Molecules availableMolecules, Sample[] samples)
    {
        var playerSamples = samples.Where(s => s.SampleLocation == SampleLocation.Player);
        if (Eta != 0)
        {
            Console.WriteLine();
        }
        else if (Module == "START_POS")
        {
            Console.WriteLine("GOTO SAMPLES");
        }
        else if (Module == "SAMPLES")
        {
            var playerSamplesCount = playerSamples.Count();
            if (playerSamplesCount < 1)
            {
                var sampleRank = 1;
                if (MolecularExpertise.TotalMolecules() >= 12)
                    sampleRank = 2;
                Console.WriteLine($"CONNECT {sampleRank}");
            }
            else if (playerSamplesCount < 2)
            {
                var sampleRank = 1;
                if (MolecularExpertise.TotalMolecules() >= 6)
                    sampleRank = 2;
                if (MolecularExpertise.TotalMolecules() >= 12)
                    sampleRank = 3;
                Console.WriteLine($"CONNECT {sampleRank}");
            }
            else if (playerSamplesCount < 3)
            {
                var sampleRank = 1;
                if (MolecularExpertise.TotalMolecules() >= 3)
                    sampleRank = 2;
                if (MolecularExpertise.TotalMolecules() >= 9)
                    sampleRank = 3;
                Console.WriteLine($"CONNECT {sampleRank}");
            }
            else
            {
                Console.WriteLine("GOTO DIAGNOSIS");
            }
        }
        else if (Module == "DIAGNOSIS")
        {
            var undiagnosedSample = playerSamples
                .FirstOrDefault(s => !s.IsDiagnosed());
            if (undiagnosedSample != null)
            {
                Console.WriteLine($"CONNECT {undiagnosedSample.Id}");
            }
            else
            {
                var impossibleSample = playerSamples
                    .FirstOrDefault(s => (s.RequiredMolecules - MolecularExpertise) > (availableMolecules + CarriedMolecules));
                if (impossibleSample != null)
                {
                    Console.WriteLine($"CONNECT {impossibleSample.Id}");
                }
                else
                {
                    if (playerSamples.Count() == 0)
                    {
                        var possibleCloudSample = samples
                            .Where(s => s.SampleLocation == SampleLocation.Cloud)
                            .FirstOrDefault(s => (s.RequiredMolecules - MolecularExpertise) <= (availableMolecules + CarriedMolecules));
                        if (possibleCloudSample != null)
                        {
                            Console.WriteLine($"CONNECT {possibleCloudSample.Id}");
                        }
                        else
                        {
                            Console.WriteLine("GOTO SAMPLES");
                        }
                    }
                    else
                    {
                        var totalMoleculesRequired = playerSamples
                            .Select(s => s.RequiredMolecules - MolecularExpertise)
                            .Aggregate((x,y) => x + y);
                        Console.Error.WriteLine($"Total molecules required: {totalMoleculesRequired}");
                        Console.Error.WriteLine($"Total molecules carried: {CarriedMolecules}");
                        if (totalMoleculesRequired <= CarriedMolecules)
                        {
                            Console.WriteLine("GOTO LABORATORY");
                        }
                        else
                        {
                            Console.WriteLine("GOTO MOLECULES");
                        }
                    }
                }
            } 
        }
        else if (Module == "MOLECULES")
        {
            var totalMoleculesRequired = playerSamples
                .Select(s => s.RequiredMolecules - MolecularExpertise)
                .Aggregate((x,y) => x + y);
            Console.Error.WriteLine($"Total molecules required: {totalMoleculesRequired}");
            Console.Error.WriteLine($"Total molecules carried: {CarriedMolecules}");
            Console.Error.WriteLine($"Total molecules available: {availableMolecules}");
            if (CarriedMolecules.TotalMolecules() < 10 && totalMoleculesRequired > CarriedMolecules)
            {
                var moleculeType = "";
                var unreservedCarriedMolecules = CarriedMolecules.Copy();
                
                foreach(var sample in playerSamples
                    .OrderBy(s => (s.RequiredMolecules - MolecularExpertise).TotalMolecules()))
                {
                    var requiredMoleculesForSample = sample.RequiredMolecules - MolecularExpertise;
                    
                    if (requiredMoleculesForSample <= unreservedCarriedMolecules)
                    {
                        unreservedCarriedMolecules -= requiredMoleculesForSample;
                        unreservedCarriedMolecules.AddMoleculeType(sample.ExpertiseGain);
                        continue;
                    }
                    
                    if (unreservedCarriedMolecules.A < requiredMoleculesForSample.A && availableMolecules.A > 0)
                        moleculeType = "A";
                    else if (unreservedCarriedMolecules.B < requiredMoleculesForSample.B && availableMolecules.B > 0)
                        moleculeType = "B";
                    else if (unreservedCarriedMolecules.C < requiredMoleculesForSample.C && availableMolecules.C > 0)
                        moleculeType = "C";
                    else if (unreservedCarriedMolecules.D < requiredMoleculesForSample.D && availableMolecules.D > 0)
                        moleculeType = "D";
                    else if (unreservedCarriedMolecules.E < requiredMoleculesForSample.E && availableMolecules.E > 0)
                        moleculeType = "E";
                    else
                        continue;
                    break;
                }
                
                if (String.IsNullOrWhiteSpace(moleculeType))
                {
                    if (playerSamples.Any(s => (s.RequiredMolecules - MolecularExpertise) <= CarriedMolecules))
                    {
                        Console.WriteLine("GOTO LABORATORY");
                    }
                    else
                    {
                        Console.WriteLine("GOTO DIAGNOSIS");
                    }
                }
                else
                {
                    Console.WriteLine($"CONNECT {moleculeType}");
                }
            }
            else
            {
                var numberOfPossibleHeldSamples = playerSamples
                    .Count(s => (s.RequiredMolecules - MolecularExpertise) <= (availableMolecules + CarriedMolecules));
                var numberOfPossibleCloudSamples = samples
                    .Where(s => s.SampleLocation == SampleLocation.Cloud)
                    .Count(s => (s.RequiredMolecules - MolecularExpertise) <= (availableMolecules + CarriedMolecules));
                if (numberOfPossibleHeldSamples >= 1)
                    Console.WriteLine("GOTO LABORATORY");
                else if (playerSamples.Count() >= 2 || numberOfPossibleCloudSamples >= 1)
                    Console.WriteLine("GOTO DIAGNOSIS");
                else
                    Console.WriteLine("GOTO SAMPLES");
            }
        }
        else if (Module == "LABORATORY")
        {
            if (playerSamples.Any())
            {
                var simplestSample = playerSamples
                    .OrderBy(s => (s.RequiredMolecules - MolecularExpertise).TotalMolecules())
                    .First();
                if ((simplestSample.RequiredMolecules - MolecularExpertise) <= CarriedMolecules)
                {
                    Console.WriteLine($"CONNECT {simplestSample.Id}");
                }
                else
                {
                    Console.WriteLine("GOTO MOLECULES");
                }
            }
            else
            {
                var numberOfPossibleCloudSamples = samples
                    .Where(s => s.SampleLocation == SampleLocation.Cloud)
                    .Count(s => (s.RequiredMolecules - MolecularExpertise) <= (availableMolecules + CarriedMolecules));
                if (numberOfPossibleCloudSamples >= 2)
                {
                    Console.WriteLine("GOTO DIAGNOSIS");
                }
                else
                {
                    Console.WriteLine("GOTO SAMPLES");
                }
            }
        }
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

    // Not needed for current league.
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

    static Robot GetRobotInfo()
    {
        string[] inputs = Console.ReadLine().Split(' ');
        Console.Error.WriteLine($"Robot: {String.Join(" ", inputs)}");
        return new Robot
        {
            Module = inputs[0],
            Eta = int.Parse(inputs[1]),
            Score = int.Parse(inputs[2]),
            CarriedMolecules = new Molecules
            {
                A = int.Parse(inputs[3]),
                B = int.Parse(inputs[4]),
                C = int.Parse(inputs[5]),
                D = int.Parse(inputs[6]),
                E = int.Parse(inputs[7])
            },
            MolecularExpertise = new Molecules
            {
                A = int.Parse(inputs[8]),
                B = int.Parse(inputs[9]),
                C = int.Parse(inputs[10]),
                D = int.Parse(inputs[11]),
                E = int.Parse(inputs[12]),
            }
        };
    }

    static void Main(string[] args)
    {
        string[] inputs;
        GetProjectStuff();

        // game loop
        while (true)
        {
            var player = GetRobotInfo();
            var oppononent = GetRobotInfo();

            var availableMolecules = GetAvailableMolecules();

            var samples = GetSamples();
            
            player.Move(availableMolecules, samples);

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
        }
    }
}