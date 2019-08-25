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
      
        public static void Verify(Func<string> select, string compareSQL)
        {

            object[][] compare ;
            try
            {
                compare = QueryToArray(compareSQL);
            }
            catch {
                System.Diagnostics.Debug.WriteLine("ERROR IN COMPARE SQL!!!!");
                System.Diagnostics.Debug.WriteLine(compareSQL);
                throw;
            }

            
            var resultSql = select();
            System.Diagnostics.Debug.WriteLine(resultSql);
            var result = QueryToArray(resultSql);
            result.Length.ShouldBe(compare.Length, "num of rows");
            for (int i = 0; i < compare.Length; i++)
            {
                result[i].ShouldBeArray(compare[i]);
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
