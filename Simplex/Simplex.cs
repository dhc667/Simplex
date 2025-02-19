using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;

class SimplexMethod
{
    private Vector<double> C { get; }
    private Matrix<double> A { get; }
    private Vector<double> B { get; }
    private Vector<double> X0 { get; }
    private List<int> InitialBasisIndexes { get; }

    public SimplexSolution Solution { get; }

    public SimplexMethod(POL problem)
    {
        /* System.Console.WriteLine("SIMPLEX: \n"); */

        /* System.Console.WriteLine($"A: {problem.ConstraintsMatrix.ToString()}"); */
        /* System.Console.WriteLine($"B: {problem.ConstraintsVector.ToString()}"); */
        /* System.Console.WriteLine($"X0: {problem.InitialSolution.ToString()}"); */
        /* System.Console.WriteLine($"C: {problem.ObjectiveFunction.ToString()}"); */
        /* System.Console.WriteLine($"InitialBasis: {string.Join(", ", problem.BasisIndexes)}"); */

        this.A = problem.ConstraintsMatrix;
        this.B = problem.ConstraintsVector;
        this.X0 = problem.InitialSolution;
        this.C = problem.ObjectiveFunction;
        this.InitialBasisIndexes = problem.BasisIndexes;
        this.Solution = this.Solve();
    }

    private SimplexSolution Solve()
    {
        var currentBasis = GetBasis(InitialBasisIndexes);
        var currentBasisIndexes = new List<int>(InitialBasisIndexes);
        var y0 = currentBasisIndexes.Select(i => X0[i]).ToVector();
        var basisLu = currentBasis.LU();
        
        var r = new DenseVector(X0.Count);

        while (true)
        {
            var cB = GetBasicCosts(currentBasisIndexes);
            var cb_bInverse = CB_BInverse(basisLu, cB); 

            var currentBasisIndexesSet = currentBasisIndexes.ToHashSet();

            UpdateRVector(currentBasisIndexesSet, cb_bInverse, r);
            var inIndex = GetNonBasicIndexOfMinimum(r, currentBasisIndexesSet);
            
            /* System.Console.WriteLine($"cb = [{string.Join(',', cB.ToList())}]"); */
            /* System.Console.WriteLine($"cb_bInverse = [{string.Join(',', cb_bInverse.ToList())}]"); */
            /* System.Console.WriteLine($"Current basis Indexes = [{string.Join(',', currentBasisIndexes)}]"); */
            /* System.Console.WriteLine($"rj = [{string.Join(",", r.ToList())}]"); */
            /* System.Console.WriteLine("Current Basis:"); */
            /* System.Console.WriteLine(currentBasis); */
            /* System.Console.WriteLine($"Current Solution: {C * BuildSolution(y0, currentBasisIndexes)}"); */

            if (inIndex == -1 || Comparers.almostGreaterThan(r[inIndex], 0)  /* r[inIndex] > 0 */)
            {
                var sol = BuildSolution(y0, currentBasisIndexes);
                return new SimplexSolution(SimplexSolution.SolutionType.SingleOptimal, C * sol, sol, currentBasis, currentBasisIndexes);
            }
            else if (Comparers.almostEqual(r[inIndex], 0)  /* r[inIndex] == 0 */)
            {
                var sol = BuildSolution(y0, currentBasisIndexes);
                /* System.Console.WriteLine("Infinite Optimal set, solution: {0}", C*sol); */
                return new SimplexSolution(SimplexSolution.SolutionType.InfiniteOptimalSet, C * sol, sol, currentBasis, currentBasisIndexes);
            }

            var yIn = GetYiVector(basisLu, inIndex);
            /* var y0 = GetY0Vector(basisLu); */

            var basisOutIndex = GetMinimumPositiveQuotientIndex(positiveVector: y0, yIn);

            if (basisOutIndex == null)
            {
                return new SimplexSolution(SimplexSolution.SolutionType.Unbounded);
            }

            /* System.Console.WriteLine($"Index in: {inIndex}, Index out: {basisOutIndex}\n"); */

            NewBasis(currentBasis, ref currentBasisIndexes, inIndex, basisOutIndex.Value);
            basisLu = currentBasis.LU();

            y0 = basisLu.Solve(this.B);

            /* i++; */
        }

        throw new Exception();
    }

    private void NewBasis(Matrix<double> currentBasis, ref List<int> basisIndexes, int inIndex, int basisOutIndex)
    {
        for(int row = 0; row < currentBasis.RowCount; row++)
        {
            currentBasis[row, basisOutIndex] = this.A[row, inIndex];
        }

        basisIndexes[basisOutIndex] = inIndex;
    }

    private Matrix<double> GetBasis(List<int> basisIndexes)
    {
        var rank = this.A.RowCount;

        var basisVectors = new double[
            rank * rank
        ];

        int k = 0;

        for (int i = 0; i < basisIndexes.Count; i++)
        {
            for (int row = 0; row < A.RowCount; row++)
            {
                basisVectors[k++] = A[row, basisIndexes[i]];
            }
        }

        return new DenseMatrix(rank, rank, basisVectors);
    }


    private int GetNonBasicIndexOfMinimum(Vector<double> v, HashSet<int> basisIndexes)
    {
        var q = -1;
        var qMin = double.MaxValue;
        for (int i = 0; i < v.Count; i++)
        {
            if (basisIndexes.Contains(i)) continue;
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

            currentSolution[i] = currentSolution[i] - yIn[i] * currentSolution[pivotIndex] / yIn[pivotIndex];
        }

    }

    private Vector<double> CB_BInverse(LU<double> basisLU, Vector<double> cB)
    {
        return basisLU.TransposeAndSolve(cB);
    }


    private Vector<double> GetBasicCosts(List<int> basisIndexes)
    {
        return basisIndexes.Select(i => this.C[i]).ToVector();
    }

    private void UpdateRVector(HashSet<int> basisIndexes, Vector<double> cb_bInverse, Vector<double> r)
    {
        for (int i = 0; i < r.Count; i++)
        {
            if (!basisIndexes.Contains(i))
            {
                r[i] = this.C[i] - cb_bInverse * A.Column(i); // TODO: this copies a column, optimize
            }
            else
            {
                r[i] = 0;
            }
        }
    }

    private Vector<double> GetYiVector(LU<double> basisLu, int i)
    {
        return basisLu.Solve(this.A.Column(i)); // TODO: this copies...
    }

    private Vector<double> GetY0Vector(LU<double> basisLu)
    {
        return basisLu.Solve(this.B);
    }

    private Vector<double> BuildSolution(Vector<double> y0, List<int> basisIndexes)
    {
        var nonZeroValues = 
            basisIndexes
            .Select((basisIndex, y0Index) => (basisIndex, y0Index))
            .ToDictionary(
                tup => tup.basisIndex,
                tup => y0[tup.y0Index]
            );

        return this.X0.Select((_, i) => nonZeroValues.ContainsKey(i) ? nonZeroValues[i] : 0).ToVector();
    }
}


