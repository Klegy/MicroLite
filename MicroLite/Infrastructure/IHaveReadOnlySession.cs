﻿// -----------------------------------------------------------------------
// <copyright file="IHaveReadOnlySession.cs" company="Project Contributors">
// Copyright Project Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// </copyright>
// -----------------------------------------------------------------------
namespace MicroLite.Infrastructure
{
    /// <summary>
    /// An interface for classes which have an <see cref="IReadOnlySession"/> property.
    /// </summary>
    public interface IHaveReadOnlySession
    {
        /// <summary>
        /// Gets the read-only MicroLite session.
        /// </summary>
        IReadOnlySession Session { get; }
    }
}
