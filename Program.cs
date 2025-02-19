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