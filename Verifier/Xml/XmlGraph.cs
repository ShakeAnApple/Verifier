using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Verifier.Xml
{
    class XmlGraph
    {
        Dictionary<string, XmlGraphNode> _nodes = new Dictionary<string, XmlGraphNode>();

        public XmlGraphNode this[string id] { get { return _nodes[id]; } }

        public XmlGraphNode CreateNode(string id = null)
        {
            var node = new XmlGraphNode(this, id ?? Guid.NewGuid().ToString());
            _nodes.Add(node.Id, node);
            return node;
        }

        /*
            <DirectedGraph xmlns="http://schemas.microsoft.com/vs/2009/dgml"
                          Layout="Sugiyama" GraphDirection="TopToBottom">
             <Nodes>
               <Node  Id="a"  />
               <Node  Id="b" Label="label" />
               <Node  Id="c" />
             </Nodes>
             <Links>
               <Link Source="a" Target="b" />
               <Link Source="a" Target="c" />
               <Link Source="b" Target="c" />
             </Links>
           </DirectedGraph>
           */

        public XmlDocument MakeXmlDocument()
        {
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
            var root = doc.AppendChild(doc.CreateElement("DirectedGraph"));

            var nodes = root.AppendChild(doc.CreateElement("Nodes"));
            var links = root.AppendChild(doc.CreateElement("Links"));

            var lc = 1;

            foreach (var item in _nodes.Values)
            {
                var node = nodes.AppendChild(doc.CreateElement("Node"));
                node.Attributes.Append(doc.CreateAttribute("Id")).Value = item.Id;

                if (item.Text != null)
                    node.Attributes.Append(doc.CreateAttribute("Label")).Value = item.Text;

                foreach (var target in item.GetConnectionTargets())
                {
                    var link = links.AppendChild(doc.CreateElement("Link"));
                    link.Attributes.Append(doc.CreateAttribute("Source")).Value = item.Id;
                    link.Attributes.Append(doc.CreateAttribute("Target")).Value = target.Target.Id;
                    link.Attributes.Append(doc.CreateAttribute("Label")).Value = target.Text;
                    link.Attributes.Append(doc.CreateAttribute("Index")).Value = (lc++).ToString();
                }
            }

            root.Attributes.Append(doc.CreateAttribute("xmlns")).Value = "http://schemas.microsoft.com/vs/2009/dgml";

            return doc;
        }

    }

    class XmlGraphNode : IComparable<XmlGraphNode>
    {
        XmlGraph _owner;

        List<XmlGraphLink> _links = new List<XmlGraphLink>();

        public string Id { get; private set; }
        public string Text { get; set; }

        public XmlGraphNode(XmlGraph owner, string id)
        {
            _owner = owner;

            this.Id = id;
        }

        public XmlGraphLink[] GetConnectionTargets()
        {
            return _links.ToArray();
        }

        public XmlGraphLink ConnectTo(XmlGraphNode target)
        {
            if (target._owner != _owner)
                throw new InvalidOperationException();

            var link = new XmlGraphLink(target);
            _links.Add(link);
            return link;
        }

        public XmlGraphNode CreateNext(string id)
        {
            var node = _owner.CreateNode(id);
            this.ConnectTo(node);
            return node;
        }

        public XmlGraphNode CreatePrev(string id)
        {
            var node = _owner.CreateNode(id);
            node.ConnectTo(this);
            return node;
        }

        public int CompareTo(XmlGraphNode other)
        {
            if (other.Id == null)
                return 1;

            return this.Id.CompareTo(other.Id);
        }
    }

    class XmlGraphLink : IComparable<XmlGraphLink>
    {
        public XmlGraphNode Target { get; private set; }
        public string Text { get; set; }

        public XmlGraphLink(XmlGraphNode target)
        {
            this.Target = target;
        }

        int IComparable<XmlGraphLink>.CompareTo(XmlGraphLink other)
        {
            return this.Target.CompareTo(other.Target);
        }
    }
}
