public interface POL
{
    Vector<double> ObjectiveFunction { get; set; }  // Vector f
    Matrix<double> ConstraintsMatrix { get; set; }  // Matriz A
    Vector<double> ConstraintsVector { get; set; }  // Vector b
    Vector<double> InitialSolution { get; set; }
    Vector<bool> BaseVector { get; set; } 
}

public class LinearProgram : POL
{
    public Vector<double> ObjectiveFunction { get; set; }
    public Matrix<double> ConstraintsMatrix { get; set; }
    public Vector<double> ConstraintsVector { get; set; }
    public Vector<double> InitialSolution { get; set; }
    public Vector<bool> BaseVector { get; set; }
    
    public LinearProgram(Vector<double> objectiveFunction, Matrix<double> constraintsMatrix, Vector<double> constraintsVector, Vector<double> initialSolution,Vector<bool> baseVector)
    {
        ObjectiveFunction = objectiveFunction;
        ConstraintsMatrix = constraintsMatrix;
        ConstraintsVector = constraintsVector;
        InitialSolution = initialSolution;
        BaseVector = baseVector;
    }
}
