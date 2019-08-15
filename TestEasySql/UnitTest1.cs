﻿using System;
using System.Collections;
using System.Collections.Generic;
using Firefly.Box.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models = Northwind.Models;
using static TestEasySql.TestUtilities;
using static ENV.Utilities.EasySql;
namespace TestEasySql
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestBasicSql()
        {
            var o = new Models.Orders();
            Verify(Select(o.CustomerID).Where(o.OrderID.IsEqualTo(10248)),
                "SELECT CUSTOMERID from orders where orderid=10248");
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