using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CompilerModelTest;
using CompilerModelTest.AntlrGrammar;
using CompilerModelTest.ASTNodes;
using CompilerModelTest.Errors;
using CompilerModelTest.Errors.Listeners;
using CompilerModelTest.Visitors;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Compiler.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            IHighlightingDefinition molaHighlighting;
            using (System.IO.Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("Compiler.UI.TextEditor.MoLaHighlighting.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    molaHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting("MoLa Highlighting", new string[] { ".mola" }, molaHighlighting);


            InitializeComponent();
        }

        private IVocabulary vocabulary;


        public void Compile(object sender, RoutedEventArgs e)
        {
            Errors.Text = "";
            LexicalErrorListener.ClearErrors();
            SyntaxErrorListener.ClearErrors();
            string programmText = TextEditor.Text;
            var stream = new AntlrInputStream(programmText);
            Console.WriteLine(stream.ToString());
            var lexer = new ModelLLexer(stream);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(LexicalErrorListener.INSTACE);
            CommonTokenStream tokens = new CommonTokenStream(lexer);

            ModelLParser syntaxErrorsChecker = new ModelLParser(tokens);
            syntaxErrorsChecker.RemoveErrorListeners();
            syntaxErrorsChecker.AddErrorListener(SyntaxErrorListener.INSTACE);
            syntaxErrorsChecker.programm();
            if (LexicalErrorListener.Errors.Count != 0)
            {
                foreach (SyntaxError syntaxError in LexicalErrorListener.Errors)
                    Errors.Text += syntaxError.Line + " - " + syntaxError.Column + ": " + syntaxError.Message + "\n\r";
                
                if (SyntaxErrorListener.Errors.Count() != 0)
                    foreach (SyntaxError syntaxError in SyntaxErrorListener.Errors)
                        Errors.Text += syntaxError.Line + " - " + syntaxError.Column + ": " + syntaxError.Message + "\n\r";

                return;
            }

            stream = new AntlrInputStream(programmText);
            lexer = new ModelLLexer(stream);
            tokens = new CommonTokenStream(lexer);
            ModelLParser parser = new ModelLParser(tokens);
            IParseTree tree = parser.programm();
            
            var astBuilder = new ProgrammAstBuilder();
            var programm = astBuilder.Visit(tree);

            ProgrammCodeGenerator s = new ProgrammCodeGenerator("NewVersion");
            s.Visit(tree);

            if (s.Errors.Count != 0)
            {
                foreach (var compileError in s.Errors)
                {
                    Errors.Text += compileError.Line+"-" + compileError.Column + " :" + compileError.Description +"\n\r";
                }
            }

            sntxTreeView.Items.Clear();
            FillLexerAndParserTables(programm, null);
        }

        public void CompileAndRun(object sender, RoutedEventArgs e)
        {
            Errors.Text = "";
            string programmText = TextEditor.Text;
            var stream = new AntlrInputStream(programmText);
            Console.WriteLine(stream.ToString());
            var lexer = new ModelLLexer(stream);
            //vocabulary = Vocabulary.FromTokenNames(lexer.TokenNames);
            CommonTokenStream tokens = new CommonTokenStream(lexer);

            ModelLParser parser = new ModelLParser(tokens);
            IParseTree tree = parser.programm();



            var astBuilder = new ProgrammAstBuilder();
            var programm = astBuilder.Visit(tree);

            ProgrammVisitor s = new ProgrammVisitor();
            s.Visit(tree);

            if (s.Errors.Count != 0)
            {
                foreach (var compileError in s.Errors)
                {
                    Errors.Text += compileError.Line + " :" + compileError.Description + "\n\r";
                }
            }

            sntxTreeView.Items.Clear();
            FillLexerAndParserTables(programm, null);

            if (s.Errors.Count == 0)
            {
                Process.Start("test");
            }
        }

        private void FillLexerAndParserTables(Node astTree, TreeViewItem parentTreeViewItem)
        {
            if (astTree != null)
            {
                var item = new TreeViewItem {Header = astTree.Name, Tag = astTree.Name};
                if (astTree.Value!= null)
                {
                    item.Items.Add(new TreeViewItem {Header = astTree.Value, Tag = astTree.Name});
                }
                if (parentTreeViewItem == null)
                    sntxTreeView.Items.Add(item);
                else parentTreeViewItem.Items.Add(item);
                
                for (int i = 0; i < astTree.Nodes.Count; i++)
                {
                    FillLexerAndParserTables(astTree.Nodes[i], item);
                }

            }
        }


    }
}
