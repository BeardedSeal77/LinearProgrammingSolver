namespace LinearProgrammingSolver.Tables
{
    /// <summary>
    /// Static class for caching tables in memory during the solution process.
    /// Acts as a central cache/stack for all tables created during LP/IP solving.
    /// Provides simple store-retrieve functionality for the solution history.
    /// </summary>
    public static class TableCache
    {
        // =================================================================
        // TABLE STORAGE (In-memory dictionary)
        // =================================================================
        
        /// <summary>
        /// Dictionary caching all tables by their unique TableId (acts like a solution history stack)
        /// Key: TableId (e.g., "t-raw", "t-canonical", "t-1", "t-optimal", "t-1.1", "t-1.2")
        /// Value: Table object
        /// Grows as algorithms work: raw → canonical → iterations → optimal → branches
        /// </summary>
        private static Dictionary<string, Table> _tableCache = new Dictionary<string, Table>();
        
        // =================================================================
        // STORAGE OPERATIONS (Create-Store-Call pattern)
        // =================================================================
        
        /// <summary>
        /// Store a table in memory with its TableId as the key.
        /// If a table with the same ID exists, it will be replaced.
        /// </summary>
        /// <param name="table">Table to store</param>
        /// <returns>True if stored successfully</returns>
        public static bool StoreTable(Table table)
        {
            if (table == null || string.IsNullOrEmpty(table.TableId))
                return false;
                
            _tableCache[table.TableId] = table;
            return true;
        }
        
        /// <summary>
        /// Retrieve a table by its unique TableId.
        /// </summary>
        /// <param name="tableId">Unique identifier of the table to retrieve</param>
        /// <returns>Table object if found, null if not found</returns>
        public static Table GetTable(string tableId)
        {
            if (string.IsNullOrEmpty(tableId))
                return null;
                
            return _tableCache.ContainsKey(tableId) ? _tableCache[tableId] : null;
        }
        
        /// <summary>
        /// Check if a table with the given ID exists in storage.
        /// </summary>
        /// <param name="tableId">Table ID to check</param>
        /// <returns>True if table exists, false otherwise</returns>
        public static bool TableExists(string tableId)
        {
            return !string.IsNullOrEmpty(tableId) && _tableCache.ContainsKey(tableId);
        }
        
        /// <summary>
        /// Remove a table from storage.
        /// </summary>
        /// <param name="tableId">Table ID to remove</param>
        /// <returns>True if removed successfully, false if not found</returns>
        public static bool RemoveTable(string tableId)
        {
            if (string.IsNullOrEmpty(tableId))
                return false;
                
            return _tableCache.Remove(tableId);
        }
        
        // =================================================================
        // RETRIEVAL OPERATIONS (Get collections of tables)
        // =================================================================
        
        /// <summary>
        /// Get all stored tables as a list.
        /// Useful for displaying solution progress or exporting results.
        /// </summary>
        /// <returns>List of all stored tables</returns>
        public static List<Table> GetAllTables()
        {
            return _tableCache.Values.ToList();
        }
        
        /// <summary>
        /// Get all table IDs currently in storage.
        /// </summary>
        /// <returns>List of all table IDs</returns>
        public static List<string> GetAllTableIds()
        {
            return _tableCache.Keys.ToList();
        }
        
        /// <summary>
        /// Get tables filtered by status.
        /// </summary>
        /// <param name="status">Status to filter by (e.g., "Optimal", "Iteration")</param>
        /// <returns>List of tables with the specified status</returns>
        public static List<Table> GetTablesByStatus(string status)
        {
            return _tableCache.Values.Where(t => t.Status == status).ToList();
        }
        
        /// <summary>
        /// Get the count of stored tables.
        /// </summary>
        /// <returns>Number of tables currently stored</returns>
        public static int GetTableCount()
        {
            return _tableCache.Count;
        }
        
        // =================================================================
        // UTILITY OPERATIONS (Management helpers)
        // =================================================================
        
        /// <summary>
        /// Clear all stored tables from memory.
        /// Use this when starting a new problem or resetting the solver.
        /// </summary>
        public static void ClearAllTables()
        {
            _tableCache.Clear();
        }
        
        /// <summary>
        /// Display summary of all stored tables.
        /// Shows TableId, Status, and creation time for each table.
        /// </summary>
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
                
                // Truncate long status messages for better formatting
                string status = table.Status.Length > 24 ? table.Status.Substring(0, 21) + "..." : table.Status;
                
                Console.WriteLine($"{table.TableId.PadRight(20)}{status.PadRight(25)}{size.PadRight(8)}{created}");
            }
        }
        
        // =================================================================
        // CONVENIENCE METHODS (Common operations)
        // =================================================================
        
        /// <summary>
        /// Create and store a table from raw data in one operation.
        /// Commonly used by FileReader and CanonicalFormConverter.
        /// </summary>
        /// <param name="tableId">Unique table identifier</param>
        /// <param name="matrix">Tableau matrix</param>
        /// <param name="rowLabels">Row labels</param>
        /// <param name="columnLabels">Column labels</param>
        /// <param name="optimizationType">Maximize or Minimize</param>
        /// <param name="status">Table status</param>
        /// <returns>The created and stored table</returns>
        public static Table CreateAndStoreTable(string tableId, double[,] matrix, 
                                              List<string> rowLabels, List<string> columnLabels, 
                                              OptimizationType optimizationType, string status = "Unknown")
        {
            var table = new Table(tableId, matrix, rowLabels, columnLabels, optimizationType, status);
            StoreTable(table);
            return table;
        }
        
        /// <summary>
        /// Clone an existing table with a new ID and store it.
        /// Commonly used for creating iteration tables (t-1, t-2, etc.).
        /// </summary>
        /// <param name="sourceTableId">ID of table to clone from</param>
        /// <param name="newTableId">ID for the new cloned table</param>
        /// <param name="newStatus">Optional new status for cloned table</param>
        /// <returns>The cloned and stored table, or null if source not found</returns>
        public static Table CloneAndStoreTable(string sourceTableId, string newTableId, string newStatus = null)
        {
            var sourceTable = GetTable(sourceTableId);
            if (sourceTable == null)
                return null;
                
            var clonedTable = new Table(newTableId, sourceTable, newStatus);
            StoreTable(clonedTable);
            return clonedTable;
        }
        
        /// <summary>
        /// Get and display a table in traditional format.
        /// Convenience method for quick table viewing.
        /// </summary>
        /// <param name="tableId">ID of table to display</param>
        /// <returns>True if table was found and displayed</returns>
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
        
        /// <summary>
        /// Get and display a table in matrix format.
        /// Convenience method for quick matrix viewing.
        /// </summary>
        /// <param name="tableId">ID of table to display</param>
        /// <returns>True if table was found and displayed</returns>
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

        /// <summary>
        /// Displays all cached tables with their complete content in chronological order.
        /// Shows each table separated by breaks for easy reading.
        /// Displays tables in solution progression order: t-raw → t-i → t-1 → t-2 → ... → t-optimal → branches
        /// </summary>
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

            // Get tables sorted by creation time to show solution progression
            var sortedTables = _tableCache.Values
                .OrderBy(t => t.CreatedTime)
                .ThenBy(t => GetTableOrder(t.TableId))
                .ToList();

            // Display each table with full details
            for (int i = 0; i < sortedTables.Count; i++)
            {
                var table = sortedTables[i];
                
                // Display table in traditional format
                table.DisplayTraditional();
                
                // Add separator between tables (except after the last one)
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

        /// <summary>
        /// Helper method to determine the logical order of tables for display purposes.
        /// Returns a sort key to ensure tables are displayed in logical progression order.
        /// </summary>
        private static int GetTableOrder(string tableId)
        {
            // Define logical ordering for common table types
            return tableId.ToLower() switch
            {
                "t-raw" => 1,           // Raw input table
                "t-i" => 2,             // Initial canonical form
                "t-canonical" => 2,     // Alternative canonical form name
                "t-1" => 10,            // First iteration
                "t-2" => 11,            // Second iteration
                "t-3" => 12,            // Third iteration
                "t-4" => 13,            // Fourth iteration
                "t-5" => 14,            // Fifth iteration
                "t-optimal" => 20,      // LP optimal solution
                "t-1.1" => 30,          // Branch & bound nodes
                "t-1.2" => 31,
                "t-1.1.1" => 32,
                "t-1.1.2" => 33,
                "t-1.2.1" => 34,
                "t-1.2.2" => 35,
                _ => 50                 // Unknown tables go at the end
            };
        }

        /// <summary>
        /// Displays all cached tables with their matrix decomposition format.
        /// Alternative detailed view showing mathematical matrix components.
        /// </summary>
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

            // Get tables sorted by creation time and logical order
            var sortedTables = _tableCache.Values
                .OrderBy(t => t.CreatedTime)
                .ThenBy(t => GetTableOrder(t.TableId))
                .ToList();

            // Display each table in matrix format
            for (int i = 0; i < sortedTables.Count; i++)
            {
                var table = sortedTables[i];
                
                // Display table in matrix decomposition format
                table.DisplayMatrix();
                
                // Add separator between tables (except after the last one)
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