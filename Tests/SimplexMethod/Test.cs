using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class Tests
{
    public static void Test(string matricesJsonPath)
    {
        void showProblem(POL pol)
        {

            System.Console.WriteLine("Test:");
            System.Console.WriteLine("Matrix:");
            System.Console.WriteLine(pol.ConstraintsMatrix.ToString());

            System.Console.WriteLine("C vector:");
            System.Console.WriteLine(pol.ObjectiveFunction.ToString());

            System.Console.WriteLine("B vector:");
            System.Console.WriteLine(pol.ConstraintsVector.ToString());
        }

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

        var totalPythonTime = 0d;
        var totalCSharpTime = 0d;
        var c = 0;

        foreach (var pol in problems)
        {
            c++;
            try
            {

                (double? pythonResult, double pythonTime) = SolveWithPython(pol);
                var t0 = DateTime.Now;
                double? csharpResult = new SimplexMethod(pol).Solution.ObjectiveFunction;
                var t = DateTime.Now - t0;


                totalCSharpTime += t.TotalSeconds;
                totalPythonTime += pythonTime;
                System.Console.WriteLine("Python time: {0}, C# time: {1}", pythonTime, t.TotalSeconds);
                System.Console.WriteLine("Python sol: {0}, C# sol: {1}", pythonResult, csharpResult);
                System.Console.WriteLine("Python avg time: {0}, C# avg time: {1}", totalPythonTime/(double)c, totalCSharpTime/(double)c);


                if (pythonResult == null)
                {
                    if (csharpResult != null)
                    {
                        System.Console.WriteLine("Results differ");
                        showProblem(pol);
                    }
                    continue;
                }
                else if (csharpResult == null)
                {
                    System.Console.WriteLine("Results differ");
                    showProblem(pol);
                    continue;
                }
                else if (Math.Abs(pythonResult.Value - csharpResult.Value) > 1e-6)
                {
                    System.Console.WriteLine("Results differ");
                    showProblem(pol);
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
        var constraintsMatrix = new DenseMatrix(
            jsonMatrix.Am, 
            jsonMatrix.An, 
            ToColumnMajor(
                jsonMatrix.A.ToArray(),
                jsonMatrix.Am,
                jsonMatrix.An
            )
        );
        var constraintsVector = new DenseVector(jsonMatrix.b.ToArray());
        var (initialSolution, basisVariables) = GetInitialSolution(constraintsVector, jsonMatrix.Am, jsonMatrix.An);

        return new LinearProgram(objectiveFunction, constraintsMatrix, constraintsVector, initialSolution, basisVariables);
    }

    private static double[] ToColumnMajor(double[] a, int rows, int columns)
    {
        var answ = new double[rows*columns];

        double fromRowMajor(int r, int c) => a[columns*r + c];

        int k = 0;
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
            {
                answ[k++] = fromRowMajor(r, c);
            }

        return answ;
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
        var path = "staging.json";
        File.WriteAllText(path, polJson);
        var startInfo = new ProcessStartInfo
        {
            FileName = "python3",
            Arguments = $"./Tests/SimplexMethod/solvePOL.py \"{path}\"",
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
                /* System.Console.WriteLine("Result: {0}", result); */
                var output = result.Split(" ");
                /* System.Console.WriteLine($"Output[1] = {output[1]}"); */
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
        var constraintMatrix = pol.ConstraintsMatrix.ToRowArrays();
        for (int i = 0; i < constraintMatrix.Length; i++)
        {
            A.Add(constraintMatrix[i].ToList());
        }
        List<double> b = pol.ConstraintsVector.ToList();
        List<double> c = pol.ObjectiveFunction.ToList();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var str = JsonSerializer.Serialize(new { A, b, c }, options);
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

