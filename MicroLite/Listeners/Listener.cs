﻿// -----------------------------------------------------------------------
// <copyright file="Listener.cs" company="Project Contributors">
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
using System.Collections.Generic;
using MicroLite.Collections;

namespace MicroLite.Listeners
{
    /// <summary>
    /// Static entry point for listener collections.
    /// </summary>
    public static class Listener
    {
        /// <summary>
        /// Gets the listener collection which contains all registered <see cref="IDeleteListener"/>s.
        /// </summary>
        public static IList<IDeleteListener> DeleteListeners { get; } = new StackCollection<IDeleteListener>();

        /// <summary>
        /// Gets the listener collection which contains all registered <see cref="IInsertListener"/>s.
        /// </summary>
        public static IList<IInsertListener> InsertListener { get; } = new StackCollection<IInsertListener>();

        /// <summary>
        /// Gets the listener collection which contains all registered <see cref="IUpdateListener"/>s.
        /// </summary>
        public static IList<IUpdateListener> UpdateListeners { get; } = new StackCollection<IUpdateListener>();
    }
}
