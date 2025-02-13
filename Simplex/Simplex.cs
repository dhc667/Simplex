using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;

class SimplexMethod
{
    private Vector<double> C { get; }
    private Matrix<double> A { get; }
    private Vector<double> B { get; }
    private Vector<double> X0 { get; }
    private List<bool> InitialBasis { get; }

    public SimplexSolution Solution { get; }

    public SimplexMethod(POL problem)
    {
        this.A = problem.ConstraintsMatrix;
        this.B = problem.ConstraintsVector;
        this.X0 = problem.InitialSolution;
        this.C = problem.ObjectiveFunction;
        this.InitialBasis = problem.BasisVectors;
        this.Solution = this.Solve();
    }

    private SimplexSolution Solve()
    {
        var currentBasis = GetBasis(InitialBasis);
        var currentBasisIndexes = new List<bool>(InitialBasis);
        var currentSolution = X0;

        while (true)
        {
            var basisLu = currentBasis.LU();

            var cB = GetBasicCosts(currentBasisIndexes);
            var cJ = GetNonBasicCosts(currentBasisIndexes);
            var lambda = GetLambdaVector(basisLu, cB);

            var r = GetRVector(cJ, lambda);
            var inIndex = GetIndexOfMinimum(r);

            if (r[inIndex] > 0)
            {
                return new SimplexSolution(SimplexSolution.SolutionType.SingleOptimal, C * currentSolution, currentSolution);
            }
            else if (r[inIndex] == 0)
            {
                return new SimplexSolution(SimplexSolution.SolutionType.InfiniteOptimalSet, C * currentSolution, currentSolution);
            }

            var yIn = GetYiVector(basisLu, inIndex);
            Console.WriteLine(currentSolution.Count());
            Console.WriteLine(yIn.Count());
            var outIndex = GetMinimumPositiveQuotientIndex(positiveVector: currentSolution, yIn);

            if (outIndex == null)
            {
                return new SimplexSolution(SimplexSolution.SolutionType.Unbounded);
            }

            NewFeasibleBasicSolutionInPlace(ref currentBasis, ref currentBasisIndexes, inIndex, yIn, outIndex.Value);
        }
    }

    private void NewFeasibleBasicSolutionInPlace(ref Matrix<double> currentBasis, ref List<bool> basisVariables, int inIndex, Vector<double> yIn, int outIndex)
    {
        PivotCurrentSolutionInPlace(X0, yIn, pivotIndex: inIndex);

        basisVariables[outIndex] = false;
        basisVariables[inIndex] = true;

        currentBasis = GetBasis(basisVariables);
    }

    private Matrix<double> GetBasis(List<bool> basisVariables)
    {
        var rank = this.A.RowCount;

        var basisVectors = new double[
            rank * rank
        ];

        int k = 0;

        for (int col = 0; col < A.ColumnCount; col++)
        {
            if (basisVariables[col])
            {
                for (int row = 0; row < A.RowCount; row++)
                {
                    basisVectors[k * col + row] = A[row, col];
                }
            }
        }

        return new DenseMatrix(rank, rank, basisVectors);
    }

    private int GetIndexOfMinimum(Vector<double> v)
    {
        var q = 0;
        var qMin = v[q];
        for (int i = 1; i < v.Count; i++)
        {
            if (v[i] < qMin)
            {
                q = i;
                qMin = v[i];
            }
        }

        return q;
    }

    private int? GetMinimumPositiveQuotientIndex(Vector<double> positiveVector, Vector<double> y)
    {
        var q = null as int?;
        var qMin = null as double?;
        for (int i = 0; i < positiveVector.Count; i++)
        {
            if (y[i] > 0 && (qMin == null || positiveVector[i] / y[i] < qMin))
            {
                q = i;
                qMin = positiveVector[i] / y[i];
            }
        }

        return q;
    }

    private void PivotCurrentSolutionInPlace(Vector<double> currentSolution, Vector<double> yIn, int pivotIndex)
    {

        for (int i = 0; i < currentSolution.Count; i++)
        {
            if (i == pivotIndex)
            {
                currentSolution[i] /= yIn[pivotIndex];
                continue;
            }

            currentSolution[i] = currentSolution[i] - yIn[pivotIndex] * currentSolution[i] / yIn[pivotIndex];
        }

    }

    private Vector<double> GetLambdaVector(LU<double> basisLU, Vector<double> cB)
    {
        return basisLU.TransposeAndSolve(cB);
    }

    private Vector<double> GetNonBasicCosts(List<bool> basisVariables)
    {
        return C.Where((x, i) => !basisVariables[i]).ToVector();
    }

    private Vector<double> GetBasicCosts(List<bool> basisVariables)
    {
        return C.Where((x, i) => basisVariables[i]).ToVector();
    }

    private Vector<double> GetRVector(Vector<double> cJ, Vector<double> lambda)
    {
        var r = new double[cJ.Count];

        for (int i = 0; i < cJ.Count; i++)
        {
            r[i] = cJ[i] - lambda * A.Column(i);
        }

        return new DenseVector(r);
    }

    private Vector<double> GetYiVector(LU<double> basisLu, int i)
    {
        System.Console.WriteLine(i);
        System.Console.WriteLine(A.Column(i).Count());
        return basisLu.Solve(A.Column(i));
    }
}

public class SimplexSolution
{
    public enum SolutionType
    {
        SingleOptimal,
        FiniteOptimalSet,
        InfiniteOptimalSet,
        Unbounded,
    }

    public SolutionType Type { get; }

    public Vector<double>? Solution { get; }
    public double? ObjectiveFunction { get; }

    public SimplexSolution(SolutionType type, double? objectiveFunction = null, Vector<double>? v1 = null)
    {
        this.Type = type;
        this.Solution = v1;
        this.ObjectiveFunction = objectiveFunction;
    }
}
