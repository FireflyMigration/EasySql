using System;
using System.Collections;
using System.Collections.Generic;
using Firefly.Box.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models = Northwind.Models;
using static TestEasySql.TestUtilities;
using static ENV.Utilities.EasySql;
using ENV.Utilities;
using Firefly.Box;

namespace TestEasySql
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void BasicSelectStatement()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID, c.CompanyName)
                .From(c)
                ,
                "select customerid,companyName from customers"
                );
        }
        [TestMethod]
        public void BasicSelectStatement_from_is_not_required()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID, c.CompanyName)
                ,
                "select customerid,companyName from customers"
                );
        }
        [TestMethod]
        public void TestListOfSelectItems()
        {
            var c = new Models.Customers();
            var columns = new SelectItems {
                c.CustomerID,c.CompanyName
            };
            Verify(
                Select( columns, c.Phone)
                ,
                "select customerid,companyName,phone from customers"
                );
        }
        [TestMethod]
        public void SQLWithWhere()
        {
            var o = new Models.Orders();
            Verify(
                Select(o.CustomerID).Where(o.OrderID.IsEqualTo(10248))
                ,
                "SELECT CUSTOMERID from orders where orderid=10248"
                );
        }

        [TestMethod]
        public void SQLWithDistinct()
        {
            var c = new Models.Customers();
            Verify(
                Select(Distinct(c.Country)).From(c)
                ,
                "SELECT DISTINCT Country FROM Customers"
                );
        }
        [TestMethod]
        public void SQLWithAnd()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID).From(c).
                Where(c.Country.IsEqualTo("Germany").And(c.City.IsEqualTo("Berlin")))
                ,
                @"SELECT CustomerID FROM Customers
                  WHERE Country = 'Germany' AND City = 'Berlin'; "
                );
        }
        [TestMethod]
        public void SQLWithOr()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID).From(c).
                Where(c.City.IsEqualTo("Berlin").Or(c.City.IsEqualTo("München")))
                ,
                @"SELECT CustomerID FROM Customers
                  WHERE City='Berlin' OR City='München' "
                );
        }

        [TestMethod]
        public void SQLWithOrderByAsc()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID).From(c).
                Where(c.Country.IsEqualTo("Germany")).OrderBy(c.CustomerID)                
                ,
                @"SELECT CustomerID FROM Customers
                  WHERE Country='Germany' order by CustomerID asc"
                );
        }

        [TestMethod]
        public void SQLWithOrderByDesc()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID).From(c).
                Where(c.Country.IsEqualTo("Germany")).OrderBy(c.CustomerID,SortDirection.Descending )
                ,
                @"SELECT CustomerID FROM Customers
                  WHERE Country='Germany' order by CustomerID Desc"
                );
        }
        [TestMethod]
        public void SQLWithOrderByDescAsc()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID).From(c).
                Where(c.Country.IsEqualTo("Germany")).OrderBy(c.Country, c.CompanyName, SortDirection.Descending)
                ,
                @"SELECT CustomerID FROM Customers
                  WHERE Country='Germany' order by Country ASC, CompanyName DESC"
                );
        }

        [TestMethod]
        public void SQLWithIsNull()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CompanyName, c.ContactName, c.Address).From(c).
                Where(c.Address.IsEqualTo((Text)null))
                ,
                @"SELECT CompanyName, ContactName, Address
                  FROM Customers
                    WHERE Address IS NULL"
                );
        }

        [TestMethod]
        public void SQLWithIsNotNull()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CompanyName, c.ContactName, c.Address).From(c).
                Where(c.Address.IsDifferentFrom((Text)null))
                ,
                @"SELECT CompanyName, ContactName, Address
                  FROM Customers
                    WHERE Address IS NOT NULL"
                );
        }

        [TestMethod]
        public void SQLWithMin()
        {
            var p = new Models.Products();
            Verify(
                Select(Min(p.UnitPrice)).From(p)
                ,
                @"SELECT MIN(UnitPrice) AS SmallestPrice
                  FROM Products"
                );
        }

        [TestMethod]
        public void SQLWithMax()
        {
            var p = new Models.Products();
            Verify(
                Select(Max(p.UnitPrice)).From(p)
                ,
                @"SELECT MAX(UnitPrice) AS LargestPrice
                  FROM Products"
                );
        }

        [TestMethod]
        public void SQLWithCount()
        {
            var p = new Models.Products();
            Verify(
                Select(Count(p.ProductID)).From(p)
                ,
                @"SELECT COUNT(ProductID)
                  FROM Products"
                );
        }
        [TestMethod]
        public void SQLWithNot()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID).Where(Not(c.Country.IsEqualTo("Germany")))
                ,
                @"SELECT CustomerId FROM Customers WHERE NOT Country='Germany'"
                );
        }

        [TestMethod]
        public void SQLWithAvg()
        {
            var p = new Models.Products();
            Verify(
                Select(Average(p.UnitPrice)).From(p)
                ,
                @"SELECT AVG(UnitPrice)
                  FROM Products"
                );
        }
        [TestMethod]
        public void SQLWithSum()
        {
            var od = new Models.Order_Details();
            Verify(
                Select(Sum(od.Quantity)).
                From(od)
                ,
                @"SELECT SUM(Quantity)
                  FROM dbo.[Order Details]"
                );
        }
        [TestMethod]
        public void SQLWithLikeUsingUserDbMethods()
        {
            
            var c = new Models.Customers();
            var db = new ENV.Data.UserDbMethods(() => c);
            Verify(
                Select(c.CustomerID).From(c).
                Where(db.Like(c.CompanyName, "*or*"))
                ,
                @"SELECT customerid FROM Customers
                  WHERE companyname LIKE '%or%'"
                );
        }
        [TestMethod]
        public void SQLWithLike()
        {

            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID).From(c).
                Where(Like(c.CompanyName, "%or%"))
                ,
                @"SELECT customerid FROM Customers
                  WHERE companyname LIKE '%or%'"
                );
        }

        [TestMethod]
        public void SQLWithCountDistinct()
        {

            var c = new Models.Customers();
            Verify(
                Select(Count(Distinct(c.Country))).From(c)
                ,
                @"SELECT COUNT(DISTINCT Country) FROM Customers;
"
                );
        }
        [TestMethod]
        public void SQLWithOrderBy()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID).From(c).OrderBy(c.Country)
                ,
                @"SELECT CustomerId FROM Customers ORDER BY Country;"
                );
        }
        [TestMethod]
        public void SQLWithOrderByWithoutFrom()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID).OrderBy(c.Country)
                ,
                @"SELECT CustomerId FROM Customers ORDER BY Country;"
                );
        }

        [TestMethod]
        public void SQLWithIN()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID).Where(c.Country.IsIn("Germany","France","UK"))
                ,
                @"SELECT CustomerID FROM Customers WHERE Country IN ('Germany', 'France', 'UK');"
                );
        }

        [TestMethod]
        public void SQLWithNotIn()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID).Where(c.Country.IsNotIn("Germany", "France", "UK"))
                ,
                @"SELECT CustomerID FROM Customers WHERE Country NOT IN ('Germany', 'France', 'UK');"
                );
        }
        [TestMethod]
        public void SelectFromSelect()
        {
            var c = new Models.Customers();
            Verify(
                Select(Count()).From(Select(Distinct(c.Country)).From(c))
                ,
                @"SELECT Count(*) AS DistinctCountries FROM (SELECT DISTINCT Country FROM Customers) x"
                );
        }

        [TestMethod]
        public void SQLWith()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CustomerID)
                ,
                @"SELECT CustomerId FROM Customers "
                );
        }

        /// <summary>
        /// Loads the application ini to get a valid database connection
        /// </summary>
        /// <param name="tc"></param>
        [AssemblyInitialize]
        public static void Init(TestContext tc)
        {
            Northwind.Program.Init(new string[0]);
        }
    }
}
