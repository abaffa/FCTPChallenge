using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCTProblem.Data
{
    /**Fixed Cost Transportation Problem Graph,
     * contains the sources, sinks and edges of the bipartite graph
     * that represents the FCTP*/
    public class FCTPGraph
    {
        private Dictionary<int, Node> supplyMap;
        private Dictionary<int, Node> demandMap;
        private Dictionary<EdgePair, Edge> edgeMap;
        private String name;

        public FCTPGraph(String name)
            : this(name, new List<Node>(), new List<Node>(), new List<Edge>())
        {

        }

        public FCTPGraph(String name, List<Node> sources, List<Node> sinks, List<Edge> edges)
        {
            this.name = name;
            this.supplyMap = new Dictionary<int, Node>();
            this.demandMap = new Dictionary<int, Node>();
            this.edgeMap = new Dictionary<EdgePair, Edge>();
            AddAllSources(sources);
            AddAllSinks(sinks);
            AddAllEdges(edges);
        }
        public String Name
        {
            get
            {
                return name;
            }
        }
        public Node GetSource(int id)
        {
            return supplyMap[id];
        }
        public Node GetSink(int id)
        {
            return demandMap[id];
        }
        public Edge GetEdge(int source, int sink)
        {
            return edgeMap[new EdgePair(source, sink)];
        }
        public Edge GetEdge(EdgePair ep)
        {
            return edgeMap[ep];
        }

        public List<Edge> GetEdgesFrom(int source)
        {
            List<Edge> sourceEdges = new List<Edge>();
            foreach (EdgePair ep in edgeMap.Keys)
            {
                if (ep.Source == source)
                {
                    sourceEdges.Add(edgeMap[ep]);
                }
            }
            return sourceEdges;
        }
        public List<Edge> GetEdgesTo(int sink)
        {
            List<Edge> sinkEdges = new List<Edge>();
            foreach (EdgePair ep in edgeMap.Keys)
            {
                if (ep.Sink == sink)
                {
                    sinkEdges.Add(edgeMap[ep]);
                }
            }
            return sinkEdges;
        }
        public List<Node> Sources
        {
            get
            {
                return new List<Node>(supplyMap.Values);
            }
        }
        public List<Node> Sinks
        {
            get
            {
                return new List<Node>(demandMap.Values);
            }
        }
        public List<Edge> Edges
        {
            get
            {
                return new List<Edge>(edgeMap.Values);
            }
        }
        public List<EdgePair> EdgePairs
        {
            get
            {

                return new List<EdgePair>(edgeMap.Keys);
            }
        }
        public void AddSourceNode(int id, double source)
        {
            supplyMap.Add(id, new Node(id, source));
        }
        public void AddSinkNode(int id, double sink)
        {
            demandMap.Add(id, new Node(id, sink));
        }
        public void AddEdge(Edge e)
        {
            edgeMap.Add(new EdgePair(e.Source, e.Sink), e);
        }
        public void AddAllSources(List<Node> sources)
        {
            foreach (Node n in sources)
            {
                AddSourceNode(n.Id, n.Amount);
            }
        }
        public void AddAllSinks(List<Node> sinks)
        {
            foreach (Node n in sinks)
            {
                AddSinkNode(n.Id, n.Amount);
            }
        }
        public void AddAllEdges(List<Edge> edges)
        {

            foreach (Edge e in edges)
            {
                AddEdge(e);
            }
        }
    }

}
