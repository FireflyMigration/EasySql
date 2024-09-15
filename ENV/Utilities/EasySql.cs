using ENV.Data.DataProvider;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using Firefly.Box;
using System.Collections;
using static ENV.Utilities.EasySql;

namespace ENV.Utilities.EasySqlExtentions
{
    public static class EasySqlExtentionsHelper
    {
        public static SqlPart IsIn<T>(this TypedColumnBase<T> column, params T[] vals)
        {
            var v = new List<object>();
            foreach (var item in vals)
            {
                v.Add(EasySql.StringValue(item));

            }
            return new SqlPart(column, " ", new SqlFunction("in", v.ToArray()));
        }
        public static SqlPart IsNull(this ColumnBase column)
        {
            return new SqlPart(column, " is null");
        }

        public static SqlPart IsNotIn<T>(this TypedColumnBase<T> column, params T[] vals)
        {
            var v = new List<object>();
            foreach (var item in vals)
            {
                v.Add(EasySql.StringValue(item));
            }
            return new SqlPart(column, " ", new SqlFunction("not in", v.ToArray()));
        }
        /// <summary>
        /// https://www.sqlshack.com/sql-union-overview-usage-and-examples/
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public static UnionClass Union(this CanUnion left, SqlStatementKeywordBase select)
        {
            return new UnionClass(left, " union ", select);
        }
        /// <summary>
        /// https://www.sqlshack.com/sql-union-overview-usage-and-examples/
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public static UnionClass UnionAll(this CanUnion left, SqlStatementKeywordBase select)
        {
            return new UnionClass(left, " union all ", select);
        }/// <summary>
         /// https://www.sqlshack.com/sql-union-overview-usage-and-examples/
         /// </summary>
         /// <param name="select"></param>
         /// <returns></returns>
        public static UnionClass UnionExcept(this CanUnion left, SqlStatementKeywordBase select)
        {
            return new UnionClass(left, " except ", select);
        }
        /// <summary>
        /// https://www.sqlshack.com/sql-union-overview-usage-and-examples/
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public static UnionClass UnionIntersect(this CanUnion left, SqlStatementKeywordBase select)
        {
            return new UnionClass(left, " intersect ", select);
        }
        public interface CanUnion
        {
        }
    }
}
namespace ENV.Utilities
{

    public static class EasySql
    {

        public static SqlPart Count(object column = null)
        {
            if (column == null)
            {
                return new SqlFunction("Count", "*");
            }
            {
                return new SqlFunction("Count", column);
            }

        }


        public static SqlPart Average(object column)
        {
            return new SqlFunction("avg", column);

        }
        public static SqlPart Devide(object a, object b)
        {
            return new SqlPart("(", a, "/", b, ")");
        }
        public static SqlPart Multiply(object a, object b)
        {
            return new SqlPart("(", a, "*", b, ")");
        }
        public static SqlPart Add(object a, object b)
        {
            return new SqlPart("(", a, "+", b, ")");
        }
        public static SqlPart Subtract(object a, object b)
        {
            return new SqlPart("(", a, "-", b, ")");
        }


        public static SqlPart Sum(object column)
        {
            return new SqlFunction("sum", column);
        }
        public static SqlPart Round(object column, int decimals = 2)
        {
            return new SqlFunction("round", column, decimals);
        }
        public static SqlPart eq(object left, object right)
        {
            return new SqlPart(left, "=", right);

        }
        public static SqlPart Left(object what, int chars)
        {
            return new SqlFunction("left", what, chars);

        }
        public static SqlPart Max(ColumnBase column)
        {
            return new SqlFunction("max", column);
        }
        public static SqlPart IfNull(object value, object valueToUseWhenNull)
        {
            return IsNull(value, value);
        }
        public static SqlPart IsNull(object value, object valueToUseWhenNull)
        {
            return new SqlFunction("isnull", value, valueToUseWhenNull);
        }
        public static ISqlPart Or(params WhereItem[] what)
        {
            return new CommaSeparated(what, " or ", true);
        }
        public static ISqlPart And(params WhereItem[] what)
        {
            return new CommaSeparated(what, " and ", true) { NewLine = true };
        }
        public static SqlPart Not(params WhereItem[] what)
        {
            return new SqlFunction("not", new CommaSeparated(what, " and ", true) { NewLine = true });
        }
        public static SqlPart Distinct(params object[] column)
        {
            return new SqlPart("Distinct ", new CommaSeparated(column));
        }
        public static SqlPart Like(TextColumn column, string like)
        {
            return new SqlPart(column, " like ", StringValue(like));
        }
        public static SqlPart Min(ColumnBase column)
        {
            return new SqlFunction("min", column);

        }



