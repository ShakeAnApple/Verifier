using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using PolinaCompiler.Peg.Parser;

namespace PolinaCompiler.Peg.Xml
{
    public class GrammarXmlSerializer
    {
        XmlSerializer _xs;

        public GrammarXmlSerializer()
        {
            _xs = new XmlSerializer(typeof(GrammarType));
        }

        public void SaveToFile(ParsingGrammar grammar, string fileName)
        {
            using (var stream = File.OpenWrite(fileName))
            {
                _xs.Serialize(stream, new GrammarType(grammar));
            }
        }

        public ParsingGrammar LoadFromFile(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var g = (GrammarType)_xs.Deserialize(stream);
                return g.ToGrammar();
            }
        }

        public ParsingGrammar LoadFromXml(string xml)
        {
            using (var stream = new StringReader(xml))
            {
                var g = (GrammarType)_xs.Deserialize(stream);
                return g.ToGrammar();
            }
        }
    }
}
