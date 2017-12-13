﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FunWithSpikes.PardonTheExpression
{
    public class Query<T>
    {
        private readonly IDatabase _db;
        private readonly List<IWhere> _wheres;

        public Query(IDatabase db)
        {
            _db = db;
            _wheres = new List<IWhere>();
        }

        public Query<T> Where<TCol>(Expression<Func<T, TCol>> colSelector, TCol value, WhereOperator op = WhereOperator.Equals)
        {
            _wheres.Add(new Where<T, TCol>(colSelector, value, op));
            return this;
        }

        public IEnumerable<T> Execute()
        {
            var sql = ToSql();
            return _db.Execute<T>(sql);
        }

        private string ToSql()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"SELECT * FROM {typeof(T).Name}");

            if (_wheres.Any())
            {
                sb.AppendLine("WHERE");
            }

            var whereSet = _wheres.Select(w => $"    {w}\r\n");
            sb.AppendLine(string.Join(" AND ", whereSet));

            return sb.ToString();
        }
    }
}