        public static SqlPart StringValue(object s)
        {
            if (s is string || s is Text)
                return new SqlPart("'" + s.ToString().TrimEnd().Replace("'", "''") + "'");
            return new SqlPart(s);
        }

        public static SqlPart NotExists(Entity inTable, FilterBase where)
        {
            return new SqlPart(helper => @" NOT EXISTS (
                        SELECT 1 FROM " + helper.Translate(inTable) + @" 
                        WHERE " + helper.WhereToString(where, inTable) + ")");
        }
        public static SqlPart Exists(Entity inTable, FilterBase where)
        {
            return new SqlPart(helper => @" EXISTS (
                        SELECT 1 FROM " + helper.Translate(inTable) + @" 
                        WHERE " + helper.WhereToString(where, inTable) + ")");
        }
        public static SqlPart CastAsDecimal(object what, int decimals = 2)
        {
            return new SqlFunction("cast", new SqlPart(what, " as ", new SqlFunction("decimal", 20, decimals)));
        }


        public static CaseClass Case(WhereItem when, object then)
        {
            return new CaseClass(when, then, null);
        }

        public class CaseClass
        {
            WhereItem _where;
            object _then;
            CaseClass _parent;
            public CaseClass(WhereItem where, object then, CaseClass parent)
            {
                _where = where;
                _then = then;
                _parent = parent;
            }
            public CaseClass When(WhereItem where, object then)
            {
                return new CaseClass(where, then, this);
            }

            SqlPart GetCase()
            {
                if (_parent == null)
                    return new SqlPart("case when ", _where, " then ", StringValue(_then));
                else
                    return new SqlPart(_parent.GetCase(), " when ", _where, " then ", StringValue(_then));
            }
            public SqlPart Else(object value)
            {
                return new SqlPart(GetCase(), " else ", StringValue(value), " end");
            }
        }

        public static SelectClass Select(params SelectItem[] columns)
        {
            return new SelectClass(columns);
        }
        public class UnionClass : SqlPart
        {

            public SqlPart OrderBy(params OrderByItem[] orderBy)
            {
                return new SqlPart(h => h.Translate(this) + " " + SqlStatementKeywordBase.BuildOrderBy(h, orderBy));
            }


            public UnionClass(params object[] parts) : base(parts)
            {
            }

        }

        public class SelectClass : SqlStatementKeywordBase, EasySqlExtentions.EasySqlExtentionsHelper.CanUnion
        {
            public SelectClass(SelectItem[] select) : base(null)
            {
                _select.AddRange(select);
            }

            public FromClass From(params FromItem[] entities)
            {
                var r = new FromClass(this);
                r._from.AddRange(entities);
                return r;
            }
            public WhereClass Where(params WhereItem[] filter)
            {
                return new WhereClass(this, filter);
            }
            public OrderByClass OrderBy(params OrderByItem[] orderBy)
            {
                return new OrderByClass(this, orderBy);
            }
            public GroupByClass GroupBy(params SelectItem[] groupBy)
            {
                return new GroupByClass(this, groupBy);
            }
        }

        public class FromClass : SqlStatementKeywordBase, EasySqlExtentions.EasySqlExtentionsHelper.CanUnion
        {

