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
            public Operation leftOperation = null;
            public Operation rightOperation = null;
            public Operation varOperation = null;
            public double value = 0;
            public char sign;
            public string variable;
            public MathParser parser;
            public Func<double, double> searchFunction;
            public bool Derivative = false;

            public Operation() { }

            public void copy(Operation original)
            {
                if (original.leftOperation != null)
                {
                    leftOperation = new Operation();
                    leftOperation.copy(original.leftOperation);
                }
                if (original.rightOperation != null)
                {
                    rightOperation = new Operation();
                    rightOperation.copy(original.rightOperation);
                }
                Derivative = original.Derivative;
                value = original.value;
                sign = original.sign;
                parser = original.parser;
                searchFunction = original.searchFunction;
                variable = original.variable;
            }


            public void tryMakeSimpleConstOrVar(bool flag)
            {
                if (leftOperation != null && rightOperation != null)
                {
                    if (leftOperation.variable == null && rightOperation.variable == null && leftOperation.rightOperation == null 
                        && rightOperation.rightOperation == null)
                    {
                        value = calculate();
                        leftOperation = null;
                        rightOperation = null;
                        flag = true;
                    }
                    else if (leftOperation.value == 0 && leftOperation.rightOperation == null && leftOperation.variable == null)
                    {
                        if (sign == '+')
                        {
                            var buffOp = rightOperation.copy();
                            leftOperation = null;
                            rightOperation = null;
                            copy(buffOp);
                            flag = true;                           
                        }
                        else if (sign == '-')
                        {
                            var buffOp = rightOperation.copy();
                            leftOperation = null;
                            rightOperation = null;
                            buffOp.changeSign();
                            copy(buffOp);
                            flag = true;
                        }
                        else if (sign == '*')
                        {
                            value = calculate();
                            leftOperation = null;
                            rightOperation = null;
                            flag = true;
                        }                           
                    }
                    else if (rightOperation.value == 0 && rightOperation.rightOperation == null && rightOperation.variable == null)
                    { 
                        if (sign == '+' || sign == '-')
                        {
                            var buffOp = leftOperation.copy();
                            leftOperation = null;
                            rightOperation = null;
                            copy(buffOp);
                            flag = true;
                        }
                        else if (sign == '*')
                        {
                            value = calculate();
                            leftOperation = null;
                            rightOperation = null;
                            flag = true;
                        }
                    } 
                } 
                else if (searchFunction != null)
                {
                    if (rightOperation.variable == null && rightOperation.rightOperation == null)
                    {
                        value = calculate();
                        rightOperation = null;
                        flag = true;
                    }
                }
                if (leftOperation != null)
                {
                    leftOperation.tryMakeSimpleConstOrVar(flag);
                }
                if (rightOperation != null)
                {
                    rightOperation.tryMakeSimpleConstOrVar(flag);
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
                if (leftOperation != null)
                {
                    leftOperation.changeSign();
                }
                    else if (searchFunction != null)
                    {
                        leftOperation = new Operation();
                        rightOperation = new Operation();
                        sign = '-';
                        rightOperation.searchFunction = searchFunction;
                        searchFunction = null;
                    }
                    else if (value != 0)
                    {
                        value = -value;
                    }
                    else if (variable != null)
                    {
                        leftOperation = new Operation();
                        rightOperation = new Operation();
                        sign = '-';
                        rightOperation.variable = variable;
                        variable = null;
                    }
            }

            public void multiply(double multiplier)
            {
                sign = '*';
                leftOperation = new Operation();
                leftOperation.value = multiplier;
            }

            public void multiplication(double multiplier)
            {
                if (leftOperation != null)
                {
                   leftOperation.multiplication(multiplier);
                }
                else if (searchFunction != null)
                {
                    var buffOp = rightOperation.copy();
                    multiply(multiplier);
                    rightOperation.searchFunction = searchFunction;
                    searchFunction = null;
                    if (rightOperation.rightOperation == null)
                    {
                        rightOperation.rightOperation = new Operation();
                    }
                    rightOperation.rightOperation = buffOp.copy();
                }
                else if (variable != null)
                {
                    multiply(multiplier);
                    rightOperation = new Operation();
                    rightOperation.variable = variable;
                    variable = null;
                }
                else
                {
                    multiply(multiplier);
                    rightOperation = new Operation();
                    rightOperation.value = value;
                    value = 0;
                }
                if (rightOperation != null && sign != '*')
                {
                    rightOperation.multiplication(multiplier);
                }
            }

            public void checkMultiplier(bool flag)
            {
                if (sign == '*')
                {
                        if (leftOperation.value == 0 && leftOperation.variable == null && leftOperation.rightOperation == null)
                        {
                            leftOperation = null;
                            rightOperation = null;
                            sign = ' ';
                            flag = true;
                        }
                        else if (leftOperation.value == 1)
                        {
                            leftOperation.value = 0;
                            sign = '+';
                            flag = true;
                        }
                        else if (leftOperation.rightOperation == null && leftOperation.variable == null
                        && rightOperation.rightOperation != null)
                        {
                            rightOperation.multiplication(leftOperation.value);
                            leftOperation.value = 0;
                            sign = '+';
                            flag = true;
                        }
                        else if (rightOperation.value == 0 && rightOperation.variable == null && leftOperation.rightOperation == null)
                        {
                            leftOperation = null;
                            rightOperation = null;
                            sign = ' ';
                            flag = true;
                        }
                        else if (rightOperation.value == 1)
                        {
                            leftOperation.value = 0;
                            sign = '+';
                            flag = true;
                        }
                        else if (rightOperation.rightOperation == null && rightOperation.variable == null
                           && leftOperation.rightOperation != null)
                        {
                            leftOperation.multiplication(rightOperation.value);
                            flag = true;
                        }
                    }
                if (flag == false)
                {
                    if (leftOperation != null)
                    {
                        leftOperation.checkMultiplier(flag);
                    }
                    if (rightOperation != null)
                    {
                        rightOperation.checkMultiplier(flag);
                    }
                }
            }

        /*    public void makeSimpleVar1(ref Operation op)
            {
                if (rightOperation.variable == leftOperation.op.variable && rightOperation.variable != null && )
                {

                }
            }
            */

            public void simplifyVaribles()
            {
                if (rightOperation != null && leftOperation != null)
                {
                    if (rightOperation.variable == leftOperation.variable && rightOperation.variable != null)
                    {
                        if (sign == '+')
                        {
                            sign = '*';
                            leftOperation.value = 2;
                            leftOperation.variable = null;
                        }
                        if (sign == '-')
                        {
                            leftOperation = null;
                            rightOperation = null;
                        }
                        if (sign == '/')
                        {
                            leftOperation = null;
                            rightOperation = null;
                            value = 1;
                        }
                        makeSimple();
                    }
                    else if (leftOperation.rightOperation != null &&
                        rightOperation.variable == leftOperation.rightOperation.variable && rightOperation.variable != null &&
                        leftOperation.sign == '*' && leftOperation.leftOperation.value != 0)
                    {
                        leftOperation.value = leftOperation.leftOperation.value + 1;
                        leftOperation.leftOperation = null;
                        leftOperation.rightOperation = null;
                        sign = '*';
                        makeSimple();
                    }
                    else if (leftOperation.sign == rightOperation.sign && leftOperation.sign == '*')
                    {
                        if (rightOperation.leftOperation != null && leftOperation.leftOperation != null &&
                            rightOperation.rightOperation.variable == leftOperation.rightOperation.variable &&
                            rightOperation.rightOperation.variable != null)
                        {
                            leftOperation.rightOperation.value = rightOperation.leftOperation.value;
                            leftOperation.rightOperation.variable = null;
                            leftOperation.sign = sign;
                            sign = '*';
                            rightOperation.variable = rightOperation.rightOperation.variable;
                            rightOperation.rightOperation = null;
                            rightOperation.leftOperation = null;
                            makeSimple();
                        }
                        else if (rightOperation.leftOperation != null && leftOperation.leftOperation != null &&
                            rightOperation.leftOperation.variable == leftOperation.leftOperation.variable &&
                            rightOperation.leftOperation.variable != null)
                        {
                            leftOperation.leftOperation.value = rightOperation.rightOperation.value;
                            leftOperation.leftOperation.variable = null;
                            leftOperation.sign = sign;
                            sign = '*';
                            rightOperation.variable = rightOperation.leftOperation.variable;
                            rightOperation.rightOperation = null;
                            rightOperation.leftOperation = null;
                            makeSimple();
                        }
                        else if (rightOperation.leftOperation != null && leftOperation.leftOperation != null &&
                            rightOperation.rightOperation.variable == leftOperation.leftOperation.variable &&
                            rightOperation.rightOperation.variable != null)
                        {
                            leftOperation.leftOperation.value = rightOperation.leftOperation.value;
                            leftOperation.leftOperation.variable = null;
                            leftOperation.sign = sign;
                            sign = '*';
                            rightOperation.variable = rightOperation.rightOperation.variable;
                            rightOperation.rightOperation = null;
                            rightOperation.leftOperation = null;
                            makeSimple();
                        }
                        else if (rightOperation.leftOperation != null && leftOperation.leftOperation != null &&
                            rightOperation.leftOperation.variable == leftOperation.rightOperation.variable &&
                            rightOperation.leftOperation.variable != null)
                        {
                            leftOperation.rightOperation.value = rightOperation.rightOperation.value;
                            leftOperation.rightOperation.variable = null;
                            leftOperation.sign = sign;
                            sign = '*';
                            rightOperation.variable = rightOperation.leftOperation.variable;
                            rightOperation.rightOperation = null;
                            rightOperation.leftOperation = null;
                            makeSimple();
                        }
                    }
                }

            }

            

            public bool exchange()
            {
                char bufSign;
                bool flag = false;
                tryMakeSimpleConstOrVar(flag);
                if (flag)
                {
                    return true;
                }
                if (leftOperation != null)
                {
                    if (leftOperation.rightOperation != null && rightOperation != null 
                        && leftOperation.rightOperation.variable != null && rightOperation.variable == null)
                    {
                        if (checkSign(this, leftOperation) == true)
                        {
                            bufSign = sign;
                            sign = leftOperation.sign;
                            leftOperation.sign = bufSign;
                            exchangeOperations(ref leftOperation.rightOperation, ref rightOperation);
                            return true;
                        }
                    }

                    else if (leftOperation.leftOperation != null && rightOperation != null 
                        && leftOperation.leftOperation.variable != null && rightOperation.variable == null)
                    {
                        if (checkSign(this, leftOperation) == true)
                        {
                            //  exchangeSigns(this, leftOperation);
                            bufSign = sign;
                            sign = leftOperation.sign;
                            leftOperation.sign = bufSign;
                            exchangeOperations(ref leftOperation.leftOperation, ref rightOperation);
                            return true;
                        }
                    }
                    else
                    {
                        return leftOperation.exchange();
                    }
                }
                return false;
            }

            public void makeSimple()
            {
                tryGetDiff();
                if (sign == '-' && rightOperation != null && rightOperation.rightOperation != null)
                {
                    sign = '+';
                    rightOperation.changeSign();
                    exchangeOperations(ref rightOperation, ref leftOperation);
                }
                if (exchange())
                {
                    makeSimple();
                }
                var flag = false;
              //  checkMultiplier(flag);
            //    if (flag)
                {
           //         makeSimple();
                }
                simplifyVaribles();
            }


            public void diffSUm()
            {
                leftOperation.diff();
                rightOperation.diff();
            }

            public void tryFoundVar(Operation buf)
            {
                if (rightOperation != null)
                {
                    if (rightOperation.variable != null)
                    {
                        rightOperation.variable = null;
                        rightOperation = buf.copy();
                    }
                    else
                    {
                        rightOperation.tryFoundVar(buf);
                    }
                }
                if (leftOperation != null)
                {
                    if (leftOperation.variable != null)
                    {
                        leftOperation.variable = null;
                        leftOperation = buf.copy();
                    }
                    else
                    {
                        leftOperation.tryFoundVar(buf);
                    }
                }
            }

            public void diffComplexFunc()
            {
                var buffOp = new Operation();
                buffOp = rightOperation.copy();
                rightOperation = null;
                rightOperation = new Operation(parser, parser.dMap[searchKey(searchFunction)]);
                rightOperation.tryFoundVar(buffOp);
                leftOperation = buffOp.copy();
                leftOperation.diff();
                /*        if (rightOperation.rightOperation == null)
                    {
                        rightOperation.rightOperation = new Operation();
                    }
                    rightOperation.rightOperation = buffOp.copy();
                    rightOperation.cleanOperation();
                    rightOperation.searchFunction = searchFunction; */
                    cleanOperation();
                    sign = '*';
            }

            public void diffMultorDividing()
            {
                var buffOp1 = new Operation();
                buffOp1 = leftOperation.copy();
                var buffOp2 = rightOperation.copy();
                    if (leftOperation.rightOperation == null)
                    {
                        leftOperation.rightOperation = new Operation();
                        leftOperation.leftOperation = new Operation();
                    }
                leftOperation.leftOperation = buffOp1.copy();
                leftOperation.rightOperation = buffOp2.copy();
                leftOperation.leftOperation.diff();                    
                    if (rightOperation.rightOperation == null)
                    {
                        rightOperation.rightOperation = new Operation();
                        rightOperation.leftOperation = new Operation();
                    }

                rightOperation.leftOperation = buffOp1.copy();
                rightOperation.rightOperation = buffOp2.copy();
                rightOperation.rightOperation.diff();
                leftOperation.cleanOperation();
                rightOperation.cleanOperation();
                leftOperation.sign = '*';
                rightOperation.sign = '*';
            }


           public void diffDividing()
            {
                if (sign == '/' || sign == ':')
                {
                    var buffOp1 = leftOperation.copy();
                    var buffOp2 = rightOperation.copy();
                    if (leftOperation.rightOperation == null)
                    {
                        leftOperation.rightOperation = new Operation();
                        leftOperation.leftOperation = new Operation();
                    }
                    leftOperation.leftOperation = buffOp1.copy();
                    leftOperation.rightOperation = buffOp2.copy();
                    leftOperation.diffMultorDividing();
                    leftOperation.cleanOperation();
                    leftOperation.sign = '-';
                    if (rightOperation.rightOperation == null)
                    {
                        rightOperation.rightOperation = new Operation();
                        rightOperation.leftOperation = new Operation();
                    }
                    rightOperation.rightOperation = buffOp2.copy();
                    rightOperation.leftOperation = rightOperation.rightOperation.copy();
                    rightOperation.cleanOperation();
                    rightOperation.sign = '*';
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
                    rightOperation.diff();
                    leftOperation = new Operation();
                    Derivative = false;
                    sign = '+';

                }
                else
                {
                    if (leftOperation != null)
                    {
                        leftOperation.tryGetDiff();
                    }
                    if (rightOperation != null)
                    {
                        rightOperation.tryGetDiff();
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
                if (leftOperation != null)
                {
                    result.leftOperation = leftOperation.copy();
                }
                if (rightOperation != null)
                {
                    result.rightOperation = rightOperation.copy();
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
                if (leftOperation != null)
                {
                    l = leftOperation.calculate();
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
                if (rightOperation != null)
                {
                    r = rightOperation.calculate();
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
                if (leftOperation != null)
                {
                    l += leftOperation.assembly();
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
                if (rightOperation != null)
                {
                    r += rightOperation.assembly();
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

            /*    public void simplify()
                {
                    string variables = "";
                    if (leftOperation != null)
                    {
                        if (leftOperation.leftOperation != null || leftOperation.rightOperation != null)
                        {
                            leftOperation.simplify();
                        }
                        else
                        {
                            if (leftOperation.variable != null)
                            {
                                variable = leftOperation.variable;
                                leftOperation.variable = null;
                            }
                            if (rightOperation.variable != null)
                            {
                                variable = rightOperation.variable;
                                rightOperation.variable = null;
                            }
                                value = calculate();
                                leftOperation = null;
                                rightOperation = null;
                            return;
                        }
                    }
                    if (rightOperation != null)
                    {
                        if (rightOperation.rightOperation != null || rightOperation.leftOperation != null)
                        {
                            rightOperation.simplify();
                        }
                        else
                        {
                            if (leftOperation.variable != null)
                            {
                                variable = leftOperation.variable;
                                leftOperation.variable = null;
                            }
                            if (rightOperation.variable != null)
                            {
                                variable = rightOperation.variable;
                                rightOperation.variable = null;
                            }
                            value = calculate();
                                leftOperation = null;
                                rightOperation = null;
                            return;
                        }
                    } 
                  //  createNewOperation(variables);
                } */

            public void createNewOperation(string s)
            {
                if (rightOperation != null)
                {
                    rightOperation.createNewOperation(s);
                }
                else
                {
                    rightOperation = new Operation(parser, s);
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
                    leftOperation = new Operation(parser, "");
                    rightOperation = new Operation(parser, exp.Substring(i + 1));
                }
                else
                {
                    sign = exp[i];
                    leftOperation = new Operation(parser, exp.Substring(0, i));
                    rightOperation = new Operation(parser, exp.Substring(i + 1));
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
                    rightOperation = new Operation(parser, expression.Substring(1));
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
                    rightOperation = new Operation(parser, expression.Substring(k + 1));
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








