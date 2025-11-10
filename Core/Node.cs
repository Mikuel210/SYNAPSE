namespace Core;

public abstract class Node(Position start, Position end) {

	public Position StartPosition { get; } = start;
	public Position EndPosition { get; } = end;

	public override string ToString() => $"({GetType().Name + (Represent() == "" ? "" : $": {Represent()}")})";
	public virtual string Represent() => string.Empty;

}

#region Nodes

/// <summary>
/// Makes a value from a literal as a token
/// </summary>
/// <param name="token"></param>
public class LiteralNode(Token token) : Node(token.StartPosition, token.EndPosition) {

	public Token Token { get; } = token;

	public override string Represent() => Token.Value == null ? "" : Token.Value.ToString()!;

}

public class NullNode(Position start, Position end) : Node(start, end);

public class ErrorNode(string message, Position start, Position end) : Node(start, end) {

	public string Message { get; } = message;

	public override string Represent() => Message;

}


public class UnaryOperationNode(Token operationToken, Node baseNode) : Node(operationToken.StartPosition, baseNode.EndPosition) {

	public Token OperationToken { get; } = operationToken;
	public Node BaseNode { get; } = baseNode;

	public override string Represent() => OperationToken.Value?.ToString() + BaseNode;

}

public class BinaryOperationNode(Token operationToken, Node leftNode, Node rightNode)
	: Node(leftNode.StartPosition, rightNode.EndPosition) {

	public Token OperationToken { get; } = operationToken;
	public Node LeftNode { get; } = leftNode;
	public Node RightNode { get; } = rightNode;
	
	public override string Represent() => LeftNode + OperationToken.Value?.ToString() + RightNode;

}


public class VariableNode(Node variableName) : Node(variableName.StartPosition, variableName.EndPosition)
{

	public Node VariableName { get; } = variableName;
	public override string Represent() => VariableName.Represent();

}

public class VariableAccessNode(VariableNode variableNode) : Node(variableNode.StartPosition, variableNode.EndPosition)
{

	public Node VariableNode { get; } = variableNode;
	public override string Represent() => VariableNode.Represent();

}

public class VariableAssignmentNode(VariableNode variableNode, Node valueNode)
	: Node(variableNode.StartPosition, valueNode.EndPosition)
{

	public Node VariableNode { get; } = variableNode;
	public Node ValueNode { get; } = valueNode;
	
	public override string Represent() => $"{VariableNode.Represent()} = {ValueNode.Represent()}";

}

#endregion