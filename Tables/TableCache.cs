namespace LinearProgrammingSolver.Tables
{
    public static class TableCache
    {
        private static Dictionary<string, Table> _tableCache = new Dictionary<string, Table>();
        
        public static bool StoreTable(Table table)
        {
            if (table == null || string.IsNullOrEmpty(table.TableId))
                return false;
                
            _tableCache[table.TableId] = table;
            return true;
        }
        
        public static Table GetTable(string tableId)
        {
            if (string.IsNullOrEmpty(tableId))
                return null;
                
            return _tableCache.ContainsKey(tableId) ? _tableCache[tableId] : null;
        }
        
        public static bool TableExists(string tableId)
        {
            return !string.IsNullOrEmpty(tableId) && _tableCache.ContainsKey(tableId);
        }
        
        public static bool RemoveTable(string tableId)
        {
            if (string.IsNullOrEmpty(tableId))
                return false;
                
            return _tableCache.Remove(tableId);
        }
        
        public static List<Table> GetAllTables()
        {
            return _tableCache.Values.ToList();
        }
        
        public static List<string> GetAllTableIds()
        {
            return _tableCache.Keys.ToList();
        }
        
        public static List<Table> GetTablesByStatus(string status)
        {
            return _tableCache.Values.Where(t => t.Status == status).ToList();
        }
        
        public static int GetTableCount()
        {
            return _tableCache.Count;
        }
        
        public static void ClearAllTables()
        {
            _tableCache.Clear();
        }

        public static void ClearAlgorithmTables()
        {
            var keysToRemove = new List<string>();
            
            foreach (var kvp in _tableCache)
            {
                string tableId = kvp.Key;
                string status = kvp.Value.Status;
                
                bool isEssentialInputTable = 
                    (tableId == "t-raw" && status == "Raw") ||
                    (tableId == "t-i" && status == "Canonical") ||
                    (tableId == "t-canonical" && status == "Canonical");
                
                if (isEssentialInputTable)
                {
                    continue;
                }
                
                keysToRemove.Add(tableId);
            }
            
            foreach (var key in keysToRemove)
            {
                _tableCache.Remove(key);
            }
        }
        
        public static void DisplayTableSummary()
        {
            Console.WriteLine("\n=== Table Storage Summary ===");
            Console.WriteLine($"Total Tables: {_tableCache.Count}");
            
            if (_tableCache.Count == 0)
            {
                Console.WriteLine("No tables stored.");
                return;
            }
            
            Console.WriteLine("\nStored Tables:");
            Console.WriteLine("TableID".PadRight(20) + "Status".PadRight(25) + "Size".PadRight(8) + "Created");
            Console.WriteLine(new string('-', 65));
            
            foreach (var kvp in _tableCache.OrderBy(x => x.Value.CreatedTime))
            {
                var table = kvp.Value;
                string size = $"{table.GetRowCount()}x{table.GetColumnCount()}";
                string created = table.CreatedTime.ToString("HH:mm:ss");
                
                string status = table.Status.Length > 24 ? table.Status.Substring(0, 21) + "..." : table.Status;
                
                Console.WriteLine($"{table.TableId.PadRight(20)}{status.PadRight(25)}{size.PadRight(8)}{created}");
            }
        }
        
        public static Table CreateAndStoreTable(string tableId, double[,] matrix, 
                                              List<string> rowLabels, List<string> columnLabels, 
                                              OptimizationType optimizationType, string status = "Unknown")
        {
            var table = new Table(tableId, matrix, rowLabels, columnLabels, optimizationType, status);
            StoreTable(table);
            return table;
        }
        
        public static Table CloneAndStoreTable(string sourceTableId, string newTableId, string newStatus = null)
        {
            var sourceTable = GetTable(sourceTableId);
            if (sourceTable == null)
                return null;
                
            var clonedTable = new Table(newTableId, sourceTable, newStatus);
            StoreTable(clonedTable);
            return clonedTable;
        }
        
        public static bool DisplayTable(string tableId)
        {
            var table = GetTable(tableId);
            if (table == null)
            {
                Console.WriteLine($"Table '{tableId}' not found.");
                return false;
            }
            
            table.DisplayTraditional();
            return true;
        }
        
        public static bool DisplayTableMatrix(string tableId)
        {
            var table = GetTable(tableId);
            if (table == null)
            {
                Console.WriteLine($"Table '{tableId}' not found.");
                return false;
            }
            
            table.DisplayMatrix();
            return true;
        }

        public static void DisplayAllTablesDetailed()
        {
            Console.WriteLine("\n=== COMPLETE TABLE CACHE CONTENTS ===");
            
            if (_tableCache.Count == 0)
            {
                Console.WriteLine("No tables cached.");
                return;
            }

            Console.WriteLine($"Total Tables in Cache: {_tableCache.Count}");
            Console.WriteLine();

            var sortedTables = _tableCache.Values
                .OrderBy(t => t.CreatedTime)
                .ThenBy(t => GetTableOrder(t.TableId))
                .ToList();

            for (int i = 0; i < sortedTables.Count; i++)
            {
                var table = sortedTables[i];
                
                table.DisplayTraditional();
                
                if (i < sortedTables.Count - 1)
                {
                    Console.WriteLine();
                    Console.WriteLine(new string('=', 80));
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
            Console.WriteLine("=== END OF TABLE CACHE ===");
        }

        private static int GetTableOrder(string tableId)
        {
            return tableId.ToLower() switch
            {
                "t-raw" => 1,
                "t-i" => 2,
                "t-canonical" => 2,
                "t-1" => 10,
                "t-2" => 11,
                "t-3" => 12,
                "t-4" => 13,
                "t-5" => 14,
                "t-optimal" => 20,
                "t-1.1" => 30,
                "t-1.2" => 31,
                "t-1.1.1" => 32,
                "t-1.1.2" => 33,
                "t-1.2.1" => 34,
                "t-1.2.2" => 35,
                _ => 50
            };
        }

        public static void DisplayAllTablesMatrix()
        {
            Console.WriteLine("\n=== COMPLETE TABLE CACHE (MATRIX FORMAT) ===");
            
            if (_tableCache.Count == 0)
            {
                Console.WriteLine("No tables cached.");
                return;
            }

            Console.WriteLine($"Total Tables in Cache: {_tableCache.Count}");
            Console.WriteLine();

            var sortedTables = _tableCache.Values
                .OrderBy(t => t.CreatedTime)
                .ThenBy(t => GetTableOrder(t.TableId))
                .ToList();

            for (int i = 0; i < sortedTables.Count; i++)
            {
                var table = sortedTables[i];
                
                table.DisplayMatrix();
                
                if (i < sortedTables.Count - 1)
                {
                    Console.WriteLine();
                    Console.WriteLine(new string('=', 80));
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
            Console.WriteLine("=== END OF TABLE CACHE (MATRIX FORMAT) ===");
        }
    }
}