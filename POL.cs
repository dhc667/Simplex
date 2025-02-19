using MathNet.Numerics.LinearAlgebra;

public interface POL
{
    Vector<double> ObjectiveFunction { get; set; }  // Vector f
    Matrix<double> ConstraintsMatrix { get; set; }  // Matriz A
    Vector<double> ConstraintsVector { get; set; }  // Vector b
    Vector<double> InitialSolution { get; set; }
    List<int> BasisIndexes { get; set; }
}

public class LinearProgram : POL
{
    public Vector<double> ObjectiveFunction { get; set; }
    public Matrix<double> ConstraintsMatrix { get; set; }
    public Vector<double> ConstraintsVector { get; set; }
    public Vector<double> InitialSolution { get; set; }
    public List<int> BasisIndexes { get; set; }

    
    public LinearProgram(Vector<double> objectiveFunction, Matrix<double> constraintsMatrix, Vector<double> constraintsVector, Vector<double> initialSolution, List<int> basisIndexes)
    {
        ObjectiveFunction = objectiveFunction;
        ConstraintsMatrix = constraintsMatrix;
        ConstraintsVector = constraintsVector;
        InitialSolution = initialSolution;
        BasisIndexes = basisIndexes;
    }
}
