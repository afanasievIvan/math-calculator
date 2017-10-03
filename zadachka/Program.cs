using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zadachka
{  
    class Program
    {        
        
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
                var Value = Convert.ToDouble(Console.ReadLine());
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








