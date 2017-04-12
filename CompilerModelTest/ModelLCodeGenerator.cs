using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using CompilerModelTest.ObjectDefs;

namespace CompilerModelTest
{
    public class ModelLCodeGenerator
    {
        #region Constant
        public static Type BoolType = typeof(bool);
        public static Type IntegerType = typeof(int);
        public static Type RealType = typeof(double);
        public MethodInfo WriteBoolMethod;
        public MethodInfo WriteRealMethod;
        public MethodInfo WriteMethod;
        public MethodInfo ReadMethod;
        public MethodInfo IntParseMethod;
        public MethodInfo RealParseMethod;
        #endregion

        #region Fields
        protected ITree Tree;

        protected string ProgramName = "TestProgramm";
        protected string FileName;
        protected AssemblyName AssemblyName;
        protected AssemblyBuilder AssemblyBuilder;
        protected ModuleBuilder ModuleBuilder;

        protected TypeBuilder CurrentTypeBuilder;
        protected ILGenerator CurrentILGenerator;

        protected Dictionary<string, TypeBuilder> ClassBuilders;
        //protected Dictionary<string, Dictionary<string, FieldObjectDef>> Fields;
        protected Dictionary<string, Dictionary<string, MethodDef>> Functions;
        protected Dictionary<string, ConstructorBuilder> Constructors;

        //protected Dictionary<string, ArgObjectDef> CurrentArgs_;

        public List<CompilerError> CompilerErrors;

        public MethodBuilder EntryPoint;

        #endregion

        public ModelLCodeGenerator(string modelLFileName, ITree tree)
        {
            WriteMethod = typeof(Console).GetMethod("WriteLine", BindingFlags.Public | BindingFlags.Static, null,
                new Type[] { IntegerType}, null);
            ReadMethod = typeof(Console).GetMethod("ReadLine", BindingFlags.Public | BindingFlags.Static, null,
                new Type[] { }, null);

            IntParseMethod = IntegerType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null,
                new Type[] { typeof(string) }, null);
            RealParseMethod = RealType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null,
                new Type[] { typeof(string) }, null);

            FileName = modelLFileName;
            Tree = tree;
            ProgramName = Path.GetFileNameWithoutExtension(FileName);
        }

        #region public

        public void Generete()
        {
            GenerateAssembly();
            bool fileSaveError = false;
            if (File.Exists(ProgramName + ".exe"))
            {
                try
                {

                }
                catch
                {
                    
                }
            }
            DefineDefaultClass();

            var iLGenertor = EntryPoint.GetILGenerator();
            /*
            iLGenertor.EmitWriteLine("azazazaza");
            iLGenertor.Emit(OpCodes.Call,ReadMethod);
            iLGenertor.Emit(OpCodes.Pop);
            iLGenertor.Emit(OpCodes.Ret);

            var ctr = Constructors["Main"].GetILGenerator();
            ctr.Emit(OpCodes.Ret);*/

            
        }

        public void Save()
        {
            var ctr = Constructors["Main"].GetILGenerator();
            ctr.Emit(OpCodes.Ret);

            ClassBuilders["Main"].CreateType();


            AssemblyBuilder.Save(ProgramName + ".exe");
        }

        #endregion

        #region Init

        private void DefineDefaultClass()
        {
            TypeBuilder classBuilder;
            ConstructorBuilder constructorBuilder;
            MethodBuilder methodBuilder;
            Constructors = new Dictionary<string, ConstructorBuilder>();
            ClassBuilders = new Dictionary<string, TypeBuilder>();

            classBuilder = ModuleBuilder.DefineType("Main", TypeAttributes.NotPublic | TypeAttributes.BeforeFieldInit,
                typeof(object));

            classBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            constructorBuilder = classBuilder.DefineConstructor(
                MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig, CallingConventions.Standard, Type.EmptyTypes);

            methodBuilder = classBuilder.DefineMethod("main",
                MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, typeof(void),
                Type.EmptyTypes);
            methodBuilder.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(STAThreadAttribute).GetConstructor(Type.EmptyTypes), new object[] { }));
            AssemblyBuilder.SetEntryPoint(methodBuilder);
            EntryPoint = methodBuilder;
            /*
            var writeBuilder = classBuilder.DefineMethod("write",MethodAttributes.Public,CallingConventions.Standard,classBuilder,new Type[] {/*BoolType,RealType,/IntegerType});
            var ilGenerator = writeBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, WriteMethod);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ret);

            var readBuilder = classBuilder.DefineMethod("read", MethodAttributes.Public, CallingConventions.Standard,
                classBuilder, new Type[] {BoolType, RealType, IntegerType});
            ilGenerator = readBuilder.GetILGenerator();
            //ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, ReadMethod);
            ilGenerator.Emit(OpCodes.Ret);
            */
            Constructors.Add("Main", constructorBuilder);
            ClassBuilders.Add("Main", classBuilder);
        }

        private void EmitFunction(ITree functionNode)
        {
            string functionName = "main";
            string className = "Main";

            CurrentILGenerator = Functions[className][functionName].MethodBuilder.GetILGenerator();

        }

        #endregion


        #region Expression Evaluate

        protected void EmitExpression(ITree expressionNode)
        {
            
        }
        #endregion 

        #region Helpers
        private void GenerateAssembly()
        {
            AssemblyName = new AssemblyName() {Name = ProgramName};
            AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule(ProgramName, ProgramName + ".exe", false);
        }
        private Type GetType(string typeName)
        {
            Type result = IntegerType;
            switch (typeName)
            {
                case "integer":
                    result = IntegerType;
                    break;
                case "boolean":
                    result = BoolType;
                    break;
                case "real":
                    result = RealType;
                    break;
            }
            return result;
        }
        #endregion
    }
}
