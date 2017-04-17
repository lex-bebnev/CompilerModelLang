using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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

            _iLGenerator.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadLine", BindingFlags.Public | BindingFlags.Static, null,
                                                                      new Type[] { }, null));
            _iLGenerator.Emit(OpCodes.Pop);
            _iLGenerator.Emit(OpCodes.Ret);

            if(Errors.Count==0) baseCodeGenerator.Save();
            return null;
        }

        public override ObjectDef VisitDeclaration(ModelLParser.DeclarationContext context)
        {
            if (context.exception!=null)
            {
                Errors.Add(new SyntaxError(context.exception.OffendingState,context.exception.OffendingToken.Line, context.exception.OffendingToken.Column, context.exception.Message));
                return new ObjectDef()
                {
                    Name = "ERROR"
                };
            }
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
