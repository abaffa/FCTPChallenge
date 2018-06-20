using FCTProblem.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCTProblem.Utils
{
    public class InstanceConverter
    {

        /**Reads a .DAT file with the graph description and returns the graph*/
        public static FCTPGraph convert(String srcPath)
        {

            StreamReader reader = new StreamReader(srcPath);

            List<Edge> edges = new List<Edge>();
            List<Node> sources = new List<Node>();
            List<Node> sinks = new List<Node>();

            // HEADER
            reader.ReadLine();//instance
            // INSTANCE INFO (name, sources, sinks)
            String info = reader.ReadLine();
            Scanner sc = new Scanner(info);

            String name = sc.next();
            sc.Close();
            // ARCS
            reader.ReadLine();//ARCS

            String line = reader.ReadLine();
            StringBuilder edgesBuilder = new StringBuilder(line + "\n");
            while (!line.Equals("S"))
            {

                line = reader.ReadLine();
                if (!line.Equals("S"))
                {
                    edgesBuilder.Append(line + "\n");
                }
            }
            sc = new Scanner(edgesBuilder.ToString());
            while (sc.hasNext())
            {
                int source = sc.nextInt();
                int sink = sc.nextInt();
                double varCost = sc.nextDouble();
                double fixedCost = sc.nextDouble();
                sc.nextLine();
                edges.Add(new Edge(source, sink, varCost, fixedCost));
            }
            sc.Close();
            // SOURCE NODES
            line = reader.ReadLine();
            StringBuilder sourceBuilder = new StringBuilder(line + "\n");
            while (!line.Equals("D"))
            {

                line = reader.ReadLine();
                if (!line.Equals("D"))
                {
                    sourceBuilder.Append(line + "\n");
                }
            }
            sc = new Scanner(sourceBuilder.ToString());
            while (sc.hasNext())
            {
                int source = sc.nextInt();
                double resource = sc.nextDouble();
                sources.Add(new Node(source, resource));
                //sc.nextLine();
            }
            sc.Close();

            line = reader.ReadLine();
            StringBuilder sinkBuilder = new StringBuilder(line + "\n");
            while((line = reader.ReadLine()) != null)
            {
                //line = reader.ReadLine();
                if (!line.Equals("END"))
                {
                    sinkBuilder.Append(line + "\n");
                }
            }
            sc = new Scanner(sinkBuilder.ToString());

            while (sc.hasNext())
            {
                int sink = sc.nextInt();
                double demand = sc.nextDouble();
                sinks.Add(new Node(sink, demand));
                //sc.nextLine();
            }

            sc.Close();

            reader.Close();

            FCTPGraph ti = new FCTPGraph(name, sources, sinks, edges);
            return ti;
        }
    }
}