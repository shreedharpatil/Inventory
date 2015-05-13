using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace roslyncompiler
{
    public class ConstructorParameter
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public string ClassName { get; set; }

        public string NameSpace { get; set; }

        public bool IsProcessed { get; set; }

        public string MethodName { get; set; }
    }
}
