using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerModelTest.ASTNodes
{
    public class Node
    {
        public string Name;
        public List<Node> Nodes = new List<Node>();
        public string Value;
    }
    public class ProgrammNode: Node
    {
        public new string Name => "programm";
    }

    public class DeclarationNode: Node
    {
        public new string Name => "declaration";
    }
    public class StatementNode: Node
    {
        public new string Name => "statement";
    }
    public class TypeNode: Node
    {
        public new string Name => "type";
        
    }


    #region Statement
    class AssigmentNode : Node
    {
        public static string Name => "assigment";
    }
    class ForNode : Node
    {
        public static string Name => "for";
    }
    class IfNode : Node
    {
        public static string Name => "if";
    }
    class WhileNode: Node
    {
        public static string Name => "while";
    }
    class ReadNode: Node
    {
        public static string Name => "read";
    }
    class WriteNode: Node
    {
        public static string Name => "write";
    }
    #endregion


    #region Expression
    class ExpressionNode : Node
    {
        public static string Name => "expression";
    }
    class InfixExpressionNode : ExpressionNode
    {
        public  Node Left { get; set; }
        public  Node Right { get; set; }
    }
    class AdditionNode : InfixExpressionNode
    {
        public static string Name => "additional";
    }
    class SubtractionNode : InfixExpressionNode
    {
        public static string Name => "substraction";
    }
    class MultiplicationNode : InfixExpressionNode
    {
        public static string Name => "multiplication";
    }
    class DivisionNode : InfixExpressionNode
    {
        public static string Name => "division";
    }
    class NegateNode : ExpressionNode
    {
        public static string Name => "negate";
        public ExpressionNode InnerNode { get; set; }
    }
    #endregion


    #region Primary
    class IdentificatorNode : ExpressionNode
    {
        public new string Name => "identificator";
    }
    class NumberNode : ExpressionNode
    {
        public static string Name => "number";
    }
    class BooleanNode : ExpressionNode
    {
        public static string Name => "boolean";
    }
    #endregion

    #region Types

    class IntegerNode : ExpressionNode
    {
    }
    class DoubleNode : ExpressionNode
    {
    }

    #endregion

}
