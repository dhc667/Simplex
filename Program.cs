using System.Text.Json;
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
                    RunSolve();
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
                string json = JsonSerializer.Serialize(testCase, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(json);
                Console.WriteLine("Expected Evaluation: " + testCase.solution.evaluationValue);
                Console.WriteLine("Obtained Evaluation: " + sol.ObjectiveFunction);
                Console.WriteLine("Expected Solution: " + string.Join(", ", testCase.solution.solutionVector));
                Console.WriteLine("Obtained Solution: " + string.Join(", ", sol.Solution));
                Console.WriteLine("Success: " + sol.Type);
                Console.WriteLine("--------------------------------------------------");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading or processing the test file: " + ex.Message);
        }
    }


    private static void RunSolve()
    {
        Console.WriteLine("Solving...");

        // Example problem
        int m = 1; // Number of constraints
        int n = 1; // Number of variables

        List<double> vectorN = new List<double> {-1 }; // Coefficients of the objective function
        List<List<double>> matrix = new List<List<double>>
        {
            new List<double> {1 },

        };
        double[] vectorM = { 2}; // Right-hand side values of the constraints
        int[] vectorSign = { 1 }; // Signs of the constraints (1 for <=, 0 for =, -1 for >=)
        int[] binaryVectorN = { 1 }; 

        SimplexSolver solver = new SimplexSolver(m, n, vectorN, matrix, vectorM, vectorSign, binaryVectorN);
        solver.Solve();
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