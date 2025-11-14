namespace Core;

public class Context(Scope scope, Context? parentContext = null, Position? parentStart = null) {

	public Scope Scope { get; } = scope;
	public Context? ParentContext { get; } = parentContext;
	public Position? ParentStart { get; } = parentStart;

}