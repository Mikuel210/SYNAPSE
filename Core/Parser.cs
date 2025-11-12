using System.Diagnostics.CodeAnalysis;

namespace Core;

public class Parser(Lexer lexer) {

	public Lexer Lexer { get; } = lexer;
	public Token CurrentToken => Lexer.TokenQueue.ElementAtOrDefault(0);

	public Node ParseStatement() {
		if (Lexer.TokenQueue.Count > 0) {
			Node node = Expression();

			if (CurrentToken.Type == Token.EType.NewLine) {
				while (CurrentToken.Type == Token.EType.NewLine)
					Advance();	
			}
			else if (Lexer.TokenQueue.Count > 0) {
				node = new ErrorNode("Newline expected", CurrentToken.StartPosition, CurrentToken.EndPosition);
			}
			
			return node;
		}

		return new NullNode(
			new(Lexer.Scope),
			new(Lexer.Scope, Lexer.Scope.Text.Length)
		);
	}

	private void Advance() => Lexer.TokenQueue.TryDequeue(out _);
	
	#region Syntax

	private bool Attempt<T>(Func<Node> syntax, [NotNullWhen(true)] out T? output) where T : Node
	{
		output = null;
		
		if (CurrentToken.Type == Token.EType.None)
			return false;
		
		var startPosition = CurrentToken.StartPosition.Clone();
		var node = syntax();

		if (node is T) {
			output = (T)node;
			return true;
		}
		
		// If failed, go back
		Lexer.GoTo(startPosition);
		return false;
	}

	private Node Expression()
	{
		if (Attempt<VariableAssignmentNode>(VariableAssignmentExpression, out var variableAssignment))
			return variableAssignment;

		return ArithmeticExpression();
	}

	private Node ArithmeticOperation(Token.EType[] operationTypes, Func<Node> syntax) {
		Node currentNode = syntax();

		while (operationTypes.Contains(CurrentToken.Type)) {
			Token operationToken = CurrentToken;
			Advance();

			Node right = syntax();
			currentNode = new BinaryOperationNode(operationToken, currentNode, right);
		}

		return currentNode;
	}
	
	private Node ArithmeticExpression() {
		return ArithmeticOperation(
			[Token.EType.Add, Token.EType.Subtract],
			Term
		);
	}

	private Node VariableAssignmentExpression()
	{
		var variable = Variable();

		if (variable is not VariableNode variableNode)
			return new ErrorNode($"Expected variable, got {variable}", variable.StartPosition, variable.EndPosition);

		if (CurrentToken.Type != Token.EType.Equals)
			return new ErrorNode($"Expected equals, got {CurrentToken.Type}", CurrentToken.StartPosition, CurrentToken.EndPosition);

		Advance();

		var valueNode = Expression();
		return new VariableAssignmentNode(variableNode, valueNode);
	}

	private Node Variable()
	{
		Token token = CurrentToken;
		
		if (token.Type == Token.EType.Variable) {
			Advance();
			token = CurrentToken;

			if (CurrentToken.Type == Token.EType.Identifier) goto Identifier;
			return new VariableNode(BaseAtom());
		}

		if (token.Type == Token.EType.Identifier) goto Identifier;
		return new ErrorNode($"Expected variable, got {token.Type}", token.StartPosition, token.EndPosition);
		
		Identifier:
			Advance();
			return new VariableNode(new LiteralNode(token));
	}
	
	private Node Term() {
		return ArithmeticOperation(
			[Token.EType.Multiply, Token.EType.Divide, Token.EType.Modulo],
			Factor
		);	
	}
	
	private Node Factor() {
		Token token = CurrentToken;
		
		if (token.Type is Token.EType.Add or Token.EType.Subtract) {
			Advance();
			Node factor = Factor();
			
			return new UnaryOperationNode(token, factor);
		}

		return Power();
	}

	private Node Power()
	{
		var atom = Atom();
		Token token = CurrentToken;

		if (token.Type == Token.EType.Power) {
			Advance();
			return new BinaryOperationNode(token, atom, Factor());
		}

		return atom;
	}

	private Node Atom() => BaseAtom();

	private Node BaseAtom() {
		Token token = CurrentToken;

		if (Attempt<VariableNode>(Variable, out var variable))
			return new VariableAccessNode(variable);

		Advance();
		
		if (token.Type == Token.EType.Number)
			return new LiteralNode(token);
		
		if (token.Type == Token.EType.OpenParenthesis) {
			if (CurrentToken.Type == Token.EType.CloseParenthesis) {
				Advance();
				return new ErrorNode("Expected expression", CurrentToken.StartPosition, CurrentToken.EndPosition); // TODO: ErrorMessage.Expected("expression", CurrentToken) -> Expected expression, got invalid
			}
			
			var expression = Expression();

			if (CurrentToken.Type != Token.EType.CloseParenthesis)
				return new ErrorNode("Expected ')'", CurrentToken.StartPosition, CurrentToken.EndPosition);

			Advance();
			return expression;
		}
		
		return new ErrorNode($"Expected number, got {token.Type}", token.StartPosition, token.EndPosition);
	}

	#endregion

}