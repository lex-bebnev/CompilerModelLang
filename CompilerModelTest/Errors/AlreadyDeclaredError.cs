using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerModelTest.Errors
{
    public class AlreadyDeclaredError: CompileError
    {
        public string IdName;
        public AlreadyDeclaredError(string idName, int number, int? line, int? column) : base(number, line, column)
        {
            IdName = idName;
        }

        public override string Description => $"Local var '{IdName}' is already declared";
    }
}
