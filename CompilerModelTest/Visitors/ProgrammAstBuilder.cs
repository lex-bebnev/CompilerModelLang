using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CompilerModelTest.AntlrGrammar;
using CompilerModelTest.ASTNodes;

namespace CompilerModelTest.Visitors
{
    public class ProgrammAstBuilder: ModelLBaseVisitor<Node>
    {
        public override Node VisitProgramm(ModelLParser.ProgrammContext context)
        {
            Node programm = new ProgrammNode();
            programm.Name = "programm";

            if (context.children != null)
            {
                foreach (var contextChild in context.children)
                {
                    programm.Nodes.Add(Visit(contextChild));
                }
            }

            return programm;
        }

        public override Node VisitDeclaration(ModelLParser.DeclarationContext context)
        {
            Node declaration = new DeclarationNode();
            
            Node type = new TypeNode();
            type.Name = "type";
            type.Value = context.type.Text;

            foreach (var identificatorContext in context.identificator())
            {
                var id = VisitIdentificator(identificatorContext);
                id.Nodes.Add(type);
                declaration.Nodes.Add(id);
            }
            declaration.Name = "declaration";
            
            return declaration;
        }

        public override Node VisitStatement(ModelLParser.StatementContext context)
        {
            Node statement = new StatementNode();
            statement.Name = "statement";

            foreach (var contextChild in context.children)
            {
                statement.Nodes.Add(Visit(contextChild));
            }

            return statement;
        }

        public override Node VisitWrite(ModelLParser.WriteContext context)
        {

            Node write = new WriteNode();
            write.Name = "write";

            foreach (var expressionContext in context.expression())
            {
                write.Nodes.Add(VisitExpression(expressionContext));
            }
          
            return write;
        }

        public override Node VisitRead(ModelLParser.ReadContext context)
        {
           // Node statement = new StatementNode();
            //statement.Name = "statement";

            Node read = new ReadNode();
            read.Name = "read";

            //statement.Nodes.Add(read);

            foreach (var identificatorContext in context.identificator())
            {
                read.Nodes.Add(VisitIdentificator(identificatorContext));
            }
            //ProgrammAST.Nodes.Add(statement);
            return read;
        }

        public override Node VisitAssigment(ModelLParser.AssigmentContext context)
        {
            //Node statement = new StatementNode();
            //statement.Name = "statement";

            Node assigment = new AssigmentNode();
            assigment.Name = "assigment";

            assigment.Nodes.Add(VisitIdentificator(context.identificator()));
            assigment.Nodes.Add(VisitExpression(context.expression()));

            //ProgrammAST.Nodes.Add(statement);

            return assigment;
        }

        public override Node VisitFor(ModelLParser.ForContext context)
        {
            Node @for = new ForNode();
            @for.Name = "forCycle";

            @for.Nodes.Add(VisitAssigment(context.assigment()));
            @for.Nodes.Add(VisitExpression(context.expression()));
            @for.Nodes.Add(VisitStatement(context.statement()));

            return @for;
        }

        public override Node VisitWhile(ModelLParser.WhileContext context)
        {
            Node @while = new WhileNode();
            @while.Name = "whileCycle";

            @while.Nodes.Add(VisitExpression(context.expression()));
            @while.Nodes.Add(VisitStatement(context.statement()));

            return @while;
        }

        public override Node VisitIf(ModelLParser.IfContext context)
        {
            Node @if = new IfNode();
            @if.Name = "if";

            @if.Nodes.Add(VisitExpression(context.expression()));
            Node ifBranch = VisitStatement(context.statement()[0]);
            ifBranch.Name = "ifStatement";
            @if.Nodes.Add(ifBranch);
            if (context.ELSE() != null)
            {
                Node elseBranch = VisitStatement(context.statement()[1]);
                elseBranch.Name = "elseStatement";
                @if.Nodes.Add(elseBranch);
            }

            return @if;
        }

        public override Node VisitExpression(ModelLParser.ExpressionContext context)
        {
            Node expression = new ExpressionNode();
            expression.Name = "expression";

            if (context.primary() != null)
            {
                if (context.unary() != null)
                {
                    Node unary = new Node();
                    unary.Value = "not";
                    unary.Name = "not";
                    expression.Nodes.Add(unary);
                }
                expression.Nodes.Add(Visit(context.primary()));
                return expression;
            }
            Node operation = new Node();

            switch (context.op.Type)
            {
                case ModelLLexer.ADD:
                    operation.Value = "add";
                    operation.Name = "add";
                    break;
                case ModelLLexer.SUB:
                    operation.Value = "sub";
                    operation.Name = "sub";
                    break;
                case ModelLLexer.OR:
                    operation.Value = "or";
                    operation.Name = "or";
                    break;
                case ModelLLexer.MUL:
                    operation.Value = "mul";
                    operation.Name = "mul";
                    break;
                case ModelLLexer.DIV:
                    operation.Value = "div";
                    operation.Name = "div";
                    break;
                case ModelLLexer.AND:
                    operation.Value = "and";
                    operation.Name = "and";
                    break;
                case ModelLLexer.EQUAL:
                    operation.Value = "equal";
                    operation.Name = "equal";
                    break;
                case ModelLLexer.NEQ:
                    operation.Value = "not_equal";
                    operation.Name = "not_equal";
                    break;
                case ModelLLexer.MR:
                    operation.Value = "more";
                    operation.Name = "more";
                    break;
                case ModelLLexer.LS:
                    operation.Value = "less";
                    operation.Name = "less";
                    break;
                case ModelLLexer.MRE:
                    operation.Value = "more_equal";
                    operation.Name = "more_equal";
                    break;
                case ModelLLexer.LE:
                    operation.Value = "less_equal";
                    operation.Name = "less_equal";
                    break;
            }
            
            expression.Nodes.Add(operation);
            expression.Nodes.Add(Visit(context.left));
            expression.Nodes.Add(Visit(context.right));

            return expression;
        }

        public override Node VisitIdentificator(ModelLParser.IdentificatorContext context)
        {
            Node id = new IdentificatorNode();
            id.Value = context.GetText();
            id.Name = "identificator";
            return id;
        }

        public override Node VisitNumber(ModelLParser.NumberContext context)
        {
            Node number = new NumberNode();
            number.Name = "number";
            number.Nodes.Add(Visit(context.GetChild(0)));

            return number;
        }

        public override Node VisitInteger(ModelLParser.IntegerContext context)
        {
            Node integer = new IntegerNode();
            integer.Name = "integer";
            integer.Value = context.GetText();
            return integer;
        }

        public override Node VisitDouble(ModelLParser.DoubleContext context)
        {
            Node @double = new DoubleNode();
            @double.Name = "real";
            @double.Value = context.GetText();
            return @double;
        }

        public override Node VisitBoolean(ModelLParser.BooleanContext context)
        {
            Node boolean = new BooleanNode();
            boolean.Name = "boolean";
            boolean.Value = context.GetText();
            return boolean;
        }
    }
}
