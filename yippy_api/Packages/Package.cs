
namespace Yippy.Packages
{
    public class Package
    {
        private Compiler _compiler;
        private bool _attachable = true;
        private string _name = string.Empty;

        public string Name
        {
            get
            {
                return _name;
            }
            protected set
            {
                _name = value;
            }
        }
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
        public bool Attachable
        {
            get
            {
                return _attachable;
            }
            protected set
            {
                _attachable = value;
            }
        }

        public Package(Compiler compiler)
        {
            _compiler = compiler;
            _attachable = true;
        }
        public virtual void ParseMethods(string data){}
        public virtual void Initialize(){}
    }
}
