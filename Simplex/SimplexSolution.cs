using MathNet.Numerics.LinearAlgebra;

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
    public Matrix<double>? Basis { get; }
    public List<int>? BasisIndexes { get; }

    public SimplexSolution(SolutionType type, double? objectiveFunction = null, Vector<double>? v1 = null, Matrix<double>? basis = null, List<int>? basisIndexes = null)
    {
        this.Type = type;
        this.Solution = v1;
        this.ObjectiveFunction = objectiveFunction;
        this.BasisIndexes = basisIndexes;
        this.Basis = basis;
    }
}
