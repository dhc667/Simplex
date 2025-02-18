using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra.Double;

public static class LUTransposeExtensor
{
    public static Vector<double> TransposeAndSolve(this LU<double> LU, Vector<double> b)
    {
        var Ut = new TransposedMatrixWrapper(LU.U);
        var Lt = new TransposedMatrixWrapper(LU.L);

        var t = Ut.BackSubstitutionLower(b);
        var x = Lt.BackSubstitutionUpper(t);
        
        var answ = new DenseVector(x.Count);
        for (int i = 0; i < answ.Count; i++)
            answ[i] = x[LU.P[i]];

        return answ;
    }
}
