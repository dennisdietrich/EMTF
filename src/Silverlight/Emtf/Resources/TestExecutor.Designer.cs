﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30109.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Emtf.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class TestExecutor {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TestExecutor() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Emtf.Resources.TestExecutor", typeof(TestExecutor).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot (de)active concurrent test runs while a test run is in progress..
        /// </summary>
        internal static string ConcurrentTestRuns_InvalidOperation {
            get {
                return ResourceManager.GetString("ConcurrentTestRuns_InvalidOperation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called..
        /// </summary>
        internal static string EndExecute_InvalidOperation {
            get {
                return ResourceManager.GetString("EndExecute_InvalidOperation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to EMTF Concurrent Test Run Thread {0}.
        /// </summary>
        internal static string ExecuteImpl_TestRunThreadName {
            get {
                return ResourceManager.GetString("ExecuteImpl_TestRunThreadName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The only supported attribute types are PreTestActionAttribute and PostTestActionAttribute..
        /// </summary>
        internal static string ExecuteTestActions_UnsupportedAttributeType {
            get {
                return ResourceManager.GetString("ExecuteTestActions_UnsupportedAttributeType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The test method is a generic method definition or open constructed method..
        /// </summary>
        internal static string IsTestMethodValid_ContainsGenericParameters {
            get {
                return ResourceManager.GetString("IsTestMethodValid_ContainsGenericParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The test method has more than one parameter or one parameter which is not of the type Emtf.TestContext..
        /// </summary>
        internal static string IsTestMethodValid_InvalidSignature {
            get {
                return ResourceManager.GetString("IsTestMethodValid_InvalidSignature", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The test method is abstract..
        /// </summary>
        internal static string IsTestMethodValid_IsAbstract {
            get {
                return ResourceManager.GetString("IsTestMethodValid_IsAbstract", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The test method is marked as a pre or post test action..
        /// </summary>
        internal static string IsTestMethodValid_IsPreOrPostTestAction {
            get {
                return ResourceManager.GetString("IsTestMethodValid_IsPreOrPostTestAction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The test method is static..
        /// </summary>
        internal static string IsTestMethodValid_IsStatic {
            get {
                return ResourceManager.GetString("IsTestMethodValid_IsStatic", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The test method is not public..
        /// </summary>
        internal static string IsTestMethodValid_NotPublic {
            get {
                return ResourceManager.GetString("IsTestMethodValid_NotPublic", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The return type of the test method is not System.Void..
        /// </summary>
        internal static string IsTestMethodValid_ReturnTypeNotVoid {
            get {
                return ResourceManager.GetString("IsTestMethodValid_ReturnTypeNotVoid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Event handler execution cannot be marshaled since the test executor was created on a thread without a synchronization context..
        /// </summary>
        internal static string MarshalEventHandlerExecution_NoSynchronizationContext {
            get {
                return ResourceManager.GetString("MarshalEventHandlerExecution_NoSynchronizationContext", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A test run is already in progress..
        /// </summary>
        internal static string PrepareTestRun_InvalidOperation {
            get {
                return ResourceManager.GetString("PrepareTestRun_InvalidOperation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An unhandled exception of the type &apos;{0}&apos; occurred during the execution of the test..
        /// </summary>
        internal static string TestRunThread_TestFailedWithException {
            get {
                return ResourceManager.GetString("TestRunThread_TestFailedWithException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test passed..
        /// </summary>
        internal static string TestRunThread_TestPassed {
            get {
                return ResourceManager.GetString("TestRunThread_TestPassed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The default constructor of type the &apos;{0}&apos; threw an exception of the type &apos;{1}&apos;..
        /// </summary>
        internal static string TryUpdateTestClassInstance_ConstructorThrewException {
            get {
                return ResourceManager.GetString("TryUpdateTestClassInstance_ConstructorThrewException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type &apos;{0}&apos; is a generic type definition or open constructed type..
        /// </summary>
        internal static string TryUpdateTestClassInstance_ContainsGenericParameters {
            get {
                return ResourceManager.GetString("TryUpdateTestClassInstance_ContainsGenericParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type &apos;{0}&apos; is an abstract class..
        /// </summary>
        internal static string TryUpdateTestClassInstance_IsAbstract {
            get {
                return ResourceManager.GetString("TryUpdateTestClassInstance_IsAbstract", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type &apos;{0}&apos; is not a class..
        /// </summary>
        internal static string TryUpdateTestClassInstance_IsNotClass {
            get {
                return ResourceManager.GetString("TryUpdateTestClassInstance_IsNotClass", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type &apos;{0}&apos; is not public..
        /// </summary>
        internal static string TryUpdateTestClassInstance_IsNotPublic {
            get {
                return ResourceManager.GetString("TryUpdateTestClassInstance_IsNotPublic", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type &apos;{0}&apos; does not have a public default constructor..
        /// </summary>
        internal static string TryUpdateTestClassInstance_NoPublicDefaultConstructor {
            get {
                return ResourceManager.GetString("TryUpdateTestClassInstance_NoPublicDefaultConstructor", resourceCulture);
            }
        }
    }
}
