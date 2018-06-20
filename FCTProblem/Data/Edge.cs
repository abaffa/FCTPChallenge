using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCTProblem.Data
{
    public class Edge
    {
        private int source;
        private int sink;
        private double c;
        private double f;

        public Edge(int source, int sink)
            : this(source, sink, 0)
        {

        }
        public Edge(int source, int sink, double c)
            : this(source, sink, c, 0)
        {

        }

        public Edge(int source, int sink, double c, double f)
        {
            this.source = source;
            this.sink = sink;
            this.c = c;
            this.f = f;
        }
        public int Source
        {
            get
            {
                return source;
            }
        }
        public int Sink
        {
            get
            {
                return sink;
            }
        }
        public double C
        {
            get
            {

                return c;
            }
        }
        public double F
        {
            get
            {
                return f;
            }
        }

        public override bool Equals(Object o)
        {
            if (o is Edge)
            {
                return Equals((Edge)o);
            }
            return false;
        }
        public bool Equals(Edge o)
        {
            Edge e = (Edge)o;
            bool b1 = e.source == source;
            bool b2 = e.sink == sink;
            bool b3 = Math.Abs(e.c - c) < Double.MinValue;
            bool b4 = Math.Abs(e.F - F) < Double.MaxValue;
            return b1 && b2 && b3 && b4;
        }
        public override string ToString()
        {
            return "(" + source.ToString() + "," + sink.ToString() + ")";
        }
        public override int GetHashCode()
        {
            int h1 = source.GetHashCode();
            int h2 = sink.GetHashCode();
            int h3 = c.GetHashCode();
            int h4 = c.GetHashCode();
            return h1 + h2 + h3 + h4;
        }
    }

}
