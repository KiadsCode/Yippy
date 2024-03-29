﻿using System;
using System.Collections.Generic;

namespace Yippy.Packages
{
    public struct PackageManager : IDisposable
    {
        private Dictionary<string, Package> _packages;
        private Dictionary<string, Package> _availablePackages;
        private Compiler _compiler;
        private bool _disposed;

        internal void UpdateCompiler(Compiler compiler)
        {
            _compiler = compiler;
        }
        public void Update()
        {
            foreach (Package item in _packages.Values)
                item.Compiler = _compiler;
        }

        public PackageManager(Compiler compiler)
        {
            _packages = new Dictionary<string, Package>();
            _availablePackages = new Dictionary<string, Package>();
            _compiler = compiler;
            _disposed = false;
        }

        public void ParsePackages(string data)
        {
            if (_disposed == false)
            {
                Update();
                foreach (Package item in _packages.Values)
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

        internal void AddCustomPackage(string packageName, Package package)
        {
            if (_availablePackages.ContainsKey(packageName) == false)
                _availablePackages.Add(packageName, package);
            else
                _compiler.ThrowException("Same package already added to compiler", "PackageManager");
        }
    }
}
