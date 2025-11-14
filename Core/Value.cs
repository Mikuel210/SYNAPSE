namespace Core;

public interface IValue {

	public object? Value { get; set;  }
	public Context Context { get; set; }
	public Position StartPosition { get; set; }
	public Position EndPosition { get; set; }

	IValue Clone();
	
	#region Operators

	IValue AddedTo(IValue value) => new Error($"Unsupported operand: {GetType().Name} + {value.GetType().Name}", StartPosition, value.EndPosition, Context);
	IValue SubtractedBy(IValue value) => new Error($"Unsupported operand: {GetType().Name} - {value.GetType().Name}", StartPosition, value.EndPosition, Context);
	IValue MultipliedBy(IValue value) => new Error($"Unsupported operand: {GetType().Name} * {value.GetType().Name}", StartPosition, value.EndPosition, Context);
	IValue DividedBy(IValue value) => new Error($"Unsupported operand: {GetType().Name} / {value.GetType().Name}", StartPosition, value.EndPosition, Context);
	IValue PoweredBy(IValue value) => new Error($"Unsupported operand: {GetType().Name} ^ {value.GetType().Name}", StartPosition, value.EndPosition, Context);
	IValue ReducedTo(IValue value) => new Error($"Unsupported operand: {GetType().Name} % {value.GetType().Name}", StartPosition, value.EndPosition, Context);
	
	IValue IsEquals(IValue value) => new Error($"Unsupported operand: {GetType().Name} == {value.GetType().Name}", StartPosition, value.EndPosition, Context);
	IValue IsGreaterThan(IValue value) => new Error($"Unsupported operand: {GetType().Name} > {value.GetType().Name}", StartPosition, value.EndPosition, Context);
	IValue IsGreaterThanEquals(IValue value) => new Error($"Unsupported operand: {GetType().Name} >= {value.GetType().Name}", StartPosition, value.EndPosition, Context);
	IValue IsLessThan(IValue value) => new Error($"Unsupported operand: {GetType().Name} < {value.GetType().Name}", StartPosition, value.EndPosition, Context);
	IValue IsLessThanEquals(IValue value) => new Error($"Unsupported operand: {GetType().Name} <= {value.GetType().Name}", StartPosition, value.EndPosition, Context);

	IValue And(IValue value) => new Error($"Unsupported operand: {GetType().Name} and {value.GetType().Name}", StartPosition, value.EndPosition, Context);
	IValue Or(IValue value) => new Error($"Unsupported operand: {GetType().Name} or {value.GetType().Name}", StartPosition, value.EndPosition, Context);
	IValue Not() => new Error($"Unsupported operand: not {GetType().Name}", StartPosition, EndPosition, Context);

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
	
	public virtual IValue AddedTo(IValue value) => ((IValue)this).AddedTo(value);
	public virtual IValue SubtractedBy(IValue value) => ((IValue)this).SubtractedBy(value);
	public virtual IValue MultipliedBy(IValue value) => ((IValue)this).MultipliedBy(value);
	public virtual IValue DividedBy(IValue value) => ((IValue)this).DividedBy(value);
	public virtual IValue PoweredBy(IValue value) => ((IValue)this).PoweredBy(value);
	public virtual IValue ReducedTo(IValue value) => ((IValue)this).ReducedTo(value);
	
	public virtual IValue IsEquals(IValue value) => ((IValue)this).IsEquals(value);
	public virtual IValue IsGreaterThan(IValue value) => ((IValue)this).IsGreaterThan(value);
	public IValue IsGreaterThanEquals(IValue value)
	{
		// TODO: This should check if it's a number, return error if it's not
		bool isEquals = !IsEquals(value).Value?.Equals(0) ?? false;
		bool isGreaterThan = !IsGreaterThan(value).Value?.Equals(0) ?? false;

		return new Number(isEquals || isGreaterThan ? 1 : 0, StartPosition, EndPosition, Context);
	}
	public IValue IsLessThan(IValue value)
	{
		bool isEquals = !IsEquals(value).Value?.Equals(0) ?? false;
		bool isGreaterThan = !IsGreaterThan(value).Value?.Equals(0) ?? false;

		return new Number(!isEquals && !isGreaterThan ? 1 : 0, StartPosition, EndPosition, Context);
	}
	public IValue IsLessThanEquals(IValue value)
	{
		bool isEquals = !IsEquals(value).Value?.Equals(0) ?? false;
		bool isGreaterThan = !IsGreaterThan(value).Value?.Equals(0) ?? false;

		return new Number(isEquals || !isGreaterThan ? 1 : 0, StartPosition, EndPosition, Context);
	}
	
	public virtual IValue And(IValue value) => ((IValue)this).And(value);
	public virtual IValue Or(IValue value) => ((IValue)this).Or(value);
	public virtual IValue Not() => ((IValue)this).Not();
	
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