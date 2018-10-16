/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace PrimaryTestSuite.DynamicTests
{
    [TestClass]
    public class ObjectTrackerTests
    {
        [TestMethod]
        [Description("Tests well-known GUID for null")]
        public void GetObjectGuid_Null()
        {
            Assert.AreEqual<Guid>(Guid.Empty, new ObjectTracker().GetObjectGuid((Object)null));
        }

        [TestMethod]
        [Description("Tests getting a GUID for an object")]
        public void GetObjectGuid_SingleObject()
        {
            Object        o  = new Object();
            ObjectTracker ot = new ObjectTracker();

            Guid guid = ot.GetObjectGuid(o);
            Assert.AreNotEqual<Guid>(Guid.Empty, guid);
            Assert.AreEqual<Guid>(guid, ot.GetObjectGuid(o));
        }

        [TestMethod]
        [Description("Verifies that GetObjectGuid() is type agnostic")]
        public void GetObjectGuid_TypeAgnostic()
        {
            ArgumentException ae = new ArgumentException();
            Object            o  = ae;
            ObjectTracker     ot = new ObjectTracker();

            Guid guid = ot.GetObjectGuid(ae);
            Assert.AreNotEqual<Guid>(Guid.Empty, guid);
            Assert.AreEqual<Guid>(guid, ot.GetObjectGuid(o));
        }

        [TestMethod]
        [Description("Verifies that two different objects are assigned different GUIDs")]
        public void GetObjectGuid_TwoObjects()
        {
            Object        o1 = new Object();
            Object        o2 = new Object();
            ObjectTracker ot = new ObjectTracker();

            Guid g1 = ot.GetObjectGuid(o1);
            Guid g2 = ot.GetObjectGuid(o2);
            Assert.AreNotEqual<Guid>(Guid.Empty, g1);
            Assert.AreNotEqual<Guid>(Guid.Empty, g2);
            Assert.AreNotEqual<Guid>(g1, g2);
            Assert.AreEqual<Guid>(g2, ot.GetObjectGuid(o2));
            Assert.AreEqual<Guid>(g1, ot.GetObjectGuid(o1));
        }

        [TestMethod]
        [Description("Verifies that the tracker does not change the object lifetime")]
        public void GetObjectGuid_ObjectLifetime()
        {
            Object        o1 = new Object();
            Object        o2 = new Object();
            WeakReference w  = new WeakReference(o1);
            dynamic       ot = WrapperFactory.CreateInstanceWrapper(new ObjectTracker());
            Guid          g2;

            ot.GetObjectGuid(o1);
            g2 = ot.GetObjectGuid(o2);
            o1 = null;
            GC.Collect();
            Assert.IsFalse(w.IsAlive);
            Assert.AreEqual<Guid>(g2, ot.GetObjectGuid(o2));

            WaitForCleanup(ot);
            Assert.AreEqual<Guid>(g2, ot.GetObjectGuid(o2));
            Assert.AreEqual(1, ot.__references.Count);
            Assert.AreEqual(1, ot.__objectGuids.Count);
        }

        [TestMethod]
        [Description("Track two instances")]
        public void GetObjectGuid_TrackTwoInstances()
        {
            Object  o1 = new Object();
            Object  o2 = new Object();
            dynamic ot = WrapperFactory.CreateInstanceWrapper(new ObjectTracker());

            ot.GetObjectGuid(o1);
            ot.GetObjectGuid(o2);

            o1 = null;
            o2 = null;

            GC.Collect();
            ot.GetObjectGuid(new Object());
            WaitForCleanup(ot);

            Assert.AreEqual(1, ot.__references.Count);
            Assert.AreEqual(1, ot.__objectGuids.Count);
        }

        [TestMethod]
        [Description("Mini stress to verify correct locking")]
        public void GetObjectGuid_MiniStress()
        {
            Thread t1;
            Thread t2;

            dynamic     ot      = WrapperFactory.CreateInstanceWrapper(new ObjectTracker());
            DateTime    endTime = DateTime.Now.AddMinutes(1);
            ThreadStart thread  = () =>
            {
                while (DateTime.Now < endTime)
                    ot.GetObjectGuid(new Object());
            };

            (t1 = new Thread(thread)).Start();
            (t2 = new Thread(thread)).Start();

            while (DateTime.Now < endTime)
            {
                Thread.Sleep(250);

                lock (ot.__syncRoot)
                {
                    Assert.AreEqual(ot.__references.Count, ot.__objectGuids.Count);
                }
            }

            t1.Join();
            t2.Join();
            GC.Collect();

            Object o = new Object();
            ot.GetObjectGuid(o);
            WaitForCleanup(ot);
            Assert.AreEqual(1, ot.__references.Count);
            Assert.AreEqual(1, ot.__objectGuids.Count);
        }

        private static void WaitForCleanup(dynamic trackerWrapper)
        {
            while (trackerWrapper.__cleanupPendingOrRunning)
                ;
        }
    }
}