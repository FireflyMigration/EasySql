using Firefly.Box.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEasySql
{

    public class TestUtilities
    {
      
        public static void Verify(string resultSql, string compareSQL, params string[] moreQueries)
        {
            System.Diagnostics.Debug.WriteLine(resultSql);
            var compare = QueryToArray(compareSQL);
            var other = new List<string>();
            other.Add(resultSql);
            other.AddRange(moreQueries);
            foreach (var item in other)
            {
                var result = QueryToArray(item);
                result.Length.ShouldBe(compare.Length, "num of rows");
                for (int i = 0; i < compare.Length; i++)
                {
                    result[i].ShouldBeArray(compare[i]);
                }
            }

        }
        private static object[][] QueryToArray(string compareSQL)
        {
            var compare = new List<object[]>();
            using (var c = Northwind.Shared.DataSources.Northwind.CreateCommand())
            {
                c.CommandText = compareSQL;
                using (var r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var row = new ArrayList();
                        for (int i = 0; i < r.FieldCount; i++)
                        {
                            row.Add(r[i]);
                        }
                        compare.Add(row.ToArray());
                    }
                }
            }
            return compare.ToArray();
        }
    }
}
