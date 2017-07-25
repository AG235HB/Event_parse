using System;
using System.Collections.Generic;

namespace Event_parse
{
    class Expression
    {
        public string name { get; set; }
        public string pattern { get; set; }
        //public int begin_shift { get; set; }
        //public int end_shift { get; set; }
        //public bool has_inner_info { get; set; }
        //public List<Expression> children { get; set; }
        public List<Expression> children = new List<Expression>();

        public bool HasChildren()
        {
            if (children.Count >0)
                return true;
            else
                return false;
        }

        public void AddChild(Expression child)
        { children.Add(child); }

        public void Print()
        {
            Console.WriteLine(this.name.ToString().PadRight(15) + this.pattern);
            if(this.HasChildren())
                foreach (Expression chExpr in this.children)
                    chExpr.Print();
        }
    }
}
