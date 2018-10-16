/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using EmtfAssert                    = Emtf.Assert;
using EmtfSkipTestAttribute         = Emtf.SkipTestAttribute;
using EmtfTestAttribute             = Emtf.TestAttribute;
using EmtfTestCompletedEventArgs    = Emtf.TestCompletedEventArgs;
using EmtfTestContext               = Emtf.TestContext;
using EmtfTestEventArgs             = Emtf.TestEventArgs;
using EmtfTestExecutor              = Emtf.TestExecutor;
using EmtfPreTestActionAttribute    = Emtf.PreTestActionAttribute;
using EmtfTestRunCompletedEventArgs = Emtf.TestRunCompletedEventArgs;
using EmtfTestRunEventArgs          = Emtf.TestRunEventArgs;
using EmtfTestSkippedEventArgs      = Emtf.TestSkippedEventArgs;

using EmtfLogger = Emtf.Logging.Logger;

namespace LoggerTests
{
    public delegate string ExpectedLogGenerator(BaseScenario scenario, EmtfTestRunEventArgs testRunEventArgs, EmtfTestRunCompletedEventArgs testRunCompletedEventArgs, Collection<EmtfTestEventArgs> testEventArgs, Collection<EmtfTestCompletedEventArgs> testCompletedEventArgs, Collection<EmtfTestSkippedEventArgs> testSkippedEventArgs);

    public abstract class TestBase
    {
        #region Public Methods

        public abstract void AllScenariosNonConcurrentRun();
        public abstract void CloseLogger();
        public abstract void EmptyTestRun();
        public abstract void FullTestNameTestRun();
        public abstract void SinglePassingTestConcurrentRun();
        public abstract void SingleSkippedTestConcurrentRun();

        #endregion Public Methods

        #region Protected Methods

        protected abstract EmtfLogger StartLogging(EmtfTestExecutor executor, BaseScenario scenario);

        protected abstract string StopLogging();
        protected abstract string GetExpectedLog(BaseScenario                           scenario,
                                                 EmtfTestRunEventArgs                   testRunEventArgs,
                                                 EmtfTestRunCompletedEventArgs          testRunCompletedEventArgs,
                                                 Collection<EmtfTestEventArgs>          testEventArgs,
                                                 Collection<EmtfTestCompletedEventArgs> testCompletedEventArgs,
                                                 Collection<EmtfTestSkippedEventArgs>   testSkippedEventArgs);

        protected void CommonLoggerInitialization(EmtfTestExecutor executor, EmtfLogger logger, BaseScenario scenario)
        {
            if (scenario == BaseScenario.CloseLogger)
                executor.TestStarted += (Object sender, EmtfTestEventArgs e) => logger.Close();

            if (scenario == BaseScenario.FullTestNameTestRun)
                logger.UseFullTestName = true;
        }

        protected void RunScenario(BaseScenario scenario, String logDirectory, String logFileBaseName)
        {
            RunScenario(scenario, logDirectory, logFileBaseName, null, null);
        }

