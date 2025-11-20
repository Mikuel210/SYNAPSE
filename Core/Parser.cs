using System.Diagnostics.CodeAnalysis;

namespace Core;

public class Parser(Lexer lexer) {

	public Lexer Lexer { get; } = lexer;
	private Token CurrentToken => Lexer.TokenQueue.ElementAtOrDefault(0);

	public Node ParseStatement() {
		// BUG: Console.WriteLine(string.Join(", ", Lexer.TokenQueue));
		
		if (Lexer.TokenQueue.Count > 0) {
			Node node = Expression();

			if (CurrentToken.Type == Token.EType.NewLine) {
				while (CurrentToken.Type == Token.EType.NewLine)
					Advance();	
			}
			else if (Lexer.TokenQueue.Count > 0) {
				node = ErrorFactory.ExpectedNode(CurrentToken, Token.EType.NewLine);
			}
			
			return node;
		}

		return new NullNode(new(
			new(Lexer.Scope),
			new(Lexer.Scope, Lexer.Scope.Text.Length)
		));
	}

	private void Advance() => Lexer.TokenQueue.TryDequeue(out _);
	
	#region Syntax

	private bool Attempt(Func<Node> syntax, [NotNullWhen(true)] out Node? output, Func<Node, bool> condition)
	{
		output = null;
		
		if (CurrentToken.Type == Token.EType.None)
			return false;
		
		var startPosition = CurrentToken.Bounds.Start.Clone();
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
			return ErrorFactory.ExpectedNode(variable, nameof(Variable), variable.GetType().Name);

		if (CurrentToken.Type != Token.EType.Equals)
			return ErrorFactory.ExpectedNode(CurrentToken, Token.EType.Equals, CurrentToken.Type);

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
		return ErrorFactory.ExpectedNode(token, Token.EType.Variable, CurrentToken.Type);
		
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

	private Node Atom()
	{
		var currentNode = BaseAtom();

		while (Attempt(Postfix, out Node? postfix)) {
			currentNode = postfix switch {
				ArgumentsNode argumentsNode => new ExecuteNode(currentNode, argumentsNode), 
				_ => throw new NotImplementedException()
			};
		}

		return currentNode;
	}

	private Node BaseAtom() {
		Token token = CurrentToken;

		if (Attempt<VariableNode>(Variable, out var variable))
			return new VariableAccessNode(variable);

		if (Attempt<ListNode>(ListExpression, out var list)) return list;
		Advance();
		
		if (token.Type is Token.EType.Number or Token.EType.Text)
			return new LiteralNode(token);
		
		if (token.Type == Token.EType.OpenParenthesis) {
			if (CurrentToken.Type == Token.EType.CloseParenthesis) {
				Advance();

				return ErrorFactory.ExpectedNode(CurrentToken, nameof(Expression));
			}
			
			var expression = Expression();

			if (CurrentToken.Type != Token.EType.CloseParenthesis)
				return ErrorFactory.ExpectedNode(CurrentToken, Token.EType.CloseParenthesis);

			Advance();
			return expression;
		}
		
		return ErrorFactory.ExpectedNode(token, [Token.EType.Number, Token.EType.Text, Token.EType.OpenParenthesis], token.Type);
	}

	private Node ListExpression()
	{
		var startPosition = CurrentToken.Bounds.Start.Clone();
		
		if (CurrentToken.Type != Token.EType.OpenBrackets)
			return ErrorFactory.ExpectedNode(CurrentToken, Token.EType.OpenBrackets, CurrentToken.Type);
		
		Advance();

		if (!Attempt<ListNode>(List, out var list)) 
			return ErrorFactory.ExpectedNode(CurrentToken, nameof(List));
		
		if (CurrentToken.Type != Token.EType.CloseBrackets)
			return ErrorFactory.ExpectedNode(CurrentToken, Token.EType.CloseBrackets, CurrentToken.Type);
		
		Advance();
		
		list.Bounds = new(startPosition, CurrentToken.Bounds.End);
		return list;
	}

	private Node Postfix()
	{
		var startPosition = CurrentToken.Bounds.Start.Clone();
		
		if (CurrentToken.Type != Token.EType.OpenParenthesis)
			return ErrorFactory.ExpectedNode(CurrentToken, Token.EType.OpenParenthesis, CurrentToken.Type);	
		
		Advance();

		if (!Attempt<ListNode>(List, out var list)) 
			return ErrorFactory.ExpectedNode(CurrentToken, nameof(List));
		
		if (CurrentToken.Type != Token.EType.CloseParenthesis)
			return ErrorFactory.ExpectedNode(CurrentToken, Token.EType.CloseParenthesis, CurrentToken.Type);
		
		Advance();
		
		list.Bounds = new(startPosition, CurrentToken.Bounds.End);
		return new ArgumentsNode(list);
	}

	private Node List()
	{
		var startPosition = CurrentToken.Bounds.Start.Clone();
		List<Node> arguments = [];

		if (!Attempt(Expression, out var expression)) goto End;
		arguments.Add(expression);

		while (CurrentToken.Type == Token.EType.Comma) {
			Advance();
			
			if (!Attempt(Expression, out expression)) goto End;
			arguments.Add(expression);
		}

		End:
		return new ListNode(arguments, new(startPosition, CurrentToken.Bounds.End));
	}

	#endregion

}