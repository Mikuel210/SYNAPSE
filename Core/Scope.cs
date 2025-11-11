namespace Core;

public class VariableTable(Scope scope)
{

	public Scope Scope { get; } = scope;
	private readonly Dictionary<IValue, IValue> _variables = [];
	
	public IValue Get(IValue key, Position start, Position end, Context context, bool isExplicit = false)
	{
		var value = _variables.FirstOrDefault(e => e.Key.Value == key.Value).Value;
		if (value != null) return value;
		
		if (isExplicit || Scope.ParentScope == null) 
			return new Error($"Variable {key.Value} is not defined", start, end, context);
		
		return Scope.ParentScope.VariableTable.Get(key, start, end, context, isExplicit);
	}
	public void Set(IValue key, IValue value) => _variables[key] = value;
	public void Remove(IValue key)
	{
		var dictionaryKey = _variables.FirstOrDefault(e => e.Key.Value == key.Value).Key;
		if (dictionaryKey != null) _variables.Remove(dictionaryKey);
	}

}

public class Scope
{

	#region Properties
	
	public string Name { get; set; }

	private string _text;
	public string Text {
		get => _text;

		private set {
			_text = value;
			OnTextChanged?.Invoke();
		}
	}
	public Scope? ParentScope { get; }
	public VariableTable VariableTable { get; }
	
	#endregion
	
	public Scope(string name, string text, Scope? parentScope = null)
	{
		Name = name;
		_text = text;
		ParentScope = parentScope;
		VariableTable = new(this);
	}

	
	#region Text
	
	public delegate void TextPrependEvent(int characterCount);
	public event TextPrependEvent? OnTextPrepend;

	public event Action? OnTextChanged;

	public void PrependText(string text) {
		OnTextPrepend?.Invoke(text.Length);
		Text = text + Text;
	}
	public void AppendText(string text) => Text += text;

	#endregion
	
}