using FCTProblem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCTProblem.Solver
{
    public class RuntimeSolver : TPSolver
    {
        private TPSolver solver;
        public RuntimeSolver()
        {
         //   solver = new GurobiSolver();
            solver = new CPLEXSolver();
        }


        public Dictionary<EdgePair, Double> Solve(FCTPGraph graph)
        {
            Dictionary<EdgePair, Double> ep = new Dictionary<EdgePair, Double>();
            ep = solver.Solve(graph);
            return ep;
        }
    }
}
