using Core;

namespace CLI;

class Program
{

	private static readonly Scope _scope = new("Program", "");
	private static readonly Lexer _lexer = new(_scope);
	private static readonly Parser _parser = new(_lexer);

	private static void Main() {
		while (true) {
			Console.Write("RUNTIME > ");
			_scope.AppendText(Console.ReadLine()!);

			var context = new Context(_scope);
			var output = Interpreter.Interpret(_parser, context);
			output.ForEach(e => Console.WriteLine(e.Value));	
		}
	}

}