using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CompilerModelTest.ObjectDefs
{
    public class ObjectDef
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public Type Type { get; set; }
        public int LocalNumber { get; set; }
        public bool IsDefine { get; set; }

        public void EmitSaveToLocal(ILGenerator generator)
        {
            switch (LocalNumber)
            {
                case 0:
                    generator.Emit(OpCodes.Stloc_0);
                    break;
                case 1:
                    generator.Emit(OpCodes.Stloc_1);
                    break;
                case 2:
                    generator.Emit(OpCodes.Stloc_2);
                    break;
                case 3:
                    generator.Emit(OpCodes.Stloc_3);
                    break;
                default:
                    generator.Emit(LocalNumber < 256 ? OpCodes.Stloc_S : OpCodes.Stloc, LocalNumber);
                    break;
            }
            IsDefine = true;
        }
        public void EmitLoadFromLocal(ILGenerator generator)
        {
            switch (LocalNumber)
            {
                case 0:
                    generator.Emit(OpCodes.Ldloc_0);
                    break;
                case 1:
                    generator.Emit(OpCodes.Ldloc_1);
                    break;
                case 2:
                    generator.Emit(OpCodes.Ldloc_2);
                    break;
                case 3:
                    generator.Emit(OpCodes.Ldloc_3);
                    break;
                default:
                    generator.Emit(LocalNumber < 256 ? OpCodes.Ldloc_S : OpCodes.Ldloc, LocalNumber);
                    break;
            }
        }
        public void EmitInteger(ILGenerator generator)
        {
            switch ((int)Value)
            {
                case -1:
                    generator.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    generator.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    generator.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    generator.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    generator.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    generator.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    generator.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    generator.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    generator.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    generator.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if ((int)Value >= -128 && (int)Value <= 127)
                        generator.Emit(OpCodes.Ldc_I4_S, (sbyte)Value);
                    else
                        generator.Emit(OpCodes.Ldc_I4, (int)Value);
                    break;
            }
        }
        public void EmitDouble(ILGenerator generator)
        {
            generator.Emit(OpCodes.Ldc_R8, Value);
        }

        public void EmitBool(ILGenerator generator)
        {
            switch ((int)Value)
            {
                case 0:
                    generator.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    generator.Emit(OpCodes.Ldc_I4_1);
                    break;
                default:
                    generator.Emit(OpCodes.Ldc_I4_0);
                    break;
            }
        }
    }
}
