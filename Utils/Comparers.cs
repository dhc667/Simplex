public static class Comparers
{
    const double Epsilon = 1E-8;

    public static bool almostEqual(double a, double b)
    {
        return Math.Abs(a - b) < Epsilon;
    }

    public static bool almostGreaterThan(double a, double b)
    {
        return a > b && !almostEqual(a, b);
    }
}
