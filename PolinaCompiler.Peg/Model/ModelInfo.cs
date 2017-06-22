using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolinaCompiler.Peg.Model
{
    class ModelClassFieldInfo
    {
        public string Name { get; private set; }
        public bool IsCollection { get; set; }
        public bool IsContent { get { return this.Name == "@string"; } }

        public ModelClassFieldInfo(string name)
        {
            this.Name = name;
        }
    }

    class ModelClassInfo
    {
        public string Name { get; private set; }
        public List<ModelClassFieldInfo> Fields { get; private set; }

        public ModelClassInfo(string name)
        {
            this.Name = name;
            this.Fields = new List<ModelClassFieldInfo>();
        }
    }

    class ModelInfo
    {
        public List<ModelClassInfo> Classes { get; private set; }
        public string Namespace { get; set; }
        public string Name { get; set; }
        public ModelClassInfo Root { get; set; }
        public bool Public { get; set; }

        public ModelInfo()
        {
            this.Classes = new List<ModelClassInfo>();
        }
    }

}