            public FromClass(SqlStatementKeywordBase s) : base(s)
            {

            }
            public FromClass InnerJoin(Entity to, WhereItem where)
            {
                var result = new FromClass(this);
                result._joins.Add(new Join(to, where));
                return result;
            }
            public GroupByClass GroupBy(params SelectItem[] groupBy)
            {
                return new GroupByClass(this, groupBy);
            }
            public OrderByClass OrderBy(params OrderByItem[] orderBy)
            {
                return new OrderByClass(this, orderBy);
            }
            public FromClass LeftOuterJoin(Entity to, FilterBase where)
            {
                var result = new FromClass(this);
                result._joins.Add(new Join(to, where, "left outer"));
                return result;
            }
            public FromClass RightOuterJoin(Entity to, FilterBase where)
            {
                var result = new FromClass(this);
                result._joins.Add(new Join(to, where, "right outer"));
                return result;
            }
            public FromClass FullOuterJoin(Entity to, FilterBase where)
            {
                var result = new FromClass(this);
                result._joins.Add(new Join(to, where, "full outer"));
                return result;
            }
            public WhereClass Where(params WhereItem[] filter)
            {
                return new WhereClass(this, filter);
            }
            public WhereClass Where(ICustomFilterMember filter)
            {
                var fc = new FilterCollection();
                fc.Add("{0}", filter);
                return new WhereClass(this, new WhereItem[] { fc });
            }

        }
        public class WhereItem : ISqlPart
        {
            object _item;
            private WhereItem(object item)
            {
                _item = item;
            }
            public string Build(SqlBuilder helper)
            {
                return helper.Translate(_item);
            }
            public static implicit operator WhereItem(FilterBase filter)
            {
                return new WhereItem(filter);
            }
            public static implicit operator WhereItem(SqlPart filter)
            {
                return new WhereItem(filter);
            }
        }
        public class FromItem : ISqlPart
        {
            internal object _what;
            public FromItem(object what)
            {
                _what = what;
            }
            public static implicit operator FromItem(Entity e)
            {
                return new FromItem(e);

            }
            public static implicit operator FromItem(SqlStatementKeywordBase e)
            {
                return new FromItem(e);

            }

            internal void RegisterEntity(SqlBuilder helper)
            {
                var e = _what as Firefly.Box.Data.Entity;
                if (e != null)
                    helper.RegisterEntities(e);
            }

            public string Build(SqlBuilder helper)
            {
                return helper.Translate(_what);
            }


        }
        public class SelectItem : ISqlPart
        {
            internal object _item;
            private SelectItem(object item)
            {
                _item = item;
            }
            public string Build(SqlBuilder helper)
            {
                return helper.Translate(_item);
            }
            public static implicit operator SelectItem(ColumnBase filter)
            {
                return new SelectItem(filter);
            }

            public static implicit operator SelectItem(SqlPart filter)
            {
                return new SelectItem(filter);
            }
            public static implicit operator SelectItem(SqlStatementKeywordBase filter)
            {
                return new SelectItem(filter);
            }
            public static implicit operator SelectItem(SelectItems filter)
            {
                return new SelectItem((object[])filter._items.ToArray());
            }

        }
        public class SelectItems : IEnumerable<SelectItem>
        {
            internal List<SelectItem> _items = new List<SelectItem>();
            public SelectItems(params SelectItem[] items)
            {
                _items.AddRange(items);
            }
            public void Add(SelectItem item)
            {
                _items.Add(item);
            }

            public IEnumerator<SelectItem> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _items.GetEnumerator();
            }
        }
        public class OrderByItem : ISqlPart
        {
            internal object _item;
            private OrderByItem(object item)
            {
                _item = item;
            }
            public string Build(SqlBuilder helper)
            {
                return helper.Translate(_item);
            }
            public static implicit operator OrderByItem(ColumnBase filter)
            {
                return new OrderByItem(filter);
            }
            public static implicit operator OrderByItem(SqlPart filter)
            {
                return new OrderByItem(filter);
            }
            public static implicit operator OrderByItem(int filter)
            {
                return new OrderByItem(filter);
            }
            public static implicit operator OrderByItem(SortDirection filter)
            {
                return new OrderByItem(filter);
            }

        }
        public class WhereClass : SqlStatementKeywordBase, EasySqlExtentions.EasySqlExtentionsHelper.CanUnion
        {
            public WhereClass(SqlStatementKeywordBase from, WhereItem[] filter) : base(from)
            {
                this._where.AddRange(filter);
            }

