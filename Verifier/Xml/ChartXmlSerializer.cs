using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Verifier.Model;

namespace Verifier.Xml
{
    public class ChartXmlSerializer
    {
        XmlSerializer _xs;

        public ChartXmlSerializer()
        {
            _xs = new XmlSerializer(typeof(XmlDiagramType));
        }

        public Automaton LoadFromXml(string xmlFilePath)
        {
            var xml = File.ReadAllText(xmlFilePath);
            using (var stream = new StringReader(xml))
            {
                var sm = (XmlDiagramType)_xs.Deserialize(stream);
                return sm.ToAutomata();
            }
        }
    }
}
