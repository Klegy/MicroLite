﻿// -----------------------------------------------------------------------
// <copyright file="PostgreSqlDbDriver.cs" company="Project Contributors">
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
using MicroLite.Characters;

namespace MicroLite.Driver
{
    /// <summary>
    /// The implementation of <see cref="IDbDriver"/> for PostgreSql server.
    /// </summary>
    internal sealed class PostgreSqlDbDriver : DbDriver
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="PostgreSqlDbDriver" /> class.
        /// </summary>
        internal PostgreSqlDbDriver()
            : base(PostgreSqlCharacters.Instance)
        {
        }

        public override bool SupportsBatchedQueries => true;

        protected override string GetCommandText(string commandText)
        {
            if (commandText is null)
            {
                throw new ArgumentNullException(nameof(commandText));
            }

            if (IsStoredProcedureCall(commandText))
            {
                int invocationCommandLength = SqlCharacters.StoredProcedureInvocationCommand.Length;
                int firstParameterPosition = SqlUtility.GetFirstParameterPosition(commandText);

                if (commandText.Contains("("))
                {
                    firstParameterPosition--;
                }

                if (firstParameterPosition > invocationCommandLength)
                {
                    return commandText
                        .Substring(invocationCommandLength, firstParameterPosition - invocationCommandLength)
                        .Trim();
                }
                else
                {
                    return commandText.Substring(invocationCommandLength, commandText.Length - invocationCommandLength).Trim();
                }
            }

            return commandText;
        }

        protected override bool IsStoredProcedureCall(string commandText)
        {
            if (commandText is null)
            {
                throw new ArgumentNullException(nameof(commandText));
            }

            return SupportsStoredProcedures
                && commandText.IndexOf("FROM", StringComparison.OrdinalIgnoreCase) == -1
                && commandText.StartsWith(SqlCharacters.StoredProcedureInvocationCommand, StringComparison.OrdinalIgnoreCase)
                && !commandText.Contains(SqlCharacters.StatementSeparator);
        }
    }
}
