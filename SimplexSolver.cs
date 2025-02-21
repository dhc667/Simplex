using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

public class SimplexSolver
{
    // Fields
    private int m; // Number of rows in the matrix
    private int n; // Number of columns in the initial vector
    private List<double> vectorN;
    private List<List<double>> matrix;
    private double[] vectorM;
    private int[] vectorSign;
    private int[] binaryVectorN;

    private Dictionary<int, (int, int)> indexMapping = new();
    private List<int> initBase;
    private List<double> vectorNFisrtFase;
    private List<bool> artificials;
    private bool twoFases = false;

    // Constructor
    public SimplexSolver(
        int m, int n, List<double> vectorN, List<List<double>> matrix, double[] vectorM, int[] vectorSign, int[] binaryVectorN)
    {
        this.m = m;
        this.n = n;
        this.vectorN = new List<double>(vectorN);
        this.matrix = new List<List<double>>(matrix);
        this.vectorM = (double[])vectorM.Clone();
        this.vectorSign = (int[])vectorSign.Clone();
        this.binaryVectorN = (int[])binaryVectorN.Clone();
        
    }
    // Convert the linear optimization problem to its dual
    public void ConvertToDual()
    {
        // Step 1: Process vectorSign and modify matrix and vectorM
        for (int i = 0; i < vectorSign.Length; i++)
        {
            if (vectorSign[i] == -1)
            {
                vectorM[i] *= -1;
                vectorSign[i] *= -1;

                for (int j = 0; j < matrix[i].Count; j++)
                {
                    matrix[i][j] *= -1;
                }
            }
        }

        // Step 2: Transpose the matrix
        List<List<double>> transposedMatrix = new();
        for (int j = 0; j < matrix[0].Count; j++)
        {
            List<double> newRow = new();
            for (int i = 0; i < matrix.Count; i++)
            {
                newRow.Add(matrix[i][j]);
            }
            transposedMatrix.Add(newRow);
        }

        // Step 3: Convert vectorN to the new vectorM and reverse it
        double[] newVectorM = vectorN.ToArray();

        // Step 4: Create a new binary vector
        int[] newBinaryVector = new int[vectorSign.Length];
        for (int i = 0; i < newBinaryVector.Length; i++)
        {
            newBinaryVector[i] = vectorSign[i] == 0 ? 0 : 1;
        }

        // Step 5: Create a new vectorSign
        int[] newVectorSign = new int[binaryVectorN.Length];

        for (int i = 0; i < binaryVectorN.Length; i++)
        {
            if (binaryVectorN[i] == 0)
            {
                newVectorSign[i] = 0;
            }
            else
            {
                newVectorSign[i] = -1;
            }
        }

        // Step 6: Update the fields with new values
        this.vectorN = vectorM.Select(x => -x).ToList();
        this.vectorM = newVectorM;
        this.matrix = transposedMatrix;
        this.binaryVectorN = newBinaryVector;
        this.vectorSign = newVectorSign;
        var t = this.n;
        this.n = m;
        this.m = t;
        // Display results
        // Console.WriteLine("\nConversion to Dual Completed:");
        // Console.WriteLine("\nUpdated vectorN (new vectorM):");
        // Console.WriteLine(string.Join(", ", vectorN));
        // Console.WriteLine("\nUpdated vectorM (new vectorN):");
        // Console.WriteLine(string.Join(", ", vectorM));
        // Console.WriteLine("\nUpdated matrix (transposed):");
        // foreach (var row in matrix)
        // {
        //     Console.WriteLine(string.Join(", ", row));
        // }
        // Console.WriteLine("\nUpdated binary vector:");
        // Console.WriteLine(string.Join(", ", binaryVectorN));
        // Console.WriteLine("\nUpdated vectorSign:");
        // Console.WriteLine(string.Join(", ", vectorSign));
    }
    // Preprocess problem for binaryVectorN
    private void PreprocessBinaryVectorN()
    {
        for (int i = 0; i < binaryVectorN.Length; i++)
        {
            if (binaryVectorN[i] == 0)
            {
                double value = vectorN[i];
                vectorN.Add(value);
                vectorN.Add(-value);

                List<double> column = new();
                foreach (var row in matrix)
                {
                    column.Add(row[i]);
                }

                for (int j = 0; j < m; j++)
                {
                    matrix[j].Add(column[j]);
                    matrix[j].Add(-column[j]);
                }

                int newIndex1 = vectorN.Count - 2;
                int newIndex2 = vectorN.Count - 1;
                indexMapping[i] = (newIndex1, newIndex2);
            }
        }

        for (int i = binaryVectorN.Length - 1; i >= 0; i--)
        {
            if (binaryVectorN[i] == 0)
            {
                vectorN.RemoveAt(i);
                foreach (var row in matrix)
                {
                    row.RemoveAt(i);
                }
                binaryVectorN[i] = 1;
            }
        }
        
        initBase = new List<int>(new int[vectorN.Count]);
        vectorNFisrtFase = new List<double>(new double[vectorN.Count]);
        artificials = new List<bool>(new bool[vectorN.Count]);
    }

