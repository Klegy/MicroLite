﻿// -----------------------------------------------------------------------
// <copyright file="MemberInfoExtensions.cs" company="Project Contributors">
// Copyright Project Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Reflection;

namespace MicroLite.Mapping
{
    internal static class MemberInfoExtensions
    {
        internal static T GetAttribute<T>(this MemberInfo memberInfo, bool inherit)
            where T : Attribute
            => memberInfo.GetCustomAttributes(typeof(T), inherit) is T[] attributes && attributes.Length == 1 ? attributes[0] : null;
    }
}
