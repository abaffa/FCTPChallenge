using FCTProblem.Data;
using FCTProblem.Solver;
using FCTProblem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCTProblem
{
    class Program
    {
        static void Main(string[] args)
        {

            String filename = @"C:\Users\Augusto Baffa\workspace\FixedCostTransportationGame\rsc\test\N104.DAT";
            FCTPGraph g = InstanceConverter.convert(filename);
            RuntimeSolver solver = new RuntimeSolver();
            Dictionary<EdgePair, double> l = solver.Solve(g);

            foreach (EdgePair ep in l.Keys)
            {
                Console.Out.WriteLine(ep.ToString() + "\t" + l[ep]);
            }
        }
    }
}
