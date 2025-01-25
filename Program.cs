using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

Matrix<double> matrix1 = DenseMatrix.OfArray(new double[,] {
    { 1, 2, 3 },
    { 4, 5, 6 },
    { 7, 8, 9 }
});

Console.WriteLine(matrix1.ToJson());