namespace GameCreator.Core.Math
{
    using System;

    public class Parser
    {
        private readonly Tokenizer tokenizer;

        // INITIALIZER: ---------------------------------------------------------------------------

        public Parser(string expression)
        {
            this.tokenizer = new Tokenizer(expression);
        }

        public float Evaluate()
        {
            Node node = this.ParseExpression();
            if (node == null) return 0;
            return node.Evaluate();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private Node ParseExpression()
        {
            Node node = this.ParseAddSubtract();
            if (this.tokenizer.currentToken != Tokenizer.Token.EOF)
            {
                throw new Exception("Unexpected characters at end of expression");
            }

            return node;
        }

        private Node ParseAddSubtract()
        {
            Node lhs = ParseMultiplyDivide();
            while (true)
            {
                Func<float, float, float> op = null;
                if (this.tokenizer.currentToken == Tokenizer.Token.Add)
                {
                    op = (a, b) => a + b;
                }
                else if (this.tokenizer.currentToken == Tokenizer.Token.Subtract)
                {
                    op = (a, b) => a - b;
                }

                if (op == null) return lhs;
                this.tokenizer.NextToken();

                Node rhs = ParseMultiplyDivide();
                lhs = new NodeBinary(lhs, rhs, op);
            }
        }

        private Node ParseMultiplyDivide()
        {
            Node lhs = ParseUnary();
            while (true)
            {
                Func<float, float, float> op = null;
                if (this.tokenizer.currentToken == Tokenizer.Token.Multiply)
                {
                    op = (a, b) => a * b;
                }
                else if (this.tokenizer.currentToken == Tokenizer.Token.Divide)
                {
                    op = (a, b) => a / b;
                }

                if (op == null) return lhs;

                this.tokenizer.NextToken();
                Node rhs = this.ParseUnary();
                lhs = new NodeBinary(lhs, rhs, op);
            }
        }

        private Node ParseUnary()
        {
            if (this.tokenizer.currentToken == Tokenizer.Token.Add)
            {
                this.tokenizer.NextToken();
                return ParseUnary();
            }

            if (this.tokenizer.currentToken == Tokenizer.Token.Subtract)
            {
                this.tokenizer.NextToken();
                var rhs = ParseUnary();
                return new NodeUnary(rhs, (a) => -a);
            }

            return this.ParseLeaf();
        }

        private Node ParseLeaf()
        {
            if (this.tokenizer.currentToken == Tokenizer.Token.Number)
            {
                Node node = new NodeNumber(this.tokenizer.number);
                this.tokenizer.NextToken();
                return node;
            }

            if (this.tokenizer.currentToken == Tokenizer.Token.OpenParens)
            {
                this.tokenizer.NextToken();
                Node node = this.ParseAddSubtract();

                if (this.tokenizer.currentToken != Tokenizer.Token.CloseParens)
                {
                    throw new Exception("Missing close parenthesis");
                }

                this.tokenizer.NextToken();
                return node;
            }

            throw new Exception("Unexpected token: " + this.tokenizer.currentToken);
        }
    }
}