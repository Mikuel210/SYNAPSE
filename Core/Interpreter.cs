using System.Reflection;

namespace Core;

public static class Interpreter {

	public static List<IValue> Interpret(Parser parser, Context context) {
		List<IValue> output = new();
		
		while (parser.Lexer.TokenQueue.Count > 0) {
			var node = parser.ParseStatement();
			Console.WriteLine(node);
			output.Add(Visit(node, context));
		}

		return output;
	}

	private static IValue Visit(Node node, Context context) {
		string nodeName = node.GetType().Name;
		string methodName = $"Visit{nodeName}";
		
		var method = typeof(Interpreter).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
		if (method == null) throw new InvalidOperationException($"Visit method missing for {nodeName}");
		
		var value = method.Invoke(null, [node, context])!;
		return (IValue)value;
	}
	
	#region Visitors

	private static IValue VisitLiteralNode(LiteralNode node, Context context) {
		Token token = node.Token;
		
		switch (token.Type) {
			case Token.EType.Number: return Number.FromToken(token, context);
			case Token.EType.Identifier: return Text.FromToken(token, context);
			default: throw new InvalidOperationException($"Attempted to make a value from {token.Type} literal"); 
		}
	}

	private static IValue VisitNullNode(NullNode node, Context context) => new Null(node.StartPosition, node.EndPosition, context);

	private static IValue VisitErrorNode(ErrorNode node, Context context) =>
		new Error(node.Message, node.StartPosition, node.EndPosition, context);

	private static IValue VisitUnaryOperationNode(UnaryOperationNode node, Context context) {
		var operationCharacter = (char)node.OperationToken.Value!;
		IValue baseValue = Visit(node.BaseNode, context);

		if (operationCharacter == '+') return baseValue;

		switch (baseValue) {
			case Number number: return new Number(-number.Value, node.StartPosition, node.EndPosition, context);
			default: throw new NotImplementedException($"Unary operation is not implemented for {baseValue.GetType().Name}");
		}
	}
	
	private static IValue VisitBinaryOperationNode(BinaryOperationNode node, Context context) {
		// TODO: Tokens should inherit from a base class so this doesnt happen:
		var operationCharacter = (char)node.OperationToken.Value!;
		IValue leftValue = Visit(node.LeftNode, context);
		IValue rightValue = Visit(node.RightNode, context);

		return operationCharacter switch {
			'+' => leftValue.AddedTo(rightValue),
			'-' => leftValue.SubtractedBy(rightValue),
			'*' => leftValue.MultipliedBy(rightValue),
			'/' => leftValue.DividedBy(rightValue),
			_ => leftValue.ReducedTo(rightValue)
		};
	}

	public record VariableData(IValue variableName)
	{

		public static VariableData FromVariableNode(VariableNode node, Context context)
			=> new(Visit(node.VariableName, context));

	}

	private static IValue VisitVariableAccessNode(VariableAccessNode node, Context context)
	{
		var variable = VariableData.FromVariableNode(node.VariableNode, context);
		return context.Scope.VariableTable.Get(variable.variableName, node.StartPosition, node.EndPosition, context);
	}

	#endregion

}

