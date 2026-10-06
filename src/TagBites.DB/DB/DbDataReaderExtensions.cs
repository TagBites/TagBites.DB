using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;

namespace TagBites.DB;

internal static class DbDataReaderExtensions
{
    public static QueryResult ReadResult(this DbDataReader reader)
    {
        // Names
        var names = new List<string>();
        var namesMap = new Dictionary<string, int>(reader.FieldCount, StringComparer.OrdinalIgnoreCase);
        var listNonUnique = new List<int>();

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);

            names.Add(name);

            if (namesMap.ContainsKey(name))
                listNonUnique.Add(i);
            else
                namesMap.Add(name, i);
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var index = 0; index < listNonUnique.Count; index++)
        {
            var i = listNonUnique[index];
            var name = names[i];
            var uniqueName = name;
            var nameSuffixIndex = 0;

            while (namesMap.ContainsKey(uniqueName))
                uniqueName = name + (++nameSuffixIndex).ToString(CultureInfo.InvariantCulture);

            names[i] = uniqueName;
            namesMap.Add(uniqueName, i);
        }

        // Rows
        var rows = new List<object[]>();
        while (reader.Read())
        {
            var row = new object[names.Count];
            reader.GetValues(row);
            rows.Add(row);
        }

        return QueryResult.Create(names, namesMap, rows);
    }
    public static QueryResult[] ReadBatchResult(this DbDataReader reader)
    {
        var results = new List<QueryResult>();

        do
        {
            var result = ReadResult(reader);
            results.Add(result);
        }
        while (reader.NextResult());

        return results.ToArray();
    }
}
