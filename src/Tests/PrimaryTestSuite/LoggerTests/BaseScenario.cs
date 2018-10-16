/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;

namespace LoggerTests
{
    public enum BaseScenario
    {
        AllScenariosNonConcurrentRun,
        CloseLogger,
        EmptyTestRun,
        FullTestNameTestRun,
        SinglePassingTestConcurrentRun,
        SingleSkippedTestConcurrentRun
    }
}