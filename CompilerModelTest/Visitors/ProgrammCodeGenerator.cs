using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Tree;
using CompilerModelTest.AntlrGrammar;
using CompilerModelTest.Errors;
using CompilerModelTest.ObjectDefs;

namespace CompilerModelTest.Visitors
{
    public class ProgrammCodeGenerator: ModelLBaseVisitor<ObjectDef>
    {
        public Dictionary<string, ObjectDef> LocalVaribles { get; private set; }
        public IList<CompileError> Errors { get; set; }
        private ILGenerator _iLGenerator;

        public string ProgrammName { get; private set; }

        //Это нужно, чтобы не писать метод подсчета уже имеющихся переменных
        private int _currentLocalVarNumber;

        public ProgrammCodeGenerator(string programmName):base()
        {
            ProgrammName = programmName;
        }

        public override ObjectDef VisitProgramm(ModelLParser.ProgrammContext context)
        {
            LocalVaribles = new Dictionary<string, ObjectDef>();
            Errors = new List<CompileError>();
            var baseCodeGenerator = new ModelLCodeGenerator(ProgrammName);
            baseCodeGenerator.Generete();
            _iLGenerator = baseCodeGenerator.EntryPoint.GetILGenerator();
            _currentLocalVarNumber = 0;

            foreach (IParseTree contextChild in context.children)
            {
                Visit(contextChild);
            }

            _iLGenerator.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadLine", BindingFlags.Public | BindingFlags.Static, null,
                                                                      new Type[] { }, null));
            _iLGenerator.Emit(OpCodes.Pop);
            _iLGenerator.Emit(OpCodes.Ret);

