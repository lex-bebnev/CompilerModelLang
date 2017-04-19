using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using CompilerModelTest.AntlrGrammar;
using CompilerModelTest.Errors;
using CompilerModelTest.ObjectDefs;

namespace CompilerModelTest.Visitors
{
    public class ProgrammVisitor: ModelLBaseVisitor<ValueObjectDef>
    {
        readonly Dictionary<string,ValueObjectDef> varibles = new Dictionary<string, ValueObjectDef>();
        protected ILGenerator CurrentILGenerator;

        public List<CompileError> Errors = new List<CompileError>();


        public override ValueObjectDef VisitProgramm(ModelLParser.ProgrammContext context)
        {
            var gen = new ModelLCodeGenerator("test");
            gen.Generete();
            CurrentILGenerator = gen.EntryPoint.GetILGenerator();
            ValueObjectDef.Generator = CurrentILGenerator;

            var res = base.VisitProgramm(context);

            CurrentILGenerator.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadLine", BindingFlags.Public | BindingFlags.Static, null,
                new Type[] { }, null));
            //----------------
            CurrentILGenerator.Emit(OpCodes.Pop);
            CurrentILGenerator.Emit(OpCodes.Ret);

            if (Errors.Count == 0)
            {
                gen.Save();
            }

            return new ValueObjectDef();
        }
        public override ValueObjectDef VisitDeclaration(ModelLParser.DeclarationContext context)
        {
            Console.WriteLine("Declaration \n\r{");
            try
            {
                foreach (var identificatorContext in context.identificator())
                {
                    Console.WriteLine("\t" + identificatorContext.GetText() + ": " + (context.type.Text));                  
                    ValueObjectDef.AlocateLocal(GetType(context.type.Type), identificatorContext.GetText());
                    varibles.Add(identificatorContext.GetText(), ValueObjectDef.GetLocalObjectDef(identificatorContext.GetText()));
                }
            }
            catch (ArgumentException e)
            {
                Errors.Add(new AlreadyDeclaredError(e.ParamName,0,null,null));
            }
            Console.WriteLine("}");
            return base.VisitDeclaration(context);
        }

        public override ValueObjectDef VisitExpression(ModelLParser.ExpressionContext context)
        {
            if (context.primary() != null)
            {
                var resultObject = VisitPrimary(context.primary());
                //if (context.unary() == null) return resultObject;

                if (resultObject.Type != typeof(bool))
                {
                    Errors.Add(new ResultTypeError("boolean", resultObject.Type.ToString(),0,context.start.Line,context.start.Column));    
                }
                CurrentILGenerator.Emit(OpCodes.Not);
                return new ValueObjectDef() { Type = typeof(bool) };
            }
            Type resultType;

            var resultObj1 = Visit(context.left);
            var resultObj2 = Visit(context.right);

            if (resultObj1.Type == typeof(int) && resultObj2.Type == typeof(int))
                resultType = typeof(int);
            else
            {
                if ((resultObj1.Type == typeof(double) && resultObj2.Type == typeof(double))
                    || (resultObj1.Type == typeof(int) && resultObj2.Type == typeof(double))
                    || (resultObj1.Type == typeof(double) && resultObj2.Type == typeof(int)))
                    resultType = typeof(double);
                else
                {
                    if ((resultObj1.Type != typeof(bool) || resultObj2.Type != typeof(bool)))
                    {
                        Errors.Add(new IncompatibleTypesError(resultObj1.Type.ToString(), resultObj2.Type.ToString(), 0,
                            context.left.start.Line, context.left.start.Column));
                    }
                    resultType = typeof(bool);
                }
            }

            switch (context.op.Type)
            {
                case ModelLLexer.ADD:
                    CurrentILGenerator.Emit(OpCodes.Add);
                    break;
                case ModelLLexer.SUB:
                    CurrentILGenerator.Emit(OpCodes.Sub);
                    break;
                case ModelLLexer.MUL:
                    CurrentILGenerator.Emit(OpCodes.Mul);
                    break;
                case ModelLLexer.DIV:
                    CurrentILGenerator.Emit(OpCodes.Div);
                    break;
                case ModelLLexer.AND:
                    CurrentILGenerator.Emit(OpCodes.And);
                    break;
                case ModelLLexer.OR:
                    CurrentILGenerator.Emit(OpCodes.Or);
                    break;
                case ModelLLexer.EQUAL:
                    CurrentILGenerator.Emit(OpCodes.Ceq);
                    resultType = typeof(bool);
                    break;
                case ModelLLexer.NEQ:
                    CurrentILGenerator.Emit(OpCodes.Ceq);
                    CurrentILGenerator.Emit(OpCodes.Not);
                    resultType = typeof(bool);
                    break;
                case ModelLLexer.MR:
                    CurrentILGenerator.Emit(OpCodes.Cgt);
                    resultType = typeof(bool);
                    break;
                case ModelLLexer.MRE:
                    CurrentILGenerator.Emit(OpCodes.Clt);
                    CurrentILGenerator.Emit(OpCodes.Ldc_I4_0);
                    CurrentILGenerator.Emit(OpCodes.Ceq);
                    resultType = typeof(bool);
                    break;
                case ModelLLexer.LS:
                    CurrentILGenerator.Emit(OpCodes.Clt);
                    resultType = typeof(bool);
                    break;
                case ModelLLexer.LE:
                    CurrentILGenerator.Emit(OpCodes.Cgt);
                    CurrentILGenerator.Emit(OpCodes.Ldc_I4_0);
                    CurrentILGenerator.Emit(OpCodes.Ceq);
                    resultType = typeof(bool);
                    break;
            }
            return new ValueObjectDef() { Type = resultType };
        }
        public override ValueObjectDef VisitPrimary(ModelLParser.PrimaryContext context)
        {
            Console.WriteLine("Primary");
            
            if (context.identificator() != null)
            {
                try
                {
                    if (!varibles[context.identificator().GetText()].IsDefine)
                        Errors.Add(new UndeclaredIdError(context.identificator().GetText(),0,context.identificator().start.Line, context.identificator().start.Column));
                    var result = ValueObjectDef.GetLocalObjectDef(context.identificator().GetText());
                    ValueObjectDef.EmitLoadFromLocal(result.Number);
                    return result;
                }
                catch (KeyNotFoundException e)
                {
                    e.ToString();
                    Errors.Add(new UndefinedIdError(context.identificator().GetText(), 0, context.identificator().start.Line, context.identificator().start.Column));
                   
                }
            }

            if (context.number() != null)
            {
                return VisitNumber(context.number());
            }
            if (context.boolean() != null)
            {
                return VisitBoolean(context.boolean());
            }
            if (context.inner_expression()!= null)
            {
                return VisitExpression(context.inner_expression().expression());
            }
            
            return new ValueObjectDef();
        }


        #region Values
        public override ValueObjectDef VisitInteger(ModelLParser.IntegerContext context)
        {

            var resultObject = new ValueObjectDef();
            resultObject.Type = typeof(int);
            resultObject.Value = 0;

            Regex decRegex = new Regex("\\b\\d{1,}[d,D]{0,}\\b");
            Regex hexRegex = new Regex("\\b\\d[0-9A-Fa-f]*[h,H]\\b");
            Regex octRegex = new Regex("\\b[0-7]+[o,O]\\b");
            Regex binRegex = new Regex("\\b[0-1]+[b,B]\\b");

            if (hexRegex.IsMatch(context.GetText()))
            {
                resultObject.Value = int.Parse(context.GetText().Trim(new [] {'h','H'}),NumberStyles.HexNumber);
                ValueObjectDef.EmitInteger((int)resultObject.Value);
                return resultObject;
            }
            if (decRegex.IsMatch(context.GetText()))
            {
                resultObject.Value = int.Parse(context.GetText().Trim(new[] { 'd', 'D' }));
                ValueObjectDef.EmitInteger((int)resultObject.Value);
                return resultObject;
            }
            if (octRegex.IsMatch(context.GetText()))
            {
                resultObject.Value = Convert.ToInt32(context.GetText().Trim(new[] { 'o', 'O' }),8);
                ValueObjectDef.EmitInteger((int)resultObject.Value);
                return resultObject;
            }
            if (binRegex.IsMatch(context.GetText()))
            {
                resultObject.Value = Convert.ToInt32(context.GetText().Trim(new[] { 'b', 'B' }), 2);
                ValueObjectDef.EmitInteger((int)resultObject.Value);
                return resultObject;
            }

            return resultObject;
        }
        public override ValueObjectDef VisitDouble(ModelLParser.DoubleContext context)
        {
            var resultObject = new ValueObjectDef();
            resultObject.Type = typeof(double);

            double doubleValue;
            if (double.TryParse(context.GetText(), out doubleValue))
            {
                resultObject.Value = doubleValue;
                CurrentILGenerator.Emit(OpCodes.Ldc_R8, doubleValue);
                Console.WriteLine(doubleValue);
            }
            else
            {
                Errors.Add(new CompileError(0,context.start.Line, context.start.Column) {Description = $"Не получилось распознать значение {context.GetText()}"});
                //Console.WriteLine("Не получилось распарсить значение");
            }
            return resultObject;
        }
        public override ValueObjectDef VisitBoolean(ModelLParser.BooleanContext context)
        {
            var resultObject = new ValueObjectDef();
            resultObject.Type = typeof(bool);

            switch (context.GetText())
            {
                case "true":
                    resultObject.Value = 1;
                    CurrentILGenerator.Emit(OpCodes.Ldc_I4_1);
                    break;
                case "false":
                    resultObject.Value = 0;
                    CurrentILGenerator.Emit(OpCodes.Ldc_I4_0);
                    break;
                default:
                    resultObject.Value = 0;
                    CurrentILGenerator.Emit(OpCodes.Ldc_I4_0);
                    break;
            }
            return resultObject;
        }
        #endregion


        #region Statements
        public override ValueObjectDef VisitAssigment(ModelLParser.AssigmentContext context)
        {
            try
            {
                var varible = varibles[context.identificator().GetText()];
                var resultObject = VisitExpression(context.expression());
                if ((varible.Type != resultObject.Type) && (varible.Type != typeof(double) && resultObject.Type != typeof(int)))
                {
                    Errors.Add(new IncompatibleTypesError(resultObject.Type.ToString(),varible.Type.ToString(),0,context.Start.Line,context.Start.Column)); 
                }
                ValueObjectDef.EmitSaveToLocal(ValueObjectDef.GetLocalObjectDef(varible.Name).Number);
                varible.IsDefine = true;
            }
            catch (KeyNotFoundException e)
            {
                Errors.Add(new UndeclaredIdError(context.identificator().GetText(),0, context.Start.Line, context.Start.Column));
            }
            return new ValueObjectDef();
        }
        public override ValueObjectDef VisitWrite(ModelLParser.WriteContext context)
        {
            Console.WriteLine("Write");

            for (int i = 0; i < context.expression().Length; i++)
            {
                var resultObject = VisitExpression(context.expression()[i]);
                CurrentILGenerator.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", BindingFlags.Public | BindingFlags.Static, null,
                new Type[] { resultObject.Type }, null));
                if (i == context.expression().Length - 1)
                {
                    CurrentILGenerator.Emit(OpCodes.Ldstr, "");
                    CurrentILGenerator.Emit(OpCodes.Call,
                        typeof(Console).GetMethod("WriteLine", BindingFlags.Public | BindingFlags.Static, null,
                            new Type[] {typeof(string)}, null));
                }
                else
                {
                    CurrentILGenerator.Emit(OpCodes.Ldstr, ", ");
                    CurrentILGenerator.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", BindingFlags.Public | BindingFlags.Static, null,
                    new Type[] { typeof(string) }, null));
                }
            }
            return new ValueObjectDef();
        }
        public override ValueObjectDef VisitRead(ModelLParser.ReadContext context)
        {
            Console.WriteLine("Read");
                foreach (var identificatorContext in context.identificator())
                {
                    try
                    {
                        CurrentILGenerator.EmitWriteLine("Read <-");
                        CurrentILGenerator.Emit(OpCodes.Call,
                            typeof(Console).GetMethod("ReadLine", BindingFlags.Public | BindingFlags.Static, null,
                                new Type[] {}, null));
                        
                        var localVar = ValueObjectDef.GetLocalObjectDef(identificatorContext.GetText());

                        switch (localVar.Type.Name)
                        {
                            case "Int32":
                            {
                                //Метод попытки разбора, если что выбросить эксепшн(или повторный запрос
                                CurrentILGenerator.Emit(OpCodes.Call,
                                    typeof(int).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null,
                                        new Type[] {typeof(string)}, null));
                                break;
                            }
                            case "Double":
                            {
                                CurrentILGenerator.Emit(OpCodes.Call,
                                    typeof(double).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null,
                                        new Type[] {typeof(string)}, null));
                                break;
                            }
                            case "Boolean":
                            { 
                                CurrentILGenerator.Emit(OpCodes.Call,
                                    typeof(bool).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null,
                                        new Type[] { typeof(string) }, null));
                                break;
                            }
                        }
                        ValueObjectDef.EmitSaveToLocal(localVar.Number);
                        varibles[identificatorContext.GetText()].IsDefine = true;
                    }
                    catch (KeyNotFoundException e)
                    {
                        Errors.Add(new UndeclaredIdError(identificatorContext.GetText(), 0, identificatorContext.Start.Line, identificatorContext.Start.Column));
                    }
                }
            return new ValueObjectDef();
        }

        public override ValueObjectDef VisitFor(ModelLParser.ForContext context)
        {
            VisitAssigment(context.assigment());

            var checkLabel = CurrentILGenerator.DefineLabel();

            CurrentILGenerator.MarkLabel(checkLabel);
            var toObject = VisitExpression(context.expression());
            if (toObject.Type != typeof(bool))
            {
                Errors.Add(new ResultTypeError("boolean", toObject.Type.ToString(), 0, context.start.Line, context.start.Column));
            }
            var exitLabel = CurrentILGenerator.DefineLabel();
            CurrentILGenerator.Emit(OpCodes.Brtrue,exitLabel);

            VisitStatement(context.statement());
            CurrentILGenerator.Emit(OpCodes.Br, checkLabel);

            CurrentILGenerator.MarkLabel(exitLabel);

            return new ValueObjectDef();
        }

        public override ValueObjectDef VisitWhile(ModelLParser.WhileContext context)
        {
            var checkLabel = CurrentILGenerator.DefineLabel();
            CurrentILGenerator.MarkLabel(checkLabel);

            var condition = VisitExpression(context.expression());
            if (condition.Type != typeof(bool))
            {
                Errors.Add(new ResultTypeError("boolean", condition.Type.ToString(), 0, context.start.Line, context.start.Column));
            }

            var exitLabel = CurrentILGenerator.DefineLabel();
            CurrentILGenerator.Emit(OpCodes.Brfalse, exitLabel);

            VisitStatement(context.statement());

            CurrentILGenerator.Emit(OpCodes.Nop);
            CurrentILGenerator.Emit(OpCodes.Br, checkLabel);

            CurrentILGenerator.MarkLabel(exitLabel);

            return new ValueObjectDef();
        }

        public override ValueObjectDef VisitIf(ModelLParser.IfContext context)
        {
            var condition = VisitExpression(context.expression());

            if (condition.Type != typeof(bool))
            {
                Errors.Add(new ResultTypeError("boolean", condition.Type.ToString(), 0, context.start.Line, context.start.Column));
            }
            var exitLabel = CurrentILGenerator.DefineLabel();
            var elseLabel = CurrentILGenerator.DefineLabel();

            CurrentILGenerator.Emit(OpCodes.Brfalse,elseLabel);

            VisitStatement(context.statement()[0]);
            CurrentILGenerator.Emit(OpCodes.Br,exitLabel);

            if (context.ELSE() != null)
            {
                CurrentILGenerator.MarkLabel(elseLabel);

                VisitStatement(context.statement()[1]);
            }
            CurrentILGenerator.MarkLabel(exitLabel);

            return new ValueObjectDef();
        }

        #endregion 

        private Type GetType(int typeName)
        {
            switch (typeName)
            {
                case ModelLLexer.INT:
                    return  typeof(int);
                case ModelLLexer.BOOL:
                    return  typeof(bool);  
                case ModelLLexer.REAL:
                    return typeof(double);
                default:
                    return typeof(int);
            }
        }
    }
}
