namespace Core;

public interface IValue : IBounds {

	public object? Value { get; set;  }
	public Context Context { get; set; }

	IValue Clone();
	
	#region Operators

	IValue AddedTo(IValue value) => ErrorFactory.UnsupportedOperator("+", this, value);
	IValue SubtractedBy(IValue value) => ErrorFactory.UnsupportedOperator("-", this, value);
	IValue MultipliedBy(IValue value) => ErrorFactory.UnsupportedOperator("*", this, value);
	IValue DividedBy(IValue value) => ErrorFactory.UnsupportedOperator("/", this, value);
	IValue PoweredBy(IValue value) => ErrorFactory.UnsupportedOperator("^", this, value);
	IValue ReducedTo(IValue value) => ErrorFactory.UnsupportedOperator("%", this, value);
	
	IValue IsEquals(IValue value) => ErrorFactory.UnsupportedOperator("==", this, value);
	IValue IsGreaterThan(IValue value) => ErrorFactory.UnsupportedOperator(">", this, value);
	IValue IsGreaterThanEquals(IValue value) => ErrorFactory.UnsupportedOperator(">=", this, value);
	IValue IsLessThan(IValue value) => ErrorFactory.UnsupportedOperator("<", this, value);
	IValue IsLessThanEquals(IValue value) => ErrorFactory.UnsupportedOperator("<=", this, value);

	IValue And(IValue value) => ErrorFactory.UnsupportedOperator("and", this, value);
	IValue Or(IValue value) => ErrorFactory.UnsupportedOperator("or", this, value);
	IValue Not() => ErrorFactory.UnsupportedOperator("not", this);

	#endregion

}

public abstract class GenericValue<TSelf, TValue>(TValue value, Bounds bounds, Context context) : IValue where TSelf : IValue{

	public TValue Value {
		get => (TValue)_value!;
		set => _value = value;
	}
	private object? _value = value;
	object? IValue.Value {
		get => _value;
		set => _value = value;
	}

	public Context Context { get; set; } = context;
	public Bounds Bounds { get; } = bounds;

	public TSelf Clone() => (TSelf)Activator.CreateInstance(GetType(), Value, Bounds, Context)!;
	IValue IValue.Clone() => Clone();
	
	#region Operators
	
	public virtual IValue AddedTo(IValue value) => ErrorFactory.UnsupportedOperator("+", this, value);
	public virtual IValue SubtractedBy(IValue value) => ErrorFactory.UnsupportedOperator("-", this, value);
	public virtual IValue MultipliedBy(IValue value) => ErrorFactory.UnsupportedOperator("*", this, value);
	public virtual IValue DividedBy(IValue value) => ErrorFactory.UnsupportedOperator("/", this, value);
	public virtual IValue PoweredBy(IValue value) => ErrorFactory.UnsupportedOperator("^", this, value);
	public virtual IValue ReducedTo(IValue value) => ErrorFactory.UnsupportedOperator("%", this, value);
	
	public virtual IValue IsEquals(IValue value) => ErrorFactory.UnsupportedOperator("==", this, value);
	public virtual IValue IsGreaterThan(IValue value) => ErrorFactory.UnsupportedOperator(">", this, value);
	public IValue IsGreaterThanEquals(IValue value)
	{
		// TODO: This should check if it's a number, return error if it's not
		bool isEquals = !IsEquals(value).Value?.Equals(0) ?? false;
		bool isGreaterThan = !IsGreaterThan(value).Value?.Equals(0) ?? false;

		return new Number(isEquals || isGreaterThan ? 1 : 0, Bounds, Context);
	}
	public IValue IsLessThan(IValue value)
	{
		bool isEquals = !IsEquals(value).Value?.Equals(0) ?? false;
		bool isGreaterThan = !IsGreaterThan(value).Value?.Equals(0) ?? false;

		return new Number(!isEquals && !isGreaterThan ? 1 : 0, Bounds, Context);
	}
	public IValue IsLessThanEquals(IValue value)
	{
		bool isEquals = !IsEquals(value).Value?.Equals(0) ?? false;
		bool isGreaterThan = !IsGreaterThan(value).Value?.Equals(0) ?? false;

		return new Number(isEquals || !isGreaterThan ? 1 : 0, Bounds, Context);
	}
	
