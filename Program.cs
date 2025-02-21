using System.Text.Json;
using MathNet.Numerics.LinearAlgebra.Double;
public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            switch (args[0])
            {
                case "--test":
                    RunTest(args[1]);
                    break;
                case "--test2":
                    RunTest2();
                    break;
                case "--solve":
                    RunSolve("problem.json");
                    break;
                default:
                    Console.WriteLine("Unknown command");
                    break;
            }
        }
        else
        {
            Console.WriteLine("No command provided");
        }
    }



    private static void RunTest(string matricesJsonPath)
    {
        Tests.Test(matricesJsonPath);
    }

    public static void RunTest2()
    {
        string testsJsonPath = "./random_solutions.json";
        try
        {
            string jsonContent = File.ReadAllText(testsJsonPath);
            var testCases = JsonSerializer.Deserialize<List<TestCase>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (testCases == null || testCases.Count == 0)
            {
                Console.WriteLine("Error: No test cases found in the file.");
                return;
            }
            
            foreach (var testCase in testCases)
            {
                if (testCase?.problem == null || testCase?.solution == null)
                {
                    Console.WriteLine("Warning: Skipping a test case due to missing problem or solution data.");
                    continue;
                }
                
                SimplexSolver solver = new SimplexSolver(
                    testCase.problem.vectorM?.Count ?? 0, testCase.problem.vectorN?.Count ?? 0,
                    testCase.problem.vectorN ?? new List<double>(), 
                    testCase.problem.matrix ?? new List<List<double>>(),
                    testCase.problem.vectorM.ToArray() ?? new double[0], 
                    testCase.problem.vectorSign.ToArray() ?? new int[0],
                    testCase.problem.binaryVectorN.ToArray() ?? new int[0]);
                
                var sol = solver.Solve();
                
                string green = "\u001b[32m"; // Green
                string red = "\u001b[31m";   // Red
                string reset = "\u001b[0m";  // Reset color

                bool isMatch = Math.Abs((double)(testCase.solution.evaluationValue - sol.ObjectiveFunction)) < 1e-6;
                string evalColor = isMatch ? green : red;

                Console.WriteLine($"{evalColor}Expected Evaluation: {testCase.solution.evaluationValue}{reset}");
                Console.WriteLine($"{evalColor}Obtained Evaluation: {sol.ObjectiveFunction}{reset}");
                Console.WriteLine($"Expected Solution: {string.Join(", ", testCase.solution.solutionVector)}");
                Console.WriteLine($"Obtained Solution: {string.Join(", ", DenseVector.OfEnumerable(sol.Solution.Take(testCase.solution.solutionVector.Count).ToArray()) )}");
                Console.WriteLine($"Success: {sol.Type}");
                Console.WriteLine("--------------------------------------------------");
            }
        }
        catch (Exception ex)
        {

            Console.WriteLine("Error reading or processing the test file: " + ex);
        }
    }


    private static void RunSolve(string jsonPath)
    {
        string jsonContent = File.ReadAllText(jsonPath);
        /* System.Console.WriteLine(jsonContent); */
        var problem = JsonSerializer.Deserialize<ProblemData>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        SimplexSolver solver = new SimplexSolver(
            problem.vectorM?.Count ?? 0, problem.vectorN?.Count ?? 0,
            problem.vectorN ?? new List<double>(), 
            problem.matrix ?? new List<List<double>>(),
            problem.vectorM.ToArray() ?? new double[0], 
            problem.vectorSign.ToArray() ?? new int[0],
            problem.binaryVectorN.ToArray() ?? new int[0]);
        
        var sol = solver.Solve();
        
        Console.WriteLine($"Obtained Evaluation: {sol.ObjectiveFunction}");
        Console.WriteLine($"Obtained Solution: ");
        System.Console.WriteLine(sol.Solution);
        /* Console.WriteLine("--------------------------------------------------"); */
    }

}


public class TestCase
{
    public ProblemData problem { get; set; }
    public SolutionData solution { get; set; }
}

public class ProblemData
{
    public List<double> vectorN { get; set; }
    public List<List<double>> matrix { get; set; }
    public List<double> vectorM { get; set; }
    public List<int> vectorSign { get; set; }
    public List<int> binaryVectorN { get; set; }
}

public class SolutionData
{
    public List<double> solutionVector { get; set; }
    public double evaluationValue { get; set; }
    public bool success { get; set; }
    public string message { get; set; }
}
