using FCTProblem.Data;
using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCTProblem.Solver
{
public class GurobiSolver : TPSolver {

	public Dictionary<EdgePair, Double> Solve(FCTPGraph ti) {
		Dictionary<EdgePair,Double> edgeFlows = new Dictionary<EdgePair,Double>();
		Dictionary<String, Edge> edgeMap = new Dictionary<String,Edge>();
		Dictionary<String,GRBVar> varMap = new Dictionary<String,GRBVar>();
		String currentVar = "";
		
		try {
			//Model
			GRBEnv env = new GRBEnv();
			env.Set(GRB.IntParam.LogToConsole, 0);
			GRBModel model = new GRBModel(env);
            model.Set(GRB.StringAttr.ModelName, "tranportation");
			
			
			//edges
			foreach(Edge e in ti.Edges) {
				String xij = edgeVarName(e.Source,e.Sink);
				edgeMap.Add(xij,e);
				GRBVar var = model.AddVar(0,GRB.INFINITY,e.C,GRB.CONTINUOUS,xij);
				varMap.Add(xij, var);
			}
			//objective min Sum cij xij
			model.Set(GRB.IntAttr.ModelSense,GRB.MINIMIZE);
			
			//integrate variables
			model.Update();
			
			//supply constraints
			foreach(Node source in ti.Sources) {
				GRBLinExpr sourceConstraint = new GRBLinExpr();
				foreach(Node sink in ti.Sinks) {
					String name = edgeVarName(source.Id,sink.Id);
					sourceConstraint.AddTerm(1, varMap[name]);
				}
				model.AddConstr(sourceConstraint, GRB.EQUAL, source.Amount, "");
			}
			//demand constraints
			foreach(Node sink in ti.Sinks) {
				GRBLinExpr sinkConstraint = new GRBLinExpr();
				foreach(Node source in ti.Sources) {
					String name = edgeVarName(source.Id,sink.Id);
					sinkConstraint.AddTerm(1, varMap[name]);
				}
                model.AddConstr(sinkConstraint, GRB.EQUAL, sink.Amount, "");
			}
            //update constraints
            model.Update();

            model.Write("mipTranportationGurobi.lp");
            env.Set(GRB.IntParam.Threads, 1);
			model.Optimize();

            bool status = model.Get(GRB.IntAttr.Status) == GRB.Status.OPTIMAL;
            Console.WriteLine("Status: " + status);
			foreach(String s in edgeMap.Keys) {
				currentVar = s;
				double flow = varMap[s].Get(GRB.DoubleAttr.X);
				Edge e = edgeMap[s];
				EdgePair ep = new EdgePair(e.Source,e.Sink);
				edgeFlows.Add(ep, flow);
			}
			model.Dispose();
			
		} catch (GRBException e) {
			Console.Out.WriteLine(currentVar + " - is current var");
            Console.Out.WriteLine(e.ErrorCode);
            Console.Out.WriteLine(e.ToString());
		}
		
		return edgeFlows;
	}

	private String edgeVarName(int i, int j) {
		return "x" + i + "_" + j;
	}

}

}