	public virtual IValue And(IValue value) => ErrorFactory.UnsupportedOperator("and", this, value);
	public virtual IValue Or(IValue value) => ErrorFactory.UnsupportedOperator("or", this, value);
	public virtual IValue Not() => ErrorFactory.UnsupportedOperator("not", this);
	
	#endregion

}

#region Values

public class Number(double value, Bounds bounds, Context context) : GenericValue<Number, double>(value, bounds, context) {

	public static Number FromToken(Token token, Context context) => 
		new((double)token.Value!, token.Bounds, context);

	public override IValue AddedTo(IValue value) {
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value += number.Value;

				return clone;
		}
		
		return ErrorFactory.UnsupportedOperator("+", this, value);
	}
	public override IValue SubtractedBy(IValue value) {
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value -= number.Value;

				return clone;
		}
		
		return ErrorFactory.UnsupportedOperator("-", this, value);
	}
	public override IValue MultipliedBy(IValue value) {
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value *= number.Value;

				return clone;
		}
		
		return ErrorFactory.UnsupportedOperator("*", this, value);
	}
	public override IValue DividedBy(IValue value) {
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value /= number.Value;

				return clone;
		}
		
		return ErrorFactory.UnsupportedOperator("/", this, value);
	}
	public override IValue PoweredBy(IValue value) {
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value = Math.Pow(clone.Value, number.Value);

				return clone;
		}
		
		return ErrorFactory.UnsupportedOperator("^", this, value);
	}
	public override IValue ReducedTo(IValue value) {
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value %= number.Value;

				return clone;
		}
		
		return ErrorFactory.UnsupportedOperator("%", this, value);
	}

	public override IValue IsEquals(IValue value)
	{
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value = Value.ApproximatelyEquals(number.Value) ? 1 : 0;

				return clone;
		}
		
		return ErrorFactory.UnsupportedOperator("==", this, value);
	}
	public override IValue IsGreaterThan(IValue value)
	{
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value = Value > number.Value ? 1 : 0;

				return clone;
		}
		
		return ErrorFactory.UnsupportedOperator(">", this, value);
	}

	public override IValue And(IValue value)
	{
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value = Value != 0 && number.Value != 0 ? 1 : 0; // TODO: To bool

				return clone;
		}
		
		return ErrorFactory.UnsupportedOperator("and", this, value);
	}
	public override IValue Or(IValue value)
	{
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value = Value != 0 || number.Value != 0 ? 1 : 0;

				return clone;
		}
		
		return ErrorFactory.UnsupportedOperator("or", this, value);
	}
	public override IValue Not()
	{
		var clone = Clone();
		clone.Value = Value == 0 ? 1 : 0;

		return clone;
	}

}

public class Text(string value, Bounds bounds, Context context) : GenericValue<Text, string>(value, bounds, context) {

	public static Text FromToken(Token token, Context context) => 
		new((string)token.Value!, token.Bounds, context);
	
}

public class Null(Bounds bounds, Context context) : IValue {
	
	public object? Value {
		get => null;
		set { }
	}

	public Context Context { get; set; } = context;
	public Bounds Bounds { get; } = bounds;

	public Null Clone() => new(Bounds, Context);
	IValue IValue.Clone() => Clone();
	
}

public class Error(string message, Bounds bounds, Context context) : IValue {

	public string Value => message;
	private object? _value = message;
	object? IValue.Value {
		get => _value;
		set => _value = value;
	}

	// TODO: Errors should have messages and a traceback of other errors and sources
	
	public Context Context { get; set; } = context;
	public Bounds Bounds { get; } = bounds;

	public Error Clone() => new(Value, Bounds, Context);
	IValue IValue.Clone() => Clone();
	
}

#endregion