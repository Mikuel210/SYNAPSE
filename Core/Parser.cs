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

	private Node Expression() => ArithmeticExpression();

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

	private Node Variable()
	{
		if (CurrentToken.Type == Token.EType.Variable) {
			Advance();
			
			
		}
	}
	
	private Node Term() {
		return ArithmeticOperation(
			[Token.EType.Multiply, Token.EType.Divide],
			Factor
		);	
	}
	
	private Node Factor() {
		Token token = CurrentToken;
		
		if (token.Type is Token.EType.Add or Token.EType.Subtract) {
			Advance();
			var factor = Factor();
			
			return new UnaryOperationNode(token, factor);
		}

		return Atom();
	}

	private Node Atom() => BaseAtom();

	private Node BaseAtom() {
		Token token = CurrentToken;
		Advance();
		
		if (token.Type == Token.EType.Number)
			return new LiteralNode(token);
		
		if (token.Type == Token.EType.OpenParenthesis) {
			if (CurrentToken.Type == Token.EType.CloseParenthesis) {
				Advance();
				return new ErrorNode("Expected expression", CurrentToken.StartPosition, CurrentToken.EndPosition);	
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