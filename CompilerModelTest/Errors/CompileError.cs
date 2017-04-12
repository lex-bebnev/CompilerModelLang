using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerModelTest.Errors
{
    public class CompileError
    {
        public int Number { get; set; }
        public virtual string Description { get; set; }
        public int? Line { get; set; }
        public int? Column { get; set; }


        public CompileError(int number, int? line, int? column)
        {
            Number = number;
            Line = line;
            Column = column;
        }
    }
}
