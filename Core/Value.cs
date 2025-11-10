namespace Core;

public interface IValue {

	public object? Value { get; set;  }
	public Context Context { get; set; }
	public Position StartPosition { get; set; }
	public Position EndPosition { get; set; }

	IValue Clone();
	
	#region Operators
	
	IValue AddedTo(IValue value);
	IValue SubtractedBy(IValue value);
	IValue MultipliedBy(IValue value);
	IValue DividedBy(IValue value);
	IValue PoweredBy(IValue value);
	IValue ReducedTo(IValue value);

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
	
	public abstract IValue AddedTo(IValue value);
	public abstract IValue SubtractedBy(IValue value);
	public abstract IValue MultipliedBy(IValue value);
	public abstract IValue DividedBy(IValue value);
	public abstract IValue PoweredBy(IValue value);
	public abstract IValue ReducedTo(IValue value);

	#endregion
	
}

#region Values

public class Number(float value, Position start, Position end, Context context) : GenericValue<Number, float>(value, start, end, context) {

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
				clone.Value = MathF.Pow(clone.Value, number.Value);

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
	
}

public class Text(string value, Position start, Position end, Context context) : GenericValue<Text, string>(value, start, end, context) {

	public static Text FromToken(Token token, Context context) => 
		new((string)token.Value!, token.StartPosition, token.EndPosition, context);

	public override IValue AddedTo(IValue value) => throw new NotImplementedException("Operation not implemented");
	public override IValue SubtractedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public override IValue MultipliedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public override IValue DividedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public override IValue PoweredBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public override IValue ReducedTo(IValue value) => throw new NotImplementedException("Operation not implemented");
	
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

	public IValue AddedTo(IValue value) => throw new NotImplementedException("Operation not implemented");
	public IValue SubtractedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public IValue MultipliedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public IValue DividedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public IValue PoweredBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public IValue ReducedTo(IValue value) => throw new NotImplementedException("Operation not implemented");
	
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
		
	public IValue AddedTo(IValue value) => throw new NotImplementedException("Operation not implemented");
	public IValue SubtractedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public IValue MultipliedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public IValue DividedBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public IValue PoweredBy(IValue value) => throw new NotImplementedException("Operation not implemented");
	public IValue ReducedTo(IValue value) => throw new NotImplementedException("Operation not implemented");
	
}

#endregion