using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CompilerModelTest.AntlrGrammar;
using CompilerModelTest.Visitors;

namespace CompilerModelTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            var Tokens = new List<Token>();

            //var stream = new AntlrInputStream("{\r\n dim a,b,c integer; \r\n write(0.34E+2+3*8); \r\n read(a); \r\n write(b+2.3+3); \r\n while a=3 do b as (a + 1 + c); \r\n}");
            //var stream = new AntlrInputStream("{ \n\r\tdim a,b integer; \n\r\tdim c real; \n\r\ta as 3; \n\r\t; \n\r\tc as b; \n\r\twrite(a = 3); \n\r\tread(b); \n\r\tc as b;\n\r}");
            var stream = new AntlrInputStream("{dim a integera;"); //dim b real; dim d,c boolean; d as false; c as true; write(d<>c); a as 10; b as 1; while a>0 do { a as a-1; write(a);} for a as 0; to a=10 do { a as a + 1; write(a);}}");//  d as 2.0;  write(c/d); read(a); read(b); read(c); write(c); write(a,b); write(c/c); write(a);}");
            Console.WriteLine(stream.ToString());
            Console.WriteLine();
            var lexer = new ModelLLexer(stream);
            
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            ModelLParser parser = new ModelLParser(tokens);

            IParseTree tree = parser.programm();
            
            ProgrammVisitor s = new ProgrammVisitor();
            s.Visit(tree);
            //Console.WriteLine(tree.GetText());
            
            //Console.WriteLine(typeof(bool));

            //gen.Generete();
            Console.ReadLine();

/*
            string a;
            Console.WriteLine("Read:");
            a = Console.ReadLine();
            Console.WriteLine(a);
            Console.ReadLine();
            */
        }
    }



    public class Token
    {
        public string Name
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

        public int Line
        {
            get;
            set;
        }

        public int Column
        {
            get;
            set;
        }
    }
}
