using System.Reflection;

namespace Core;

public static class Interpreter {

	public static List<IValue> Interpret(Parser parser, Context context) {
		List<IValue> output = new();
		
		while (parser.Lexer.TokenQueue.Count > 0) {
			var node = parser.ParseStatement();
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
			case Token.EType.Number: 
				return Number.FromToken(token, context);
			
			case Token.EType.Text:
			case Token.EType.Identifier: 
			case Token.EType.Keyword:
				return Text.FromToken(token, context);
			
			default: throw new InvalidOperationException($"Attempted to make a value from {token.Type} literal"); 
		}
	}

	private static IValue VisitNullNode(NullNode node, Context context) => new Null(node.Bounds, context);

	private static IValue VisitErrorNode(ErrorNode node, Context context) =>
		new Error(node.Message, node.Bounds, context);

	private static IValue VisitUnaryOperationNode(UnaryOperationNode node, Context context) {
		var operation = node.OperationToken.Value!.ToString();
		IValue baseValue = Visit(node.BaseNode, context);

		switch (operation) {
			case "+":
				return baseValue;
			
			case "-":
				return baseValue switch {
					Number number => new Number(-number.Value, node.Bounds, context),
					_ => throw new NotImplementedException($"Unary operation is not implemented for {baseValue.GetType().Name}")
				};
			
			case "not":
				return baseValue.Not();
				
			default:
				throw new NotImplementedException("Operation not implemented");
		}
	}
	
	private static IValue VisitBinaryOperationNode(BinaryOperationNode node, Context context) {
		// TODO: Tokens should inherit from a base class so this doesnt happen:
		var operation = node.OperationToken.Value!.ToString();
		IValue leftValue = Visit(node.LeftNode, context);
		IValue rightValue = Visit(node.RightNode, context);

		return operation switch {
			"+"   => leftValue.AddedTo(rightValue),
			"-"   => leftValue.SubtractedBy(rightValue),
			"*"   => leftValue.MultipliedBy(rightValue),
			"/"   => leftValue.DividedBy(rightValue),
			"^"   => leftValue.PoweredBy(rightValue),
			"%"   => leftValue.ReducedTo(rightValue),
			"=="  => leftValue.IsEquals(rightValue),
			">"   => leftValue.IsGreaterThan(rightValue),
			">="  => leftValue.IsGreaterThanEquals(rightValue),
			"<"   => leftValue.IsLessThan(rightValue),
			"<="  => leftValue.IsLessThanEquals(rightValue),
			"and" => leftValue.And(rightValue),
			"or"  => leftValue.Or(rightValue),
			_     => throw new NotImplementedException($"Binary operation not implemented: {operation}")
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
		return context.Scope.VariableTable.Get(variable.variableName, node.Bounds, context);
	}

	private static IValue VisitVariableAssignmentNode(VariableAssignmentNode node, Context context)
	{
		var variable = VariableData.FromVariableNode(node.VariableNode, context);
		var value = Visit(node.ValueNode, context);
		context.Scope.VariableTable.Set(variable.variableName, value);
		
		return value;
	}

	#endregion

}

