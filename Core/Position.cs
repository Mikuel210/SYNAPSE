namespace Core;

public class Position {
 
	public int Index { get; private set; }
	public int Line => GetLine();
	public int Column => GetColumn();
	public Source Source { get; }

	public Position(Source source, int index = 0) {
		Index = index;
		Source = source;

		Source.OnTextPrepend += characters => Index += characters;
	}

	public void Advance(int steps = 1) => Index += steps;
	public void Reverse(int steps = 1) => Index -= steps;

	public Position Clone() => new(Source, Index);

	private void GetLineAndColumn(out int line, out int column) {
		line = 0;
		column = 0;

		for (int i = 0; i < Index; i++) {
			char character = Source.Text[i];

			if (character == '\n') {
				column++;
				line = 0;
			}
			else { line++; }
		}
	}
	private int GetLine() {
		GetLineAndColumn(out int line, out int column);
		return line;
	}
	private int GetColumn() {
		GetLineAndColumn(out int line, out int column);
		return column;
	}

}
