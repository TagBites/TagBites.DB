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
        var namesMap = new Dictionary<string, int>();
        var listNonUnique = new List<int>();

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);
            var lowerName = name.ToLower();

            names.Add(name);

            if (namesMap.ContainsKey(lowerName))
                listNonUnique.Add(i);
            else
                namesMap.Add(lowerName, i);
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var index = 0; index < listNonUnique.Count; index++)
        {
            var i = listNonUnique[index];
            var name = names[i];
            var lowerName = name.ToLower();
            var nameSuffixIndex = 0;

            while (namesMap.TryGetValue(lowerName, out _))
                lowerName = name.ToLower() + (++nameSuffixIndex).ToString(CultureInfo.InvariantCulture);

            if (nameSuffixIndex > 0)
                name += nameSuffixIndex.ToString(CultureInfo.InvariantCulture);

            names[i] = name;
            namesMap.Add(lowerName, i);
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
