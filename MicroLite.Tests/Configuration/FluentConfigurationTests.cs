﻿namespace MicroLite.Tests.Configuration
{
    using System;
    using System.Data.Common;
    using MicroLite.Configuration;
    using MicroLite.Dialect;
    using MicroLite.Driver;
    using MicroLite.FrameworkExtensions;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for the <see cref="FluentConfiguration"/> class.
    /// </summary>
    public class FluentConfigurationTests
    {
        public class WhenCallingCreateSessionFactory_WithNamedConnection : UnitTest
        {
            private readonly Mock<IDbDriver> mockDbDriver = new Mock<IDbDriver>();
            private readonly Mock<ISqlDialect> mockSqlDialect = new Mock<ISqlDialect>();
            private readonly ISessionFactory sessionFactory;
            private bool sessionFactoryCreatedCalled = false;

            public WhenCallingCreateSessionFactory_WithNamedConnection()
            {
                var fluentConfiguration = new FluentConfiguration((ISessionFactory s) =>
                {
                    this.sessionFactoryCreatedCalled = true;
                    return s;
                });

                this.sessionFactory = fluentConfiguration
                    .ForConnection("SqlConnection", this.mockSqlDialect.Object, this.mockDbDriver.Object)
                    .CreateSessionFactory();
            }

            [Fact]
            public void TheConnectionStringShouldBeSetOnTheDbDriver()
            {
                this.mockDbDriver.VerifySet(x => x.ConnectionString = It.IsNotNull<string>());
            }

            [Fact]
            public void TheDbProviderFactoryShouldBeSetOnTheDbDriver()
            {
                this.mockDbDriver.VerifySet(x => x.DbProviderFactory = It.IsNotNull<DbProviderFactory>());
            }

            [Fact]
            public void TheSessionFactoryCreatedActionShouldBeCalled()
            {
                Assert.True(this.sessionFactoryCreatedCalled);
            }

            [Fact]
            public void TheSessionFactoryShouldBeAddedToTheSessionFactoriesProperty()
            {
                Assert.Contains(this.sessionFactory, Configure.SessionFactories);
            }
        }

        public class WhenCallingCreateSessionFactory_WithNamedConnection_MultipleTimesForTheSameConnection : UnitTest
        {
            private readonly ISessionFactory sessionFactory1;
            private readonly ISessionFactory sessionFactory2;
            private int sessionFactoryCreatedCount = 0;

            public WhenCallingCreateSessionFactory_WithNamedConnection_MultipleTimesForTheSameConnection()
            {
                var fluentConfiguration = new FluentConfiguration((ISessionFactory s) =>
                {
                    this.sessionFactoryCreatedCount++;
                    return s;
                });

                this.sessionFactory1 = fluentConfiguration
                    .ForConnection("SqlConnection", new Mock<ISqlDialect>().Object, new Mock<IDbDriver>().Object)
                    .CreateSessionFactory();

                this.sessionFactory2 = fluentConfiguration
                    .ForConnection("SqlConnection", new Mock<ISqlDialect>().Object, new Mock<IDbDriver>().Object)
                    .CreateSessionFactory();
            }

            [Fact]
            public void TheSameSessionFactoryShouldBeReturned()
            {
                Assert.Same(this.sessionFactory1, this.sessionFactory2);
            }

            [Fact]
            public void TheSessionFactoryCreatedActionShouldBeCalledOnce()
            {
                Assert.Equal(1, this.sessionFactoryCreatedCount);
            }
        }

        public class WhenCallingForConnection_WithConnectionDetails_AndTheConnectionNameIsNull
        {
            [Fact]
            public void AnArgumentNullExceptionShouldBeThrown()
            {
                var fluentConfiguration = new FluentConfiguration(sessionFactoryCreated: null);

                var exception = Assert.Throws<ArgumentNullException>(
                    () => fluentConfiguration.ForConnection(null, @"Data Source=.\;Initial Catalog=Northwind;", "System.Data.SqlClient", new Mock<ISqlDialect>().Object, new Mock<IDbDriver>().Object));

                Assert.Equal("connectionName", exception.ParamName);
            }
        }

        public class WhenCallingForConnection_WithConnectionDetails_AndTheConnectionStringIsNull
        {
            [Fact]
            public void AnArgumentNullExceptionShouldBeThrown()
            {
                var fluentConfiguration = new FluentConfiguration(sessionFactoryCreated: null);

                var exception = Assert.Throws<ArgumentNullException>(
                    () => fluentConfiguration.ForConnection("SqlConnection", null, "System.Data.SqlClient", new Mock<ISqlDialect>().Object, new Mock<IDbDriver>().Object));

                Assert.Equal("connectionString", exception.ParamName);
            }
        }

        public class WhenCallingForConnection_WithConnectionDetails_AndTheDbDriverIsNull
        {
            [Fact]
            public void AnArgumentNullExceptionShouldBeThrown()
            {
                var fluentConfiguration = new FluentConfiguration(sessionFactoryCreated: null);

                var exception = Assert.Throws<ArgumentNullException>(
                    () => fluentConfiguration.ForConnection("SqlConnection", @"Data Source=.\;Initial Catalog=Northwind;", "System.Data.SqlClient", new Mock<ISqlDialect>().Object, null));

                Assert.Equal("dbDriver", exception.ParamName);
            }
        }

        public class WhenCallingForConnection_WithConnectionDetails_AndTheProviderNameIsNull
        {
            [Fact]
            public void AnArgumentNullExceptionShouldBeThrown()
            {
                var fluentConfiguration = new FluentConfiguration(sessionFactoryCreated: null);

                var exception = Assert.Throws<ArgumentNullException>(
                    () => fluentConfiguration.ForConnection("SqlConnection", @"Data Source=.\;Initial Catalog=Northwind;", null, new Mock<ISqlDialect>().Object, new Mock<IDbDriver>().Object));

                Assert.Equal("providerName", exception.ParamName);
            }
        }

        public class WhenCallingForConnection_WithConnectionDetails_AndTheSqlDialectIsNull
        {
            [Fact]
            public void AnArgumentNullExceptionShouldBeThrown()
            {
                var fluentConfiguration = new FluentConfiguration(sessionFactoryCreated: null);

                var exception = Assert.Throws<ArgumentNullException>(
                    () => fluentConfiguration.ForConnection("SqlConnection", @"Data Source=.\;Initial Catalog=Northwind;", "System.Data.SqlClient", null, new Mock<IDbDriver>().Object));

                Assert.Equal("sqlDialect", exception.ParamName);
            }
        }

        public class WhenCallingForConnection_WithNamedConnection_AndTheConnectionNameDoesNotExistInTheAppConfig
        {
            [Fact]
            public void AMicroLiteConfigurationExceptionShouldBeThrown()
            {
                var fluentConfiguration = new FluentConfiguration(sessionFactoryCreated: null);

                var exception = Assert.Throws<ConfigurationException>(
                    () => fluentConfiguration.ForConnection("TestDB", new Mock<ISqlDialect>().Object, new Mock<IDbDriver>().Object));

                Assert.Equal(ExceptionMessages.FluentConfiguration_ConnectionNotFound.FormatWith("TestDB"), exception.Message);
            }
        }

        public class WhenCallingForConnection_WithNamedConnection_AndTheConnectionNameIsNull
        {
            [Fact]
            public void AnArgumentNullExceptionShouldBeThrown()
            {
                var fluentConfiguration = new FluentConfiguration(sessionFactoryCreated: null);

                var exception = Assert.Throws<ArgumentNullException>(
                    () => fluentConfiguration.ForConnection(null, new Mock<ISqlDialect>().Object, new Mock<IDbDriver>().Object));

                Assert.Equal("connectionName", exception.ParamName);
            }
        }

        public class WhenCallingForConnection_WithNamedConnection_AndTheDbDriverIsNull
        {
            [Fact]
            public void AnArgumentNullExceptionShouldBeThrown()
            {
                var fluentConfiguration = new FluentConfiguration(sessionFactoryCreated: null);

                var exception = Assert.Throws<ArgumentNullException>(
                    () => fluentConfiguration.ForConnection("SqlConnection", new Mock<ISqlDialect>().Object, null));

                Assert.Equal("dbDriver", exception.ParamName);
            }
        }

        public class WhenCallingForConnection_WithNamedConnection_AndTheSqlDialectIsNull
        {
            [Fact]
            public void AnArgumentNullExceptionShouldBeThrown()
            {
                var fluentConfiguration = new FluentConfiguration(sessionFactoryCreated: null);

                var exception = Assert.Throws<ArgumentNullException>(
                    () => fluentConfiguration.ForConnection("SqlConnection", null, new Mock<IDbDriver>().Object));

                Assert.Equal("sqlDialect", exception.ParamName);
            }
        }
    }
}