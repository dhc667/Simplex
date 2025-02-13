using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


public static class IEnumerableToColumnVectorExtensor
{
    public static Vector<double> ToVector(this IEnumerable<double> source)
    {
        return new DenseVector(source.ToArray());
    }
}