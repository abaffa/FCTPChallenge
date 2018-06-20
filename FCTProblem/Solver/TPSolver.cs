using FCTProblem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCTProblem.Solver
{
    public interface TPSolver
    {
        /**Solves the transportation problem on the graph, returns
         * the flow for each edge*/
        Dictionary<EdgePair, Double> Solve(FCTPGraph graph);
    }
}
