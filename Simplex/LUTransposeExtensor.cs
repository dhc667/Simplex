using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;


public static class LUTransposeExtensor
{
    public static Vector<double> TransposeAndSolve(this LU<double> LU, Vector<double> b)
    {
        var Ut = new TransposedMatrixWrapper(LU.U);
        var Lt = new TransposedMatrixWrapper(LU.L);

        var t = Ut.BackSubstitutionLower(b);
        var x = Lt.BackSubstitutionUpper(t);

        return x;
    }
}