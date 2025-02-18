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
        // Add solve code here
    }
}