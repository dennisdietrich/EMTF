using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("EMTF 2.1 (Silverlight)")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("EMTF (Silverlight)")]
[assembly: AssemblyCopyright("Copyright © Dennis Dietrich")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("461dfd50-10bf-4a99-8ae3-df49f597434c")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("2.1.0.0")]
[assembly: AssemblyFileVersion("2.1.0.0")]

[assembly: NeutralResourcesLanguageAttribute("en")]

[assembly: SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "Emtf.Dynamic.InstanceWrapperBase.#Emtf.Dynamic.IInstanceWrapper.WrappedInstance", Justification = "InstanceWrapperBase is a base class for generated types so IInstanceWrapper will never be reimplemented and is also only useful to external callers")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId = "PostTest", Scope = "resource", Target = "Emtf.Resources.Logging.DebugLogger.resources")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId = "PreTest",  Scope = "resource", Target = "Emtf.Resources.Logging.DebugLogger.resources")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId = "PostTest", Scope = "resource", Target = "Emtf.Resources.Logging.StreamLogger.resources")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId = "PreTest",  Scope = "resource", Target = "Emtf.Resources.Logging.StreamLogger.resources")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId = "PostTest", Scope = "resource", Target = "Emtf.Resources.TestExecutor.resources")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId = "PreTest",  Scope = "resource", Target = "Emtf.Resources.TestExecutor.resources")]