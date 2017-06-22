using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PolinaCompiler.Peg
{
    public static class Extensions
    {
        public static IndentedWriter CollectTree(this StringTreeNode node, params string[] except)
        {
            return CollectTree(node, except, null);
        }

        public static IndentedWriter CollectTree(this StringTreeNode node, string[] except = null, IndentedWriter w = null)
        {
            if (w == null)
                w = new IndentedWriter("  ");

            if (except.Contains(node.Rule.Name))
                return w;

            w.WriteLine(node.ToString());

            w.Push();
            foreach (var item in node.Childs)
                CollectTree(item, except, w);

            w.Pop();

            return w;
        }
    }
}
