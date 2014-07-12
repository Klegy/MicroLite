﻿namespace MicroLite.Tests.Dialect
{
    using System;
    using MicroLite.Dialect;
    using MicroLite.Mapping;
    using MicroLite.Tests.TestEntities;
    using Xunit;

    /// <summary>
    /// Unit Tests for the <see cref="SqlServerCeDialect"/> class.
    /// </summary>
    public class SqlServerCeDialectTests : UnitTest
    {
        [Fact]
        public void BuildSelectIdentitySqlQuery()
        {
            var sqlDialect = new SqlServerCeDialect();

            var sqlQuery = sqlDialect.BuildSelectIdentitySqlQuery(ObjectInfo.For(typeof(Customer)));

            Assert.Equal("SELECT @@IDENTITY", sqlQuery.CommandText);
            Assert.Equal(0, sqlQuery.Arguments.Count);
        }

        [Fact]
        public void InsertInstanceQueryForIdentifierStrategyAssigned()
        {
            ObjectInfo.MappingConvention = new ConventionMappingConvention(
                UnitTest.GetConventionMappingSettings(IdentifierStrategy.Assigned));

            var customer = new Customer
            {
                Created = new DateTime(2011, 12, 24),
                CreditLimit = 10500.00M,
                DateOfBirth = new System.DateTime(1975, 9, 18),
                Id = 134875,
                Name = "Joe Bloggs",
                Status = CustomerStatus.Active,
                Updated = DateTime.Now,
                Website = new Uri("http://microliteorm.wordpress.com")
            };

            var sqlDialect = new SqlServerCeDialect();

            var sqlQuery = sqlDialect.BuildInsertSqlQuery(ObjectInfo.For(typeof(Customer)), customer);

            Assert.Equal("INSERT INTO \"Sales\".\"Customers\" (\"Created\",\"CreditLimit\",\"DateOfBirth\",\"Id\",\"Name\",\"CustomerStatusId\",\"Website\") VALUES (@p0,@p1,@p2,@p3,@p4,@p5,@p6)", sqlQuery.CommandText);
            Assert.Equal(7, sqlQuery.Arguments.Count);
            Assert.Equal(customer.Created, sqlQuery.Arguments[0]);
            Assert.Equal(customer.CreditLimit, sqlQuery.Arguments[1]);
            Assert.Equal(customer.DateOfBirth, sqlQuery.Arguments[2]);
            Assert.Equal(customer.Id, sqlQuery.Arguments[3]);
            Assert.Equal(customer.Name, sqlQuery.Arguments[4]);
            Assert.Equal(1, sqlQuery.Arguments[5]);
            Assert.Equal("http://microliteorm.wordpress.com/", sqlQuery.Arguments[6]);
        }

        [Fact]
        public void InsertInstanceQueryForIdentifierStrategyDbGenerated()
        {
            ObjectInfo.MappingConvention = new ConventionMappingConvention(
                UnitTest.GetConventionMappingSettings(IdentifierStrategy.DbGenerated));

            var customer = new Customer
            {
                Created = new DateTime(2011, 12, 24),
                CreditLimit = 10500.00M,
                DateOfBirth = new System.DateTime(1975, 9, 18),
                Id = 134875,
                Name = "Joe Bloggs",
                Status = CustomerStatus.Active,
                Updated = DateTime.Now,
                Website = new Uri("http://microliteorm.wordpress.com")
            };

            var sqlDialect = new SqlServerCeDialect();

            var sqlQuery = sqlDialect.BuildInsertSqlQuery(ObjectInfo.For(typeof(Customer)), customer);

            Assert.Equal("INSERT INTO \"Sales\".\"Customers\" (\"Created\",\"CreditLimit\",\"DateOfBirth\",\"Name\",\"CustomerStatusId\",\"Website\") VALUES (@p0,@p1,@p2,@p3,@p4,@p5)", sqlQuery.CommandText);
            Assert.Equal(6, sqlQuery.Arguments.Count);
            Assert.Equal(customer.Created, sqlQuery.Arguments[0]);
            Assert.Equal(customer.CreditLimit, sqlQuery.Arguments[1]);
            Assert.Equal(customer.DateOfBirth, sqlQuery.Arguments[2]);
            Assert.Equal(customer.Name, sqlQuery.Arguments[3]);
            Assert.Equal(1, sqlQuery.Arguments[4]);
            Assert.Equal("http://microliteorm.wordpress.com/", sqlQuery.Arguments[5]);
        }

        [Fact]
        public void PageNonQualifiedQuery()
        {
            var sqlQuery = new SqlQuery("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers ORDER BY CustomerId");

            var sqlDialect = new SqlServerCeDialect();

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers ORDER BY CustomerId OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY", paged.CommandText);
            Assert.Equal(0, paged.Arguments[0]);
            Assert.Equal(25, paged.Arguments[1]);
        }

        [Fact]
        public void PageNonQualifiedWildcardQuery()
        {
            var sqlQuery = new SqlQuery("SELECT * FROM Customers ORDER BY CustomerId");

            var sqlDialect = new SqlServerCeDialect();

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT * FROM Customers ORDER BY CustomerId OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY", paged.CommandText);
            Assert.Equal(0, paged.Arguments[0]);
            Assert.Equal(25, paged.Arguments[1]);
        }

        [Fact]
        public void PageQueryThrowsArgumentNullExceptionForNullSqlCharacters()
        {
            var sqlDialect = new SqlServerCeDialect();

            var exception = Assert.Throws<ArgumentNullException>(
                () => sqlDialect.PageQuery(null, PagingOptions.None));
        }

        [Fact]
        public void PageThrowsMicroLiteExceptionIfNoOrderByClause()
        {
            var sqlQuery = new SqlQuery("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers");

            var sqlDialect = new SqlServerCeDialect();

            var exception = Assert.Throws<MicroLiteException>(
                () => sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25)));

            Assert.Equal(ExceptionMessages.SqlServerCeDialect_PagedRequiresOrderBy, exception.Message);
        }

        [Fact]
        public void PageWithMultiWhereAndMultiOrderByMultiLine()
        {
            var sqlQuery = new SqlQuery(@"SELECT
 CustomerId,
 Name,
 DateOfBirth,
 CustomerStatusId
 FROM
 Customers
 WHERE
 (CustomerStatusId = @p0 AND DoB > @p1)
 ORDER BY
 Name ASC,
 DoB ASC", new object[] { CustomerStatus.Active, new DateTime(1980, 01, 01) });

            var sqlDialect = new SqlServerCeDialect();

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers WHERE (CustomerStatusId = @p0 AND DoB > @p1) ORDER BY Name ASC, DoB ASC OFFSET @p2 ROWS FETCH NEXT @p3 ROWS ONLY", paged.CommandText);
            Assert.Equal(sqlQuery.Arguments[0], paged.Arguments[0]);
            Assert.Equal(sqlQuery.Arguments[1], paged.Arguments[1]);
            Assert.Equal(0, paged.Arguments[2]);
            Assert.Equal(25, paged.Arguments[3]);
        }

        [Fact]
        public void PageWithNoWhereButOrderBy()
        {
            var sqlQuery = new SqlQuery("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers ORDER BY CustomerId ASC");

            var sqlDialect = new SqlServerCeDialect();

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers ORDER BY CustomerId ASC OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY", paged.CommandText);
            Assert.Equal(0, paged.Arguments[0]);
            Assert.Equal(25, paged.Arguments[1]);
        }

        [Fact]
        public void PageWithWhereAndOrderByFirstResultsPage()
        {
            var sqlQuery = new SqlQuery("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers WHERE CustomerStatusId = @p0 ORDER BY Name ASC", CustomerStatus.Active);

            var sqlDialect = new SqlServerCeDialect();

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers WHERE CustomerStatusId = @p0 ORDER BY Name ASC OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY", paged.CommandText);
            Assert.Equal(sqlQuery.Arguments[0], paged.Arguments[0]);
            Assert.Equal(0, paged.Arguments[1]);
            Assert.Equal(25, paged.Arguments[2]);
        }

        [Fact]
        public void PageWithWhereAndOrderByMultiLine()
        {
            var sqlQuery = new SqlQuery(@"SELECT
 CustomerId,
 Name,
 DateOfBirth,
 CustomerStatusId
 FROM
 Customers
 WHERE
 CustomerStatusId = @p0
 ORDER BY
 Name ASC", new object[] { CustomerStatus.Active });

            var sqlDialect = new SqlServerCeDialect();

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers WHERE CustomerStatusId = @p0 ORDER BY Name ASC OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY", paged.CommandText);
            Assert.Equal(sqlQuery.Arguments[0], paged.Arguments[0]);
            Assert.Equal(0, paged.Arguments[1]);
            Assert.Equal(25, paged.Arguments[2]);
        }

        [Fact]
        public void PageWithWhereAndOrderBySecondResultsPage()
        {
            var sqlQuery = new SqlQuery("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers WHERE CustomerStatusId = @p0 ORDER BY Name ASC", CustomerStatus.Active);

            var sqlDialect = new SqlServerCeDialect();

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 2, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers WHERE CustomerStatusId = @p0 ORDER BY Name ASC OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY", paged.CommandText);
            Assert.Equal(sqlQuery.Arguments[0], paged.Arguments[0]);
            Assert.Equal(25, paged.Arguments[1]);
            Assert.Equal(25, paged.Arguments[2]);
        }

        [Fact]
        public void SupportsIdentityReturnsTrue()
        {
            var sqlDialect = new SqlServerCeDialect();

            Assert.True(sqlDialect.SupportsIdentity);
        }

        [Fact]
        public void UpdateInstanceQueryForIdentifierStrategyAssigned()
        {
            ObjectInfo.MappingConvention = new ConventionMappingConvention(
                UnitTest.GetConventionMappingSettings(IdentifierStrategy.Assigned));

            var customer = new Customer
            {
                Created = new DateTime(2011, 12, 24),
                CreditLimit = 10500.00M,
                DateOfBirth = new System.DateTime(1975, 9, 18),
                Id = 134875,
                Name = "Joe Bloggs",
                Status = CustomerStatus.Active,
                Updated = DateTime.Now,
                Website = new Uri("http://microliteorm.wordpress.com")
            };

            var sqlDialect = new SqlServerCeDialect();

            var sqlQuery = sqlDialect.BuildUpdateSqlQuery(ObjectInfo.For(typeof(Customer)), customer);

            Assert.Equal("UPDATE \"Sales\".\"Customers\" SET \"CreditLimit\" = @p0,\"DateOfBirth\" = @p1,\"Name\" = @p2,\"CustomerStatusId\" = @p3,\"Updated\" = @p4,\"Website\" = @p5 WHERE \"Id\" = @p6", sqlQuery.CommandText);
            Assert.Equal(7, sqlQuery.Arguments.Count);
            Assert.Equal(customer.CreditLimit, sqlQuery.Arguments[0]);
            Assert.Equal(customer.DateOfBirth, sqlQuery.Arguments[1]);
            Assert.Equal(customer.Name, sqlQuery.Arguments[2]);
            Assert.Equal(1, sqlQuery.Arguments[3]);
            Assert.Equal(customer.Updated, sqlQuery.Arguments[4]);
            Assert.Equal("http://microliteorm.wordpress.com/", sqlQuery.Arguments[5]);
            Assert.Equal(customer.Id, sqlQuery.Arguments[6]);
        }

        [Fact]
        public void UpdateInstanceQueryForIdentifierStrategyDbGenerated()
        {
            ObjectInfo.MappingConvention = new ConventionMappingConvention(
                UnitTest.GetConventionMappingSettings(IdentifierStrategy.DbGenerated));

            var customer = new Customer
            {
                Created = new DateTime(2011, 12, 24),
                CreditLimit = 10500.00M,
                DateOfBirth = new System.DateTime(1975, 9, 18),
                Id = 134875,
                Name = "Joe Bloggs",
                Status = CustomerStatus.Active,
                Updated = DateTime.Now,
                Website = new Uri("http://microliteorm.wordpress.com")
            };

            var sqlDialect = new SqlServerCeDialect();

            var sqlQuery = sqlDialect.BuildUpdateSqlQuery(ObjectInfo.For(typeof(Customer)), customer);

            Assert.Equal("UPDATE \"Sales\".\"Customers\" SET \"CreditLimit\" = @p0,\"DateOfBirth\" = @p1,\"Name\" = @p2,\"CustomerStatusId\" = @p3,\"Updated\" = @p4,\"Website\" = @p5 WHERE \"Id\" = @p6", sqlQuery.CommandText);
            Assert.Equal(7, sqlQuery.Arguments.Count);
            Assert.Equal(customer.CreditLimit, sqlQuery.Arguments[0]);
            Assert.Equal(customer.DateOfBirth, sqlQuery.Arguments[1]);
            Assert.Equal(customer.Name, sqlQuery.Arguments[2]);
            Assert.Equal(1, sqlQuery.Arguments[3]);
            Assert.Equal(customer.Updated, sqlQuery.Arguments[4]);
            Assert.Equal("http://microliteorm.wordpress.com/", sqlQuery.Arguments[5]);
            Assert.Equal(customer.Id, sqlQuery.Arguments[6]);
        }
    }
}