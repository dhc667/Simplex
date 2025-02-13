using MathNet.Numerics.LinearAlgebra;


public static class VectorOperationExtensors
{
    public static Matrix<double> PreMultiplyAsRowTimesMatrix(this Vector<double> v, Matrix<double> m)
    {
        var result = new double[v.Count * m.RowCount];

        for (int i = 0; i < v.Count; i++)
        {
            for (int j = 0; j < m.RowCount; j++)
            {
                result[i * m.RowCount + j] = v[i] * m[j, i];
            }
        }

        return Matrix<double>.Build.Dense(m.RowCount, v.Count, result);
    }
}