            public GroupByClass GroupBy(params SelectItem[] groupBy)
            {
                return new GroupByClass(this, groupBy);
            }
            public OrderByClass OrderBy(params OrderByItem[] orderBy)
            {
                return new OrderByClass(this, orderBy);
            }

        }
        public class SqlStatementKeywordBase : ISqlPart
        {
            internal List<SelectItem> _select = new List<SelectItem>(),

            _groupBy = new List<SelectItem>();
            internal List<OrderByItem> _orderBy = new List<OrderByItem>();
            internal List<WhereItem> _where = new List<WhereItem>();
            internal List<FromItem> _from = new List<FromItem>();
            internal List<Join> _joins = new List<Join>();
            internal List<WhereItem> _having = new List<WhereItem>();
            public SqlStatementKeywordBase(SqlStatementKeywordBase original)
            {
                if (original != null)
                    CopyFrom(original);
            }
            public static implicit operator Func<string>(SqlStatementKeywordBase sql)
            {
                return sql.ToSql;
            }

            public string ToSql()
            {
                return InternalBuild(new SqlBuilder());
            }
            protected void CopyFrom(SqlStatementKeywordBase b)
            {
                _select.AddRange(b._select);
                _where.AddRange(b._where);
                _groupBy.AddRange(b._groupBy);
                _orderBy.AddRange(b._orderBy);
                _having.AddRange(b._having);
                _from.AddRange(b._from);
                _joins.AddRange(b._joins);
            }

            public string Build(SqlBuilder helper)
            {
                return "(" + InternalBuild(helper) + ")";
            }


            private string InternalBuild(SqlBuilder helper)
            {
                if (_from.Count == 0)
                {
                    var entitiesDone = new HashSet<Entity>();
                    foreach (var item in _select)
                    {
                        var c = item._item as ColumnBase;
                        if (c != null)
                        {
                            if (!entitiesDone.Contains(c.Entity))
                            {
                                _from.Add(c.Entity);
                                entitiesDone.Add(c.Entity);
                            }
                        }
                    }
                    if (_from.Count == 0)
                        throw new InvalidOperationException("Couldn't figure out the from");
                }
                foreach (var item in _from)
                {
                    item.RegisterEntity(helper);
                }

                if (_joins != null)
                    foreach (var item in _joins)
                    {
                        helper.RegisterEntities(item.To);
                    }

                var tempFrom = new List<object>();
                foreach (var item in _from)
                {
                    var z = item._what as SqlStatementKeywordBase;
                    if (z != null)
                    {
                        tempFrom.Add(new SqlPart(z, " ", helper.generateAlias()));
                    }
                    else
                        tempFrom.Add(item._what);
                }

                var theFrom = helper.Translate(new CommaSeparated(tempFrom) { NewLine = true });
                helper.Indent();
                if (_joins != null)
                    foreach (var j in _joins)
                    {
                        theFrom += helper.NewLine + (j.joinType) + " join " + helper.Translate(j.To) + " on " + helper.Translate(j.On);

                    }
                helper.UnIndent();

                string theWhere = helper.Translate(And(_where.ToArray()));
                if (theWhere != "")
                    theWhere = helper.NewLine + " Where " + theWhere;

                var result = "Select " + helper.Translate(new CommaSeparated(_select) { NewLine = true }) +
                    helper.NewLine + "  From " + theFrom +
                    theWhere;
                if (_groupBy != null && _groupBy.Count > 0)
                {
                    result += helper.NewLine + "Group by " + helper.Translate(new CommaSeparated(_groupBy) { NewLine = true });
                }
                if (_having.Count > 0)
                    result += helper.NewLine + "Having " + helper.Translate(And(_having.ToArray()));


                if (_orderBy != null && _orderBy.Count > 0)
                {
                    result += BuildOrderBy(helper, _orderBy); ;
                }
                return result;
            }

            internal static string BuildOrderBy(SqlBuilder helper, IEnumerable<OrderByItem> _orderBy)
            {
                var ob = "";
                foreach (var item in _orderBy)
                {
                    if (item._item is SortDirection)
                    {
                        if (((SortDirection)item._item) == SortDirection.Descending)
                            ob += " desc";
                        continue;
                    }
                    if (ob.Length > 0)
                    {
                        ob += ", " + helper.NewLine;
                    }


                    ob += helper.Translate(item);
                }
                return helper.NewLine + "Order by " + ob;

            }

