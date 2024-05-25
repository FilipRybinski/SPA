using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Tables
{
    public class Variable
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public Variable(string name)
        {
            Name = name;
        }

    }
}