            if(Errors.Count==0) baseCodeGenerator.Save();
            return null;
        }

        public override ObjectDef VisitDeclaration(ModelLParser.DeclarationContext context)
        {
            ObjectDef identificator = new ObjectDef();
            int currentLine = 0, currentColumn = 0;
            try
            {
                foreach (ModelLParser.IdentificatorContext identificatorContext in context.identificator())
                {
                    identificator = new ObjectDef
                    {
                        Name = identificatorContext.GetText(),
                        Type = GetType(context.type.Type),
                        LocalNumber = _currentLocalVarNumber
                    };
                    currentLine = identificatorContext.Start.Line;
                    currentColumn = identificatorContext.start.Column;
                    LocalVaribles.Add(identificator.Name, identificator);
                    _iLGenerator.DeclareLocal(identificator.Type);
                    _currentLocalVarNumber++;
                }
            }
            catch (ArgumentException e)
            {
                Errors.Add(new AlreadyDeclaredError(identificator.Name,0,currentLine,currentColumn));
            }
            return null;
        }

        public override ObjectDef VisitAssigment(ModelLParser.AssigmentContext context)
        {
            ObjectDef idObject = VisitIdentificator(context.identificator());
            if (idObject == null)
                return null;

            ObjectDef idValueObj = VisitExpression(context.expression());
            if (idValueObj == null)
                return null;

            if (idObject.Type != idValueObj.Type)
            {
                Errors.Add(new IncompatibleTypesError(idObject.Type.ToString(), idValueObj.Type.ToString(), 0, context.identificator().start.Line,
                                                      context.identificator().start.Column));
                return null;
            }

            idObject.EmitSaveToLocal(_iLGenerator);
            idObject.Value = idValueObj.Value;
            return null;
        }

        public override ObjectDef VisitExpression(ModelLParser.ExpressionContext context)
        {
            if (context.primary() != null)
            {
                ObjectDef primaryObj = VisitPrimary(context.primary());
                if (context.unary() == null)
                    return primaryObj;

                if (primaryObj.Type != typeof(bool))
                {
                    Errors.Add(new ResultTypeError(typeof(bool).ToString(), primaryObj.Type.ToString(), 0, context.primary().start.Line, context.primary().start.Column));
                    return null;
                }
                _iLGenerator.Emit(OpCodes.Not);
                return primaryObj;
            }

            ObjectDef leftObj  = Visit(context.left);
            ObjectDef rightObj = Visit(context.right);

            if (leftObj == null || rightObj == null)
                return null;

            if ((leftObj.Type == typeof(bool) && rightObj.Type != typeof(bool)) || (leftObj.Type != typeof(bool) && rightObj.Type == typeof(bool)))
            {
                Errors.Add(new IncompatibleTypesError(leftObj.Type.ToString(), rightObj.Type.ToString(), 0, context.left.start.Line, context.left.start.Column));
                return null;
            }

            ObjectDef resultObj = OperateObjects(context.op?.Type,leftObj,rightObj);

            switch (context.op?.Type)
            {
                case ModelLLexer.ADD:
                    _iLGenerator.Emit(OpCodes.Add);
                    break;
                case ModelLLexer.SUB:
                    _iLGenerator.Emit(OpCodes.Sub);
                    break;
                case ModelLLexer.MUL:
                    _iLGenerator.Emit(OpCodes.Mul);
                    break;
                case ModelLLexer.DIV:
                    _iLGenerator.Emit(OpCodes.Div);
                    break;
                case ModelLLexer.AND:
                    _iLGenerator.Emit(OpCodes.And);
                    break;
                case ModelLLexer.OR:
                    _iLGenerator.Emit(OpCodes.Or);
                    break;
                case ModelLLexer.EQUAL:
                    _iLGenerator.Emit(OpCodes.Ceq);
                    break;
                case ModelLLexer.NEQ:
                    _iLGenerator.Emit(OpCodes.Ceq);
                    _iLGenerator.Emit(OpCodes.Not);
                    break;
                case ModelLLexer.MR:
                    _iLGenerator.Emit(OpCodes.Cgt);
                    break;
                case ModelLLexer.MRE:
                    _iLGenerator.Emit(OpCodes.Clt);
                    _iLGenerator.Emit(OpCodes.Ldc_I4_0);
                    _iLGenerator.Emit(OpCodes.Ceq);
                    break;
                case ModelLLexer.LS:
                    _iLGenerator.Emit(OpCodes.Clt);
                    break;
                case ModelLLexer.LE:
                    _iLGenerator.Emit(OpCodes.Cgt);
                    _iLGenerator.Emit(OpCodes.Ldc_I4_0);
                    _iLGenerator.Emit(OpCodes.Ceq);
                    break;
               default:
                    Errors.Add(new CompileError(0,context.op?.Line,context.op?.Column) {Description = "Ошибка распознавания операции в выражении"});
                    return null;
            }

            return resultObj;

        }
        public override ObjectDef VisitPrimary(ModelLParser.PrimaryContext context)
        {
            if (context.identificator() != null)
            {
                ObjectDef idObj = VisitIdentificator(context.identificator());
                if (idObj == null)
                    return null;

                if (idObj.IsDefine)
                {
                    idObj.EmitLoadFromLocal(_iLGenerator);
                    return idObj;
                }

                Errors.Add(new UndeclaredIdError(idObj.Name,0,context.identificator().start.Line,context.identificator().start.Column));
                return null;
            }
            if (context.number() != null)
                return VisitNumber(context.number());
            
            if (context.boolean() != null)
                return VisitBoolean(context.boolean());
            
            if (context.inner_expression() != null)
                return VisitExpression(context.inner_expression().expression());
            
            return null;
        }


        #region Values
        public override ObjectDef VisitInteger(ModelLParser.IntegerContext context)
        {
            ObjectDef integerObj = new ObjectDef
            {
                Type = typeof(int)
            };

            bool isMatch = false;
            Regex decRegex = new Regex("\\b\\d{1,}[d,D]{0,}\\b");
            Regex hexRegex = new Regex("\\b\\d[0-9A-Fa-f]*[h,H]\\b");
            Regex octRegex = new Regex("\\b[0-7]+[o,O]\\b");
            Regex binRegex = new Regex("\\b[0-1]+[b,B]\\b");

            if (hexRegex.IsMatch(context.GetText()))
            {
                isMatch = true;
                integerObj.Value = int.Parse(context.GetText().Trim(new[] {'h', 'H'}), NumberStyles.HexNumber);
            }
            if (decRegex.IsMatch(context.GetText()))
            {
                isMatch = true;
                integerObj.Value = int.Parse(context.GetText().Trim(new[] {'d', 'D'}));
            }
            if (octRegex.IsMatch(context.GetText()))
            {
                isMatch = true;
                integerObj.Value = Convert.ToInt32(context.GetText().Trim(new[] {'o', 'O'}), 8);
            }
            if (binRegex.IsMatch(context.GetText()))
            {
                isMatch = true;
                integerObj.Value = Convert.ToInt32(context.GetText().Trim(new[] {'b', 'B'}), 2);
            }
            if (isMatch)
            {
                integerObj.EmitInteger(_iLGenerator);
                return integerObj;
            }
            Errors.Add(new CompileError(0,context.start.Line,context.start.Column) {Description = "Ошибка распознавания целочисленного значения"});
            return null;
        }
        public override ObjectDef VisitDouble(ModelLParser.DoubleContext context)
        {
            ObjectDef doubleObj = new ObjectDef
            {
                Type = typeof(double)
            };

            double doubleValue;
            if (double.TryParse(context.GetText(), out doubleValue))
            {
                doubleObj.Value = doubleValue;
                doubleObj.EmitDouble(_iLGenerator);
                return doubleObj;
            }

            Errors.Add(new CompileError(0, context.start.Line, context.start.Column) {Description = $"Не получилось распознать значение {context.GetText()}"});
            return null;
        }
        public override ObjectDef VisitBoolean(ModelLParser.BooleanContext context)
        {
            ObjectDef boolObj = new ObjectDef {Type = typeof(bool)};

            switch (context.GetText())
            {
                case "true":
                    boolObj.Value = 1;
                    break;
                case "false":
                    boolObj.Value = 0;
                    break;
                default:
                    boolObj.Value = 0;
                    Errors.Add(new CompileError(0,context.start.Line,context.start.Column) {Description = "Ошибка при распознавании булевского значения"});
                    return null;
            }
            boolObj.EmitBool(_iLGenerator);
            return boolObj;
        }
        #endregion

        public override ObjectDef VisitIdentificator(ModelLParser.IdentificatorContext context)
        {
            string idName = context.GetText();
            ObjectDef idObject;
            if (LocalVaribles.TryGetValue(idName, out idObject))
                return idObject;

            Errors.Add(new UndeclaredIdError(idName, 0, context.Start.Line, context.Start.Column));
            return null;
        }


        private ObjectDef OperateObjects(int? opType, ObjectDef obj1, ObjectDef obj2)
        {
            ObjectDef result = new ObjectDef();
            switch (opType)
            {
                case ModelLLexer.ADD:
                    result.Type = typeof(double);
                    break;
                case ModelLLexer.SUB:
                    result.Type = typeof(double);
                    break;
                case ModelLLexer.MUL:
                    result.Type = typeof(double);
                    break;
                case ModelLLexer.DIV:
                    result.Type = typeof(double);
                    break;
                case ModelLLexer.AND:
                    result.Type = typeof(bool);
                    break;
                case ModelLLexer.OR:
                    result.Type = typeof(bool);
                    break;
                case ModelLLexer.EQUAL:
                    result.Type = typeof(bool);
                    break;
                case ModelLLexer.NEQ:
                    result.Type = typeof(bool);
                    break;
                case ModelLLexer.MR:
                    result.Type = typeof(bool);
                    break;
                case ModelLLexer.MRE:
                    result.Type = typeof(bool);
                    break;
                case ModelLLexer.LS:
                    result.Type = typeof(bool);
                    break;
                case ModelLLexer.LE:
                    result.Type = typeof(bool);
                    break;
                case null: break;
            }
            return result;
        }
        private Type GetType(int typeName)
        {
            switch (typeName)
            {
                case ModelLLexer.INT:
                    return typeof(int);
                case ModelLLexer.BOOL:
                    return typeof(bool);
                case ModelLLexer.REAL:
                    return typeof(double);
                default:
                    return typeof(int);
            }
        }
    }
}
