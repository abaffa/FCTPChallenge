using FCTProblem.Data;
using Gurobi;
using ILOG.Concert;
using ILOG.CPLEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCTProblem.Solver
{
    public class CPLEXSolver : TPSolver
    {

	public Dictionary<EdgePair, Double> Solve(FCTPGraph ti) {
		Dictionary<EdgePair,Double> edgeFlows = new Dictionary<EdgePair,Double>();
		Dictionary<String, Edge> edgeMap = new Dictionary<String,Edge>();
		Dictionary<String,INumVar> varMap = new Dictionary<String,INumVar>();
		String currentVar = "";
		
		try {
			//Model
            Cplex cplex = new Cplex();
            IModel model = cplex.GetModel();
            //model.Set(GRB.StringAttr.ModelName, "tranportation");

            ILinearNumExpr expr = cplex.LinearNumExpr();

			//edges
			foreach(Edge e in ti.Edges) {
				String xij = edgeVarName(e.Source,e.Sink);
				edgeMap.Add(xij,e);
				INumVar var = cplex.NumVar(0, System.Double.MaxValue);
                var.Name = xij;
				varMap.Add(xij, var);
                expr.AddTerm(e.C, var);
            }

            //objective min Sum c_{ij} x_{ij}
            model.Add(cplex.Minimize(expr));
			
			
			//supply constraints
            foreach(Node ssource in ti.Sources) {
                List<INumExpr> sourceConstraint = new List<INumExpr>();
                foreach(Node ssink in ti.Sinks) {
                    String name = edgeVarName(ssource.Id,ssink.Id);
					sourceConstraint.Add(varMap[name]);
				}
                cplex.AddEq(cplex.Sum(sourceConstraint.ToArray()), ssource.Amount);
            }

			//demand constraints
  			foreach(Node dsink in ti.Sinks) {
                List<INumExpr> sinkConstraint = new List<INumExpr>();
				foreach(Node dsource in ti.Sources) {
					String name = edgeVarName(dsource.Id,dsink.Id);
					sinkConstraint.Add(varMap[name]);
				}
                cplex.AddEq(cplex.Sum(sinkConstraint.ToArray()), dsink.Amount);
			}

            cplex.SetParam(Cplex.BooleanParam.Threads, 1);
            cplex.ExportModel("mipTranportationCplex.lp");

            bool status = cplex.Solve();
            Console.WriteLine("Status: " + status);

			foreach(String s in edgeMap.Keys) {
				currentVar = s;
				double flow =  cplex.GetValue(varMap[s]);
				Edge e = edgeMap[s];
				EdgePair ep = new EdgePair(e.Source,e.Sink);
				edgeFlows.Add(ep, flow);
			}
			cplex.End();
			
		} catch (ILOG.Concert.Exception e) {
			Console.Out.WriteLine(currentVar + " - is current var");
            Console.Out.WriteLine(e.ToString());
		}
		
		return edgeFlows;
	}

	private String edgeVarName(int i, int j) {
		return "x" + i + "_" + j;
	}

}

}
