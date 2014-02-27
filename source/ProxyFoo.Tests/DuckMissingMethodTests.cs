// Copyright © 2013, Silverlake Software LLC.  All Rights Reserved.
// SILVERLAKE SOFTWARE LLC CONFIDENTIAL INFORMATION
//
// Created by Jamie da Silva on 11/27/2013 12:19 AM
// $Id: //Lab/ProxyFoo/source/ProxyFoo.Tests/DuckMissingMethodTests.cs#3 $
// $Author: Jamie_da_Silva $
// $DateTime: 2014/01/13 21:06:36 $
// $Change: 2230 $

using System;
using NUnit.Framework;
using ProxyFoo.Attributes;

namespace ProxyFoo.Tests
{
    [TestFixture]
    public class DuckMissingMethodTests
    {
        public interface ISample
        {
            [DuckOptional]
            void Action();
        }

        [Test]
        public void MissingMethodExceptionIsThrown()
        {
            var duck = Duck.Cast<ISample>(new object());
            Assert.Throws<MissingMethodException>(() => duck.Action());
        }

    }
}