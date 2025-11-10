using Core;

namespace CLI;

class Program {

	static void Main(string[] args) {
		while (true) {
			Console.Write("RUNTIME > ");
			var source = new Scope("Program", Console.ReadLine()!);
			var lexer = new Lexer(source);
			var parser = new Parser(lexer);

			var context = new Context(source);
			var output = Interpreter.Interpret(parser, context);
			output.ForEach(e => Console.WriteLine(e.Value));	
		}
	}

}