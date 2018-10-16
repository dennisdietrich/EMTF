/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;

namespace Emtf.Logging
{
    [SuppressUnmanagedCodeSecurity]
    internal static class Kernel32SafeNativeMethods
    {
        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Only used internally for allocating a new console")]
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int AllocConsole();

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Only used internally for detaching the current process from the console")]
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int FreeConsole();

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Only used internally to determine if a console is associated with the current process")]
        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetConsoleWindow();
    }
}

#endif