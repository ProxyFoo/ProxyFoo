// Copyright © 2014, Silverlake Software LLC.  All Rights Reserved.
// SILVERLAKE SOFTWARE LLC CONFIDENTIAL INFORMATION
//
// Created by Jamie da Silva on 3/11/2014 10:25 PM
// $Id: $
// $Author: $
// $DateTime: $
// $Change: $

using System;
using System.Reflection;
using System.Reflection.Emit;
using ProxyFoo.Core.Foo;

namespace ProxyFoo.Core
{
    public interface IProxyModuleCoderAccess
    {
        ModuleBuilder ModuleBuilder { get; }
        string AssemblyName { get; }
        IFooType GetTypeFromProxyClassDescriptor(ProxyClassDescriptor pcd);
        FieldInfo GetProxyModuleField();
    }
}