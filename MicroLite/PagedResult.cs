﻿// -----------------------------------------------------------------------
// <copyright file="PagedResult.cs" company="Project Contributors">
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

namespace MicroLite
{
    /// <summary>
    /// A class which contains the result of a paged query.
    /// </summary>
    /// <typeparam name="T">The type of object the contained in the results.</typeparam>
    [System.Diagnostics.DebuggerDisplay("Page {Page} of {TotalPages} showing {ResultsPerPage} results per page with a total of {TotalResults} results")]
    public sealed class PagedResult<T>
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="PagedResult&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="page">The page number for the results.</param>
        /// <param name="results">The results in the page.</param>
        /// <param name="resultsPerPage">The number of results per page.</param>
        /// <param name="totalResults">The total number of results for the query.</param>
        public PagedResult(int page, IList<T> results, int resultsPerPage, int totalResults)
        {
            Page = page;
            Results = results;
            ResultsPerPage = resultsPerPage;
            TotalResults = totalResults;
        }

        /// <summary>
        /// Gets a value indicating whether this page contains any results.
        /// </summary>
        public bool HasResults => Results.Count > 0;

        /// <summary>
        /// Gets a value indicating whether there are more results available.
        /// </summary>
        public bool MoreResultsAvailable => Page < TotalPages;

        /// <summary>
        /// Gets the page number for the results.
        /// </summary>
        public int Page { get; }

        /// <summary>
        /// Gets the results in the page.
        /// </summary>
        public IList<T> Results { get; }

        /// <summary>
        /// Gets the number of results per page.
        /// </summary>
        public int ResultsPerPage { get; }

        /// <summary>
        /// Gets the total number of pages for the query.
        /// </summary>
        public int TotalPages => ((TotalResults - 1) / ResultsPerPage) + 1;

        /// <summary>
        /// Gets the total number of results for the query.
        /// </summary>
        public int TotalResults { get; }
    }
}
