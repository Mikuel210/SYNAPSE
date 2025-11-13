namespace Core;

public struct Token(Token.EType type, object? value, Position start, Position end) {

	public enum EType {
		
		None, // Default value when no tokens are left
		Invalid,
		NewLine,
		Keyword,
		
		OpenParenthesis,
		CloseParenthesis,
		
		Number,
		Add,
		Subtract,
		Multiply,
		Divide,
		Power,
		Modulo,
		
		Variable,
		Identifier,
		Equals,
		
		DoubleEquals,
		LessThan,
		GreaterThan,
		LessThanEquals,
		GreaterThanEquals,
		
		Text

	}

	public EType Type { get; set; } = type;
	public object? Value { get; } = value;
	public Position StartPosition { get; } = start;
	public Position EndPosition { get; } = end;
	
	
	public bool Matches(EType type, object? value) => Type == type && Value.EqualsSafe(value);
	public override string ToString() => Type + (Value == null ? "" : $": {Value}");

}

public class Lexer {

	public Scope Scope { get; }
	
	/// <summary>
	/// Represents the tokens yet to be processed by the parser
	/// </summary>
	public Queue<Token> TokenQueue { get; private set; } = new();
	
	
	/// <summary>
	/// Represents the position after the last token to be parsed
	/// </summary>
	private Position _currentPosition;
	private char CurrentCharacter => Scope.Text.ElementAtOrDefault(_currentPosition.Index);

	private static string[] Keywords => ["not", "and", "or"];
	
	
	public Lexer(Scope scope) {
		Scope = scope;
		_currentPosition = new(Scope);
		
		Tokenize();
		Scope.OnTextChanged += Retokenize;
	}

	
	private void Advance(int steps = 1) => _currentPosition.Advance(steps);
	
	/// <summary>
	/// Makes a token and advances to the character after it
	/// </summary>
	/// <returns></returns>
	private Token? TokenizeNext() {
		while ("\t ".Contains(CurrentCharacter))
			Advance();

		char character = CurrentCharacter;
		if (CurrentCharacter == '\0') return null;

		// Make token
		if (";\n".Contains(character)) return MakeToken(Token.EType.NewLine);
		if (char.IsNumber(character)) return MakeNumber();
		if (char.IsLetter(character) || character == '_') return MakeIdentifier();

		if (character == '=') return MakeEquals();
		if (character == '>') return MakeGreaterThan();
		if (character == '<') return MakeLessThan();
		if ("\"{".Contains(character)) return MakeText();

		return character switch {
			'(' => MakeToken(Token.EType.OpenParenthesis, character),
			')' => MakeToken(Token.EType.CloseParenthesis, character),
			'+' => MakeToken(Token.EType.Add, character),
			'-' => MakeToken(Token.EType.Subtract, character),
			'*' => MakeToken(Token.EType.Multiply, character),
			'/' => MakeToken(Token.EType.Divide, character),
			'^' => MakeToken(Token.EType.Power, character),
			'%' => MakeToken(Token.EType.Modulo, character),
			'$' => MakeToken(Token.EType.Variable, character),
			_   => MakeToken(Token.EType.Invalid, CurrentCharacter)
		};

	}
	private bool TryTokenizeNext(out Token token) {
		var nullableToken = TokenizeNext();
		token = nullableToken ?? default;

		return nullableToken != null;
	}

	/// <summary>
	/// Fills the token queue from a starting position
	/// </summary>
	/// <param name="position"></param>
	private void TokenizeFrom(Position position) {
		_currentPosition = position.Clone();
		List<Token> tokens = new();
		
		while (true) {
			if (!TryTokenizeNext(out var token)) break;
			tokens.Add(token);
		}

		TokenQueue = new(tokens);
	}
	
	public void GoTo(Position position) => TokenizeFrom(position);
	
	/// <summary>
	/// Fills the token queue with all tokens from the scope 
	/// </summary>
	public void Tokenize() => TokenizeFrom(new(Scope));

	/// <summary>
	/// Recalculates tokens on the token queue
	/// </summary>
	/// <returns></returns>
	public void Retokenize() {
		if (TokenQueue.TryPeek(out var token))
			TokenizeFrom(token.StartPosition);
		else
			TokenizeFrom(_currentPosition); // Tokenize from the end
	}
	
	#region Makers

	/// <summary>
	/// Makes a token from a type and value and advances to the character after it
	/// </summary>
	/// <param name="type"></param>
	/// <param name="value"></param>
	/// <param name="steps"></param>
	/// <returns></returns>
	private Token MakeToken(Token.EType type, object? value = null, int steps = 1) {
		var start = _currentPosition.Clone();
		Advance(steps);
		
		var end = _currentPosition;
		Token token = new(type, value, start, end);

		return token;
	}
	
	/// <summary>
	/// Makes a number and advances to the character after it
	/// </summary>
	/// <returns></returns>
	private Token MakeNumber() {
		var numberString = "";
		var hasDot = false;
		var startPosition = _currentPosition.Clone();

		while (CurrentCharacter != '\0' && char.IsNumber(CurrentCharacter)) {
			char character = CurrentCharacter;

			if (character == '.') {
				if (hasDot) break;
				hasDot = true;
			}
			
			numberString += CurrentCharacter;
			Advance();
		}

		float number = float.Parse(numberString);
		return new(Token.EType.Number, number, startPosition, _currentPosition);
	}

	private Token MakeIdentifier()
	{
		var identifierString = "";
		var startPosition = _currentPosition.Clone();

		while (char.IsLetterOrDigit(CurrentCharacter) || CurrentCharacter == '_') {
			identifierString += CurrentCharacter;
			Advance();
		}
		
		var tokenType = Keywords.Contains(identifierString) ? Token.EType.Keyword : Token.EType.Identifier;
		return new(tokenType, identifierString, startPosition, _currentPosition);
	}

	private Token MakeOperator(Token.EType singleType, Token.EType equalsType)
	{
		var startPosition = _currentPosition.Clone();
		var tokenType = singleType;
		var tokenString = CurrentCharacter.ToString();
		
		Advance();

		if (CurrentCharacter == '=') {
			tokenString += CurrentCharacter;
			tokenType = equalsType;
			
			Advance();
		}
		
		return new(tokenType, tokenString, startPosition, _currentPosition);
	}
	private Token MakeEquals() => MakeOperator(Token.EType.Equals, Token.EType.DoubleEquals);
	private Token MakeGreaterThan() => MakeOperator(Token.EType.GreaterThan, Token.EType.GreaterThanEquals);
	private Token MakeLessThan() => MakeOperator(Token.EType.LessThan, Token.EType.LessThanEquals);

	private Token MakeText()
	{
		var startPosition = _currentPosition.Clone();
		var textString = "";
		var startCharacter = CurrentCharacter;
			
		Advance();
		
		if (startCharacter == '"') {
			while (CurrentCharacter != '"') {
				if (CurrentCharacter == '\0') goto Invalid;
				
				textString += CurrentCharacter;
				Advance();
			}
		}
		else {
			var levels = 1;

			while (true) {
				if (CurrentCharacter == '\0') goto Invalid;
				if (CurrentCharacter == '{') levels++;
				if (CurrentCharacter == '}') levels--;
				if (levels == 0) break;

				textString += CurrentCharacter;
				Advance();
			}
		}

		Advance();
		return new(Token.EType.Text, textString, startPosition, _currentPosition);
		
		Invalid:
		return new(Token.EType.Invalid, textString, startPosition, _currentPosition);
	}

	#endregion

}