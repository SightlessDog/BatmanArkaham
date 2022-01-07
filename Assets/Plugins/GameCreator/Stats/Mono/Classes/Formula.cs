namespace GameCreator.Stats
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Globalization;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Variables;

    [Serializable]
    public class Formula
    {
        public class Operation
        {
            public Func<FormulaData, string, string> action;
            public Regex regex;

            public Operation(string pattern, Func<FormulaData, string, string> action)
            {
                this.regex = new Regex(pattern);
                this.action = action;
            }
        }

        public class FormulaData
        {
            public float baseValue;
            public Table table;

            public Stats origin;
            public Stats target;

            public FormulaData(float baseValue, Table table, Stats origin, Stats target)
            {
                this.baseValue = baseValue;
                this.table = table;
                this.origin = origin;
                this.target = target;
            }
        }

        private class Expression
        {
            public StringBuilder term;
            public char operation;
            public bool needsEvaluation;

            public Expression()
            {
                this.term = new StringBuilder();
                this.needsEvaluation = false;
            }

            public bool HasOp()
            {
                return this.operation != default(char);
            }
        }

        // DATA: ----------------------------------------------------------------------------------

        private static readonly Regex RX_IS_VAR_CHECK = new Regex(@"^[a-zA-Z0-9-\/]+$");
        private static readonly char[] ALLOWED_MATH_SYMBOLS = new char[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            '+', '-', '/', '*', '.', '(', ')'
        };

        private static readonly Regex RX_EXTRACT_NAME = new Regex(@"\b[^\[\]]+\[(.*)\]$");

        private static readonly Dictionary<char, char> DELIMITERS = new Dictionary<char, char>()
        {
            { ')', '(' },
            { ']', '[' },
        };

        private static readonly CultureInfo CULTURE = CultureInfo.InvariantCulture;

        public static Operation[] OPERATIONS = new Operation[]
        {
            new Operation(@"^this\[value\]", OnMatch_ThisBaseValue),       // this[value]
            new Operation(@"^stat\[\S+\]", OnMatch_StatName),              // stat[name]
            new Operation(@"^attr\[\S+\]", OnMatch_AttrName),              // attr[name]
            new Operation(@"^stat:other\[\S+\]", OnMatch_StatOtherName),   // stat:other[name]
            new Operation(@"^attr:other\[\S+\]", OnMatch_AttrOtherName),   // attr:other[name]
            new Operation(@"^local\[\S+\]", OnMatch_LocalName),            // local[name]
            new Operation(@"^local:other\[\S+\]", OnMatch_LocalOtherName), // local:other[name]
            new Operation(@"^global\[\S+\]", OnMatch_GlobalName),          // global[name]
            new Operation(@"^rand\[\S+,\S+\]", OnMatch_Rand),              // rand[min,max]
            new Operation(@"^dice\[\S+,\S+\]", OnMatch_Dice),              // dice[rolls,sides]
            new Operation(@"^chance\[\S+\]", OnMatch_Chance),              // chance[value]
            new Operation(@"^min\[\S+,\S+\]", OnMatch_Min),                // min[a,b]
            new Operation(@"^max\[\S+,\S+\]", OnMatch_Max),                // max[a,b]
            new Operation(@"^round\[\S+\]", OnMatch_Round),                // round[value]
            new Operation(@"^floor\[\S+\]", OnMatch_Floor),                // floor[value]
            new Operation(@"^ceil\[\S+\]", OnMatch_Ceil),                  // ceil[value]
            new Operation(@"^table:rise\[\S+\]", OnMatch_TableRise),       // table:rise[value]
            new Operation(@"^table\[\S+\]", OnMatch_Table),                // table[value]
        };

        private static string OnMatch_ThisBaseValue(FormulaData data, string clause)
        {
            return data.baseValue.ToString(CULTURE);
        }

        private static string OnMatch_StatName(FormulaData data, string clause)
        {
            string name = ClauseParseName(clause, data);
            return data.origin.GetStat(name, data.target).ToString(CULTURE);
        }

        private static string OnMatch_AttrName(FormulaData data, string clause)
        {
            string name = ClauseParseName(clause, data);
            return data.origin.GetAttrValue(name, data.target).ToString(CULTURE);
        }

        private static string OnMatch_StatOtherName(FormulaData data, string clause)
        {
            string name = ClauseParseName(clause, data);
            return data.target.GetStat(name, data.origin).ToString(CULTURE);
        }

        private static string OnMatch_AttrOtherName(FormulaData data, string clause)
        {
            string name = ClauseParseName(clause, data);
            return data.target.GetAttrValue(name, data.origin).ToString(CULTURE);
        }

        private static string OnMatch_LocalName(FormulaData data, string clause)
        {
            string name = ClauseParseName(clause, data);

            Variable.DataType localType = VariablesManager.GetLocalType(data.origin.gameObject, name, true);
            object localObject = VariablesManager.GetLocal(data.origin.gameObject, name, true);

            if (localType != Variable.DataType.Number) return localObject.ToString();
            return Convert.ToSingle(localObject).ToString(CULTURE);
        }

        private static string OnMatch_LocalOtherName(FormulaData data, string clause)
        {
            string name = ClauseParseName(clause, data);

            Variable.DataType localType = VariablesManager.GetLocalType(data.target.gameObject, name, true);
            object localObject = VariablesManager.GetLocal(data.target.gameObject, name, true);

            if (localType != Variable.DataType.Number) return localObject.ToString();
            return Convert.ToSingle(localObject).ToString(CULTURE);
        }

        private static string OnMatch_GlobalName(FormulaData data, string clause)
        {
            string name = ClauseParseName(clause, data);

            Variable.DataType globalType = VariablesManager.GetGlobalType(name, true);
            object globalObject = VariablesManager.GetGlobal(name);

            if (globalType != Variable.DataType.Number) return globalObject.ToString();
            return Convert.ToSingle(globalObject).ToString(CULTURE);
        }

        private static string OnMatch_Table(FormulaData data, string clause)
        {
            string value = ClauseParseInput(clause, data);
            return data.table.Tier(float.Parse(value)).ToString(CULTURE);
        }

        private static string OnMatch_TableRise(FormulaData data, string clause)
        {
            string value = ClauseParseInput(clause, data);
            return data.table.PercentNextTier(float.Parse(value)).ToString(CULTURE);
        }

        private static string OnMatch_Rand(FormulaData data, string clause)
        {
            string[] parse = ClauseParse2Inputs(clause, data);
            int min = int.Parse(parse[0]);
            int max = int.Parse(parse[1]);
            return UnityEngine.Random.Range(min, max).ToString(CULTURE);
        }

        private static string OnMatch_Dice(FormulaData data, string clause)
        {
            string[] parse = ClauseParse2Inputs(clause, data);

            int rolls = int.Parse(parse[0]);
            int sides = int.Parse(parse[1]);

            float amount = 0.0f;
            for (int i = 0; i < rolls; ++i)
            {
                amount += (float)UnityEngine.Random.Range(1, sides + 1);
            }

            return amount.ToString(CULTURE);
        }

        private static string OnMatch_Chance(FormulaData data, string clause)
        {
            string value = ClauseParseInput(clause, data);
            float chance = float.Parse(value);

            float percent = UnityEngine.Random.Range(0f, 1f);
            return (chance <= percent ? 1 : 0).ToString(CULTURE);
        }

        private static string OnMatch_Min(FormulaData data, string clause)
        {
            string[] parse = ClauseParse2Inputs(clause, data);

            int a = int.Parse(parse[0]);
            int b = int.Parse(parse[1]);
            return Mathf.Min(a, b).ToString(CULTURE);
        }

        private static string OnMatch_Max(FormulaData data, string clause)
        {
            string[] parse = ClauseParse2Inputs(clause, data);
            int a = int.Parse(parse[0]);
            int b = int.Parse(parse[1]);
            return Mathf.Max(a, b).ToString(CULTURE);
        }

        private static string OnMatch_Round(FormulaData data, string clause)
        {
            string input = ClauseParseInput(clause, data);

            float value = Mathf.Round(float.Parse(input));
            return value.ToString(CULTURE);
        }

        private static string OnMatch_Floor(FormulaData data, string clause)
        {
            string input = ClauseParseInput(clause, data);
            float value = Mathf.Floor(float.Parse(input));
            return value.ToString(CULTURE);
        }

        private static string OnMatch_Ceil(FormulaData data, string clause)
        {
            string input = ClauseParseInput(clause, data);
            float value = Mathf.Ceil(float.Parse(input));
            return value.ToString(CULTURE);
        }

        // CLAUSES: -------------------------------------------------------------------------------

        private static string ClauseParseName(string clause, FormulaData data)
        {
            StringBuilder value = new StringBuilder();

            Match match = RX_EXTRACT_NAME.Match(clause);
            if (match.Success && match.Groups.Count == 2)
            {
                value = new StringBuilder(match.Groups[1].Value);
            }

            bool isVariableName = RX_IS_VAR_CHECK.IsMatch(value.ToString());

            if (!isVariableName) value = Formula.ParseFormula(value, data);
            return value.ToString();
        }

        private static string ClauseParseInput(string clause, FormulaData data)
        {
            StringBuilder value = new StringBuilder();

            Match match = RX_EXTRACT_NAME.Match(clause);
            if (match.Success && match.Groups.Count == 2)
            {
                value = new StringBuilder(match.Groups[1].Value);
            }

            value = Formula.ParseFormula(value, data);
            return ExpressionEvaluator.Evaluate(value.ToString()).ToString();
        }

        private static string[] ClauseParse2Inputs(string clause, FormulaData data)
        {
            StringBuilder value1 = new StringBuilder();
            StringBuilder value2 = new StringBuilder();

            Match match = RX_EXTRACT_NAME.Match(clause);

            if (match.Success && match.Groups.Count == 2)
            {
                List<StringBuilder> parameters = ExtractParameters(match.Groups[1].Value);
                if (parameters.Count == 2)
                {
                    value1 = parameters[0];
                    value2 = parameters[1];
                }
            }

            value1 = Formula.ParseFormula(value1, data);
            value2 = Formula.ParseFormula(value2, data);

            return new string[]
            {
                ExpressionEvaluator.Evaluate(value1.ToString()).ToString(),
                ExpressionEvaluator.Evaluate(value2.ToString()).ToString()
            };
        }

        private static List<StringBuilder> ExtractParameters(string content)
        {
            int parametersIndex = 0;
            List<StringBuilder> parameters = new List<StringBuilder>()
            {
                new StringBuilder()
            };

            Stack<char> delimiters = new Stack<char>();

            for (int i = 0; i < content.Length; ++i)
            {
                char character = content[i];
                switch (character)
                {
                    case ',':
                        if (delimiters.Count == 0)
                        {
                            parameters.Add(new StringBuilder());
                            parametersIndex += 1;
                        }
                        else
                        {
                            parameters[parametersIndex].Append(character);
                        }
                        break;

                    case '(':
                    case '[':
                        delimiters.Push(character);
                        parameters[parametersIndex].Append(character);
                        break;

                    case ')':
                    case ']':
                        if (delimiters.Count > 0 && delimiters.Peek() == DELIMITERS[content[i]]) delimiters.Pop();
                        parameters[parametersIndex].Append(character);
                        break;

                    default:
                        parameters[parametersIndex].Append(character);
                        break;
                }
            }

            return parameters;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        private const int MAX_ITERATIONS = 500;
        private static int NUM_ITERATIONS;

        public string formula = "this[value]";
        public Table table = new Table();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public float Calculate(float value, Stats origin, Stats target)
        {
            NUM_ITERATIONS = 0;

            string stringFormula = this.formula.Replace(" ", string.Empty);
            if (string.IsNullOrEmpty(stringFormula)) return 0f;

            FormulaData data = new FormulaData(value, this.table, origin, target);
            StringBuilder sbFormula = new StringBuilder(stringFormula);

            sbFormula = Formula.ParseFormula(sbFormula, data);
            return ExpressionEvaluator.Evaluate(sbFormula.ToString());
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static StringBuilder ParseFormula(StringBuilder sbFormula, FormulaData data)
        {
            if (NUM_ITERATIONS++ > MAX_ITERATIONS)
            {
                return new StringBuilder("-1");
            }

            if (sbFormula.Length == 0) return sbFormula;

            List<Expression> expressions = SplitClauses(sbFormula);
            sbFormula.Remove(0, sbFormula.Length);

            for (int i = 0; i < expressions.Count; ++i)
            {
                StringBuilder sbTerm = expressions[i].term;
                string term = sbTerm.ToString();

                string mathExpression = IsMathExpression(term);
                if (mathExpression != string.Empty)
                {
                    sbFormula.Append('(').Append(mathExpression).Append(')');
                    if (expressions[i].HasOp())
                    {
                        sbFormula.Append(expressions[i].operation);
                    }

                    continue;
                }

                if (expressions[i].needsEvaluation && expressions.Count > 1)
                {
                    StringBuilder result = ParseFormula(sbTerm, data);
                    sbFormula.Append('(').Append(result).Append(')');

                    if (expressions[i].HasOp())
                    {
                        sbFormula.Append(expressions[i].operation);
                    }

                    continue;
                }

                int operationsLength = OPERATIONS.Length;
                for (int j = 0; j < operationsLength; ++j)
                {
                    Match match = OPERATIONS[j].regex.Match(term);
                    if (match.Success)
                    {
                        string result = OPERATIONS[j].action.Invoke(data, match.Value);
                        sbFormula.Append(result);

                        if (expressions[i].HasOp())
                        {
                            sbFormula.Append(expressions[i].operation);
                        }
                        break;
                    }
                }
            }

            return sbFormula;
        }

        private static string IsMathExpression(string content)
        {
            bool atLeastNumber = false;
            int contentLength = content.Length;

            for (int i = 0; i < contentLength; ++i)
            {
                if (char.IsDigit(content[i])) atLeastNumber = true;
                if (!ALLOWED_MATH_SYMBOLS.Contains(content[i])) return string.Empty;
            }

            return atLeastNumber ? content : string.Empty;
        }

        private static List<Expression> SplitClauses(StringBuilder content)
        {
            List<Expression> expressions = new List<Expression>();
            expressions.Add(new Expression());
            int expressionIndex = 0;

            Stack<char> delimiters = new Stack<char>();

            while (content.Length > 1 && content[0] == '(' && content[content.Length - 1] == ')')
            {
                content.Remove(0, 1);
                content.Remove(content.Length - 1, 1);
            }

            if (content.Length > 1 && content[0] == '-')
            {
                content.Remove(0, 1);
                content.Insert(0, "0-");
            }

            for (int i = 0; i < content.Length; ++i)
            {
                switch (content[i])
                {
                    case '[':
                    case '(':
                        delimiters.Push(content[i]);
                        expressions[expressionIndex].term.Append(content[i]);
                        break;

                    case ']':
                    case ')':
                        if (delimiters.Count > 0 && delimiters.Peek() == DELIMITERS[content[i]]) delimiters.Pop();
                        expressions[expressionIndex].term.Append(content[i]);
                        break;

                    case '+':
                    case '-':
                    case '/':
                    case '*':
                        if (delimiters.Count == 0)
                        {
                            expressions[expressionIndex].operation = content[i];
                            expressions.Add(new Expression());
                            expressionIndex += 1;
                        }
                        else
                        {
                            expressions[expressionIndex].term.Append(content[i]);
                            expressions[expressionIndex].needsEvaluation = true;
                        }
                        break;

                    default:
                        expressions[expressionIndex].term.Append(content[i]);
                        break;
                }
            }

            foreach (Expression expression in expressions)
            {
                StringBuilder term = expression.term;
                while (term.Length > 1 && term[0] == '(' && term[term.Length - 1] == ')')
                {
                    term.Remove(0, 1);
                    term.Remove(term.Length - 1, 1);
                }
            }

            return expressions;
        }
    }
}
 