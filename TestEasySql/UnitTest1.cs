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
        public void SelectFromSelect_1()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.Country).From(Select(Distinct(c.Country)).From(c))
                ,
                @"SELECT Country AS DistinctCountries FROM (SELECT DISTINCT Country FROM Customers) x"
                );
        }


        [TestMethod]
        public void SQLWithBetween()
        {
            var p = new Models.Products();
            Verify(
                Select(p.ProductID).Where(p.UnitPrice.IsBetween(10, 20))
                ,
                @"SELECT ProductID FROM Products
                  WHERE UnitPrice BETWEEN 10 AND 20"
                );
        }

        [TestMethod]
        public void SQLWithNotBetween()
        {
            var p = new Models.Products();
            Verify(
                Select(p.ProductID).Where(Not(p.UnitPrice.IsBetween(10, 20)))
                ,
                @"SELECT ProductID FROM Products
                  WHERE UnitPrice Not BETWEEN 10 AND 20"
                );
        }

        [TestMethod]
        public void SQLWithInnerJoins()
        {
            var o = new Models.Orders();
            var c = new Models.Customers();
            Verify(
                Select(o.OrderID, c.CompanyName, o.OrderDate).
                From(o).
                InnerJoin(c, o.CustomerID.IsEqualTo(c.CustomerID))
                ,
                @"SELECT  Orders.OrderID, Customers.CompanyName, Orders.OrderDate
                  FROM Orders
                  INNER JOIN Customers ON Orders.CustomerID=Customers.CustomerID "
                );
        }

        [TestMethod]
        public void SQLWithLeftJoin()
        {
            var o = new Models.Orders();
            var c = new Models.Customers();
            Verify(
                Select(c.CompanyName, o.OrderID).
                From(o).
                InnerJoin(c, o.CustomerID.IsEqualTo(c.CustomerID)).
                OrderBy(c.CompanyName)
                ,
                @"SELECT Customers.CompanyName, Orders.OrderID
                FROM Customers
                LEFT JOIN Orders ON Customers.CustomerID = Orders.CustomerID
                ORDER BY Customers.CompanyName "
                );
        }

        [TestMethod]
        public void SQLWithRightJoin()
        {
            var o = new Models.Orders();
            var c = new Models.Customers();
            Verify(
                Select(c.CompanyName, o.OrderID).
                From(c)
                ,
                @"SELECT Customers.CompanyName, Orders.OrderID
                  FROM Customers
                  RIGHT JOIN Orders ON Customers.CustomerID=Orders.CustomerID
                  ORDER BY Customers.CompanyName "
                );
        }

        [TestMethod]
        public void SQLWithFullJoin()
        {
            var o = new Models.Orders();
            var c = new Models.Customers();
            Verify(
                Select(c.CompanyName, o.OrderID).
                From(c).
                InnerJoin(o, c.CustomerID.IsEqualTo(o.CustomerID)).
                OrderBy(c.CompanyName)
                ,
                @"SELECT Customers.CompanyName, Orders.OrderID
                  FROM Customers
                  FULL OUTER JOIN Orders ON Customers.CustomerID=Orders.CustomerID
                  ORDER BY Customers.CompanyName "
                );
        }

        [TestMethod]
        public void SQLWithUnion()
        {
            var o = new Models.Orders();
            var c = new Models.Customers();
            Verify(
                Select(c.City, c.Country).Where(c.Country.IsEqualTo("Germany")) + @" UNION " +
                Select(o.ShipCity, o.ShipCountry).Where(o.ShipCountry.IsEqualTo("Germany")).OrderBy(c.City)

                ,
                @"SELECT City, Country FROM Customers WHERE Country='Germany'
                  UNION 
                  SELECT ShipCity, ShipCountry FROM orders WHERE ShipCountry='Germany'
                  ORDER BY City;"
                );
        }
        [TestMethod]
        public void SQLWithUnionAll()
        {
            var o = new Models.Orders();
            var c = new Models.Customers();
            Verify(
                Select(c.City, c.Country).Where(c.Country.IsEqualTo("Germany")) + @" UNION ALL" +
                Select(o.ShipCity, o.ShipCountry).Where(o.ShipCountry.IsEqualTo("Germany")).OrderBy(c.City)

                ,
                @"SELECT City, Country FROM Customers WHERE Country='Germany'
                  UNION  ALL
                  SELECT ShipCity, ShipCountry FROM orders WHERE ShipCountry='Germany'
                  ORDER BY City;"
                );
        }

        [TestMethod]
        public void SQLWithGroupBy()
        {
            var c = new Models.Customers();
            Verify(
                Select(Count(c.CustomerID), c.Country).Where(Like(c.CompanyName, "%an%")).GroupBy(c.Country)
                ,
                @"SELECT COUNT(CustomerID), Country
                 FROM Customers
                 WHERE CompanyName LIKE '%an%'
                 GROUP BY Country;"
                );
        }
        [TestMethod]
        public void SQLWithGroupByWithoutWhere()
        {
            var c = new Models.Customers();
            Verify(
                Select(Count(c.CustomerID), c.Country).Where().GroupBy(c.Country)
                ,
                @"SELECT COUNT(CustomerID), Country
                 FROM Customers
                 GROUP BY Country;"
                );
        }



        [TestMethod]
        public void SQLWithHaving()
        {
            var c = new Models.Customers();
            Verify(
                Select(Count(c.CustomerID),c.Country).Where().GroupBy(c.Country)+
                @" HAVING COUNT(t1.CustomerID) > 5"
                ,
                @"SELECT COUNT(CustomerID), Country
                FROM Customers
                GROUP BY Country
                HAVING COUNT(CustomerID) > 5"
                );
        }
        [TestMethod]
        public void SQLWithExists()
        {
            var od = new Models.Order_Details();
            var p = new Models.Products();
            Verify(
                Select(od.OrderID).From(od).
                Where(Exists(p, p.ProductID.IsEqualTo(od.ProductID).And(p.UnitPrice.IsEqualTo(22))))
                ,
                @"SELECT OrderID
                FROM [Order Details]
                WHERE EXISTS (SELECT ProductName FROM Products WHERE Products.ProductID = [Order Details].ProductID AND UnitPrice = 22) "
                );
        }

        [TestMethod]
        public void SQLWithNotExists()
        {
            var od = new Models.Order_Details();
            var p = new Models.Products();
            Verify(
                Select(od.OrderID).From(od).
                Where(NotExists(p, p.ProductID.IsEqualTo(od.ProductID).And(p.UnitPrice.IsEqualTo(22))))
                ,
                @"SELECT OrderID
                FROM [Order Details]
                WHERE NOT EXISTS (SELECT ProductName FROM Products WHERE Products.ProductID = [Order Details].ProductID AND UnitPrice = 22) "
                );
        }
 


        [TestMethod]
        public void SQLWithCase()
        {
            var od = new Models.Order_Details();
            Verify(
                Select(od.OrderID, od.Quantity )
                ,
                @"SELECT OrderID, Quantity,
                    CASE WHEN Quantity > 30 THEN 'The quantity is greater than 30'
                    WHEN Quantity = 30 THEN 'The quantity is 30'
                    ELSE 'The quantity is under 30'
                    END AS QuantityText
                FROM [Order Details]"
                );
        }


        [TestMethod]
        public void SQLWithCase2()
        {
            var c = new Models.Customers();
            Verify(
                Select(c.CompanyName, c.City, c.Country).OrderBy()
                ,
                @"SELECT CompanyName, City, Country
                FROM Customers
                ORDER BY
                (CASE
                    WHEN City IS NULL THEN Country
                    ELSE City
                END)"
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
