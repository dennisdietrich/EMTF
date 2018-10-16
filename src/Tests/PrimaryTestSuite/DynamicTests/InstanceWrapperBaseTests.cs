/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PrimaryTestSuite.DynamicTests
{
    [TestClass]
    public class InstanceWrapperBaseTests
    {
        [TestMethod]
        [Description("Verifies the implementation of IInstanceWrapper.WrappedInstance")]
        public void GetWrappedInstance()
        {
            Object o = new Object();
            InstanceWrapperBase wrapper = WrapperFactory.CreateConstructorWrapper(typeof(InstanceWrapperBase)).CreateInstance(o);
            Assert.AreSame(o, ((IInstanceWrapper)wrapper).WrappedInstance);
        }

        [TestMethod]
        [Description("Verifies the methods that hide the public methods on System.Object")]
        public void PublicMethods()
        {
            dynamic factory = WrapperFactory.CreateConstructorWrapper(typeof(InstanceWrapperBase));

            DateTime dateTime = DateTime.Now;
            {
                InstanceWrapperBase wrapper = factory.CreateInstance(dateTime);
                Assert.IsTrue(wrapper.Equals(dateTime));
                Assert.AreEqual(dateTime.GetHashCode(), wrapper.GetHashCode());
                Assert.AreEqual(typeof(DateTime), wrapper.GetType());
                Assert.AreEqual(dateTime.ToString(), wrapper.ToString());
            }
            {
                dynamic wrapper = factory.CreateInstance(dateTime);
                Assert.IsTrue(wrapper.Equals(dateTime));
                Assert.AreEqual(dateTime.GetHashCode(), wrapper.GetHashCode());
                Assert.AreEqual(typeof(DateTime), wrapper.GetType());
                Assert.AreEqual(dateTime.ToString(), wrapper.ToString());
            }
        }
    }
}