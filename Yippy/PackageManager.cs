using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yippy.Packages;

namespace Yippy
{
    public class PackageManager : IDisposable
    {
        private Dictionary<string, Package> _packages;
        private Dictionary<string, Packages.Package> _availablePackages;
        private Compiler _compiler;
        private bool _disposed = false;

        internal void UpdateCompiler(Compiler compiler)
        {
            _compiler = compiler;
        }
        public void Update()
        {
            foreach (Packages.Package item in _packages.Values)
                item.Compiler = _compiler;
        }

        public PackageManager(Compiler compiler)
        {
            _packages = new Dictionary<string, Packages.Package>();
            _availablePackages = new Dictionary<string, Packages.Package>();
        }

        public void ParsePackages(string data)
        {
            if (_disposed == false)
            {
                Update();
                foreach (Packages.Package item in _packages.Values)
                    item.ParseMethods(data);
            }
        }

        public bool Disposed
        {
            get
            {
                return _disposed;
            }
        }

        public void Initialize()
        {
            DefaultLibrariesInitialize();
        }

        private void DefaultLibrariesInitialize()
        {
            _availablePackages.Add("System", new Packages.System(_compiler));
        }
        public void Remove(string name)
        {
            if (_disposed == false)
            {
                if (!_packages.ContainsKey(name))
                {
                    _compiler.ThrowException("Package not attached", "PackageManager");
                    return;
                } else
                {
                    _packages.Remove(name);
                    Update();
                }
            }
        }
        public void Add(string name)
        {
            if (_disposed == false)
            {
                if (!_availablePackages.ContainsKey(name))
                {
                    _compiler.ThrowException("No available packages with \"" + name + "\" name", "PackageManager");
                    return;
                }
                if (_packages.ContainsKey(name))
                {
                    _compiler.ThrowException("Package already attached");
                    return;
                }
                Packages.Package outlib;
                _availablePackages.TryGetValue(name, out outlib);
                _packages.Add(name, outlib);
                Update();
                outlib.Initialize();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool p)
        {
            if (p)
            {
                _compiler = null;
                _packages.Clear();
                _packages = null;
                _availablePackages.Clear();
                _availablePackages = null;
                _disposed = true;
            }
        }
    }
}
