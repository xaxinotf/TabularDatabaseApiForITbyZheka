using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TabularDatabaseApiForITbyZheka.Models;

namespace TabularDatabaseApiForITbyZheka.Services
{
    public class DatabaseService
    {
        private readonly string _filePath;
        private List<DatabaseModel> _databases;

        public DatabaseService(string filePath)
        {
            _filePath = filePath;
            _databases = LoadData();
        }

        /// <summary>
        /// Завантажує дані з JSON файлу.
        /// </summary>
        /// <returns>Список баз даних.</returns>
        public List<DatabaseModel> LoadData()
        {
            if (!File.Exists(_filePath))
                return new List<DatabaseModel>();

            var jsonData = File.ReadAllText(_filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<DatabaseContainer>(jsonData, options)?.Databases ?? new List<DatabaseModel>();
        }

        /// <summary>
        /// Зберігає дані у JSON файл.
        /// </summary>
        public void SaveData()
        {
            var container = new DatabaseContainer { Databases = _databases };
            var jsonData = JsonSerializer.Serialize(container, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, jsonData);
        }

        // Методи для управління базами даних
        public List<DatabaseModel> GetDatabases() => _databases;

        public bool CreateDatabase(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (_databases.Any(db => db.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return false;

            _databases.Add(new DatabaseModel { Name = name });
            SaveData();
            return true;
        }

        public bool DeleteDatabase(string name)
        {
            var db = _databases.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (db == null)
                return false;

            _databases.Remove(db);
            SaveData();
            return true;
        }

        // Методи для управління таблицями
        public DatabaseModel GetDatabase(string name) => _databases.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public bool CreateTable(string databaseName, string tableName)
        {
            var db = GetDatabase(databaseName);
            if (db == null || db.Tables.Any(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
                return false;

            db.Tables.Add(new TableModel { Name = tableName });
            SaveData();
            return true;
        }

        public bool DeleteTable(string databaseName, string tableName)
        {
            var db = GetDatabase(databaseName);
            if (db == null)
                return false;

            var table = db.Tables.FirstOrDefault(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (table == null)
                return false;

            db.Tables.Remove(table);
            SaveData();
            return true;
        }

        // Методи для управління стовпцями
        public bool AddColumn(string databaseName, string tableName, ColumnModel column)
        {
            var table = GetTable(databaseName, tableName);
            if (table == null || table.Columns.Any(c => c.Name.Equals(column.Name, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Додати валідацію специфічних типів даних
            if (column.DataType == "$" && !column.MaxMoney.HasValue)
                return false;

            if (column.DataType == "$Invl" && (column.Interval == null || column.Interval.Start >= column.Interval.End))
                return false;

            table.Columns.Add(column);
            SaveData();
            return true;
        }

        public bool RenameColumn(string databaseName, string tableName, string oldName, string newName)
        {
            var table = GetTable(databaseName, tableName);
            if (table == null)
                return false;

            var column = table.Columns.FirstOrDefault(c => c.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase));
            if (column == null || table.Columns.Any(c => c.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                return false;

            column.Name = newName;
            SaveData();
            return true;
        }

        public bool ReorderColumns(string databaseName, string tableName, List<string> newOrder)
        {
            var table = GetTable(databaseName, tableName);
            if (table == null || newOrder.Count != table.Columns.Count || !newOrder.All(name => table.Columns.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))))
                return false;

            table.Columns = newOrder.Select(name => table.Columns.First(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))).ToList();
            SaveData();
            return true;
        }

        // Методи для управління рядками
        public bool AddRow(string databaseName, string tableName, RowModel row)
        {
            var table = GetTable(databaseName, tableName);
            if (table == null)
                return false;

            // Валідація відповідності кількості значень кількості стовпців
            if (row.Values.Count != table.Columns.Count)
                return false;

            // Валідація типів даних
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];
                var value = row.Values[i];
                if (!ValidateValue(column.DataType, value, column))
                    return false;
            }

            table.Rows.Add(row);
            SaveData();
            return true;
        }

        public bool EditRow(string databaseName, string tableName, int rowIndex, RowModel updatedRow)
        {
            var table = GetTable(databaseName, tableName);
            if (table == null)
                return false;

            if (rowIndex < 0 || rowIndex >= table.Rows.Count)
                return false;

            if (updatedRow.Values.Count != table.Columns.Count)
                return false;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];
                var value = updatedRow.Values[i];
                if (!ValidateValue(column.DataType, value, column))
                    return false;
            }

            table.Rows[rowIndex] = updatedRow;
            SaveData();
            return true;
        }

        public bool DeleteRow(string databaseName, string tableName, int rowIndex)
        {
            var table = GetTable(databaseName, tableName);
            if (table == null)
                return false;

            if (rowIndex < 0 || rowIndex >= table.Rows.Count)
                return false;

            table.Rows.RemoveAt(rowIndex);
            SaveData();
            return true;
        }

        /// <summary>
        /// Валідує значення відповідно до типу даних стовпця.
        /// </summary>
        /// <param name="dataType">Тип даних стовпця.</param>
        /// <param name="value">Значення для валідації.</param>
        /// <param name="column">Стовпець, для якого виконується валідація.</param>
        /// <returns>Повертає true, якщо значення валідне, інакше false.</returns>
        private bool ValidateValue(string dataType, object value, ColumnModel column)
        {
            try
            {
                switch (dataType.ToLower())
                {
                    case "integer":
                        return int.TryParse(value.ToString(), out _);

                    case "real":
                        return double.TryParse(value.ToString(), out _);

                    case "char":
                        return value is string s && s.Length == 1;

                    case "string":
                        return value is string;

                    case "$":
                        return decimal.TryParse(value.ToString(), out var dec) && dec <= 10000000000000.00m;

                    case "$invl":
                        if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                        {
                            if (jsonElement.TryGetProperty("Start", out var startProp) &&
                                jsonElement.TryGetProperty("End", out var endProp))
                            {
                                if (startProp.TryGetDecimal(out var start) && endProp.TryGetDecimal(out var end))
                                {
                                    return start < end;
                                }
                            }
                        }
                        return false;

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Отримує таблицю за іменем бази даних та іменем таблиці.
        /// </summary>
        /// <param name="databaseName">Ім'я бази даних.</param>
        /// <param name="tableName">Ім'я таблиці.</param>
        /// <returns>Повертає об'єкт TableModel або null, якщо не знайдено.</returns>
        public TableModel GetTable(string databaseName, string tableName)
        {
            var db = GetDatabase(databaseName);
            return db?.Tables.FirstOrDefault(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        }

        // Додатковий контейнер для десеріалізації
        private class DatabaseContainer
        {
            public List<DatabaseModel> Databases { get; set; } = new();
        }
    }
}
