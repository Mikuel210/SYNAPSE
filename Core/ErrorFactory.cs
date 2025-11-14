namespace Core;

public static class ErrorFactory
{

	private static string Represent(object @object)
	{
		if (@object is Error error) return $"Error ({error.Value})";
		if (@object is IValue value) return value.GetType().Name;
		
		return @object.ToString()!;
	}

	public static ErrorNode ExpectedNode(IBounds bounds, List<object> expected, object? got = null)
	{
		string message;

		if (expected.Count > 1) {
			var expectedWithoutLast = new List<object>(expected);
			expectedWithoutLast.RemoveAt(expected.Count - 1);
		
			message = string.Join(", ", expectedWithoutLast.Select(Represent).ToArray()) 
							 + ", or " + expected.LastOrDefault();	
		}
		else message = expected[0].ToString()!;

		message = $"Expected {message}" + (got == null ? "" : $", got {got}"); 
		return new(message, bounds.Bounds);
	}

	public static ErrorNode ExpectedNode(IBounds bounds, object expected, object? got = null) 
		=> ExpectedNode(bounds, [expected], got);

	public static Error UnsupportedOperator(string operatorString, IValue left, IValue? right = null)
	{
		var message = "Unsupported operator: ";
		if (right == null) message += $"{operatorString} ";
		
		message += left.GetType().Name;
		if (right != null) message += $" {operatorString} {Represent(right)}";

		return new(message, new(left.Bounds.Start, right == null ? left.Bounds.End : right.Bounds.End), left.Context);
	}

	public static Error InvalidOperation(string operation, Bounds bounds, Context context)
		=> new($"Invalid operation: {operation}", bounds, context);
	
}