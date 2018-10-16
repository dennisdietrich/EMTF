/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.Specialized;

namespace PrimaryTestSuite
{
    [TestClass]
    public class EmtfTests
    {
        [TestMethod]
        [Description("Verifies that no EMTF types are compiled if the symbol DISABLE_EMTF is defined")]
        public void DisableEmtf()
        {
            using (CSharpCodeProvider codeProvider = new CSharpCodeProvider(new Dictionary<String, String> { { "CompilerVersion", "v3.5" } }))
            {
                CompilerParameters options = new CompilerParameters();
                options.GenerateInMemory = true;
                options.CompilerOptions = "/define:DISABLE_EMTF";
                options.ReferencedAssemblies.Add("System.Core.dll");

                String[] emtfSourceFiles = Directory.GetFiles(".\\..\\..\\..\\Silverlight\\Emtf\\", "*.cs");
                Assert.AreEqual(25, emtfSourceFiles.Length);

                CompilerResults results = codeProvider.CompileAssemblyFromFile(options, emtfSourceFiles);

                Assert.AreEqual(0, (from CompilerError e in results.Errors where !e.IsWarning select e).Count(), "EMTF build failed.");
                Assert.AreEqual(0, results.CompiledAssembly.GetTypes().Length, "EMTF source contains types declared outside an #if !DISABLE_EMTF directive.");
            }
        }
    }
}