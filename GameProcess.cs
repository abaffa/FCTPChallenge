using FCTProblem.Control;
using FCTProblem.Data;
using FCTProblem.Utils;
using FCTPServer;
using FCTPServer.Socket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCTPServer.Game
{
    public class GameProcessedEventArgs : System.EventArgs
    {
        public String RowsResults { get; set; }
        public String RowsOwners { get; set; }
    }

    public class GameProcess
    {
        public event EventHandler GameProcessedEvent;

        String rowsResults = "";
        String rowsOwners = "";

        Dictionary<String, Double> payoffs = new Dictionary<String, Double>();

        /*
        private int computeEdgesPerRound(FCTPGraph problem, int nPlayers)
        {
            return (int)((problem.Edges.Count * 0.4) / (nRounds * nPlayers));
        }
        */
        private String getSortedPayoffMapMsg(Dictionary<String, Double> payoffMap)
        {
            Dictionary<String, Double> sorted = new Dictionary<String, Double>();
            List<Double> payoffs = new List<Double>(payoffMap.Values);
            payoffs.Sort();


            StringBuilder msg = new StringBuilder();
            for (int i = payoffs.Count - 1; i >= 0; i--)
            {

                Double val = payoffs[i];
                foreach (String player in payoffMap.Keys)
                {
                    if (payoffMap[player].Equals(val) && !sorted.ContainsKey(player))
                    {
                        sorted.Add(player, val);

                        msg.Append(player + " " + val + "\n");
                    }
                }
            }
            return msg.ToString();
        }


        /**
         * <b>Performs procedures to calculate round results</b><br>
         * This function gets all bids, computes and send results to all connected users.
         */
        public void runRound()
        {
            payoffs.Clear();
            rowsResults = "";
            rowsOwners = "";
            try
            {

                List<string> players = new List<string>();
                foreach (HandleClient c in GamePlay.socket.ClientList) // GamePlay.socket.ClientList - Gets list of players
                    players.Add(c.player.name);

                foreach (FileInfo child in GamePlay.instances) // GamePlay.instances - Gets list of instances
                    matchOn(child, players);
            }

            catch (Exception exx)
            {
                Console.WriteLine(exx.ToString());
            }


            EventHandler handler = GameProcessedEvent;
            if (handler != null)
            {
                GameProcessedEventArgs e = new GameProcessedEventArgs();
                e.RowsOwners = rowsOwners;
                e.RowsResults = rowsResults;
                handler(this, e);
            }
        }

        public void matchOn(FileInfo child, List<string> players)
        {
            FCTPGraph problem =
                    InstanceConverter.convert(
                            child.FullName);

            log("Match " + problem.Name);

            //Initialize Edge Auction
            EdgeAuction auction = new EdgeAuction(problem, GamePlay.U, players); // GamePlay.U - Gets Constant Factor K = 20 (U = K on problem description)
            //int edgesPerRound = computeEdgesPerRound(problem, players.Count);
            //log(edgesPerRound + " edges per round");

            //String msg = "instance " + problem.Name + " " + edgesPerRound;
            playRound(child.Name, auction, GamePlay.round); // GamePlay.round - Gets the current round number.

            Dictionary<String, Double> instancePayoffs = auction.getPayoffMap();
            foreach (HandleClient c in GamePlay.socket.ClientList) // GamePlay.socket.ClientList - Gets list of players
            {
                String s = c.player.name;

                if (instancePayoffs.ContainsKey(c.player.name))
                {
                    c.player.SetScore(instancePayoffs[s]);

                    if (!payoffs.ContainsKey(s))
                        payoffs.Add(s, instancePayoffs[s]);
                    else
                        payoffs[s] = payoffs[s] + instancePayoffs[s];
                }
            }

            log("PARTIAL");
            String partial = getSortedPayoffMapMsg(payoffs);
            log(partial);
        }

        private void playRound(String instance, EdgeAuction auction, int round)
        {
            log("Round: " + round);

            List<Bid> bids = new List<Bid>();
            foreach (HandleClient c in GamePlay.socket.ClientList) // GamePlay.socket.ClientList - Gets list of players
            {
                List<Bid> iBids = c.player.GetBidsTo(instance);
                bids.AddRange(iBids);
            }
            auction.update(bids);

            log("Round " + round + " Payoffs");
            Dictionary<String, Double> payoffMap = auction.getPayoffMap();

            String msg = getSortedPayoffMapMsg(payoffMap);
            log(msg);

            sendResults(instance, auction);
        }
        private void sendResults(String instance, EdgeAuction auction)
        {
            Dictionary<EdgePair, Double> flow = auction.getCurrentFlow();
            Dictionary<EdgePair, List<Bid>> edgeBids = auction.getEdgeBids();
            Dictionary<EdgePair, List<String>> W = auction.getWinners();
            makeResultsMessage(instance, flow, edgeBids, W);
        }

        private void makeResultsMessage(string instanceName,
                Dictionary<EdgePair, Double> flow,
                Dictionary<EdgePair, List<Bid>> edgeBids,
                Dictionary<EdgePair, List<String>> W)
        {

            StringBuilder sb = new StringBuilder();

            //result instance numberOfOwnedEdges
            ConsoleLog.WriteLine("result;" + instanceName + ";" + edgeBids.Keys.Count);
            GamePlay.socket.SendAll("result;" + instanceName + ";" + edgeBids.Keys.Count); // Send result to all connected users
            //rowsResults += "<tr><td>" + instanceName + "</td><td>" + edgeBids.Keys.Count + "</td></tr>";
            rowsResults += instanceName + "\t" + edgeBids.Keys.Count + "\n";

            foreach (EdgePair ep in edgeBids.Keys)
            {
                Bid winBid = edgeBids[ep][0];
                int i = ep.Source;
                int j = ep.Sink;

                foreach (String owner in W[ep])
                {
                    int nBids = edgeBids[ep].Count;
                    double winVal = winBid.Value;
                    double eFlow = flow[ep];
                    //owner instance source sink ownerValue numberOfBids winnerBidValue edgeFlow
                    /*
                    ConsoleLog.WriteLine("owner;" + instanceName 
                            + ";(" + i + "," + j + ");" + owner 
                            + ";" + nBids + ";" + winVal + ";" + eFlow);
                     */
        GamePlay.socket.SendAll("owner;" + instanceName  // Send results to all connected users
                            + ";" + ep.ToString() + ";" + owner
                            + ";" + nBids + ";" + winVal + ";" + eFlow);

                    rowsOwners += instanceName
                        + "\t(" + i + "," + j + ")\t" + owner
                        + "\t" + nBids + "\t" + winVal + "\t" + eFlow + "\n";
                    /*
                    rowsOwners += "<tr><td>" + instanceName
                            + "</td><td>(" + i + "," + j + ")</td><td>" + owner
                            + "</td><td>" + nBids + "</td><td>" + winVal + "</td><td>" + eFlow + "</td></tr>";
                     */
                }
            }

        }
        private void log(String msg)
        {
            //Console.WriteLine(msg);
        }

    }
}
