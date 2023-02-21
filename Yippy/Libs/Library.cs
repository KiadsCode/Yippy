using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yippy.Libs
{
    public class Library
    {
        private Compiler _compiler;
        public Compiler Compiler
        {
            get
            {
                return _compiler;
            }
            internal set
            {
                _compiler = value;
            }
        }
        public Library(Compiler compiler)
        {
            _compiler = compiler;
        }
        public virtual void ParseMethods(string data)
        {}
        public virtual void Initialize()
        {
        }
    }
}