            public void TestOn(DynamicSQLSupportingDataProvider connection)
            {
                DoTest(connection, ToSql());
            }
            internal static void DoTest(DynamicSQLSupportingDataProvider connection, string sql)
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
.error {
    color: black;
    background-color: rgba(172,0,0,.1);
    text-shadow: none;
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
                    try
                    {

                        // If you get a build error - try the following line instead
                        //var r = connection.GetResult(sql);
                        var r = connection.GetHtmlTableBasedOnSQLResultForDebugPurposes(sql);
                        sw.WriteLine("<h2>Sql:</h2>");
                        sw.WriteLine("<pre>" + sql + "</pre>");
                        sw.WriteLine("<h2>Result:</h2>");
                        sw.WriteLine(r);
                    }
                    catch (Exception ex)
                    {
                        sw.WriteLine("<h2>Sql Error:</h2>");
                        sw.WriteLine("<pre class=\"error\">" + ex.Message + "</pre>");

                        int errorLine = 0;
                        var sqlErr = ex as System.Data.SqlClient.SqlException;
                        if (sqlErr != null && sqlErr.Errors.Count > 0)
                            errorLine = sqlErr.Errors[0].LineNumber;
                        if (errorLine == 1)
                            errorLine = 0;

                        sw.WriteLine("<h2>Sql:</h2>");
                        sw.WriteLine("<pre>");
                        using (var sr = new System.IO.StringReader(sql))
                        {
                            int i = 1;
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (errorLine == i)
                                {
                                    line = "<span class=\"error\">" + line + "</span>";
                                }
                                sw.WriteLine(line);
                                i++;
                            }
                        }
                        sw.WriteLine("</pre>");

                    }
                    finally
                    {
                    }
                    sw.WriteLine("on: " + DateTime.Now.ToString());
                }
                // If you get a build error - try the following line instead
                // Windows.StartRun(t);
                Windows.OSCommand(t);
            }


        }

        public class GroupByClass : SqlStatementKeywordBase, EasySqlExtentions.EasySqlExtentionsHelper.CanUnion
        {
            public GroupByClass(SqlStatementKeywordBase where, SelectItem[] columns) : base(where)
            {
                _groupBy.AddRange(columns);

            }
            public HavingClass Having(params WhereItem[] filter)
            {
                return new HavingClass(this, filter);
            }

            public OrderByClass OrderBy(params OrderByItem[] orderBy)
            {
                return new OrderByClass(this, orderBy);
            }
        }
        public class HavingClass : SqlStatementKeywordBase, EasySqlExtentions.EasySqlExtentionsHelper.CanUnion
        {
            public HavingClass(GroupByClass where, WhereItem[] columns) : base(where)
            {
                _having.AddRange(columns);

            }

            public OrderByClass OrderBy(params OrderByItem[] orderBy)
            {
                return new OrderByClass(this, orderBy);
            }
        }
        public class OrderByClass : SqlStatementKeywordBase
        {
            public OrderByClass(SqlStatementKeywordBase parent, OrderByItem[] orderBy) : base(parent)
            {
                _orderBy.AddRange(orderBy);
            }
        }
    }
    public class CommaSeparated : ISqlPart
    {
        System.Collections.IEnumerable _what;
        string _separator;
        bool _addParenthesis;
        public bool NewLine { get; set; }
        public CommaSeparated(System.Collections.IEnumerable what, string separator = ", ", bool addParenthesis = false)
        {
            _what = what;
            _separator = separator;
            _addParenthesis = addParenthesis;
        }

        public string Build(SqlBuilder helper)
        {
            helper.Indent();
            var selectString = "";
            foreach (var item in _what)
            {
                if (selectString.Length != 0)
                {
                    selectString += _separator;
                    if (NewLine)
                        selectString += helper.NewLine;
                }
                string x;

                if (item is object[])
                {
                    x = helper.Translate(new CommaSeparated((object[])item, _separator, _addParenthesis) { NewLine = NewLine });
                }
                else if (item is EasySql.SelectItems)
                {
                    x = helper.Translate(new CommaSeparated((object[])((EasySql.SelectItems)item)._items.ToArray(), _separator, _addParenthesis) { NewLine = NewLine });
                }
                else if (item is EasySql.SelectItem && ((EasySql.SelectItem)item)._item is EasySql.SelectItem[])
                {
                    x = helper.Translate(new CommaSeparated((object[])((EasySql.SelectItem[])((EasySql.SelectItem)item)._item), _separator, _addParenthesis) { NewLine = NewLine });
                }
                else
                    x = helper.Translate(item);
                if (_addParenthesis)
                {
                    x = "(" + x + ")";
                }
                selectString += x;

            }
            helper.UnIndent();
            return selectString;
        }
    }
    public class Join
    {
        public Entity To;
        public WhereItem On;
        public string joinType;
        public Join(Entity to, WhereItem on, string joinType = "inner")
        {
            this.joinType = joinType;
            To = to;
            On = on;
        }
    }
    public interface ISqlPart
    {
        string Build(SqlBuilder helper);
    }
    public class SqlFunction : SqlPart
    {

        public SqlFunction(string name, params object[] args) : base(name, "(", new CommaSeparated(args), ")")
        {

        }

    }
    public class SqlPart : ISqlPart
    {
        Func<SqlBuilder, string> _part;
        public SqlPart(Func<SqlBuilder, string> part)
        {
            _part = part;
        }
        public string Build(SqlBuilder helper)
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

        public string ToSql()
        {
            return this.Build(new SqlBuilder());
        }
        public static implicit operator Func<string>(SqlPart uc)
        {
            return () => uc.ToSql();
        }
        public void TestOn(DynamicSQLSupportingDataProvider connection)
        {
            EasySql.SqlStatementKeywordBase.DoTest(connection, ToSql());
        }
    }


    public class SqlBuilder
    {
        string _tab = "       ";
        string _indentTab = "";
        public string NewLine { get { return "\r\n" + _indentTab; } }
        int _indent = 0;
        internal void Indent()
        {
            _indent++;
            buildIndent();
        }
        void buildIndent()
        {
            _indentTab = "";
            for (int i = 0; i < _indent; i++)
            {
                _indentTab += _tab;
            }
        }
        internal void UnIndent()
        {
            _indent--;
            buildIndent();
        }
        Dictionary<Firefly.Box.Data.Entity, string> _aliases = new Dictionary<Firefly.Box.Data.Entity, string>();
        int _aliasCount = 0;
        public void RegisterEntities(params Entity[] entities)
        {
            foreach (var item in entities)
            {
                if (!_aliases.ContainsKey(item))
                    _aliases.Add(item, generateAlias());
            }
        }
        public void RegisterEntity(Firefly.Box.Data.Entity entity, string alias = null)
        {
            if (alias == null)
                alias = entity.EntityName;
            _aliases[entity] = alias;
        }
        bool _isOracle = false;
        internal string WhereToString(FilterBase f, params Entity[] moreEntities)
        {

            RegisterEntities(moreEntities);

            var x = FilterBase.GetIFilter(f, false, _aliases.Keys.ToArray());
            // If you get a build error - try the following line instead
            // var p = new NoParametersFilterItemSaver();
            var p = new NoParametersFilterItemSaver(true, _isOracle ? OracleClientEntityDataProvider.DateTimeStringFormat : SQLClientEntityDataProvider.DateTimeStringFormat, DummyDateTimeCollector.Instance);
            var z = new SQLFilterConsumer(
                    p,
                    y =>
                    {
                        return WriteColumnWithAlias(y);
                        // If you get a build error - try the following line instead
                        //  }, false, new dummySqlFilterHelper());  
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
        public string ToSql(params object[] parts)
        {
            var sb = "";
            foreach (var item in parts)
            {
                sb += this.Translate(item);
            }
            return sb;
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

        internal string generateAlias()
        {
            return "t" + (++_aliasCount);
        }
    }
}
