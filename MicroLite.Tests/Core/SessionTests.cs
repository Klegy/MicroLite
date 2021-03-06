﻿namespace MicroLite.Tests.Core
{
    using System;
    using System.Data;
    using MicroLite.Core;
    using MicroLite.Dialect;
    using MicroLite.Driver;
    using MicroLite.Listeners;
    using MicroLite.Mapping;
    using MicroLite.Tests.TestEntities;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for the <see cref="Session"/> class.
    /// </summary>
    public class SessionTests : UnitTest
    {
        [Fact]
        public void AdvancedReturnsSameSessionByDifferentInterface()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            var advancedSession = session.Advanced;

            Assert.Same(session, advancedSession);
        }

        [Fact]
        public void DeleteInstanceInvokesListeners()
        {
            var customer = new Customer
            {
                Id = 187224
            };

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildDeleteSqlQuery(It.IsNotNull<IObjectInfo>(), customer.Id)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Returns(1);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            int counter = 0;

            var mockListener1 = new Mock<IDeleteListener>();
            mockListener1.Setup(x => x.AfterDelete(customer, 1)).Callback(() => Assert.Equal(4, ++counter));
            mockListener1.Setup(x => x.BeforeDelete(customer)).Callback(() => Assert.Equal(1, ++counter));

            var mockListener2 = new Mock<IDeleteListener>();
            mockListener2.Setup(x => x.AfterDelete(customer, 1)).Callback(() => Assert.Equal(3, ++counter));
            mockListener2.Setup(x => x.BeforeDelete(customer)).Callback(() => Assert.Equal(2, ++counter));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners(
                    new[] { mockListener1.Object, mockListener2.Object },
                    new IInsertListener[0],
                    new IUpdateListener[0]));

            session.DeleteAsync(customer).Wait();

            mockListener1.VerifyAll();
            mockListener2.VerifyAll();
        }

        [Fact]
        public void DeleteInstanceReturnsFalseIfNoRecordsDeleted()
        {
            var customer = new Customer
            {
                Id = 14556
            };

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildDeleteSqlQuery(It.IsNotNull<IObjectInfo>(), customer.Id)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Returns(0);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            Assert.False(session.DeleteAsync(customer).Result);

            mockSqlDialect.VerifyAll();
            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void DeleteInstanceReturnsTrueIfRecordDeleted()
        {
            var customer = new Customer
            {
                Id = 14556
            };

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildDeleteSqlQuery(It.IsNotNull<IObjectInfo>(), customer.Id)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Returns(1);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            Assert.True(session.DeleteAsync(customer).Result);

            mockSqlDialect.VerifyAll();
            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void DeleteInstanceThrowsArgumentNullExceptionForNullInstance()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            var exception = Assert.Throws<ArgumentNullException>(
                () => session.DeleteAsync(null).Result);

            Assert.Equal("instance", exception.ParamName);
        }

        [Fact]
        public void DeleteInstanceThrowsMicroLiteExceptionIfExecuteNonQueryThrowsException()
        {
            var customer = new Customer
            {
                Id = 187224
            };

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildDeleteSqlQuery(It.IsNotNull<IObjectInfo>(), customer.Id)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Throws<InvalidOperationException>();

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            var exception = Assert.Throws<AggregateException>(() => session.DeleteAsync(customer).Wait());

            Assert.IsType<MicroLiteException>(exception.InnerException);
            Assert.IsType<InvalidOperationException>(exception.InnerException.InnerException);
            Assert.Equal(exception.InnerException.InnerException.Message, exception.InnerException.Message);

            // Command should still be disposed.
            mockCommand.VerifyAll();
        }

        [Fact]
        public void DeleteInstanceThrowsMicroLiteExceptionIfIdentifierNotSet()
        {
            var customer = new Customer
            {
                Id = 0
            };

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(new Mock<IDbCommand>().Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                mockDbDriver.Object,
                new SessionListeners());

            var exception = Assert.Throws<AggregateException>(
                () => session.DeleteAsync(customer).Result);

            Assert.IsType<MicroLiteException>(exception.InnerException);
            Assert.Equal(ExceptionMessages.Session_IdentifierNotSetForDelete, exception.InnerException.Message);
        }

        [Fact]
        public void DeleteInstanceThrowsObjectDisposedExceptionIfDisposed()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            using (session)
            {
            }

            Assert.Throws<ObjectDisposedException>(() => session.DeleteAsync(new Customer()).Result);
        }

        [Fact]
        public void DeleteTypeByIdentifierReturnsFalseIfNoRecordsDeleted()
        {
            var type = typeof(Customer);
            var identifier = 1234;

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildDeleteSqlQuery(It.IsNotNull<IObjectInfo>(), identifier)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Returns(0);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            Assert.False(session.DeleteAsync(type, identifier).Result);

            mockSqlDialect.VerifyAll();
            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void DeleteTypeByIdentifierReturnsTrueIfRecordDeleted()
        {
            var type = typeof(Customer);
            var identifier = 1234;

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildDeleteSqlQuery(It.IsNotNull<IObjectInfo>(), identifier)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Returns(1);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            Assert.True(session.DeleteAsync(type, identifier).Result);

            mockSqlDialect.VerifyAll();
            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void DeleteTypeByIdentifierThrowsArgumentNullExceptionForNullIdentifier()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            var exception = Assert.Throws<ArgumentNullException>(
                () => session.DeleteAsync(typeof(Customer), null).Result);

            Assert.Equal("identifier", exception.ParamName);
        }

        [Fact]
        public void DeleteTypeByIdentifierThrowsArgumentNullExceptionForNullType()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            var exception = Assert.Throws<ArgumentNullException>(
                () => session.DeleteAsync(null, 1234).Result);

            Assert.Equal("type", exception.ParamName);
        }

        [Fact]
        public void DeleteTypeByIdentifierThrowsMicroLiteExceptionIfExecuteNonQueryThrowsException()
        {
            var type = typeof(Customer);
            var identifier = 1234;

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildDeleteSqlQuery(It.IsNotNull<IObjectInfo>(), identifier)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Throws<InvalidOperationException>();

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            var exception = Assert.Throws<AggregateException>(() => session.DeleteAsync(type, identifier).Result);

            Assert.IsType<MicroLiteException>(exception.InnerException);
            Assert.IsType<InvalidOperationException>(exception.InnerException.InnerException);
            Assert.Equal(exception.InnerException.InnerException.Message, exception.InnerException.Message);

            // Command should still be disposed.
            mockCommand.VerifyAll();
        }

        [Fact]
        public void DeleteTypeByIdentifierThrowsObjectDisposedExceptionIfDisposed()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            using (session)
            {
            }

            Assert.Throws<ObjectDisposedException>(() => session.DeleteAsync(typeof(Customer), 1234).Result);
        }

        [Fact]
        public void ExecuteBuildsAndExecutesCommandNotInTransaction()
        {
            var result = 1;

            var mockSqlDialect = new Mock<ISqlDialect>();

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Returns(result);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            Assert.Equal(result, session.ExecuteAsync(new SqlQuery("")).Result);

            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void ExecuteScalarBuildsAndExecutesCommand()
        {
            var result = new object();

            var mockSqlDialect = new Mock<ISqlDialect>();

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteScalar()).Returns(result);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            Assert.Equal(result, session.ExecuteScalarAsync<object>(new SqlQuery("")).Result);

            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void ExecuteScalarThrowsArgumentNullExceptionForNullSqlQuery()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            var exception = Assert.Throws<ArgumentNullException>(
                () => session.ExecuteScalarAsync<object>(null).Result);

            Assert.Equal("sqlQuery", exception.ParamName);
        }

        [Fact]
        public void ExecuteScalarThrowsObjectDisposedExceptionIfDisposed()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            using (session)
            {
            }

            Assert.Throws<ObjectDisposedException>(() => session.ExecuteScalarAsync<int>(new SqlQuery("SELECT")).Result);
        }

        [Fact]
        public void ExecuteScalarUsesTypeConvertersToResolveResultType()
        {
            var result = (byte)1;

            var mockSqlDialect = new Mock<ISqlDialect>();

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteScalar()).Returns(result);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            Assert.Equal((CustomerStatus)result, session.ExecuteScalarAsync<CustomerStatus>(new SqlQuery("")).Result);

            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void ExecuteThrowsArgumentNullExceptionForNullSqlQuery()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            var exception = Assert.Throws<ArgumentNullException>(
                () => session.ExecuteAsync(null).Result);

            Assert.Equal("sqlQuery", exception.ParamName);
        }

        [Fact]
        public void ExecuteThrowsObjectDisposedExceptionIfDisposed()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            using (session)
            {
            }

            Assert.Throws<ObjectDisposedException>(() => session.ExecuteAsync(new SqlQuery("SELECT")).Result);
        }

        [Fact]
        public void InsertBuildsAndExecutesAnInsertCommandOnlyIfIdentifierStrategyAssigned()
        {
            ObjectInfo.MappingConvention = new ConventionMappingConvention(
                UnitTest.GetConventionMappingSettings(IdentifierStrategy.Assigned));

            var customer = new Customer
            {
                Id = 12345
            };

            var insertSqlQuery = new SqlQuery("INSERT");

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildInsertSqlQuery(It.IsNotNull<IObjectInfo>(), customer)).Returns(insertSqlQuery);
            mockSqlDialect.Setup(x => x.SupportsSelectInsertedIdentifier).Returns(false);

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Returns(1);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            session.InsertAsync(customer).Wait();

            mockSqlDialect.Verify(x => x.BuildInsertSqlQuery(It.IsNotNull<IObjectInfo>(), customer), Times.Once());
            mockSqlDialect.Verify(x => x.BuildSelectInsertIdSqlQuery(It.IsAny<IObjectInfo>()), Times.Never());

            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void InsertBuildsAndExecutesAnInsertCommandOnlyIfIdentifierStrategyNotAssignedAndSqlDialectDoesNotSupportSelectInsertedIdentifier()
        {
            ObjectInfo.MappingConvention = new ConventionMappingConvention(
                UnitTest.GetConventionMappingSettings(IdentifierStrategy.DbGenerated));

            var customer = new Customer();
            var insertSqlQuery = new SqlQuery("INSERT");
            object identifier = 23543;

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildInsertSqlQuery(It.IsNotNull<IObjectInfo>(), customer)).Returns(insertSqlQuery);
            mockSqlDialect.Setup(x => x.SupportsSelectInsertedIdentifier).Returns(false);

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteScalar()).Returns(identifier);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            session.InsertAsync(customer).Wait();

            mockSqlDialect.Verify(x => x.BuildInsertSqlQuery(It.IsNotNull<IObjectInfo>(), customer), Times.Once());
            mockSqlDialect.Verify(x => x.BuildSelectInsertIdSqlQuery(It.IsAny<IObjectInfo>()), Times.Never());

            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void InsertBuildsAndExecutesCombinedCommandIfIdentifierStrategyNotAssignedAndSqlDialectSupportsSelectInsertedIdentifierAndDbDriverSupportsBatchedQueries()
        {
            ObjectInfo.MappingConvention = new ConventionMappingConvention(
                UnitTest.GetConventionMappingSettings(IdentifierStrategy.DbGenerated));

            var customer = new Customer();
            var insertSqlQuery = new SqlQuery("INSERT");
            var selectIdSqlQuery = new SqlQuery("SELECT");
            var combinedSqlQuery = new SqlQuery("INSERT;SELECT");
            object identifier = 23543;

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildInsertSqlQuery(It.IsNotNull<IObjectInfo>(), customer)).Returns(insertSqlQuery);
            mockSqlDialect.Setup(x => x.BuildSelectInsertIdSqlQuery(It.IsNotNull<IObjectInfo>())).Returns(selectIdSqlQuery);
            mockSqlDialect.Setup(x => x.SupportsSelectInsertedIdentifier).Returns(true);

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteScalar()).Returns(identifier);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));
            mockDbDriver.Setup(x => x.SupportsBatchedQueries).Returns(true);
            mockDbDriver.Setup(x => x.Combine(insertSqlQuery, selectIdSqlQuery)).Returns(combinedSqlQuery);

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            session.InsertAsync(customer).Wait();

            mockSqlDialect.VerifyAll();
            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void InsertBuildsAndExecutesIndividualCommandsIfIdentifierStrategyNotAssignedAndSqlDialectSupportsSelectInsertedIdentifierAndDbDriverDoesNotSupportBatchedQueries()
        {
            ObjectInfo.MappingConvention = new ConventionMappingConvention(
                UnitTest.GetConventionMappingSettings(IdentifierStrategy.DbGenerated));

            var customer = new Customer();
            var insertSqlQuery = new SqlQuery("INSERT");
            var selectIdSqlQuery = new SqlQuery("SELECT");
            object identifier = 23543;

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildInsertSqlQuery(It.IsNotNull<IObjectInfo>(), customer)).Returns(insertSqlQuery);
            mockSqlDialect.Setup(x => x.BuildSelectInsertIdSqlQuery(It.IsNotNull<IObjectInfo>())).Returns(selectIdSqlQuery);
            mockSqlDialect.Setup(x => x.SupportsSelectInsertedIdentifier).Returns(true);

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery());
            mockCommand.Setup(x => x.ExecuteScalar()).Returns(identifier);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));
            mockDbDriver.Setup(x => x.SupportsBatchedQueries).Returns(false);

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            session.InsertAsync(customer).Wait();

            mockSqlDialect.VerifyAll();
            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void InsertInvokesListeners()
        {
            var customer = new Customer();
            object identifier = 23543;

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildInsertSqlQuery(It.IsNotNull<IObjectInfo>(), customer)).Returns(new SqlQuery(""));
            mockSqlDialect.Setup(x => x.BuildSelectInsertIdSqlQuery(It.IsNotNull<IObjectInfo>())).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteScalar()).Returns(identifier);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            int counter = 0;

            var mockListener1 = new Mock<IInsertListener>();
            mockListener1.Setup(x => x.AfterInsert(customer, identifier)).Callback(() => Assert.Equal(4, ++counter));
            mockListener1.Setup(x => x.BeforeInsert(customer)).Callback(() => Assert.Equal(1, ++counter));

            var mockListener2 = new Mock<IInsertListener>();
            mockListener2.Setup(x => x.AfterInsert(customer, identifier)).Callback(() => Assert.Equal(3, ++counter));
            mockListener2.Setup(x => x.BeforeInsert(customer)).Callback(() => Assert.Equal(2, ++counter));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners(
                    new IDeleteListener[0],
                    new[] { mockListener1.Object, mockListener2.Object },
                    new IUpdateListener[0]));

            session.InsertAsync(customer).Wait();

            mockListener1.VerifyAll();
            mockListener2.VerifyAll();
        }

        [Fact]
        public void InsertThrowsArgumentNullExceptionForNullInstance()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            var exception = Assert.Throws<ArgumentNullException>(
                () => session.InsertAsync(null).Wait());

            Assert.Equal("instance", exception.ParamName);
        }

        [Fact]
        public void InsertThrowsMicroLiteExceptionIfExecuteScalarThrowsException()
        {
            var customer = new Customer();

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildInsertSqlQuery(It.IsNotNull<IObjectInfo>(), customer)).Returns(new SqlQuery(""));
            mockSqlDialect.Setup(x => x.BuildSelectInsertIdSqlQuery(It.IsNotNull<IObjectInfo>())).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteScalar()).Throws<InvalidOperationException>();

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            var exception = Assert.Throws<AggregateException>(() => session.InsertAsync(customer).Wait());

            Assert.IsType<MicroLiteException>(exception.InnerException);
            Assert.IsType<InvalidOperationException>(exception.InnerException.InnerException);
            Assert.Equal(exception.InnerException.InnerException.Message, exception.InnerException.Message);

            // Command should still be disposed.
            mockCommand.VerifyAll();
        }

        [Fact]
        public void InsertThrowsObjectDisposedExceptionIfDisposed()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            using (session)
            {
            }

            Assert.Throws<ObjectDisposedException>(() => session.InsertAsync(new Customer()).Wait());
        }

        [Fact]
        public void UpdateInstanceBuildsAndExecutesQuery()
        {
            var customer = new Customer
            {
                Id = 187224
            };

            var rowsAffected = 1;

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildUpdateSqlQuery(It.IsNotNull<IObjectInfo>(), customer)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Returns(rowsAffected);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            session.UpdateAsync(customer).Wait();

            mockSqlDialect.VerifyAll();
            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void UpdateInstanceInvokesListeners()
        {
            var customer = new Customer
            {
                Id = 187224
            };

            var rowsAffected = 1;

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildUpdateSqlQuery(It.IsNotNull<IObjectInfo>(), customer)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Returns(rowsAffected);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            int counter = 0;

            var mockListener1 = new Mock<IUpdateListener>();
            mockListener1.Setup(x => x.AfterUpdate(customer, 1)).Callback(() => Assert.Equal(4, ++counter));
            mockListener1.Setup(x => x.BeforeUpdate(customer)).Callback(() => Assert.Equal(1, ++counter));

            var mockListener2 = new Mock<IUpdateListener>();
            mockListener2.Setup(x => x.AfterUpdate(customer, 1)).Callback(() => Assert.Equal(3, ++counter));
            mockListener2.Setup(x => x.BeforeUpdate(customer)).Callback(() => Assert.Equal(2, ++counter));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners(
                    new IDeleteListener[0],
                    new IInsertListener[0],
                    new[] { mockListener1.Object, mockListener2.Object }));

            session.UpdateAsync(customer).Wait();

            mockListener1.VerifyAll();
            mockListener2.VerifyAll();
        }

        [Fact]
        public void UpdateInstanceReturnsFalseIfNoRecordsUpdated()
        {
            var customer = new Customer
            {
                Id = 187224
            };

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildUpdateSqlQuery(It.IsNotNull<IObjectInfo>(), customer)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Returns(0);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            Assert.False(session.UpdateAsync(customer).Result);

            mockSqlDialect.VerifyAll();
            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void UpdateInstanceReturnsTrueIfRecordUpdated()
        {
            var customer = new Customer
            {
                Id = 187224
            };

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildUpdateSqlQuery(It.IsNotNull<IObjectInfo>(), customer)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Returns(1);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            Assert.True(session.UpdateAsync(customer).Result);

            mockSqlDialect.VerifyAll();
            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void UpdateInstanceThrowsArgumentNullExceptionForNullInstance()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            var exception = Assert.Throws<ArgumentNullException>(
                () => session.UpdateAsync((Customer)null).Result);

            Assert.Equal("instance", exception.ParamName);
        }

        [Fact]
        public void UpdateInstanceThrowsMicroLiteExceptionIfExecuteNonQueryThrowsException()
        {
            var customer = new Customer
            {
                Id = 187224
            };

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildUpdateSqlQuery(It.IsNotNull<IObjectInfo>(), customer)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Throws<InvalidOperationException>();

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            var exception = Assert.Throws<AggregateException>(() => session.UpdateAsync(customer).Wait());

            Assert.IsType<MicroLiteException>(exception.InnerException);
            Assert.IsType<InvalidOperationException>(exception.InnerException.InnerException);
            Assert.Equal(exception.InnerException.InnerException.Message, exception.InnerException.Message);

            // Command should still be disposed.
            mockCommand.VerifyAll();
        }

        [Fact]
        public void UpdateInstanceThrowsMicroLiteExceptionIfIdentifierNotSet()
        {
            var customer = new Customer
            {
                Id = 0
            };

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(new Mock<IDbConnection>().Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                mockDbDriver.Object,
                new SessionListeners());

            var exception = Assert.Throws<AggregateException>(() => session.UpdateAsync(customer).Wait());

            Assert.IsType<MicroLiteException>(exception.InnerException);
            Assert.Equal(ExceptionMessages.Session_IdentifierNotSetForUpdate, exception.InnerException.Message);
        }

        [Fact]
        public void UpdateInstanceThrowsObjectDisposedExceptionIfDisposed()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            using (session)
            {
            }

            Assert.Throws<ObjectDisposedException>(() => session.UpdateAsync(new Customer()).Result);
        }

        [Fact]
        public void UpdateObjectDeltaReturnsFalseIfNoRecordUpdated()
        {
            var objectDelta = new ObjectDelta(typeof(Customer), 1234);
            objectDelta.AddChange("Name", "Fred Flintstone");

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildUpdateSqlQuery(objectDelta)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Returns(0);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            Assert.False(session.UpdateAsync(objectDelta).Result);

            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void UpdateObjectDeltaReturnsTrueIfRecordUpdated()
        {
            var objectDelta = new ObjectDelta(typeof(Customer), 1234);
            objectDelta.AddChange("Name", "Fred Flintstone");

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildUpdateSqlQuery(objectDelta)).Returns(new SqlQuery(""));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteNonQuery()).Returns(1);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new Session(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object,
                new SessionListeners());

            Assert.True(session.UpdateAsync(objectDelta).Result);

            mockDbDriver.VerifyAll();
            mockCommand.VerifyAll();
        }

        [Fact]
        public void UpdateObjectDeltaThrowsArgumentNullExceptionForNullObjectDelta()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            var exception = Assert.Throws<ArgumentNullException>(
                () => session.UpdateAsync(default(ObjectDelta)).Result);

            Assert.Equal("objectDelta", exception.ParamName);
        }

        [Fact]
        public void UpdateObjectDeltaThrowsMicroLiteExceptionIfItContainsNoChanges()
        {
            var objectDelta = new ObjectDelta(typeof(Customer), 1234);

            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            var exception = Assert.Throws<MicroLiteException>(() => session.UpdateAsync(objectDelta).Result);

            Assert.Equal(ExceptionMessages.ObjectDelta_MustContainAtLeastOneChange, exception.Message);
        }

        [Fact]
        public void UpdateObjectDeltaThrowsObjectDisposedExceptionIfDisposed()
        {
            var session = new Session(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object,
                new SessionListeners());

            using (session)
            {
            }

            Assert.Throws<ObjectDisposedException>(() => session.UpdateAsync(new ObjectDelta(typeof(Customer), 1234)).Result);
        }
    }
}
