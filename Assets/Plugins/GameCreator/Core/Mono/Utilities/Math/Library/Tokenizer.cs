namespace GameCreator.Core.Math
{
    using System;
    using System.IO;
    using System.Text;
    using System.Globalization;

    public class Tokenizer
    {
        public enum Token
        {
            EOF,
            Add,
            Subtract,
            Multiply,
            Divide,
            OpenParens,
            CloseParens,
            Number,
        }

        private static readonly CultureInfo CULTURE = new CultureInfo("en-US");
        private const char CHAR_EOF = '\0';
        private const char CHAR_PLUS = '+';
        private const char CHAR_MINUS = '-';
        private const char CHAR_MULT = '*';
        private const char CHAR_DIV = '/';
        private const char CHAR_S_PARENTHESIS = '(';
        private const char CHAR_E_PARENTHESIS = ')';
        private const char CHAR_DOT = '.';

        // PROPERTIES: ----------------------------------------------------------------------------

        private TextReader reader;

        public char currentCharacter { get; private set; }
        public Token currentToken { get; private set; }
        public float number { get; private set; }
        public string identifier { get; private set; }

        // INITIALIZER: ---------------------------------------------------------------------------

        public Tokenizer(string expression)
        {
            this.reader = new StringReader(expression);

            this.NextChar();
            this.NextToken();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        void NextChar()
        {
            int character = reader.Read();
            this.currentCharacter = character < 0 ? CHAR_EOF : (char)character;
        }

        public void NextToken()
        {
            while (char.IsWhiteSpace(this.currentCharacter)) this.NextChar();
            switch (this.currentCharacter)
            {
                case CHAR_EOF:
                    this.currentToken = Token.EOF;
                    return;

                case CHAR_PLUS:
                    this.NextChar();
                    this.currentToken = Token.Add;
                    return;

                case CHAR_MINUS:
                    this.NextChar();
                    this.currentToken = Token.Subtract;
                    return;

                case CHAR_MULT:
                    this.NextChar();
                    this.currentToken = Token.Multiply;
                    return;

                case CHAR_DIV:
                    this.NextChar();
                    this.currentToken = Token.Divide;
                    return;

                case CHAR_S_PARENTHESIS:
                    this.NextChar();
                    this.currentToken = Token.OpenParens;
                    return;

                case CHAR_E_PARENTHESIS:
                    this.NextChar();
                    this.currentToken = Token.CloseParens;
                    return;
            }

            if (char.IsDigit(this.currentCharacter) || this.currentCharacter == CHAR_DOT)
            {
                var sb = new StringBuilder();
                bool hasFloatingPoint = false;
                while (char.IsDigit(this.currentCharacter) || (!hasFloatingPoint && this.currentCharacter == CHAR_DOT))
                {
                    sb.Append(this.currentCharacter);
                    hasFloatingPoint = this.currentCharacter == CHAR_DOT;
                    this.NextChar();
                }

                this.number = float.Parse(sb.ToString(), CULTURE);
                this.currentToken = Token.Number;
                return;
            }

            throw new Exception(string.Format("Unexpected character: {0}", this.currentCharacter));
        }
    }
}