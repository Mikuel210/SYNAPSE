namespace Core;

public struct Bounds(Position start, Position end)
{

	public Position Start { get; } = start;
	public Position End { get; } = end;

}

public interface IBounds
{

	Bounds Bounds { get; }

}