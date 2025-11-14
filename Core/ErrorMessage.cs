namespace Core;

public static class ErrorMessage
{

	public static string Expected(object[] expected, object? got = null)
	{
		string enumeration = string.Join(", ", expected, 0, expected.Length - 1) + ", or " + expected.LastOrDefault();
		return $"Expected {enumeration}" + (got == null ? "" : $", got {got}") + ".";
	}

	public static string Expected(object expected, object? got = null) => Expected([expected], got);

}