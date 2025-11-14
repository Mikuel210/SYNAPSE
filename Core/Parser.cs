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
				node = new ErrorNode(ErrorMessage.Expected(Token.EType.NewLine), CurrentToken.StartPosition, CurrentToken.EndPosition);
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

	private bool Attempt(Func<Node> syntax, [NotNullWhen(true)] out Node? output, Func<Node, bool> condition)
	{
		output = null;
		
		if (CurrentToken.Type == Token.EType.None)
			return false;
		
		var startPosition = CurrentToken.StartPosition.Clone();
		var node = syntax();

		if (condition(node)) {
			output = node;
			return true;
		}
		
		// If failed, go back
		Lexer.GoTo(startPosition);
		return false;
	}

	private bool Attempt<T>(Func<Node> syntax, [NotNullWhen(true)] out T? output)
		where T : Node
	{
		var result = Attempt(syntax, out var node, node => node is T);
		
		output = node as T;
		return result;
	}

	private bool Attempt(Func<Node> syntax, [NotNullWhen(true)] out Node? output)
		=> Attempt(syntax, out output, node => node is not ErrorNode);
	

	private Node Expression()
	{
		if (Attempt<VariableAssignmentNode>(VariableAssignmentExpression, out var variableAssignment))
			return variableAssignment;

		return LogicalExpression();
	}
	
	private Node BinaryOperation(Func<Token, bool> condition, Func<Node> syntax) {
		Node currentNode = syntax();

		while (condition(CurrentToken)) {
			Token operationToken = CurrentToken;
			Advance();

			Node right = syntax();
			currentNode = new BinaryOperationNode(operationToken, currentNode, right);
		}

		return currentNode;
	}
	
	private Node BinaryOperation(Token.EType[] operationTypes, Func<Node> syntax)
		=> BinaryOperation(token => operationTypes.Contains(token.Type), syntax);

	private Node LogicalExpression()
	{
		return BinaryOperation(
			token => token.Type == Token.EType.Keyword && 
					 new[] { "and", "or" }.Contains(token.Value),
			
			ComparisonExpression
		);
	}

	private Node ComparisonExpression()
	{
		var token = CurrentToken;
		
		if (token.Matches(Token.EType.Keyword, "not")) {
			Advance();

			var comparison = ComparisonExpression();
			return new UnaryOperationNode(token, comparison);
		}
		
		return BinaryOperation(
			[
				Token.EType.DoubleEquals, 
				Token.EType.LessThan, 
				Token.EType.GreaterThan, 
				Token.EType.LessThanEquals,
				Token.EType.GreaterThanEquals
			],
			ArithmeticExpression
		);
	}
	
	private Node ArithmeticExpression() {
		return BinaryOperation(
			[Token.EType.Add, Token.EType.Subtract],
			Term
		);
	}

	private Node VariableAssignmentExpression()
	{
		var variable = Variable();

		if (variable is not VariableNode variableNode)
			return new ErrorNode(ErrorMessage.Expected(nameof(Variable), variable.GetType().Name), variable.StartPosition, variable.EndPosition);

		if (CurrentToken.Type != Token.EType.Equals)
			return new ErrorNode(ErrorMessage.Expected(Token.EType.Equals, CurrentToken.Type), CurrentToken.StartPosition, CurrentToken.EndPosition);

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

			if (CurrentToken.Type is Token.EType.Identifier or Token.EType.Keyword) goto Identifier;
			return new VariableNode(BaseAtom());
		}

		if (token.Type is Token.EType.Identifier) goto Identifier;
		return new ErrorNode(ErrorMessage.Expected(Token.EType.Variable, CurrentToken.Type), token.StartPosition, token.EndPosition);
		
		Identifier:
			Advance();
			return new VariableNode(new LiteralNode(token));
	}
	
	private Node Term() {
		return BinaryOperation(
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
		
		if (token.Type is Token.EType.Number or Token.EType.Text)
			return new LiteralNode(token);
		
		if (token.Type == Token.EType.OpenParenthesis) {
			if (CurrentToken.Type == Token.EType.CloseParenthesis) {
				Advance();

				return new ErrorNode(ErrorMessage.Expected(nameof(Expression)), CurrentToken.StartPosition, CurrentToken.EndPosition);
			}
			
			var expression = Expression();

			if (CurrentToken.Type != Token.EType.CloseParenthesis)
				return new ErrorNode(ErrorMessage.Expected(Token.EType.CloseParenthesis), CurrentToken.StartPosition, CurrentToken.EndPosition);

			Advance();
			return expression;
		}
		
		return new ErrorNode(ErrorMessage.Expected([Token.EType.Number, Token.EType.Text, Token.EType.OpenParenthesis]), token.StartPosition, token.EndPosition);
	}

	#endregion

}