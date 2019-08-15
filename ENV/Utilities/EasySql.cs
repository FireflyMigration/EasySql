using ENV.Data.DataProvider;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using Firefly.Box;

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
    public interface ISqlPart
    {
        string Build(SQLPartHelper helper);
    }
    public class SqlFunction : ISqlPart
    {
        string _name;
        object[] _args;
        public SqlFunction(string name, params object[] args)
        {
            _name = name;
            _args = args;
        }
        public string Build(SQLPartHelper helper)
        {
            var args = EasySql.CreateCommaSeprated(_args, helper);
            return _name + " (" + args + ")";
        }
    }
    public class SqlPart : ISqlPart
    {
        Func<SQLPartHelper, string> _part;
        public SqlPart(Func<SQLPartHelper, string> part)
        {
            _part = part;
        }
        public string Build(SQLPartHelper helper)
        {
            return _part(helper);
        }
        public SqlPart(params object[] parts)
        {
            _part = h =>
            {
                var sb = "";
                foreach (var item in parts)
                {
                    sb += h.Translate(item);
                }
                return sb;
            };
        }
    }


    public class SQLPartHelper
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
            var sqlPart = x as ISqlPart;
            if (sqlPart != null)
            {
                x = sqlPart.Build(this);
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
                    sw.WriteLine(r);
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
            return new SqlPart(x => CreateCommaSeprated(what, x, " or ", true));
        }
        public static SqlPart And(params object[] what)
        {
            return new SqlPart(x => CreateCommaSeprated(what, x, " and ", true));
        }
        public static SqlPart Distinct(params object[] column)
        {
            return new SqlPart(x => "Distinct " + CreateCommaSeprated(column, x, ", ", false));
        }


        public static ISqlPart Count(ColumnBase column = null)
        {
            if (column == null)
            {
                return new SqlFunction("Count", "*");
            }
            {
                return new SqlFunction("Count", column);
            }

        }

        public static ISqlPart Average(object column)
        {
            return new SqlFunction("avg", column);

        }
        public static ISqlPart Devide(object a, object b)
        {
            return new SqlPart(a, "/", b);
        }
        public static ISqlPart Sum(object column)
        {
            return new SqlFunction("sum", column);
        }
        public static ISqlPart Round(object column, int decimals = 2)
        {
            return new SqlFunction("round", column , decimals);
        }

        public static ISqlPart Max(ColumnBase column)
        {
            return new SqlFunction("max", column);
        }
        public static ISqlPart Min(ColumnBase column)
        {
            return new SqlFunction("min", column);

        }
        public static SqlPart NotExist(Entity inTable, FilterBase where)
        {
            return new SqlPart(helper => @" NOT EXISTS (
                        SELECT 1 FROM " + helper.Translate(inTable) + @" 
                        WHERE " + helper.WhereToString(where, inTable) + ")");
        }
        public static ISqlPart CastAsDecimal(object what, int decimals = 2)
        {
            return new SqlFunction("cast", new SqlPart(what, " as ", new SqlFunction("decimal", 20, decimals)));
        }
        internal static string CreateCommaSeprated(object[] select, SQLPartHelper h, string seprator = ", ", bool addParents = false)
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
        
        public static SelectClass Select(params object[] columns)
        {
            return new SelectClass(columns);
        }
        public class SelectClass : SqlStatementKeywordBase
        {
            public SelectClass(object[] select) : base(null)
            {
                _select.AddRange(select);
            }

            public FromClass From(params Entity[] entities)
            {
                var r = new FromClass(this);
                r._from.AddRange(entities);
                return r;
            }
            public WhereClass Where(params object[] filter)
            {
                return new WhereClass(this, filter);
            }
        }
        public class FromClass : SqlStatementKeywordBase
        {

            public FromClass(SqlStatementKeywordBase s) : base(s)
            {

            }
            public FromClass InnerJoin(Entity to, FilterBase where)
            {
                var result = new FromClass(this);
                result._joins.Add(new Join(to, where));
                return result;
            }
            public FromClass OuterJoin(Entity to, FilterBase where)
            {
                var result = new FromClass(this);
                result._joins.Add(new Join(to, where, true));
                return result;
            }
            public WhereClass Where(params object[] filter)
            {
                return new WhereClass(this, filter);
            }

        }
        public class WhereClass : SqlStatementKeywordBase
        {
            public WhereClass(SqlStatementKeywordBase from, object[] filter) : base(from)
            {
                this._where.AddRange(filter);
            }

            public GroupByClass GroupBy(params object[] groupBy)
            {
                return new GroupByClass(this, groupBy);
            }
            public OrderByClass OrderBy(params object[] orderBy)
            {
                return new OrderByClass(this, orderBy);
            }

        }
        public class SqlStatementKeywordBase :ISqlPart
        {
            internal List<object> _select = new List<object>(),
                _where = new List<object>(),
            _groupBy = new List<object>(),
                _orderBy = new List<object>();
            internal List<Entity> _from = new List<Entity>();
            internal List<Join> _joins = new List<Join>();
            public SqlStatementKeywordBase(SqlStatementKeywordBase original)
            {
                if (original != null)
                    CopyFrom(original);
            }
            public static implicit operator string(SqlStatementKeywordBase w)
            {
                return w.ToString();
            }
            public override string ToString()
            {
                return InternalBuild (new SQLPartHelper());
            }
            protected void CopyFrom(SqlStatementKeywordBase b)
            {
                _select.AddRange(b._select);
                _where.AddRange(b._where);
                _groupBy.AddRange(b._groupBy);
                _orderBy.AddRange(b._orderBy);
                _from.AddRange(b._from);
                _joins.AddRange(b._joins);
            }

            public string Build(SQLPartHelper helper)
            {
                return "("+InternalBuild(helper)+")";
            }

            private string InternalBuild(SQLPartHelper helper)
            {
                if (_from.Count == 0)
                {
                    foreach (var item in _select)
                    {
                        var c = item as ColumnBase;
                        if (c != null)
                        {
                            if (!_from.Contains(c.Entity))
                                _from.Add(c.Entity);
                        }
                    }
                    if (_from.Count == 0)
                        throw new InvalidOperationException("Couldn't figure out the from");
                }
                helper.RegisterEntities(_from.ToArray());
                if (_joins != null)
                    foreach (var item in _joins)
                    {
                        helper.RegisterEntities(item.To);
                    }
                var theFrom = EasySql.CreateCommaSeprated(_from.ToArray(), helper);
                if (_joins != null)
                    foreach (var j in _joins)
                    {
                        theFrom += (j.outer ? " left outer" : " inner") + " join " + helper.Translate(j.To) + " on " + helper.WhereToString(j.On);

                    }

                string theWhere = EasySql.CreateCommaSeprated(_where.ToArray(), helper, " and ", true);
                if (theWhere != "")
                    theWhere = " Where " + theWhere;

                var result = "Select " + EasySql.CreateCommaSeprated(_select.ToArray(), helper) +
                    " from " + theFrom +
                    theWhere;
                if (_groupBy != null && _groupBy.Count> 0)
                {
                    result += " group by " + EasySql.CreateCommaSeprated(_groupBy.ToArray(), helper);
                }


                if (_orderBy != null && _orderBy.Count > 0)
                {
                    var ob = "";
                    foreach (var item in _orderBy)
                    {
                        if (item is SortDirection)
                        {
                            if (((SortDirection)item) == SortDirection.Descending)
                                ob += " desc";
                            continue;
                        }
                        if (ob.Length > 0)
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
            }
        }
        public class GroupByClass : SqlStatementKeywordBase
        {
            public GroupByClass(WhereClass where, object[] columns) : base(where)
            {
                _groupBy.AddRange(columns);

            }

            public OrderByClass OrderBy(params object[] orderBy)
            {
                return new OrderByClass(this, orderBy);
            }
        }
        public class OrderByClass : SqlStatementKeywordBase
        {
            public OrderByClass(SqlStatementKeywordBase parent, object[] orderBy) : base(parent)
            {
                _orderBy.AddRange(orderBy);
            }
        }
    }


}
