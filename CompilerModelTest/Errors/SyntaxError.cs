using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerModelTest.Errors
{
    public class SyntaxError: CompileError
    {
        public string Message { get; set; }

        public SyntaxError(int number, int? line, int? column, string msg)
            : base(number, line, column)
        {
            Message = msg;
        }
    }
}
