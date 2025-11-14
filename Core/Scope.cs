namespace Core;

public class VariableTable(Scope scope)
{

	public Scope Scope { get; } = scope;
	private readonly Dictionary<IValue, IValue> _variables = [];
	
	public IValue Get(IValue key, Bounds bounds, Context context, bool isExplicit = false)
	{
		var value = _variables.FirstOrDefault(e => e.Key.Value?.Equals(key.Value) ?? false).Value;
		if (value != null) return value;
		
		if (isExplicit || Scope.ParentScope == null)
			return new Error($"Variable {key.Value} is not defined", bounds, context);
		
		return Scope.ParentScope.VariableTable.Get(key, bounds, context, isExplicit);
	}
	public void Set(IValue key, IValue value)
	{
		var foundKey = _variables
			.FirstOrDefault(e => e.Key.Value?.Equals(key.Value) ?? false).Key;
		
		if (foundKey is not null) key = foundKey;
		_variables[key] = value;
	}
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