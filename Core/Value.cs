namespace Core;

public interface IValue {

	public object? Value { get; set;  }
	public Context Context { get; set; }
	public Position StartPosition { get; set; }
	public Position EndPosition { get; set; }

	IValue Clone();
	
	#region Operators
	
	IValue AddedTo(IValue value) => throw new NotImplementedException("Operation not implemented");
	IValue SubtractedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	IValue MultipliedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	IValue DividedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	IValue PoweredBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	IValue ReducedTo(IValue value) => throw new NotImplementedException("Operation not implemented");
	
	Number IsEquals(IValue value) => throw new NotImplementedException("Operation not implemented");
	Number IsGreaterThan(IValue value) => throw new NotImplementedException("Operation not implemented");
	Number IsGreaterThanEquals(IValue value) => throw new NotImplementedException("Operation not implemented");
	Number IsLessThan(IValue value) => throw new NotImplementedException("Operation not implemented");
	Number IsLessThanEquals(IValue value) => throw new NotImplementedException("Operation not implemented");

	IValue And(IValue value) => throw new NotImplementedException("Operation not implemented");
	IValue Or(IValue value) => throw new NotImplementedException("Operation not implemented");
	IValue Not() => throw new NotImplementedException("Operation not implemented");

	#endregion

}

public abstract class GenericValue<TSelf, TValue>(TValue value, Position start, Position end, Context context) : IValue where TSelf : IValue{

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
	public Position StartPosition { get; set; } = start;
	public Position EndPosition { get; set; } = end;

	public TSelf Clone() => (TSelf)Activator.CreateInstance(GetType(), Value, StartPosition, EndPosition, Context)!;
	IValue IValue.Clone() => Clone();
	
	#region Operators
	
	public virtual IValue AddedTo(IValue value) => throw new NotImplementedException("Operation not implemented");
	public virtual IValue SubtractedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public virtual IValue MultipliedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public virtual IValue DividedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public virtual IValue PoweredBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public virtual IValue ReducedTo(IValue value) => throw new NotImplementedException("Operation not implemented");

	public virtual Number IsEquals(IValue value) => throw new NotImplementedException("Operation not implemented");
	public virtual Number IsGreaterThan(IValue value) => throw new NotImplementedException("Operation not implemented");
	public Number IsGreaterThanEquals(IValue value)
	{
		bool isEquals = IsEquals(value).Value != 0;
		bool isGreaterThan = IsGreaterThan(value).Value != 0;

		return new(isEquals || isGreaterThan ? 1 : 0, StartPosition, EndPosition, Context);
	}
	public Number IsLessThan(IValue value)
	{
		var isEquals = IsEquals(value);
		var isGreaterThan = IsGreaterThan(value);

		return new(isEquals.Value == 0 && isGreaterThan.Value == 0 ? 1 : 0, StartPosition, EndPosition, Context);
	}
	public Number IsLessThanEquals(IValue value)
	{
		bool isEquals = IsEquals(value).Value != 0;
		bool isLessThan = IsLessThan(value).Value != 0;

		return new(isEquals || isLessThan ? 1 : 0, StartPosition, EndPosition, Context);
	}

	public virtual IValue And(IValue value) => throw new NotImplementedException("Operation not implemented");
	public virtual IValue Or(IValue value) => throw new NotImplementedException("Operation not implemented");
	public virtual IValue Not() => throw new NotImplementedException("Operation not implemented");
	
	#endregion

}

#region Values

public class Number(double value, Position start, Position end, Context context) : GenericValue<Number, double>(value, start, end, context) {

	public static Number FromToken(Token token, Context context) => 
		new((float)token.Value!, token.StartPosition, token.EndPosition, context);

	public override IValue AddedTo(IValue value) {
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value += number.Value;

				return clone;
		}
		
		throw new NotImplementedException("Operation not implemented");
	}
	public override IValue SubtractedBy(IValue value) {
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value -= number.Value;

				return clone;
		}
		
		throw new NotImplementedException("Operation not implemented");
	}
	public override IValue MultipliedBy(IValue value) {
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value *= number.Value;

				return clone;
		}
		
		throw new NotImplementedException("Operation not implemented");
	}
	public override IValue DividedBy(IValue value) {
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value /= number.Value;

				return clone;
		}
		
		throw new NotImplementedException("Operation not implemented");
	}
	public override IValue PoweredBy(IValue value) {
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value = Math.Pow(clone.Value, number.Value);

				return clone;
		}
		
		throw new NotImplementedException("Operation not implemented");
	}
	public override IValue ReducedTo(IValue value) {
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value %= number.Value;

				return clone;
		}
		
		throw new NotImplementedException("Operation not implemented");
	}

	public override Number IsEquals(IValue value)
	{
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value = Value.ApproximatelyEquals(number.Value) ? 1 : 0;

				return clone;
		}
		
		throw new NotImplementedException("Operation not implemented");
	}
	public override Number IsGreaterThan(IValue value)
	{
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value = Value > number.Value ? 1 : 0;

				return clone;
		}
		
		throw new NotImplementedException("Operation not implemented");
	}

	public override IValue And(IValue value)
	{
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value = Value != 0 && number.Value != 0 ? 1 : 0; // TODO: To bool

				return clone;
		}
		
		throw new NotImplementedException("Operation not implemented");
	}
	public override IValue Or(IValue value)
	{
		switch (value) {
			case Number number:
				var clone = Clone();
				clone.Value = Value != 0 || number.Value != 0 ? 1 : 0;

				return clone;
		}
		
		throw new NotImplementedException("Operation not implemented");
	}
	public override IValue Not()
	{
		var clone = Clone();
		clone.Value = Value == 0 ? 1 : 0;

		return clone;
	}

}

public class Text(string value, Position start, Position end, Context context) : GenericValue<Text, string>(value, start, end, context) {

	public static Text FromToken(Token token, Context context) => 
		new((string)token.Value!, token.StartPosition, token.EndPosition, context);
	
}

public class Null(Position start, Position end, Context context) : IValue {
	
	public object? Value {
		get => null;
		set { }
	}

	public Context Context { get; set; } = context;
	public Position StartPosition { get; set; } = start;
	public Position EndPosition { get; set; } = end;

	public Null Clone() => new(StartPosition, EndPosition, Context);
	IValue IValue.Clone() => Clone();
	
}

public class Error(string message, Position start, Position end, Context context) : IValue {

	public string Value => message;
	private object? _value = message;
	object? IValue.Value {
		get => _value;
		set => _value = value;
	}

	// TODO: Errors should have messages and a traceback of other errors and sources
	
	public Context Context { get; set; } = context;
	public Position StartPosition { get; set; } = start;
	public Position EndPosition { get; set; } = end;

	public Error Clone() => new(Value, StartPosition, EndPosition, Context);
	IValue IValue.Clone() => Clone();
	
}

#endregion