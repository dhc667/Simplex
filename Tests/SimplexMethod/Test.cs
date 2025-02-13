using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class Tests
{
    public static void Test(string matricesJsonPath)
    {
        List<POL> problems;
        try
        {
            problems = ParseJsonMatrices(matricesJsonPath);
        }
        catch (Exception)
        {
            System.Console.WriteLine("Invalid Json parsing");
            throw;
        }

        foreach (var pol in problems)
        {
            try
            {
                System.Console.WriteLine("Test:");
                System.Console.WriteLine(pol.ConstraintsMatrix.ToString());

                System.Console.WriteLine(pol.ObjectiveFunction.ToString());

                (double? pythonResult, double pythonTime) = SolveWithPython(pol);
                var t0 = DateTime.Now;
                double? csharpResult = new SimplexMethod(pol).Solution.ObjectiveFunction;
                var t = DateTime.Now - t0;
                System.Console.WriteLine("Python time: {0}, C# time: {1}", pythonTime, t.TotalSeconds);
                if (pythonResult == null)
                {
                    if (csharpResult != null)
                    {
                        System.Console.WriteLine("Results differ");
                    }
                    continue;
                }
                else if (csharpResult == null)
                {
                    System.Console.WriteLine("Results differ");
                    continue;
                }
                else if (Math.Abs(pythonResult.Value - csharpResult.Value) > 1e-6)
                {
                    System.Console.WriteLine("Results differ");
                }
            }
            catch (Exception)
            {
                System.Console.WriteLine("Invalid solution");
                throw;
            }
        }
    }

    private static List<POL> ParseJsonMatrices(string matricesJsonPath)
    {
        string content = File.ReadAllText(matricesJsonPath);


        var data = JsonSerializer.Deserialize<List<JsonMatrix>>(content);


        if (data == null)
        {
            System.Console.WriteLine("Invalid json input at " + matricesJsonPath);
            throw new Exception();
        }
        List<POL> problems = [];

        foreach (var jsonMatrix in data)
        {
            try
            {
                problems.Add(GetPOL(jsonMatrix));
            }
            catch (Exception)
            {
                System.Console.WriteLine("Invalid POL parsing");
                throw;
            }
        }

        return problems;
    }

    private static POL GetPOL(JsonMatrix jsonMatrix)
    {
        var objectiveFunction = new DenseVector(jsonMatrix.c.ToArray());
        var constraintsMatrix = new DenseMatrix(jsonMatrix.Am, jsonMatrix.An, jsonMatrix.A.ToArray());
        var constraintsVector = new DenseVector(jsonMatrix.b.ToArray());
        var (initialSolution, basisVariables) = GetInitialSolution(constraintsVector, jsonMatrix.Am, jsonMatrix.An);

        return new LinearProgram(objectiveFunction, constraintsMatrix, constraintsVector, initialSolution, basisVariables);
    }

    private static (Vector<double> initialSolution, List<bool> basisVariables) GetInitialSolution(Vector<double> b, int rowCount, int colCount)
    {
        var initialSolution = new double[colCount];
        var basisVariables = new bool[colCount];

        for (int i = 0; i < colCount - b.Count; i++)
        {
            initialSolution[i] = 0;
            basisVariables[i] = false;
        }
        for (int i = colCount - b.Count, j = 0; i < colCount; i++, j++)
        {
            initialSolution[i] = b[j];
            basisVariables[i] = true;
        }

        return (new DenseVector(initialSolution), new List<bool>(basisVariables));
    }

    private static (double?, double time) SolveWithPython(POL pol)
    {
        var polJson = GetPOLJson(pol);
        var startInfo = new ProcessStartInfo
        {
            FileName = "python3",
            Arguments = $"./Tests/SimplexMethod/solvePOL.py \"{polJson}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(startInfo))
        {
            using (var reader = process!.StandardOutput)
            {
                string result = reader.ReadToEnd();
                process.WaitForExit();
                System.Console.WriteLine("Result: {0}", result);
                var output = result.Split(" ");
                var time = double.Parse(output[1]);
                if (output[0] == "None")
                {
                    return (null, time);
                }
                return (double.Parse(output[0]), time);
            }
        }
    }

    private static string GetPOLJson(POL pol)
    {
        List<List<double>> A = [];
        for (int i = 0; i < pol.ConstraintsMatrix.RowCount; i++)
        {
            A.Add(pol.ConstraintsMatrix.Row(i).ToList());
        }
        List<double> b = pol.ConstraintsVector.ToList();
        List<double> c = pol.ObjectiveFunction.ToList();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var str = JsonSerializer.Serialize(new { A, b, c }, options);
        str = str.Replace("\"", "\\\"");
        return str;
    }

    private class JsonMatrix
    {
        public List<double> A { get; set; } = null!;
        public int Am { get; set; }
        public int An { get; set; }
        public List<double> c { get; set; } = null!;
        public List<double> b { get; set; } = null!;
    }
}

