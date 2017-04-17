using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace CompilerModelTest.Errors.Listeners
{
    public class LexicalErrorListener: IAntlrErrorListener<int>
    {
        public static IAntlrErrorListener<int> INSTACE = new LexicalErrorListener();
        public static List<SyntaxError> Errors;
       
        public LexicalErrorListener()
        {
            Errors=new List<SyntaxError>();
        }
        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Errors.Add(new SyntaxError(0, line, charPositionInLine, "Lexical error: "+msg));
        }

        public static void ClearErrors()
        {
            Errors.Clear();
        }
    }

    public class SyntaxErrorListener: IAntlrErrorListener<IToken>
    {
        public static IAntlrErrorListener<IToken> INSTACE = new SyntaxErrorListener();
        public static List<SyntaxError> Errors;

        public SyntaxErrorListener()
        {
            Errors = new List<SyntaxError>();
        }
        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Errors.Add(new SyntaxError(0, line, charPositionInLine, "Syntaxis error: " + msg));
        }
        public static void ClearErrors()
        {
            Errors.Clear();
        }
    }
}
