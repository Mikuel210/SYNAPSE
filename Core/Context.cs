namespace Core;

public class Context(Scope scope, Context? parentContext = null, Position? parentStartPosition = null) {

	public Scope Scope { get; } = scope;
	public Context? ParentContext { get; } = parentContext;
	public Position? ParentStartPosition { get; } = parentStartPosition;

}