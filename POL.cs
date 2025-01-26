public interface POL
{
    Vector<double> ObjectiveFunction { get; set; }  // Vector f
    Matrix<double> ConstraintsMatrix { get; set; }  // Matriz A
    Vector<double> ConstraintsVector { get; set; }  // Vector b
    Vector<int> InitialSolution { get; set; }
}

public class LinearProgram : POL
{
    public Vector<double> ObjectiveFunction { get; set; }
    public Matrix<double> ConstraintsMatrix { get; set; }
    public Vector<double> ConstraintsVector { get; set; }
    public Vector<int> InitialSolution { get; set; }
    
    public LinearProgram(Vector<double> objectiveFunction, Matrix<double> constraintsMatrix, Vector<double> constraintsVector, Vector<int> initialSolution)
    {
        ObjectiveFunction = objectiveFunction;
        ConstraintsMatrix = constraintsMatrix;
        ConstraintsVector = constraintsVector;
        InitialSolution = initialSolution;
    }
}
