using ENV.Data.DataProvider;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace ENV.Utilities
{
    public class Join
    {
        public Entity To;
        public FilterBase On;
        public bool outer;
        public Join(Entity to, FilterBase on, bool outer = false)
        {
            this.outer = outer;
            To = to;
            On = on;
        }
    }

    public delegate string SqlPart(SQLParthelper helper);
    public class SQLParthelper
    {
        Dictionary<Firefly.Box.Data.Entity, string> _aliases = new Dictionary<Firefly.Box.Data.Entity, string>();

        public void RegisterEntities(params Entity[] entities)
        {
            foreach (var item in entities)
            {
                if (!_aliases.ContainsKey(item))
                    _aliases.Add(item, "t" + (_aliases.Count + 1));
            }
        }
        bool _isOracle = false;
        public string WhereToString(FilterBase f, params Entity[] moreEntities)
        {

            RegisterEntities(moreEntities);

            var x = FilterBase.GetIFilter(f, false, _aliases.Keys.ToArray());
            var p = new NoParametersFilterItemSaver(true, _isOracle ? OracleClientEntityDataProvider.DateTimeStringFormat : SQLClientEntityDataProvider.DateTimeStringFormat, DummyDateTimeCollector.Instance);
            var z = new SQLFilterConsumer(
                    p,
                    y =>
                    {
                        return WriteColumnWithAlias(y);
                    }, false, new dummySqlFilterHelper(p));
            x.AddTo(z);
            return z.Result.ToString();
        }

        private string WriteColumnWithAlias(ColumnBase y)
        {
            string s = GetAliasOf(y.Entity);
            if (!string.IsNullOrEmpty(s))
                return s + "." + y.Name;

            throw new InvalidOperationException("Only expected columns from main table or inner table");
        }

        internal string Translate(object x)
        {
            var f = x as FilterBase;
            if (f != null)
            {
                x = this.WhereToString(f);
            }
            var sqlPart = x as SqlPart;
            if (sqlPart != null)
            {
                x = sqlPart(this);
            }
            var c = x as ColumnBase;
            if (c != null)
            {
                x = WriteColumnWithAlias(c);
            }
            var e = x as Entity;
            if (e != null)
            {
                RegisterEntities(e);
                x = e.EntityName + " " + GetAliasOf(e);
            }
            return x.ToString();
        }

        private string GetAliasOf(Firefly.Box.Data.Entity e)
        {
            string s;
            if (_aliases.TryGetValue(e, out s))
                return s;
            return "";
        }
    }
    public static class EasySql
    {
        public static void TestSql(string sql, DynamicSQLSupportingDataProvider connection)
        {
            var t = System.IO.Path.GetTempFileName() + ".html";
            using (var sw = new System.IO.StreamWriter(t))
            {
                sw.WriteLine(@"<style type=""text/css"" media=""screen"">
body{
    font-family: ""Helvetica Neue"",Helvetica,Arial,sans-serif;
    font-size: 14px;
    line-height: 1.42857143;
    color: #333;
    background-color: #fff;
}
table{
    border-spacing: 0;
    border-collapse: collapse;
    
    margin-bottom: 20px;
    border: 1px solid #ddd;
    min-height: .01%;
    overflow-x: auto;
}
th {
    text-align: left;

}
pre{
    color: #393A34;
    font-family: ""Consolas"", ""Bitstream Vera Sans Mono"", ""Courier New"", Courier, monospace;
    direction: ltr;
    text-align: left;
    white-space: pre;
    word-spacing: normal;
    word-break: normal;
    font-size: 0.95em;
    line-height: 1.2em;
    -moz-tab-size: 4;
    -o-tab-size: 4;
    tab-size: 4;
    -webkit-hyphens: none;
    -moz-hyphens: none;
    -ms-hyphens: none;
    hyphens: none;
    padding: 1em;
    margin: .5em 0;
    overflow: auto;
    border: 1px solid #dddddd;
    background-color: white;
    background: #fff;
}
td,th{
    padding:5px;
    font-size:14px;
}

table tr:nth-of-type(odd) {
    background-color: #f9f9f9;
}
.h1, h1 {
    font-size: 36px;
}
.h1, .h2, .h3, h1, h2, h3 {
    margin-top: 20px;
    margin-bottom: 10px;
}
.h1, .h2, .h3, .h4, .h5, .h6, h1, h2, h3, h4, h5, h6 {
    font-family: inherit;
    font-weight: 500;
    line-height: 1.1;
    color: inherit;
}


</style>");
                sw.WriteLine("<h2>Sql:</h2>");
                sw.WriteLine("<pre>" + sql + "</pre>");
                try
                {
                    // Note that this method was called GetResult in older versions of ENV
                    var r = connection.GetHtmlTableBasedOnSQLResultForDebugPurposes(sql);
                    sw.WriteLine("<h2>Result:</h2>");
                    sw.WriteLine( r);
                }
                catch (Exception ex)
                {
                    sw.WriteLine("<h2>Error:</h2>");
                    sw.WriteLine("<pre>" + ex.Message + "</pre>");
                }
                finally
                {
                }
                sw.WriteLine("on: " + DateTime.Now.ToString());
            }
            Windows.OSCommand(t);
        }
        public static SqlPart Or(params object[] what)
        {
            return x => SQLBuilder.CreateCommaSeprated(what, x, " or ", true);
        }
        public static SqlPart And(params object[] what)
        {
            return x => SQLBuilder.CreateCommaSeprated(what, x, " and ", true);
        }
        public static SqlPart Distinct(params object[] column)
        {
            return x => "Distinct " + SQLBuilder.CreateCommaSeprated(column, x, ", ", false);
        }


        public static SqlPart Count(ColumnBase column = null)
        {
            if (column != null)
            {
                return h => "Count(" + h.Translate(column) + ")";
            }
            {
                return h => "Count(*)";
            }

        }
        public static SqlPart SortDescending(object what)
        {
            return h => h.Translate(what) + " desc";
        }
        public static SqlPart SortAscending(object what)
        {
            return h => h.Translate(what) + " asc";
        }
        public static SqlPart Average(object column)
        {
            return h => "avg(" + h.Translate(column) + ")";
        }
        public static SqlPart Devide(object a, object b)
        {
            return h => h.Translate(a) + " / " + h.Translate(b);
        }
        public static SqlPart Sum(object column)
        {
            return h => "sum(" + h.Translate(column) + ")";
        }
        public static SqlPart Round(object column, int decimals = 2)
        {
            return h => "round (" + h.Translate(column) + "," + decimals + ")";
        }

        public static SqlPart Max(ColumnBase column)
        {
            return h => "max(" + h.Translate(column) + ")";
        }
        public static SqlPart Min(ColumnBase column)
        {
            return h => "min(" + h.Translate(column) + ")";
        }
        public static SqlPart NotExist(Entity inTable, FilterBase where)
        {
            return helper => @" NOT EXISTS (
                        SELECT 1 FROM " + helper.Translate(inTable) + @" 
                        WHERE " + helper.WhereToString(where, inTable) + ")";
        }
        public static SqlPart CastAsDecimal(object what, int decimals = 2)
        {
            return h => "cast (" + h.Translate(what) + " as decimal(20, " + decimals + "))";
        }
        static SQLBuilder _sql = new SQLBuilder();
        public static SelectClass Select(params object[] columns)
        {
            return new SelectClass(columns);
        }
        public class SelectClass
        {
            internal object[] _select;
            public SelectClass(object[] select)
            {
                _select = select;
            }

            public FromClass From(params Entity[] entities)
            {
                return new FromClass(this, entities);
            }
            public WhereClass Where(params object[] filter)
            {
                var from = new List<Entity>();
                foreach (var item in _select)
                {
                    var c = item as ColumnBase;
                    if (c != null && c.Entity != null && !from.Contains(c.Entity))
                        from.Add(c.Entity);
                }

                return new WhereClass(new FromClass(this, from.ToArray()), filter);
            }
        }
        public class FromClass
        {
            internal SelectClass _select;
            internal Entity[] _from;
            internal List<Join> _joins = new List<Join>();
            public FromClass(SelectClass s, Entity[] from)
            {
                _select = s;
                _from = from;
            }
            public FromClass InnerJoin(Entity to, FilterBase where)
            {
                var result = new FromClass(_select, _from);
                result._joins.AddRange(this._joins);
                result._joins.Add(new Join(to, where));
                return result;
            }
            public FromClass OuterJoin(Entity to, FilterBase where)
            {
                var result = new FromClass(_select, _from);
                result._joins.AddRange(this._joins);
                result._joins.Add(new Join(to, where, true));
                return result;
            }
            public WhereClass Where(params object[] filter)
            {
                return new WhereClass(this, filter);
            }

        }
        public class WhereClass
        {
            internal FromClass _from;
            internal object[] _where;
            public WhereClass(FromClass from, object[] where)
            {
                _from = from;
                _where = where;
            }
            public override string ToString()
            {
                return new SQLBuilder().Query(_from._select._select, _from._from, _where, innerJoin: _from._joins.ToArray());
            }
            public static implicit operator string(WhereClass w)
            {
                return w.ToString();
            }
            public GroupByClass GroupBy(params object[] groupBy)
            {
                return new GroupByClass(this, groupBy);
            }
            public string OrderBy(params object[] orderBy)
            {
                return new SQLBuilder().Query(_from._select._select, _from._from, _where, innerJoin: _from._joins.ToArray(), orderBy: orderBy);
            }

        }
        public class GroupByClass
        {
            WhereClass _where;
            object[] _columns;
            public GroupByClass(WhereClass where, object[] columns)
            {
                _where = where;
                _columns = columns;

            }
            public override string ToString()
            {
                return new SQLBuilder().Query(_where._from._select._select, _where._from._from, _where._where, groupBy: _columns, innerJoin: _where._from._joins.ToArray());
            }
            public string OrderBy(params object[] orderBy)
            {
                return new SQLBuilder().Query(_where._from._select._select, _where._from._from, _where._where, _columns, innerJoin: _where._from._joins.ToArray(), orderBy: orderBy);
            }
            public static implicit operator string(GroupByClass w)
            {
                return w.ToString();
            }


        }
    }


    public class SQLBuilder
    {



        public string Query(object[] select, Entity[] from, object[] where, object[] groupBy = null, Join[] innerJoin = null, object[] orderBy = null)
        {
            var helper = new SQLParthelper();
            return helper.Translate(InternalQuery(select, from, where, groupBy, innerJoin, orderBy));

        }


        private static SqlPart InternalQuery(object[] select, Firefly.Box.Data.Entity[] from, object[] where, object[] groupBy = null, Join[] joins = null, object[] orderBy = null)
        {
            return helper =>
            {
                helper.RegisterEntities(from);
                if (joins != null)
                    foreach (var item in joins)
                    {
                        helper.RegisterEntities(item.To);
                    }
                var theFrom = CreateCommaSeprated(from, helper);
                if (joins != null)
                    foreach (var j in joins)
                    {
                        theFrom += (j.outer ? " left outer" : " inner") + " join " + helper.Translate(j.To) + " on " + helper.WhereToString(j.On);

                    }

                string theWhere = CreateCommaSeprated(where, helper, " and ", true);
                if (theWhere != "")
                    theWhere = " Where " + theWhere;

                var result = "Select " + CreateCommaSeprated(select, helper) +
                    " from " + theFrom +
                    theWhere;
                if (groupBy != null)
                {
                    result += " group by " + CreateCommaSeprated(groupBy, helper);
                }


                if (orderBy != null)
                {
                    var ob = "";
                    foreach (var item in orderBy)
                    {
                        if (!(item is SortDirection) && ob.Length > 0)
                        {
                            ob += ", ";
                        }
                        else if (ob.Length == 0)
                        {
                            ob = " order by ";
                        }
                        ob += helper.Translate(item);
                    }
                    result += ob;
                }
                return result;
            };
        }



        internal static string CreateCommaSeprated(object[] select, SQLParthelper h, string seprator = ", ", bool addParents = false)
        {
            var selectString = "";
            foreach (var item in select)
            {
                if (selectString.Length != 0)
                    selectString += seprator;
                string x;

                if (item is object[])
                {
                    x = CreateCommaSeprated((object[])item, h, seprator, addParents);
                }
                else
                    x = h.Translate(item);
                if (addParents)
                {
                    x = "(" + x + ")";
                }
                selectString += x;

            }

            return selectString;
        }

        public SqlPart InnerSelect(ColumnBase select, object where)
        {
            return h => "(" + h.Translate(InternalQuery(new[] { select }, new[] { select.Entity }, new[] { where })) + ")";
        }
    }

}
