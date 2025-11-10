using Core;

namespace CLI;

class Program {

	static void Main(string[] args) {
		while (true) {
			Console.Write("RUNTIME > ");
			var scope = new Scope("Program", Console.ReadLine()!);
			var lexer = new Lexer(scope);
			var parser = new Parser(lexer);

			var context = new Context(scope);
			var output = Interpreter.Interpret(parser, context);
			output.ForEach(e => Console.WriteLine(e.Value));	
		}
	}

}