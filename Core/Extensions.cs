using System.Diagnostics.Contracts;

namespace Core;

public static class Extensions
{

	private const double TOLERANCE = 1e-9;

	[Pure] public static bool ApproximatelyEquals(this double a, double b) => Math.Abs(a - b) < TOLERANCE;
	[Pure] public static bool EqualsSafe(this object? a, object? b)
	{
		if (a == null && b == null) return true;
		return a?.Equals(b) ?? false;
	}
	
}