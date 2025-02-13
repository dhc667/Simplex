using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


public interface IDoubleMatrix
{
    double this[int row, int col] { get; set; }
    int RowCount { get; }
    int ColumnCount { get; }
    Vector<double> BackSubstitutionLower(Vector<double> b);
    Vector<double> BackSubstitutionUpper(Vector<double> b);

}

public class TransposedMatrixWrapper : IDoubleMatrix
{
    private Matrix<double> matrix;

    public TransposedMatrixWrapper(Matrix<double> matrix)
    {
        this.matrix = matrix;
    }

    public double this[int row, int col]
    {
        get => this.matrix[col, row];
        set => this.matrix[col, row] = value;
    }

    public int RowCount => this.matrix.ColumnCount;
    public int ColumnCount => this.matrix.RowCount;

    public Vector<double> BackSubstitutionLower(Vector<double> b)
    {
        var n = this.RowCount;
        var x = new double[n];

        for (int i = 0; i < n; i++)
        {
            x[i] = b[i];
            for (int j = 0; j < i; j++)
            {
                x[i] -= this[i, j] * x[j];
            }
            x[i] /= this[i, i];
        }

        return new DenseVector(x);
    }

    public Vector<double> BackSubstitutionUpper(Vector<double> b)
    {
        var n = this.RowCount;
        var x = new double[n];

        for (int i = n - 1; i >= 0; i--)
        {
            x[i] = b[i];
            for (int j = i + 1; j < n; j++)
            {
                x[i] -= this[i, j] * x[j];
            }
            x[i] /= this[i, i];
        }

        return new DenseVector(x);
    }
}
