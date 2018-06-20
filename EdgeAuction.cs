using FCTProblem.Data;
using FCTProblem.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCTProblem.Control
{
    /**
     * Represents an auction on the variable costs of the edges into one instance of
     * the fixed-cost transportation problem. The players are charged with the fixed
     * cost and paid with the xij * (bij - cij), where xij is the flow on the edge,
     * bij is its bid and cij is the original variable cost<br>
     * <br>
     * It estipulates a max value
     */
    public class EdgeAuction
    {
        private FCTPGraph G;// problem instance
        private Dictionary<EdgePair, List<Bid>> P;// Accumulated bids List
        private Dictionary<EdgePair, Double> flow;
        private Dictionary<String, Double> payoff;// payoff Dictionary
        private Dictionary<EdgePair, List<String>> W;
        private double U;// upper bound on bid
        private TPSolver solver;// Linear solver

        public EdgeAuction(FCTPGraph instance, double U, List<String> players)
        {
            String[] nameArray = players.ToArray();
            init(instance, U, nameArray);
        }

        public EdgeAuction(FCTPGraph instance, double U, String[] players)
        {
            init(instance, U, players);
        }

        private void init(FCTPGraph instance, double U, String[] players)
        {
            this.G = instance;
            this.U = U;
            this.P = new Dictionary<EdgePair, List<Bid>>();
            this.payoff = new Dictionary<String, Double>();
            this.flow = new Dictionary<EdgePair, Double>();
            this.W = new Dictionary<EdgePair, List<String>>();
            solver = new RuntimeSolver();
            foreach (String player in players)
            {
                payoff.Add(player, 0.0);
            }
        }

        public void update(List<Bid> B)
        {
            // augment bids List and return fixed cost share of previous round
            updateBidSet(B);
            // compute new variable cost for each edge<i,j> on modified TP
            Dictionary<EdgePair, Double> v = computeBestBids();
            // compute flow on modified TP problem
            flow = solveTP(v);
            // compute winners (players with best value bids)
            Dictionary<EdgePair, List<String>> W = computeWinners(v);
            // update payoffs
            updatePayoffs(W, v, flow);
        }

        /** Maps each player into its currently accumulated payoff */
        public Dictionary<String, Double> getPayoffMap()
        {
            return payoff;
        }

        /** Returns the flow on each edge on the last round */
        public Dictionary<EdgePair, Double> getCurrentFlow()
        {
            return flow;
        }

        /** Returns the list of bids on that edge up to this round */
        public Dictionary<EdgePair, List<Bid>> getEdgeBids()
        {
            return P;
        }

        /** Returns the list of winners of each edge */
        public Dictionary<EdgePair, List<String>> getWinners()
        {
            return W;
        }

        /**
         * Update bids sets with new bids and return fixed cost for old players
         * 
         * @param B
         *            - the list of new bids
         */
        private void updateBidSet(List<Bid> B)
        {
            repayFixedCharge();
            Dictionary<EdgePair, List<Bid>> P_ = makeBidMap(B);
            // Sorts the List of bids for each edge
            foreach (EdgePair xij in P_.Keys)
            {
                P_[xij].Sort();
            }
            removeRedundancy(P_);
            // add new bids
            foreach (EdgePair xij in P_.Keys)
            {
                if (!P.ContainsKey(xij))
                {
                    P.Add(xij, new List<Bid>());
                }
                substitute(P[xij], P_[xij]);
            }
            // Sorts the List of bids for each edge
            foreach (EdgePair xij in P.Keys)
            {
                P[xij].Sort();
            }
        }

        private void substitute(List<Bid> oldBids, List<Bid> newBids)
        {
            List<Bid> replaced = new List<Bid>();
            foreach (Bid b1 in oldBids)
            {
                foreach (Bid b2 in newBids)
                {
                    if (b1.Owner.Equals(b2.Owner))
                    {
                        replaced.Add(b1);
                    }
                }
            }

            foreach (Bid b in replaced)
                oldBids.Remove(b);
            oldBids.AddRange(newBids);
        }

        private Dictionary<EdgePair, List<Bid>> makeBidMap(List<Bid> B)
        {
            Dictionary<EdgePair, List<Bid>> bidMap = new Dictionary<EdgePair, List<Bid>>();
            foreach (Bid b in B)
            {
                EdgePair xij = b.Edge;
                if (this.G.EdgePairs.Contains(xij))
                {
                    if (!bidMap.ContainsKey(xij))
                    {
                        bidMap.Add(xij, new List<Bid>());
                    }
                    bidMap[xij].Add(b);
                }
            }
            return bidMap;
        }

        /**
         * Compute the variable cost based on best bid value for each edge (i,j). If
         * the best xij bid value > U*cij than U*cij is used as value
         */
        private Dictionary<EdgePair, Double> computeBestBids()
        {
            Dictionary<EdgePair, Double> b = new Dictionary<EdgePair, Double>();
            // Compute the upper bound values for bids U*cij for each edge
            foreach (EdgePair xij in G.EdgePairs)
            {
                b.Add(xij, G.GetEdge(xij).C * U);
            }
            foreach (EdgePair xij in P.Keys)
            {
                // If B is sorted, first bid is best bid
                // if its bigger than cij*U than it won't change
                double bVal = Math.Min(b[xij], P[xij][0].Value);
                b[xij] = bVal;
            }
            return b;
        }

        /**
         * Use an external solver to compute the flows based on the transportation
         * problem
         */
        private Dictionary<EdgePair, Double> solveTP(Dictionary<EdgePair, Double> values)
        {
            List<Edge> edges = makeEdgesFromBids(values);
            FCTPGraph Gcap = new FCTPGraph(G.Name);
            Gcap.AddAllSources(G.Sources);
            Gcap.AddAllSinks(G.Sinks);
            Gcap.AddAllEdges(edges);
            Dictionary<EdgePair, Double> flow = solver.Solve(Gcap);
            return flow;
        }

        /**
         * For each edge gets the players who bid the best value(draw) for each edge
         */
        private Dictionary<EdgePair, List<String>> computeWinners(Dictionary<EdgePair, Double> v)
        {
            W.Clear();

            foreach (EdgePair xij in P.Keys)
            {
                W.Add(xij, new List<String>());
            }
            foreach (EdgePair xij in P.Keys)
            {
                foreach (Bid bid in P[xij])
                {
                    if (Math.Abs(bid.Value - v[xij]) == 0)
                    {
                        W[xij].Add(bid.Owner);
                    }
                }
            }
            return W;
        }

        /**
         * <b>Update the payoff Dictionary</b><br>
         * It charges the fixed cost equally between players who bid on it,
         * independently of the round<br>
         * <br>
         * It divides the flow between the owners if more than one player won the
         * edge
         * 
         * @param W
         *            - winner List, players who won (with best bid draw) for each
         *            edge
         * @param v
         *            - the values of the best bid for each edge
         * @param flow
         *            - the flows passing on each edge
         */
        private void updatePayoffs(Dictionary<EdgePair, List<String>> W,
                Dictionary<EdgePair, Double> v, Dictionary<EdgePair, Double> flow)
        {
            // compute the charge for bidding on edges
            foreach (EdgePair xij in P.Keys)
            {
                double Psize = P[xij].Count;
                double cost = G.GetEdge(xij).F / Psize;
                foreach (Bid bk in P[xij])
                {
                    String k = bk.Owner;
                    if (!payoff.ContainsKey(k))
                        payoff.Add(k, payoff[k] - cost);
                    else
                        payoff[k] = payoff[k] - cost;
                }
            }
            // compute the rewards for the edge winners
            foreach (EdgePair xij in W.Keys)
            {
                double Wsize = W[xij].Count;
                double fij = flow[xij];
                double cij = G.GetEdge(xij).C;
                foreach (String k in W[xij])
                {
                    double reward = (fij / Wsize) * (v[xij] - cij);
                    if (!payoff.ContainsKey(k))
                        payoff.Add(k, payoff[k] + reward);
                    else
                        payoff[k] = payoff[k] + reward;
                }
            }
        }

        /**
         * If a player bids on the same edge multiple times it gets the bid with
         * less value and ignores the others, so that this player isn't counted
         * multiple times on the payoff computation<br>
         * <br>
         * <i>Requires each Pij to be sorted</i>
         */
        private void removeRedundancy(Dictionary<EdgePair, List<Bid>> P)
        {
            foreach (EdgePair xij in P.Keys)
            {
                // bidders on xij
                List<String> J = new List<String>();
                // redundant bids with greater value
                List<Bid> R = new List<Bid>();
                foreach (Bid b in P[xij])
                {
                    String k = b.Owner;
                    if (!J.Contains(k))
                    {
                        J.Add(k);
                    }
                    else
                    {
                        R.Add(b);
                    }
                }

                foreach (Bid b in R)
                    P[xij].Remove(b);
            }
        }

        /** Returns the fixed charge from previous round for each player */
        private void repayFixedCharge()
        {
            foreach (EdgePair xij in P.Keys)
            {
                double Pijsize = P[xij].Count;
                double Fij = G.GetEdge(xij).F;
                foreach (Bid b in P[xij])
                {
                    String k = b.Owner;
                    payoff.Add(k, payoff[k] + Fij / Pijsize);
                }
            }
        }

        /**
         * Make edges substituting the variable cost by the best bid value
         * 
         * @param bids
         *            - maps each edge on the value of its best bid
         */
        private List<Edge> makeEdgesFromBids(Dictionary<EdgePair, Double> bids)
        {
            List<Edge> edges = new List<Edge>();
            foreach (EdgePair xij in bids.Keys)
            {
                double cij = bids[xij];
                double Fij = G.GetEdge(xij).F;
                edges.Add(new Edge(xij.Source, xij.Sink, cij, Fij));
            }
            return edges;
        }
    }

}