        protected void RunScenario(BaseScenario scenario, String logDirectory, String logFileBaseName, Action<EmtfTestExecutor, EmtfLogger> customInitialization, ExpectedLogGenerator logGenerator)
        {
            EmtfTestRunEventArgs          testRunEventArgs          = null;
            EmtfTestRunCompletedEventArgs testRunCompletedEventArgs = null;

            Collection<EmtfTestEventArgs>          testEventArgs          = new Collection<EmtfTestEventArgs>();
            Collection<EmtfTestCompletedEventArgs> testCompletedEventArgs = new Collection<EmtfTestCompletedEventArgs>();
            Collection<EmtfTestSkippedEventArgs>   testSkippedEventArgs   = new Collection<EmtfTestSkippedEventArgs>();

            EmtfTestExecutor executor = new EmtfTestExecutor();

            executor.TestRunStarted   += (object sender, EmtfTestRunEventArgs e)          => testRunEventArgs = e;
            executor.TestRunCompleted += (object sender, EmtfTestRunCompletedEventArgs e) => testRunCompletedEventArgs = e;
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)             => testEventArgs.Add(e);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e)    => testCompletedEventArgs.Add(e);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)      => testSkippedEventArgs.Add(e);

            EmtfLogger logger = StartLogging(executor, scenario);

            if (customInitialization != null)
                customInitialization(executor, logger);

            switch (scenario)
            {
                case BaseScenario.AllScenariosNonConcurrentRun:
                    executor.Execute(typeof(AllScenarios_01).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Concat(typeof(AllScenarios_02).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)).Concat(typeof(AllScenarios_03<>).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)));
                    break;
                case BaseScenario.CloseLogger:
                    executor.Execute(new MethodInfo[] { typeof(AllScenarios_01).GetMethod("_01_Passing_NoDescription") });
                    break;
                case BaseScenario.EmptyTestRun:
                    executor.Execute(new MethodInfo[0]);
                    break;
                case BaseScenario.FullTestNameTestRun:
                    executor.Execute(typeof(AllScenarios_01).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Concat(typeof(AllScenarios_02).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)).Concat(typeof(AllScenarios_03<>).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)));
                    break;
                case BaseScenario.SinglePassingTestConcurrentRun:
                    executor.ConcurrentTestRuns = true;
                    executor.Execute(new MethodInfo[] { typeof(AllScenarios_01).GetMethod("_01_Passing_NoDescription") });
                    break;
                case BaseScenario.SingleSkippedTestConcurrentRun:
                    executor.ConcurrentTestRuns = true;
                    executor.Execute(new MethodInfo[] { typeof(AllScenarios_01).GetMethod("_04_Skip_SkipTestDefined_NoDescription") });
                    break;
                default:
                    throw new ArgumentException("Undefined or unknown scenario.", "scenario");
            }

            String actualLog = StopLogging();
            String expectedLog;

            if (logGenerator == null)
                expectedLog = GetExpectedLog(scenario, testRunEventArgs, testRunCompletedEventArgs, testEventArgs, testCompletedEventArgs, testSkippedEventArgs);
            else
                expectedLog = logGenerator(scenario, testRunEventArgs, testRunCompletedEventArgs, testEventArgs, testCompletedEventArgs, testSkippedEventArgs);

            Directory.CreateDirectory(logDirectory);
            File.WriteAllText(Path.Combine(logDirectory, logFileBaseName + "Expected.log"), expectedLog, Encoding.Unicode);
            File.WriteAllText(Path.Combine(logDirectory, logFileBaseName + "Actual.log"), actualLog, Encoding.Unicode);

            Assert.AreEqual(expectedLog, actualLog);
        }

        protected string GetBaseline(string name)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
                using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
                    return reader.ReadToEnd();
        }

        #endregion Protected Methods

        #region Nested Types

        public class AllScenarios_01
        {
            [EmtfTestAttribute]
            public void _01_Passing_NoDescription()
            {
            }

            [EmtfTestAttribute("This is the test description")]
            public void _02_Passing_WithDescription()
            {
            }

            [EmtfTestAttribute]
            public void _03_Passing_WithCustomLog(EmtfTestContext context)
            {
                context.LogLine("This is the custom log");
            }

            [EmtfTestAttribute]
            [EmtfSkipTestAttribute]
            public void _04_Skip_SkipTestDefined_NoDescription()
            {
            }

            [EmtfTestAttribute]
            [EmtfSkipTestAttribute("This is the skip reason")]
            public void _05_Skip_SkipTestDefined_WithDescription()
            {
            }

            [EmtfTestAttribute]
            public void _06_Skip_MethodNotSupported(object o1, object o2)
            {
            }

            [EmtfTestAttribute]
            [EmtfPreTestActionAttribute]
            public void _07_Skip_PreTestActionDefined()
            {
            }

            [EmtfTestAttribute]
            public void _08_Abort_NoDescription(EmtfTestContext context)
            {
                context.AbortTest();
            }

            [EmtfTestAttribute]
            public void _09_Abort_WithDescription(EmtfTestContext context)
            {
                context.AbortTest("This is the abort description");
            }

            [EmtfTestAttribute]
            public void _10_Fail_Assert_NoDescription()
            {
                EmtfAssert.IsTrue(false);
            }

            [EmtfTestAttribute]
            public void _11_Fail_Assert_WithDescription()
            {
                EmtfAssert.IsTrue(false, "This is the assert message");
            }

            [EmtfTestAttribute]
            public void _12_Throws_NotImplementedException()
            {
                throw new NotImplementedException();
            }
        }

        public class AllScenarios_02
        {
            public AllScenarios_02()
            {
                throw new NotImplementedException();
            }

            [EmtfTestAttribute]
            public void _01_Skip_ConstructorThrows()
            {
            }
        }

        public class AllScenarios_03<T>
        {
            [EmtfTestAttribute]
            public void _01_Skip_TypeNotSupported()
            {
            }
        }

        #endregion Nested Types
    }
}