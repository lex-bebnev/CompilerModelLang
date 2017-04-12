using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerModelTest.Errors
{
    public class ResultTypeError: CompileError
    {
        public string Type1;
        public string Type2;

        public ResultTypeError(string type1, string type2,int number, int? line, int? column) : base(number, line, column)
        {
            Type1 = type1;
            Type2 = type2;
        }

        public override string Description => $"Expected result type: {Type1}, found {Type2}";
    }
}
