using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CompilerModelTest.ObjectDefs
{
    public class ValueObjectDef
    {
        protected static List<ValueObjectDef> Locals;

        public static ILGenerator Generator { get; set; }

        public string Name { get; set; }
        public string OriginValue { get; set; }
        public double Value { get; set; }
        public Type Type { get; set; }
        public bool IsDefine { get; set; }
        public int Number { get; set; }


        public static int AlocateLocal(Type type, string name = "")
        {
            if (Locals == null) Locals = new List<ValueObjectDef>();
            List<ValueObjectDef> duplicatedLocals = new List<ValueObjectDef>();
            int number = 0;
            int i = 0;
            for (i = 0; i < Locals.Count; i++)
                if (Locals[i].Type.Name == type.Name && Locals[i].Name == name)
                {
                    number = i;
                    Locals[i] = new ValueObjectDef() {Type = type, Number = number, Name = name};
                    break;
                }
            if (i == Locals.Count)
            {
                var localVar = Generator.DeclareLocal(type);
                number = localVar.LocalIndex;
                Locals.Add(new ValueObjectDef() {Name = name,Number = localVar.LocalIndex, Type = type});
            }
            return number;
        }


        public static void EmitSaveToLocal(int localVarNumber)
        {
            switch (localVarNumber)
            {
                case 0:
                    Generator.Emit(OpCodes.Stloc_0);
                    break;
                case 1:
                    Generator.Emit(OpCodes.Stloc_1);
                    break;
                case 2:
                    Generator.Emit(OpCodes.Stloc_2);
                    break;
                case 3:
                    Generator.Emit(OpCodes.Stloc_3);
                    break;
                default:
                    if (localVarNumber < 256)
                        Generator.Emit(OpCodes.Stloc_S, localVarNumber);
                    else
                        Generator.Emit(OpCodes.Stloc, localVarNumber);
                    break;
            }
        }

        public static void EmitLoadFromLocal(int localVarNumber)
        {
            switch (localVarNumber)
            {
                case 0:
                    Generator.Emit(OpCodes.Ldloc_0);
                    break;
                case 1:
                    Generator.Emit(OpCodes.Ldloc_1);
                    break;
                case 2:
                    Generator.Emit(OpCodes.Ldloc_2);
                    break;
                case 3:
                    Generator.Emit(OpCodes.Ldloc_3);
                    break;
                default:
                    if (localVarNumber < 256)
                        Generator.Emit(OpCodes.Ldloc_S, localVarNumber);
                    else
                        Generator.Emit(OpCodes.Ldloc, localVarNumber);
                    break;
            }
        }

        public static void EmitInteger(int value)
        {
            switch (value)
            {
                case -1:
                    Generator.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    Generator.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    Generator.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    Generator.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    Generator.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    Generator.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    Generator.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    Generator.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    Generator.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    Generator.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if (value >= -128 && value <= 127)
                        Generator.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    else
                        Generator.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }

        public static void EmitDouble(double value)
        {
            Generator.Emit(OpCodes.Ldc_R8, value);
        }

        public static ValueObjectDef GetLocalObjectDef(string Name)
        {
            for (int i = 0; i < Locals.Count; i++)
                if ((Locals[i] as ValueObjectDef).Name == Name)
                    return Locals[i];
            return null;
        }

    }
}
