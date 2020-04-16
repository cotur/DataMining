using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Association
{
    public class DataFields
    {
        public List<string> FieldNames { get; private set; } = new List<string>();

        public List<bool[]> Rows { get; private set; }

        public DataFields(IEnumerable<string> fieldNames, IEnumerable<IEnumerable<bool>> rows)
        {
            FieldNames = fieldNames.ToList();
            Rows = rows.Select(x => x.ToArray()).ToList();
        }

        public DataFields(IEnumerable<IEnumerable<bool>> rows)
        {
            Rows = rows.Select(x => x.ToArray()).ToList();
            FillNames();
        }

        public DataFields(IEnumerable<IEnumerable<int>> transactions, IEnumerable<string> fieldNames = null)
        {
            Implement(transactions.Max(x => x.Max()), transactions, fieldNames);
        }

        public DataFields(int fieldCount, IEnumerable<IEnumerable<int>> transactions, IEnumerable<string> fieldNames = null)
        {
            Implement(fieldCount, transactions, fieldNames);
        }

        private void Implement(int fieldCount, IEnumerable<IEnumerable<int>> transactions, IEnumerable<string> fieldNames = null)
        {
            Rows = new List<bool[]>();
            foreach (var transaction in transactions)
            {
                Rows.Add(CreateRow(fieldCount, transaction));
            }

            if (fieldNames == null)
            {
                FillNames();
            }
            else
            {
                FieldNames = fieldNames.ToList();
            }
        }

        private bool[] CreateRow(int size, IEnumerable<int> idS)
        {
            var row = new bool[size + 1];
            foreach (int id in idS)
            {
                row[id] = true;
            }
            return row;
        }
        
        private void FillNames()
        {
            for (int i = 0; i < Rows.First().Length; i++)
            {
                FieldNames.Add("Column " + i);
            }
        }

        public List<string> GetElementsName(List<int> elementIDs)
        {
            return FieldNames.Where((e, i) => elementIDs.Contains(i)).ToList();
        }

        public static DataFields ReadFromFile(string filePath, char splitWith = ',', bool headIsInfo = true, string acceptAsTrue = "1")
        {
            List<string> fieldNames = null;
            List<bool[]> rows = new List<bool[]>();

            foreach (var line in File.ReadAllLines(filePath))
            {
                var splintedLine = line.Split(splitWith);
                if (headIsInfo)
                {
                    fieldNames = splintedLine.Select(x => x.Trim()).ToList();
                    headIsInfo = false;
                    continue;
                }

                rows.Add(
                    splintedLine.Select(x => 
                                    x.Trim() == acceptAsTrue
                                ).ToArray()
                    );
            }

            return fieldNames == null ? new DataFields(rows) : new DataFields(fieldNames, rows);
        }
    }
}
