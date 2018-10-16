/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf;
using Emtf.Logging;
using System;

namespace SecondaryTestSuite
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            TestExecutor  executor = new TestExecutor();
            ConsoleLogger logger   = new ConsoleLogger(executor);
            executor.Execute(new String[] { "Emtf" });
            Console.ReadKey(true);
        }
    }
}