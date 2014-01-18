﻿namespace MicroLite.Tests.Core
{
    using System.Data.Common;
    using MicroLite.Core;
    using MicroLite.Dialect;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for the <see cref="SessionFactory"/> class.
    /// </summary>
    public class SessionFactoryTests
    {
        public class WhenCallingOpenReadOnlySession
        {
            private readonly Mock<DbConnection> mockConnection = new Mock<DbConnection>();
            private readonly Mock<DbProviderFactory> mockFactory = new Mock<DbProviderFactory>();
            private readonly SessionFactoryOptions options;
            private readonly IReadOnlySession readOnlySession;
            private readonly SessionFactory sessionFactory;

            public WhenCallingOpenReadOnlySession()
            {
                this.mockConnection.SetupProperty(x => x.ConnectionString);

                this.mockFactory.Setup(x => x.CreateConnection()).Returns(mockConnection.Object);

                this.options = new SessionFactoryOptions
                {
                    ConnectionString = "Data Source=localhost;Initial Catalog=TestDB;",
                    ProviderFactory = mockFactory.Object,
                    SqlDialect = new Mock<ISqlDialect>().Object
                };

                this.sessionFactory = new SessionFactory(new Mock<IObjectBuilder>().Object, this.options);
                this.readOnlySession = this.sessionFactory.OpenReadOnlySession();
            }

            [Fact]
            public void ANewConnectionShouldBeCreated()
            {
                this.mockFactory.Verify(x => x.CreateConnection(), Times.Once());
            }

            [Fact]
            public void TheConnectionStringShouldBeSetOnTheConnection()
            {
                this.mockConnection.VerifySet(x => x.ConnectionString = this.options.ConnectionString, Times.Once());
            }
        }

        public class WhenCallingOpenReadOnlySession_MultipleTimes
        {
            private readonly Mock<DbConnection> mockConnection = new Mock<DbConnection>();
            private readonly Mock<DbProviderFactory> mockFactory = new Mock<DbProviderFactory>();
            private readonly SessionFactoryOptions options;
            private readonly IReadOnlySession session1;
            private readonly IReadOnlySession session2;

            public WhenCallingOpenReadOnlySession_MultipleTimes()
            {
                this.mockConnection.SetupProperty(x => x.ConnectionString);

                this.mockFactory.Setup(x => x.CreateConnection()).Returns(mockConnection.Object);

                this.options = new SessionFactoryOptions
                {
                    ConnectionString = "Data Source=localhost;Initial Catalog=TestDB;",
                    ProviderFactory = mockFactory.Object,
                    SqlDialect = new Mock<ISqlDialect>().Object
                };

                var sessionFactory = new SessionFactory(new Mock<IObjectBuilder>().Object, this.options);

                this.session1 = sessionFactory.OpenReadOnlySession();
                this.session2 = sessionFactory.OpenReadOnlySession();
            }

            [Fact]
            public void ANewSessionShouldBeOpenedEachTime()
            {
                Assert.NotSame(this.session1, this.session2);
            }
        }

        public class WhenCallingOpenSession
        {
            private readonly Mock<DbConnection> mockConnection = new Mock<DbConnection>();
            private readonly Mock<DbProviderFactory> mockFactory = new Mock<DbProviderFactory>();
            private readonly SessionFactoryOptions options;

            public WhenCallingOpenSession()
            {
                this.mockConnection.SetupProperty(x => x.ConnectionString);

                this.mockFactory.Setup(x => x.CreateConnection()).Returns(mockConnection.Object);

                this.options = new SessionFactoryOptions
                {
                    ConnectionString = "Data Source=localhost;Initial Catalog=TestDB;",
                    ProviderFactory = mockFactory.Object,
                    SqlDialect = new Mock<ISqlDialect>().Object
                };

                var sessionFactory = new SessionFactory(new Mock<IObjectBuilder>().Object, this.options);
                sessionFactory.OpenSession();
            }

            [Fact]
            public void ANewConnectionShouldBeCreated()
            {
                this.mockFactory.Verify(x => x.CreateConnection(), Times.Once());
            }

            [Fact]
            public void TheConnectionStringShouldBeSetOnTheConnection()
            {
                this.mockConnection.VerifySet(x => x.ConnectionString = this.options.ConnectionString, Times.Once());
            }
        }

        public class WhenCallingOpenSession_MultipleTimes
        {
            private readonly Mock<DbConnection> mockConnection = new Mock<DbConnection>();
            private readonly Mock<DbProviderFactory> mockFactory = new Mock<DbProviderFactory>();
            private readonly SessionFactoryOptions options;
            private readonly ISession session1;
            private readonly ISession session2;

            public WhenCallingOpenSession_MultipleTimes()
            {
                this.mockConnection.SetupProperty(x => x.ConnectionString);

                this.mockFactory.Setup(x => x.CreateConnection()).Returns(mockConnection.Object);

                this.options = new SessionFactoryOptions
                {
                    ConnectionString = "Data Source=localhost;Initial Catalog=TestDB;",
                    ProviderFactory = mockFactory.Object,
                    SqlDialect = new Mock<ISqlDialect>().Object
                };

                var sessionFactory = new SessionFactory(new Mock<IObjectBuilder>().Object, this.options);

                this.session1 = sessionFactory.OpenSession();
                this.session2 = sessionFactory.OpenSession();
            }

            [Fact]
            public void ANewSessionShouldBeOpenedEachTime()
            {
                Assert.NotSame(this.session1, this.session2);
            }
        }

        public class WhenCreated
        {
            private readonly SessionFactoryOptions options = new SessionFactoryOptions
            {
                ConnectionName = "Northwind",
                SqlDialect = new Mock<ISqlDialect>().Object
            };

            private readonly SessionFactory sessionFactory;

            public WhenCreated()
            {
                this.sessionFactory = new SessionFactory(new Mock<IObjectBuilder>().Object, this.options);
            }

            [Fact]
            public void ConnectionNameReturnsConnectionNameFromOptions()
            {
                Assert.Equal(this.options.ConnectionName, this.sessionFactory.ConnectionName);
            }

            [Fact]
            public void SqlDialectReturnsSqlDialectFromOptions()
            {
                Assert.Equal(options.SqlDialect, sessionFactory.SqlDialect);
            }
        }
    }
}