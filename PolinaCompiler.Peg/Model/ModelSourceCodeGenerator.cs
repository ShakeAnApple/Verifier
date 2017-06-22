using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolinaCompiler.Peg.Model
{
    class ModelSourceCodeGenerator
    {
        readonly IndentedWriter _w = new IndentedWriter();
        readonly ModelInfo _model;

        public ModelSourceCodeGenerator(ModelInfo model)
        {
            _model = model;
        }

        private void GenerateUsings()
        {
            _w.WriteLine("using System;");
            _w.WriteLine("using System.Collections.Generic;");
            _w.WriteLine("using System.Linq;");
            _w.WriteLine("using System.Text;");
            _w.WriteLine("using PolinaCompiler.Peg;");
            _w.WriteLine();
        }

        private void GenerateModelClasses()
        {
            foreach (var c in _model.Classes)
            {
                if (_model.Public)
                    _w.Write("public ");

                _w.WriteLine("class {0}", c.Name);
                _w.WriteLine("{").Push();
                _w.WriteLine("public StringTreeNode _rawTreeNode;");

                foreach (var f in c.Fields)
                {
                    var typeName = f.Name.Trim('@');

                    if (f.IsCollection)
                        typeName += "[]";

                    _w.WriteLine("public {0} {1}{2};", typeName, f.Name, f.IsCollection ? "s" : string.Empty);
                }

                _w.Pop().WriteLine("}");
                _w.WriteLine();
            }
        }

        string MakeMethodName(string ruleName)
        {
            return "Map" + char.ToUpper(ruleName[0]) + ruleName.Substring(1);
        }

        private void GenerateMapping()
        {
            if (_model.Public)
                _w.Write("public ");

            _w.WriteLine("static class Source{0}Mapping", char.ToUpper(_model.Name[0]) + _model.Name.Substring(1));
            _w.WriteLine("{").Push();

            foreach (var c in _model.Classes)
            {
                if (c == _model.Root)
                    _w.Write("public ");

                _w.WriteLine("static {0} {1}(StringTreeNode node)", c.Name, MakeMethodName(c.Name));
                _w.WriteLine("{").Push();

                _w.WriteLine("return new {0}() {{", c.Name).Push();
                _w.WriteLine("_rawTreeNode = node,");

                foreach (var f in c.Fields)
                {
                    if (f.IsContent)
                    {
                        if (f.IsCollection)
                        {
                            _w.WriteLine("{0}s = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),", f.Name, MakeMethodName(f.Name));
                        }
                        else
                        {
                            _w.WriteLine("{0} = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),", f.Name, MakeMethodName(f.Name));
                        }
                    }
                    else if (f.IsCollection)
                    {
                        _w.WriteLine("{0}s = node.Childs.Where(n => n.Rule.Name == \"{0}\").Select(n => {1}(n)).ToArray(),", f.Name, MakeMethodName(f.Name));
                    }
                    else
                    {
                        _w.WriteLine("{0} = node.Childs.Where(n => n.Rule.Name == \"{0}\").Select(n => {1}(n)).FirstOrDefault(),", f.Name, MakeMethodName(f.Name));
                    }
                }

                _w.Pop().WriteLine("};");

                _w.Pop().WriteLine("}");
                _w.WriteLine();
            }

            _w.Pop().WriteLine("}");
            _w.WriteLine();
        }

        public IndentedWriter Generate()
        {
            this.GenerateUsings();

            if (!string.IsNullOrWhiteSpace(_model.Namespace))
            {
                _w.WriteLine("namespace {0}", _model.Namespace);
                _w.WriteLine("{").Push();
                _w.WriteLine();

            }

            this.GenerateModelClasses();
            this.GenerateMapping();

            if (!string.IsNullOrWhiteSpace(_model.Namespace))
            {
                _w.WriteLine();
                _w.Pop().WriteLine("}");
            }

            return _w;
        }
    }
}
