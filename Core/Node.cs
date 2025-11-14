namespace Core;

public abstract class Node(Bounds bounds) : IBounds
{

	public Bounds Bounds { get; } = bounds;

	public override string ToString() => $"({GetType().Name + (Represent() == "" ? "" : $": {Represent()}")})";
	public virtual string Represent() => string.Empty;

}

#region Nodes

/// <summary>
/// Makes a value from a literal as a token
/// </summary>
/// <param name="token"></param>
public class LiteralNode(Token token) : Node(token.Bounds) {

	public Token Token { get; } = token;

	public override string Represent() => Token.Value == null ? "" : Token.Value.ToString()!;

}

public class NullNode(Bounds bounds) : Node(bounds);

public class ErrorNode(string message, Bounds bounds) : Node(bounds) {

	public string Message { get; } = message;

	public override string Represent() => Message;

}


public class UnaryOperationNode(Token operationToken, Node baseNode) 
	: Node(new(operationToken.Bounds.Start, baseNode.Bounds.End)) {

	public Token OperationToken { get; } = operationToken;
	public Node BaseNode { get; } = baseNode;

	public override string Represent() => OperationToken.Value?.ToString() + BaseNode;

}

public class BinaryOperationNode(Token operationToken, Node leftNode, Node rightNode)
	: Node(new(leftNode.Bounds.Start, rightNode.Bounds.End)) {

	public Token OperationToken { get; } = operationToken;
	public Node LeftNode { get; } = leftNode;
	public Node RightNode { get; } = rightNode;
	
	public override string Represent() => LeftNode + OperationToken.Value?.ToString() + RightNode;

}


public class VariableNode(Node variableName) : Node(variableName.Bounds)
{

	public Node VariableName { get; } = variableName;
	public override string Represent() => VariableName.Represent();

}

public class VariableAccessNode(VariableNode variableNode) : Node(variableNode.Bounds)
{

	public VariableNode VariableNode { get; } = variableNode;
	public override string Represent() => VariableNode.Represent();

}

public class VariableAssignmentNode(VariableNode variableNode, Node valueNode)
	: Node(new(variableNode.Bounds.Start, valueNode.Bounds.End))
{

	public VariableNode VariableNode { get; } = variableNode;
	public Node ValueNode { get; } = valueNode;
	
	public override string Represent() => $"{VariableNode.Represent()} = {ValueNode.Represent()}";

}

public class ExecuteNode(Node baseNode)
{

	public Node BaseNode { get; } = baseNode;

}

#endregion