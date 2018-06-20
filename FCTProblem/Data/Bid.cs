using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCTProblem.Data
{
    public class Bid : IComparable<Bid>
    {
        
	    private EdgePair edge;
	    private String owner;
	    private double value;
	
	    public Bid(int source, int sink, String owner, double value) : this(new EdgePair(source,sink),owner,value){
		
	    }
 	    public Bid(EdgePair edge, String owner, double value) {
		    this.edge = edge;
		    this.owner = owner;
		    this.value = value;
	    }
	
	    public Edge makeEdge() {
		    return new Edge(edge.Source,edge.Sink,value,0);
	    }
	
	    public EdgePair Edge {
            get{
		    return edge;
                }
	    }

	    public String Owner {
            get{
		    return owner;
            }
	    }

	    public double Value {
            get{
		    return value;
            }
	    }


        public override bool Equals(Object o)
        {
            if (o is Bid)
            {
                return Equals((Bid)o);
            }
            return false;
        }

	    public bool Equals(Bid b) {
			    bool t1 = b.edge.Equals(edge);
			    bool t2 = b.owner.Equals(owner);
			    bool t3 = Math.Abs(b.value - value) < Double.MinValue;
			    return t1 && t2 && t3;
	    }

	    public int CompareTo(Bid b) {
		    return (int)Math.Sign(value - b.value);
	    }
        public override String ToString()
        {
		    return owner + " " + edge + " " + value;
	    }
    }
}
