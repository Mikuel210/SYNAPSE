namespace Core;

public struct Token(Token.EType type, object? value, Position start, Position end) {

	public enum EType {
		
		None, // Default value when no tokens are left
		Invalid,
		NewLine,
		
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
		Identifier

	}

	public EType Type { get; set; } = type;
	public object? Value { get; set; } = value;
	public Position StartPosition { get; } = start;
	public Position EndPosition { get; } = end;

	public T? GetValueAs<T>() where T : class => Value as T;
	public void SetValue<T>(T value) where T : class => Value = value;

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
		char character = CurrentCharacter;

		while ("\t ".Contains(character)) {
			Advance();
			character = CurrentCharacter;
		}

		if (character == '\0') return null;

		// Make token
		if (";\n".Contains(character)) return MakeToken(Token.EType.NewLine);

		if (character == '(') return MakeToken(Token.EType.OpenParenthesis, character);
		if (character == ')') return MakeToken(Token.EType.CloseParenthesis, character);
		
		if (char.IsNumber(character)) return MakeNumber();
		if (character == '+') return MakeToken(Token.EType.Add, character);
		if (character == '-') return MakeToken(Token.EType.Subtract, character);
		if (character == '*') return MakeToken(Token.EType.Multiply, character);
		if (character == '/') return MakeToken(Token.EType.Divide, character);
		if (character == '^') return MakeToken(Token.EType.Power, character);
		if (character == '%') return MakeToken(Token.EType.Modulo, character);

		if (character == '$') return MakeToken(Token.EType.Variable, character);
		if (char.IsLetter(character) || character == '_') return MakeIdentifier();

		return MakeToken(Token.EType.Invalid, CurrentCharacter);
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
	/// Fills the token queue with all tokens from the source 
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
	/// <param name="steps"></param>
	/// <returns></returns>
	private Token MakeToken(Token.EType type, object? value = null, int steps = 1) {
		var start = _currentPosition;
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
		var startPosition = _currentPosition;

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
		var startPosition = _currentPosition;

		while (char.IsLetter(CurrentCharacter) || CurrentCharacter == '_') {
			identifierString += CurrentCharacter;
			Advance();
		}

		return new(Token.EType.Identifier, identifierString, startPosition, _currentPosition);
	}

	#endregion
}