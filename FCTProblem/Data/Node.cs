using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCTProblem.Data
{
    public class Node
    {
        private int id;
        private double amount;

        public Node(int id, double amount)
        {
            this.id = id;
            this.amount = amount;
        }

        public int Id
        {
            get
            {
                return id;
            }
        }

        public double Amount
        {
            get
            {
                return amount;
            }
        }
    }

}
