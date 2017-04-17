using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerModelTest.ObjectDefs
{
    public class ObjectDef
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public Type Type { get; set; }
        public int LocalNumber { get; set; }
        public bool IsDefine { get; set; }
    }
}
