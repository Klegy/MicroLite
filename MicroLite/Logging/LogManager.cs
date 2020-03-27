﻿// -----------------------------------------------------------------------
// <copyright file="LogManager.cs" company="Project Contributors">
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
using System.Diagnostics;

namespace MicroLite.Logging
{
    /// <summary>
    /// A class which the MicroLite ORM framework can call to resolve an ILog implementation.
    /// </summary>
    public static class LogManager
    {
        /// <summary>
        /// Gets or sets the function which can be called by MicroLite to resolve the <see cref="ILog"/> to use.
        /// </summary>
        internal static Func<Type, ILog> GetLogger { get; set; }

        /// <summary>
        /// Gets the log for the current (calling) class.
        /// </summary>
        /// <returns>The <see cref="ILog"/> for the class which called the method.</returns>
        public static ILog GetCurrentClassLog()
        {
            Func<Type, ILog> getLogger = GetLogger;

            if (getLogger != null)
            {
                var stackFrame = new StackFrame(skipFrames: 1);

                return getLogger(stackFrame.GetMethod().DeclaringType);
            }

            return EmptyLog.Instance;
        }
    }
}