    // Handle negative elements in vectorM
    private void ProcessVectorM()
    {
        for (int i = 0; i < vectorM.Length; i++)
        {
            if (vectorM[i] < 0)
            {
                vectorM[i] *= -1;
                vectorSign[i] *= -1;

                for (int j = 0; j < matrix[i].Count; j++)
                {
                    matrix[i][j] *= -1;
                }
            }
        }
    }

    // Handle vectorSign and setup for simplex method
    private void ProcessVectorSign()
    {
        for (int i = 0; i < vectorSign.Length; i++)
        {
            if (vectorSign[i] == 1)
            {
                AddColumn(0,true, false, i, 1);
            }
            else if (vectorSign[i] == 0)
            {
                twoFases = true;
                AddColumn(1,true, true, i, 1);
            }
            else if (vectorSign[i] == -1)
            {
                twoFases = true;
                AddColumn(0,false, false, i, -1);
                AddColumn(1,true, true, i, 1);
            }
        }
    }

    private void AddColumn(double firstFaseValue,bool isBase, bool isArtificial, int rowIndex, int value)
    {
        artificials.Add(isArtificial);
        vectorN.Add(0);
        vectorNFisrtFase.Add(firstFaseValue);
        initBase.Add(isBase? 1 : 0);

        for (int j = 0; j < m; j++)
        {
            matrix[j].Add(j == rowIndex ? value : 0);
        }
    }

    // Display results
    public void DisplayResults()
    {
        Console.WriteLine("\nUpdated vectorN:");
        Console.WriteLine(string.Join(", ", vectorN));

        Console.WriteLine("\nUpdated matrix:");
        foreach (var row in matrix)
        {
            Console.WriteLine(string.Join(", ", row));
        }

        Console.WriteLine("\nIndex mapping:");
        foreach (var kvp in indexMapping)
        {
            Console.WriteLine($"Original index {kvp.Key} -> New indexes {kvp.Value.Item1}, {kvp.Value.Item2}");
        }

        Console.WriteLine("\nInitial Base:");
        Console.WriteLine(string.Join(", ", initBase));

        if (twoFases)
        {
            Console.WriteLine("\nTwo phases required:");
            Console.WriteLine("\nvectorNFisrtFase:");
            Console.WriteLine(string.Join(", ", vectorNFisrtFase));
            Console.WriteLine("\nArtificial Variables:");
            Console.WriteLine(string.Join(", ", artificials));
            Console.WriteLine("\nvectorM:");
            Console.WriteLine(string.Join(", ", vectorM));
        }
        else
        {
            Console.WriteLine("\nSingle phase method is sufficient.");
        }
    }

