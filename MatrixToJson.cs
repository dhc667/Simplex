using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;

public static class MatrixToJsonExtensor
{
    public static string ToJson(this Matrix<double> matrix)
    {
        return JsonSerializer.Serialize(matrix.ToRowArrays());
    }
}