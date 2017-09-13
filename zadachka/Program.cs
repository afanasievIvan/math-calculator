using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zadachka
{
    using FuncMap = Dictionary<string, Func<double, double>>;
    using VarMap = Dictionary<string, double>;
    using DerivativeMap = Dictionary<string, string>;


    class Program
    {        

        public static double getNumber(string st)
        {
            double number = 0;
            if (st == null)
            {
                return number;
            }
            else
              if (st.Contains("."))
            {
                for (int i = 0; i < st.IndexOf('.'); i++)
                {
                    number += Math.Pow(10, (st.IndexOf('.') - i - 1)) * Convert.ToInt32(st[i] - 48);
                }
                for (int i = st.IndexOf('.')+1; i < st.Length; i++)
                {
                    number += Math.Pow(10, (st.IndexOf('.') - i)) * Convert.ToInt32(st[i] - 48);
                }
            }
            else
            {
                for (int i1 = 0; i1 < st.Length; i1++)
                {
                    number += Math.Pow(10, (st.Length - i1 - 1)) * Convert.ToInt32(st[i1] - 48);
                }
            }
            return number;
        }


        class Operation
        {
            public Operation left = null;
            public Operation right = null;
            public Operation varOperation = null;
            public double value = 0;
            public char sign;
            public string variable;
            public MathParser parser;
            public Func<double, double> searchFunction;
            public bool Derivative = false;

            public Operation() { }

            public bool hasLeft { get { return left != null; } }
            public bool hasRight { get { return right != null; } }

            public void copy(Operation original)
            {
                if (original.left != null)
                {
                    left = new Operation();
                    left.copy(original.left);
                }
                if (original.right != null)
                {
                    right = new Operation();
                    right.copy(original.right);
                }
                Derivative = original.Derivative;
                value = original.value;
                sign = original.sign;
                parser = original.parser;
                searchFunction = original.searchFunction;
                variable = original.variable;
            }


            public void tryMakeSimpleConstOrVar()
            {
                if (left != null && right != null)
                {
                    if (left.variable == null && right.variable == null && left.right == null 
                        && right.right == null)
                    {
                        value = calculate();
                        left = null;
                        right = null;
                        sign = ' ';
                        searchFunction = null;
                        makeSimple();
                    }
                    else if (left.value == 0 && left.right == null && left.variable == null)
                    {
                        if (sign == '+')
                        {
                            var buffOp = right.copy();
                            left = null;
                            right = null;
                            copy(buffOp);
                            makeSimple();
                        }
                        else if (sign == '-')
                        {
                            var buffOp = right.copy();
                            left = null;
                            right = null;
                            buffOp.changeSign();
                            copy(buffOp);
                            makeSimple();
                        }
                        else if (sign == '*')
                        {
                            value = calculate();
                            left = null;
                            right = null;
                            sign = ' ';
                            makeSimple();
                        }                           
                    }
                    else if (right.value == 0 && right.right == null && right.variable == null)
                    { 
                        if (sign == '+' || sign == '-')
                        {
                            var buffOp = left.copy();
                            left = null;
                            right = null;
                            copy(buffOp);
                            makeSimple();
                        }
                        else if (sign == '*')
                        {
                            value = calculate();
                            left = null;
                            right = null;
                            sign = ' ';
                            makeSimple();
                        }
                    } 
                } 
                else if (searchFunction != null)
                {
                    if (right.variable == null && right.right == null)
                    {
                        value = calculate();
                        right = null;
                        searchFunction = null;
                        makeSimple();
                    }
                }
                if (left != null)
                {
                    left.tryMakeSimpleConstOrVar();
                }
                if (right != null)
                {
                    right.tryMakeSimpleConstOrVar();
                }              
            }

            public void cleanOperation()
            {
                searchFunction = null;
                value = 0;
                variable = null;
                sign = ' ';
            }

            public void exchangeOperations(ref Operation op1, ref Operation op2)
            {
                Operation bufOp = op1.copy();
                op1 = op2.copy();
                op2 = bufOp.copy();
            }
            public void exchangeSigns(ref Operation op1, ref Operation op2)
            {
                char bufSign = op1.sign;
                op1.sign = op2.sign;
                op2.sign = bufSign;
            }

            public void exchangeFunc(ref Operation op1, ref Operation op2)
            {
                var bufFunc = op1.searchFunction;
                op2.searchFunction = op1.searchFunction;
                op1.searchFunction = bufFunc;
            }

            public void changeSign()
            {
                if (sign == '+')
                {
                    sign = '-';
                }
                else if (sign == '-')
                {
                    sign = '+';
                }
                if (left != null)
                {
                    left.changeSign();
                }
                    else if (searchFunction != null)
                    {
                        left = new Operation();
                        right = new Operation();
                        sign = '-';
                        right.searchFunction = searchFunction;
                        searchFunction = null;
                    }
                    else if (value != 0)
                    {
                        value = -value;
                    }
                    else if (variable != null)
                    {
                        left = new Operation();
                        right = new Operation();
                        sign = '-';
                        right.variable = variable;
                        variable = null;
                    }
            }

            public void multiply(double multiplier)
            {
                sign = '*';
                left = new Operation();
                left.value = multiplier;
            }

            public void multiplication(double multiplier)
            {
                if (left != null)
                {
                   left.multiplication(multiplier);
                }
                else if (searchFunction != null)
                {
                    var buffOp = right.copy();
                    multiply(multiplier);
                    right.searchFunction = searchFunction;
                    searchFunction = null;
                    if (right.right == null)
                    {
                        right.right = new Operation();
                    }
                    right.right = buffOp.copy();
                }
                else if (variable != null)
                {
                    multiply(multiplier);
                    right = new Operation();
                    right.variable = variable;
                    variable = null;
                }
                else
                {
                    multiply(multiplier);
                    right = new Operation();
                    right.value = value;
                    value = 0;
                }
                if (right != null && sign != '*')
                {
                    right.multiplication(multiplier);
                }
            }

            public void checkMultiplier()
            {
                if (sign == '*')
                {
                    if (left.value == 0 && left.variable == null && left.right == null)
                    {
                        left = null;
                        right = null;
                        sign = ' ';
                        makeSimple();
                    }
                    else if (left.value == 1)
                    {
                        left.value = 0;
                        sign = '+';
                        makeSimple();
                    }
                    else if (left.right == null && left.variable == null
                    && right.right != null)
                    {
                        right.multiplication(left.value);
                        left.value = 0;
                        sign = '+';
                        makeSimple();
                    }
                    else if (right.value == 0 && right.variable == null && left.right == null)
                    {
                        left = null;
                        right = null;
                        sign = ' ';
                        makeSimple();
                    }
                    else if (right.value == 1)
                    {
                        right.value = 0;
                        sign = '+';
                        makeSimple();
                    }
                    else if (right.right == null && right.variable == null
                       && left.right != null)
                    {
                        left.multiplication(right.value);
                        makeSimple();
                    }
                }

                    if (left != null)
                    {
                        left.checkMultiplier();
                    }
                    if (right != null)
                    {
                        right.checkMultiplier();
                    }
                }

        /*    public void makeSimpleVar1(ref Operation op)
            {
                if (rightOperation.variable == leftOperation.op.variable && rightOperation.variable != null && )
                {

                }
            }
            */

            public bool isSameVar(Operation op1, Operation op2)
            {
                return op1.variable == op2.variable && op1.variable != "";
            }

            public void checkCommonMultiplier(Operation op)
            {
                if (!op.hasLeft || !op.hasRight)
                {
                    return;
                }
                if (op.left.left == null || op.left.right == null)
                {
                    return;
                }
                if (op.right.left == null || op.right.right == null)
                {
                    return;
                }

                var variable = "";
                if (isSameVar(op.left.left, op.right.left))
                {
                    variable = op.left.left.variable;
                }
                if (isSameVar(op.left.left, op.right.right))
                {
                    variable = op.left.left.variable;
                }
                if (isSameVar(op.right.left, op.right.left))
                {
                    variable = op.left.left.variable;
                }
                if (isSameVar(op.right.left, op.right.right))
                {
                    variable = op.left.left.variable;
                }

                var newRight = new Operation();
                var list = new List<Operation>();
                if (op.left.left.variable != variable)
                {
                    list.Add(op.left.left);
                }
                if (op.left.right.variable != variable)
                {
                    list.Add(op.left.left);
                }
                if (op.right.left.variable != variable)
                {
                    list.Add(op.left.left);
                }
                if (op.right.right.variable != variable)
                {
                    list.Add(op.left.left);
                }
                if (list.Count == 2)
                {
                    newRight.left = list[0];
                    newRight.right = list[1];
                    newRight.sign = op.sign;
                    op.right = newRight;
                    op.sign = '*';
                    op.left = new Operation();
                    op.left.variable = variable;
                }
            }

            public void simplifyVaribles()
            {
                if (right != null && left != null)
                {
                    if (right.variable == left.variable && right.variable != null)
                    {
                        if (sign == '+')
                        {
                            sign = '*';
                            left.value = 2;
                            left.variable = null;
                        }
                        if (sign == '-')
                        {
                            left = null;
                            right = null;
                            sign = ' ';
                        }
                        if (sign == '/')
                        {
                            left = null;
                            right = null;
                            value = 1;
                            sign = ' ';
                        }
                        makeSimple();
                    }
                    else if (left.right != null &&
                        right.variable == left.right.variable && right.variable != null &&
                        left.sign == '*' && sign == '+' && left.left.value != 0)
                    {
                        left.value = left.left.value + 1;
                        left.left = null;
                        left.right = null;
                        sign = '*';
                        makeSimple();
                    }
                    else if (left.sign == right.sign && left.sign == '*' && (sign == '+' || sign == '-'))
                    {
                        if (right.left != null && left.left != null &&
                            right.right.variable == left.right.variable &&
                            right.right.variable != null)
                        {
                            left.right.value = right.left.value;
                            left.right.variable = null;
                            left.sign = sign;
                            sign = '*';
                            right.variable = right.right.variable;
                            right.right = null;
                            right.left = null;
                            makeSimple();
                        }
                        else if (right.left != null && left.left != null &&
                            right.left.variable == left.left.variable &&
                            right.left.variable != null)
                        {
                            left.left.value = right.right.value;
                            left.left.variable = null;
                            left.sign = sign;
                            sign = '*';
                            right.variable = right.left.variable;
                            right.right = null;
                            right.left = null;
                            makeSimple();
                        }
                        else if (right.left != null && left.left != null &&
                            right.right.variable == left.left.variable &&
                            right.right.variable != null)
                        {
                            left.left.value = right.left.value;
                            left.left.variable = null;
                            left.sign = sign;
                            sign = '*';
                            right.variable = right.right.variable;
                            right.right = null;
                            right.left = null;
                            makeSimple();
                        }
                        else if (right.left != null && left.left != null &&
                            right.left.variable == left.right.variable &&
                            right.left.variable != null)
                        {
                            left.right.value = right.right.value;
                            left.right.variable = null;
                            left.sign = sign;
                            sign = '*';
                            right.variable = right.left.variable;
                            right.right = null;
                            right.left = null;
                            makeSimple();
                        }
                    }
                }
                if (left != null)
                {
                    left.simplifyVaribles();
                }
                if (right != null)
                {
                    right.simplifyVaribles();
                }
            }

            

            public bool exchange()
            {
                char bufSign;
                tryMakeSimpleConstOrVar();
                if (left != null)
                {
                    if (left.right != null && right != null 
                        && left.right.variable != null && right.variable == null)
                    {
                        if (checkSign(this, left) == true)
                        {
                            bufSign = sign;
                            sign = left.sign;
                            left.sign = bufSign;
                            exchangeOperations(ref left.right, ref right);
                            return true;
                        }
                    }

                    else if (left.left != null && right != null 
                        && left.left.variable != null && right.variable == null)
                    {
                        if (checkSign(this, left) == true)
                        {
                            //  exchangeSigns(this, leftOperation);
                            bufSign = sign;
                            sign = left.sign;
                            left.sign = bufSign;
                            exchangeOperations(ref left.left, ref right);
                            return true;
                        }
                    }
                    else
                    {
                        return left.exchange();
                    }
                }
                return false;
            }

            public void makeSimple()
            {
                tryGetDiff();
                if (sign == '-' && right != null && right.right != null)
                {
                    sign = '+';
                    right.changeSign();
                    exchangeOperations(ref right, ref left);
                }
                if (exchange())
                {
                    makeSimple();
                }
                checkMultiplier();
              //  simplifyVaribles();
            }


            public void diffSUm()
            {
                left.diff();
                right.diff();
            }

            public void tryFoundVar(Operation buf)
            {
                if (right != null)
                {
                    if (right.variable != null)
                    {
                        right.variable = null;
                        right = buf.copy();
                    }
                    else
                    {
                        right.tryFoundVar(buf);
                    }
                }
                if (left != null)
                {
                    if (left.variable != null)
                    {
                        left.variable = null;
                        left = buf.copy();
                    }
                    else
                    {
                        left.tryFoundVar(buf);
                    }
                }
            }

            public void diffComplexFunc()
            {
                var buffOp = new Operation();
                buffOp = right.copy();
                right = null;
                right = new Operation(parser, parser.dMap[searchKey(searchFunction)]);
                right.tryFoundVar(buffOp);
                left = buffOp.copy();
                left.diff();
                    cleanOperation();
                    sign = '*';
            }

            public void diffMultorDividing()
            {
                var buffOp1 = new Operation();
                buffOp1 = left.copy();
                var buffOp2 = right.copy();
                    if (left.right == null)
                    {
                        left.right = new Operation();
                        left.left = new Operation();
                    }
                left.left = buffOp1.copy();
                left.right = buffOp2.copy();
                left.left.diff();                    
                    if (right.right == null)
                    {
                        right.right = new Operation();
                        right.left = new Operation();
                    }

                right.left = buffOp1.copy();
                right.right = buffOp2.copy();
                right.right.diff();
                left.cleanOperation();
                right.cleanOperation();
                left.sign = '*';
                right.sign = '*';
            }


           public void diffDividing()
            {
                if (sign == '/' || sign == ':')
                {
                    var buffOp1 = left.copy();
                    var buffOp2 = right.copy();
                    if (left.right == null)
                    {
                        left.right = new Operation();
                        left.left = new Operation();
                    }
                    left.left = buffOp1.copy();
                    left.right = buffOp2.copy();
                    left.diffMultorDividing();
                    left.cleanOperation();
                    left.sign = '-';
                    if (right.right == null)
                    {
                        right.right = new Operation();
                        right.left = new Operation();
                    }
                    right.right = buffOp2.copy();
                    right.left = right.right.copy();
                    right.cleanOperation();
                    right.sign = '*';
                } 
            }

            public void diff()
            {
                if (sign == '-' || sign == '+')
                {
                    diffSUm();
                }
                else if (sign == '*')
                {
                    diffMultorDividing();
                    sign = '+';
                }
                else if (sign == '/' || sign == ':')
                {
                    diffDividing();
                }
                else if (searchFunction != null)
                {
                    diffComplexFunc();
                }
                else if (variable != null)
                {
                    variable = null;
                    value = 1;
                }
                else
                {
                    value = 0;
                }
            }

            public void tryGetDiff()
            {
                if (Derivative)
                {
                    right.diff();
                    left = new Operation();
                    Derivative = false;
                    sign = '+';

                }
                else
                {
                    if (left != null)
                    {
                        left.tryGetDiff();
                    }
                    if (right != null)
                    {
                        right.tryGetDiff();
                    }
                }
            }
                

 
       

            public bool checkSign(Operation Op1, Operation Op2)
            {
                if ((Op1.sign == '+' && Op2.sign == '-') || (Op1.sign == '-' && Op2.sign == '+') || (Op1.sign == '*' && Op2.sign == '/')
                    || (Op1.sign == '/' && Op2.sign == '*') || (Op1.sign == Op2.sign))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public Operation copy()
            {
                var result = new Operation();
                if (left != null)
                {
                    result.left = left.copy();
                }
                if (right != null)
                {
                    result.right = right.copy();
                }
                result.Derivative = Derivative;
                result.sign = sign;
                result.variable = variable;
                result.value = value;
                result.searchFunction = searchFunction;
                result.parser = parser;
                return result;
            }

       /*     public void makeSimple(Operation op)
            {
                if 
            }
            */

            public Operation(MathParser mathParser, string expression)
            {
                parser = mathParser;
                if (expression == "")
                {
                    return;
                }
                else
                    parse(expression);
            }

            public double calculate()
            {
                double l = 0;
                Func<double, double> fun = null;
                if (left != null)
                {
                    l = left.calculate();
                }
                else if (searchFunction != null)
                {
                    fun = searchFunction;
                }
                else if (variable != null)
                {
                    return parser.varMap[variable];
                }
                else
                {
                    return value;
                }
                double r = 0;
                if (right != null)
                {
                    r = right.calculate();
                }
                else if (variable != null)
                {
                    return parser.varMap[variable];
                }
                else
                {
                    return value;
                }
                if (sign == '+')
                {
                    return l + r;
                }
                else if (sign == '-')
                {
                    return l - r;
                }
                else if (sign == '*')
                {
                    return l * r;
                }
                else if (sign == '/' || sign == ':')
                {
                    return l / r;
                }
                else
                {
                    return fun(r);
                }
            }

            public string assembly()
            {
                string r = "";
                string l = "";
                if (left != null)
                {
                    l += left.assembly();
                }
                
                else if (searchFunction != null)
                {
                    l += searchKey(searchFunction);
                }
                
                else if (variable != null)
                {
                    return variable;
                }
                else
                {
                    return Convert.ToString(value);
                }
                if (right != null)
                {
                    r += right.assembly();
                }
                else if (variable != null)
                {
                    return variable;
                }
                else
                {
                    return Convert.ToString(value);
                }
                if (sign == '+' || sign == '-')
                {
                    if (r.Contains('-') && sign == '-')
                    {
                        r = "(" + r + "}";
                    }
                    return l + sign + r;
                }
                else if (sign == '*' || sign == '/' || sign == ':')
                {
                    if (signOutsideBrackets(l , "+-") == true)
                    {
                        l = "(" + l + ")"; 
                    }
                    if (signOutsideBrackets(r, "+-") == true)
                    {
                        r = "(" + r + ")";
                    }
                    return l + sign + r;
                }
                else
                {
                    return l + "(" + r + ")";
                }
            }


            public void createNewOperation(string s)
            {
                if (right != null)
                {
                    right.createNewOperation(s);
                }
                else
                {
                    right = new Operation(parser, s);
                }
            }

            public bool signOutsideBrackets(string s, string signs)
            {
                    int k = 0;
                    for (int i = 0; i < s.Length; i++)
                    {
                        if ((signs.Contains(s[i])) && k == 0)
                        {
                            return true;
                        }
                        else if (s[i] == '(')
                        {
                            k++;
                        }
                        else if (s[i] == ')')
                        {
                            k--;
                        }
                    }
                return false;
            }

            public void separation(string exp, int i)
            {
                if ((String.IsNullOrEmpty(exp.Substring(0, i))) == true)
                {
                    sign = exp[i];
                    left = new Operation(parser, "");
                    right = new Operation(parser, exp.Substring(i + 1));
                }
                else
                {
                    sign = exp[i];
                    left = new Operation(parser, exp.Substring(0, i));
                    right = new Operation(parser, exp.Substring(i + 1));
                }
            }

            public bool parseOperators(string expression, string operators)
            {
                int k = 0;
                int NOp = -1;
                bool flag = false;
                for (int i = 0; i < expression.Length; i++)
                {
                    if ((operators.Contains(expression[i])) && k == 0)
                    {
                        NOp = i;
                    }
                    else if (expression[i] == '(')
                    {
                        k++;
                    }
                    else if (expression[i] == ')')
                    {
                        k--;
                        if (k == 0 && (i + 1) != expression.Length)
                        {
                            flag = true;
                        }
                        else if (k == 0 && i + 1 == expression.Length && flag == false && expression[0] == '(')
                        {
                            parse(expression.Substring(1, i - 1));
                            return true;
                        }
                    }
                }
                if (NOp >= 0)
                {
                    separation(expression, NOp);
                    return true;
                }
                else
                return false;
            }

 /*           public double operate(double a, double b, Func<double, double, double> fun) {
                return fun(a, b);
            }

            public double add(double a, double b)
            {
                return operate(a, b, (v1, v2) => v1 + v2);
            }
 */
            public bool parseFunctions(string expression)
            {
                int k = 0;
                expression = expression.Trim(' ');
                string func = "";
                bool flag = false;
                if (expression[0] == 'D')
                {
                    Derivative = true;
                    right = new Operation(parser, expression.Substring(1));
                    return true;
                }
                for (int i = 0; i < expression.Length; i++)
                {
                    if (Char.IsLetter(expression[i]) == true)
                    {
                        func += expression[i];
                        k = i;
                        flag = true;
                    }
                    else
                    {
                        break;
                    }
                }
                if (parser.funcMap.TryGetValue(func, out searchFunction) == true)
                {
                    right = new Operation(parser, expression.Substring(k + 1));
                    return true;
                }
                else if (flag == true && parser.varMap.ContainsKey(func) == false)
                {
                    variable = func;
                    parser.varMap.Add(func, 0);
                    return true;
                }
                else if (flag == true && parser.varMap.ContainsKey(func) == true)
                {
                    variable = func;
                    return true;
                }
                else return false;
            }

            public void parse(string expression)
            {
                expression = expression.Trim(' ');
                if (parseOperators(expression, "+-") == false)
                {
                    if (parseOperators(expression, "*/") == false)
                    {
                        if (parseFunctions(expression) == false)
                        {
                            value = getNumber(expression.Trim(' '));
                        }
                    }
                }             
            }


            public string searchKey(Func<double, double> searchFun)
            {
                foreach(var Row in parser.funcMap)
                {
                    if (Row.Value == searchFun)
                    {
                        return Row.Key;
                    }
                }

            /*    var keys = new List<string>(parser.funcMap.Keys);
                foreach (string key in keys)
                {
                    if (parser.funcMap[key] == searchFun)
                    {
                        return key;
                    }
                }
                */
                return "";
            }
        }



        class MathParser
        {
            public Operation operation;
            public FuncMap funcMap = new FuncMap();
            public VarMap varMap = new VarMap();
            public DerivativeMap dMap = new DerivativeMap();
            public MathParser(string expression)
            {
                funcMap.Add("sin", a => Math.Sin(a * Math.PI / 180));                
                funcMap.Add("cos", a => Math.Cos(a * Math.PI / 180));
                funcMap.Add("ln", a => Math.Log(a));
                funcMap.Add("arcsin", a => Math.Asin(a));
                funcMap.Add("arccos", a => Math.Acos(a));
                funcMap.Add("Sqrt", a => Math.Sqrt(a));
                dMap.Add("cos", "-sin(x)");
                dMap.Add("sin", "cos(x)");
                dMap.Add("ln", "1/x");
                dMap.Add("arcsin",  "1 /Sqrt(1-x*x)");
                dMap.Add("arccos", "-1/Sqrt(1-x*x)");
                operation = new Operation(this, expression);
            }

            public MathParser() { }

            public MathParser copy()
            {
                var result = new MathParser();
                result.funcMap = funcMap;
                result.varMap = varMap;
                result.dMap = dMap;
                result.operation = operation.copy();
                return result;
            }

            public void makeSimple()
            {
                 operation.makeSimple();
            }

            public double calculate()
            {
                return operation.calculate();
            }

            public string assembly()
            {
                return operation.assembly();
            }
        }


        static void Main(string[] args)
        {
           /* var Map = new FuncMap();
            Map.Add("sin", (double a) =>
            {
                return Math.Sin(a);
            });
            Map.Add("cos", a => Math.Cos(a));
            Map.Add("ln", a => Math.Log(a));
            Map.Add("arcsin", a => Math.Asin(a));
            Map.Add("arccos", a => Math.Acos(a));
            Func<double, double> searchFunction;
            if (Map.TryGetValue("sin", out searchFunction) == true)
            {
                var k = searchFunction(0.5);
                Console.WriteLine(k);
            }
            */
            // test();
            Console.WriteLine("enter a mathematical expression");
            string s = Console.ReadLine();
            var parser = new MathParser(s);
            var parser1 = parser.copy();
            parser1.makeSimple();
            Console.WriteLine(parser1.assembly());
            // parser.varMap.Add("x", 0);
            //   parser.varMap.Add("y", 0);

            // заполняем переменные
            var Variables = new List<string>(parser1.varMap.Keys);
            foreach (var variable in Variables)
            {
                Console.WriteLine("enter variable " + variable);
                var Value = Convert.ToInt32(Console.ReadLine());
                parser1.varMap[variable] = Value;
            }

            var result = parser1.calculate();
            //var EXPR = parser.assembly();
            Console.WriteLine(result);
            //Console.WriteLine(EXPR);
            Console.ReadKey();

        }
    }
}