    // Solve the simplex problem
    public SimplexSolution Solve()
    {
        if(false){
            ConvertToDual();
        }
        PreprocessBinaryVectorN();
        ProcessVectorM();
        ProcessVectorSign();
        // DisplayResults();
        
        List<bool> baseVector = initBase.Select(x => x == 1).ToList();
        List<int> basisIndexes = initBase.Select((x, i) => x == 1 ? i : -1).Where(i => i != -1).ToList();

        int vectormIndex = 0;
        var initialSolution = DenseVector.OfEnumerable(
            baseVector.Select((x, i) => x ? vectorM[vectormIndex++] : 0)
        );

        var objectiveFunction = DenseVector.OfEnumerable(vectorN);
        var constraintsMatrix = DenseMatrix.OfRows(matrix.Count, matrix[0].Count, matrix);
        var constraintsVector = DenseVector.OfArray(vectorM);

        // Display the initial setup
        // Console.WriteLine("\nInitial Setup:");
        // Console.WriteLine("\nObjective Function:");
        // Console.WriteLine(string.Join(", ", objectiveFunction));
        // Console.WriteLine("\nConstraints Matrix:");
        // for (int i = 0; i < constraintsMatrix.RowCount; i++)
        // {
        //     Console.WriteLine(string.Join(", ", constraintsMatrix.Row(i)));
        // }
        // Console.WriteLine("\nConstraints Vector:");
        // Console.WriteLine(string.Join(", ", constraintsVector));
        // Console.WriteLine("\nInitial Solution:");
        // Console.WriteLine(string.Join(", ", initialSolution));
        // Console.WriteLine("\nBase Vector:");
        // Console.WriteLine(string.Join(", ", baseVector));
        // Console.WriteLine("\nBasis Indexes List:");
        // Console.WriteLine(string.Join(", ", basisIndexes));

        SimplexMethod simplexMethod;

        if (twoFases)
        {
            // Handle the first phase
            var firstPhaseObjectiveFunction = DenseVector.OfEnumerable(vectorNFisrtFase);

            LinearProgram firstPhaseProgram = new LinearProgram(firstPhaseObjectiveFunction, constraintsMatrix, constraintsVector, initialSolution, basisIndexes);
            simplexMethod = new SimplexMethod(firstPhaseProgram);
            var firstPhaseSolution = simplexMethod.Solution;

            // Check if the first phase solution is feasible

            if (firstPhaseSolution.Type == SimplexSolution.SolutionType.Unbounded || !Comparers.almostEqual(firstPhaseSolution.ObjectiveFunction.Value, 0))
            {
                Console.WriteLine("First phase did not find a feasible solution.");
                return new SimplexSolution(SimplexSolution.SolutionType.Unbounded,null,null,null);
            }

            // Use the first phase solution as the initial solution for the second phase
            var newInitialSolution = (DenseVector)firstPhaseSolution.Solution;
            var newBasisList = firstPhaseSolution.BasisIndexes;


            for (int i = 0; i < matrix.Count; i++)
            {
                matrix[i].Add(0);
            }

            List<double> artificialRow = new List<double>(new double[matrix[0].Count]);
            for (int i = 0; i < artificials.Count; i++)
            {
                if (artificials[i])
                {
                    artificialRow[i] = 1; // Place 1 in artificial variable column
                }
            }
            //Console.WriteLine(string.Join(" ",artificialRow));
            artificialRow[artificials.Count] = 1;

            matrix.Add(artificialRow);
             // Add the new row to the matrix
            vectorM = vectorM.Append(0).ToArray(); // Extend constraint vector

            // Add a 0 at the end of each row of the matrix
            

            // Add a 0 to the end of the objective function
            var updatedObjectiveFunction = DenseVector.OfEnumerable(objectiveFunction.Append(0));

            // Add a 0 to the end of the initial solution
            var updatedInitialSolution = DenseVector.OfEnumerable(newInitialSolution.Append(0));

            // Add objectiveFunction.Length - 1 to the new basis list
            newBasisList.Add(updatedObjectiveFunction.Count - 1);

            // foreach (var row in matrix)
            // {
            //     Console.WriteLine(string.Join(" ",row));
            // }

            var constraintsMatrix2 = DenseMatrix.OfRows(matrix.Count, matrix[0].Count, matrix);
            var constraintsVector2 = DenseVector.OfArray(vectorM);
            
            LinearProgram linearProgram = new LinearProgram(updatedObjectiveFunction, constraintsMatrix2, constraintsVector2, updatedInitialSolution, newBasisList);
            
            simplexMethod = new SimplexMethod(linearProgram);
            
        }
        else
        {
            LinearProgram linearProgram = new LinearProgram(objectiveFunction, constraintsMatrix, constraintsVector, initialSolution, basisIndexes);
            simplexMethod = new SimplexMethod(linearProgram);
        }

        var finalSolution = simplexMethod.Solution;
        var truncatedSolution = new SimplexSolution(
            finalSolution.Type,
            finalSolution.ObjectiveFunction,
            finalSolution.Solution?.Take(n).ToArray() == null ? null : DenseVector.OfEnumerable(finalSolution.Solution.Take(n).ToArray()),
            finalSolution.Basis,
            finalSolution.BasisIndexes
        );
        // Display the final solution
        if (finalSolution.Type == SimplexSolution.SolutionType.SingleOptimal)
        {
            /* Console.WriteLine("Optimal solution found:"); */
            /* Console.WriteLine($"Objective function value: {finalSolution.ObjectiveFunction}"); */
            /* Console.WriteLine($"Solution: {string.Join(", ", finalSolution.Solution?.ToArray() ?? new double[0])}"); */
        }
        else if (finalSolution.Type == SimplexSolution.SolutionType.InfiniteOptimalSet)
        {
            /* Console.WriteLine(finalSolution.Solution); */
            /* Console.WriteLine("Infinite optimal solutions found."); */
        }
        else if (finalSolution.Type == SimplexSolution.SolutionType.Unbounded)
        {
            Console.WriteLine("The problem is unbounded.");
        }
        else
        {
            Console.WriteLine("No feasible solution found.");
        }

        return finalSolution;
    }
}
