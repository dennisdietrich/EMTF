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
using System.Collections.Specialized;
using System.IO;
using System.Linq;

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
                Assert.AreEqual(24, emtfSourceFiles.Length);

                String[] dynamicSourceFiles = Directory.GetFiles(".\\..\\..\\..\\Silverlight\\Emtf\\Dynamic\\", "*.cs");
                Assert.AreEqual(10, dynamicSourceFiles.Length);

                String[] silverlightLoggingSourceFiles = Directory.GetFiles(".\\..\\..\\..\\Silverlight\\Emtf\\Logging\\", "*.cs");
                Assert.AreEqual(4, silverlightLoggingSourceFiles.Length);

                String[] desktopLoggingSourceFiles = Directory.GetFiles(".\\..\\..\\..\\Desktop\\Emtf\\Logging\\", "*.cs");
                Assert.AreEqual(2, desktopLoggingSourceFiles.Length);

                CompilerResults results = codeProvider.CompileAssemblyFromFile(options, emtfSourceFiles.Concat(silverlightLoggingSourceFiles).Concat(desktopLoggingSourceFiles).ToArray());

                Assert.AreEqual(0, (from CompilerError e in results.Errors where !e.IsWarning select e).Count(), "EMTF build failed.");
                Assert.AreEqual(0, results.CompiledAssembly.GetTypes().Length, "EMTF source contains types declared outside an #if !DISABLE_EMTF directive.");
            }
        }
    }
}