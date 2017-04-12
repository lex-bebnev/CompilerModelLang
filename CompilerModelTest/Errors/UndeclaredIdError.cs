using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerModelTest.Errors
{
    public class UndeclaredIdError: CompileError
    {
        public string IdName;
        public UndeclaredIdError(string idName, int number, int? line, int? column) : base(number, line, column)
        {
            IdName = idName;
        }

        public override string Description => $"Local var '{IdName}' is not declared";
    }
}
