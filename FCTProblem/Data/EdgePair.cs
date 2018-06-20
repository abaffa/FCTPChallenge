using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCTProblem.Data
{
    public class EdgePair : IEqualityComparer<EdgePair>
    {
        private int source;
        private int sink;
        public EdgePair(int source, int sink)
        {
            this.source = source;
            this.sink = sink;
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

        public override bool Equals(Object o)
        {
            if (o is EdgePair)
            {
                return Equals((EdgePair)o);
            }
            return false;
        }

        public bool Equals(EdgePair ep)
        {

            return ep.source == source && ep.sink == sink;
        }


        public bool Equals(EdgePair x, EdgePair y)
        {
            return x.source == y.source && x.sink == y.sink;
        }

        public override int GetHashCode()
        {
            int h1 = source.GetHashCode();
            int h2 = sink.GetHashCode();
            return h1 + h2;
        }

        public int GetHashCode(EdgePair x)
        {
            return x.source.GetHashCode() + x.sink.GetHashCode();
        }
        public override String ToString()
        {
            return "(" + source + "," + sink + ")";
        }
    }
}
