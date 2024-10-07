using Entities;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.IO;
using System.Linq;

namespace StoreDataManager
{
    public sealed class Store
    {
        private static Store? instance = null;
        private static readonly object _lock = new object();

        public static Store GetInstance()
        {
            lock (_lock)
            {
                if (instance == null)

        public static Store GetInstance()
        {
            lock (_lock)
            {
                if (instance == null)
                {
                    instance = new Store();
                }
                return instance;
            }
        }

        private const string DatabaseBasePath = @"C:\TinySql\";
        private const string DataPath = $@"{DatabaseBasePath}\Data";
        private const string SystemCatalogPath = $@"{DataPath}\SystemCatalog";
        private const string SystemDatabasesFile = $@"{SystemCatalogPath}\SystemDatabases.table";
        private const string SystemTablesFile = $@"{SystemCatalogPath}\SystemTables.table";
        private const string DatabaseBasePath = @"C:\\TinySql\\";
        private const string DataPath = $@"{DatabaseBasePath}\\Data";
        private const string SystemCatalogPath = $@"{DataPath}\\SystemCatalog";
        private const string SystemTablesFile = $@"{SystemCatalogPath}\\SystemTables.table";

        public Store()
        {
            this.InitializeSystemCatalog();

        }

        private void InitializeSystemCatalog()
        {
            // Always make sure that the system catalog and above folder
            // exist when initializing
            Directory.CreateDirectory(SystemCatalogPath);
        }

        public OperationStatus CreateTable()
        {
            // Creates a default DB called TESTDB
            Directory.CreateDirectory($@"{DataPath}\TESTDB");

            // Creates a default Table called ESTUDIANTES
            var tablePath = $@"{DataPath}\TESTDB\ESTUDIANTES.Table";

            using (FileStream stream = File.Open(tablePath, FileMode.OpenOrCreate))
            using (BinaryWriter writer = new(stream))
            {
                // Create an object with a hardcoded.
                // First field is an int, second field is a string of size 30,
                // third is a string of 50
                int id = 1;
                string nombre = "Isaac".PadRight(30); // Pad to make the size of the string fixed
                string apellido = "Ramirez".PadRight(50);

                writer.Write(id);
                writer.Write(nombre);
                writer.Write(apellido);
            }
            return OperationStatus.Success;
        }

        public OperationStatus Select()
        {
            // Creates a default Table called ESTUDIANTES
            var tablePath = $@"{DataPath}\TESTDB\ESTUDIANTES.Table";
            using (FileStream stream = File.Open(tablePath, FileMode.OpenOrCreate))
            using (BinaryReader reader = new(stream))
            {
                // Print the values as a I know exactly the types, but this needs to be done right
                Console.WriteLine(reader.ReadInt32());
                Console.WriteLine(reader.ReadString());
                Console.WriteLine(reader.ReadString());
                return OperationStatus.Success;
            }
        }
            // Ensure that the system catalog directory exists
            Directory.CreateDirectory(SystemCatalogPath);
            // Create the SystemTables file if it doesn't exist
            if (!File.Exists(SystemTablesFile))
            {
                File.Create(SystemTablesFile).Dispose();
            }
        }

        public bool TableExists(string tableName)
        {
            // Check if the table file exists in the system catalog
            string tablePath = Path.Combine(DataPath, "TESTDB", $"{tableName}.Table");
            return File.Exists(tablePath);
        }

        public OperationStatus CreateTable(string tableName, string columnDefinitions)
        {
            // Create the directory for the new table if it doesn't exist
            string tableDirectory = Path.Combine(DataPath, "TESTDB");
            Directory.CreateDirectory(tableDirectory);

            // Create the table file
            string tablePath = Path.Combine(tableDirectory, $"{tableName}.Table");

            // Parse the column definitions
            var columns = ParseColumnDefinitions(columnDefinitions);
            if (columns == null || columns.Length == 0)
            {
                return OperationStatus.InvalidColumnDefinitions;
            }

            using (FileStream stream = File.Open(tablePath, FileMode.OpenOrCreate))
            using (BinaryWriter writer = new(stream))
            {
                // Write column definitions and initial data to the file
                foreach (var column in columns)
                {
                    writer.Write(column);
                }
            }

            // Log the table creation
            LogTableCreation(tableName, columnDefinitions);
            return OperationStatus.Success;
        }

        public void LogTableCreation(string tableName, string columnDefinitions)
        {
            // Log the table creation in the system catalog
            string logEntry = $"{tableName};{columnDefinitions};{DateTime.Now}\n";
            File.AppendAllText(SystemTablesFile, logEntry);
        }

        public OperationStatus Select(string tableName, string condition = null)
        {
            // Check if the table exists
            if (!TableExists(tableName))
            {
                return OperationStatus.TableNotFound;
            }

            string tablePath = Path.Combine(DataPath, "TESTDB", $"{tableName}.Table");

            // Read data from the table file
            using (FileStream stream = File.Open(tablePath, FileMode.Open))
            using (BinaryReader reader = new(stream))
            {
                // Example: Read data assuming a fixed structure (adapt as needed)
                while (stream.Position < stream.Length)
                {
                    // Assuming each record consists of an int and two strings
                    int id = reader.ReadInt32();
                    string nombre = reader.ReadString();
                    string apellido = reader.ReadString();

                    // Apply condition if specified
                    if (string.IsNullOrEmpty(condition) || EvaluateCondition(id, nombre, apellido, condition))
                    {
                        // Display or process the data as needed (for demo, we will write to console)
                        Console.WriteLine($"ID: {id}, Nombre: {nombre}, Apellido: {apellido}");
                    }
                }
            }

            return OperationStatus.Success;
        }

        private bool EvaluateCondition(int id, string nombre, string apellido, string condition)
        {
            // This is a simple implementation to evaluate conditions.
            // You would need to parse the condition string and check accordingly.
            // For example, you could check for equality or other comparisons.
            if (condition.StartsWith("ID ="))
            {
                return id == int.Parse(condition.Split('=')[1].Trim());
            }
            if (condition.StartsWith("Nombre LIKE"))
            {
                string value = condition.Split('LIKE')[1].Trim().Trim('"'); // Adjust as needed
                return nombre.Contains(value);
            }
            return true; // Default to true if no valid condition is found
        }

        public OperationStatus InsertIntoTable(string tableName, string[] values)
        {
            // Check if the table exists
            if (!TableExists(tableName))
            {
                return OperationStatus.TableNotFound;
            }

            string tablePath = Path.Combine(DataPath, "TESTDB", $"{tableName}.Table");

            // Validate the number of values against the table structure (this is simplified)
            // You might want to check types here based on your column definitions
            using (FileStream stream = File.Open(tablePath, FileMode.Append))
            using (BinaryWriter writer = new(stream))
            {
                // Write values to the table
                foreach (var value in values)
                {
                    writer.Write(value);
                }
            }

            return OperationStatus.Success;
        }

        private string[]? ParseColumnDefinitions(string columnDefinitions)
        {
            // Split the column definitions into an array and validate
            var columns = columnDefinitions.Split(';');
            return columns.Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
        }

        public OperationStatus UpdateTable(string tableName, string[] setValues, string condition)
        {
            // Check if the table exists
            if (!TableExists(tableName))
            {
                return OperationStatus.TableNotFound;
            }

            string tablePath = Path.Combine(DataPath, "TESTDB", $"{tableName}.Table");
            var updatedRecords = new List<string>();

            // Read all records, update as necessary, and store updated records
            using (FileStream stream = File.Open(tablePath, FileMode.Open))
            using (BinaryReader reader = new(stream))
            {
                while (stream.Position < stream.Length)
                {
                    // Read each record
                    int id = reader.ReadInt32();
                    string nombre = reader.ReadString();
                    string apellido = reader.ReadString();

                    // Check if this record matches the condition
                    if (EvaluateCondition(id, nombre, apellido, condition))
                    {
                        // Update the record with new values from setValues
                        foreach (var setValue in setValues)
                        {
                            var parts = setValue.Split('=');
                            string columnName = parts[0].Trim();
                            string newValue = parts[1].Trim().Trim('\''); // Remove quotes if present

                            // Update logic (you may need to adjust for your schema)
                            if (columnName.Equals("Nombre", StringComparison.OrdinalIgnoreCase))
                            {
                                nombre = newValue;
                            }
                            else if (columnName.Equals("Apellido", StringComparison.OrdinalIgnoreCase))
                            {
                                apellido = newValue;
                            }
                        }
                    }
                    // Store the updated record
                    updatedRecords.Add($"{id};{nombre};{apellido}");
                }
            }

            // Write the updated records back to the file
            using (FileStream stream = File.Open(tablePath, FileMode.Create))
            using (BinaryWriter writer = new(stream))
            {
                foreach (var record in updatedRecords)
                {
                    var fields = record.Split(';');
                    writer.Write(int.Parse(fields[0])); // ID
                    writer.Write(fields[1]);             // Nombre
                    writer.Write(fields[2]);             // Apellido
                }
            }

            return OperationStatus.Success;
        }
        public OperationStatus DeleteFromTable(string tableName, string condition)
        {
            // Check if the table exists
            if (!TableExists(tableName))
            {
                return OperationStatus.TableNotFound;
            }

            string tablePath = Path.Combine(DataPath, "TESTDB", $"{tableName}.Table");
            var remainingRecords = new List<string>();

            // Read all records and keep the ones that do not match the condition
            using (FileStream stream = File.Open(tablePath, FileMode.Open))
            using (BinaryReader reader = new(stream))
            {
                while (stream.Position < stream.Length)
                {
                    // Read each record
                    int id = reader.ReadInt32();
                    string nombre = reader.ReadString();
                    string apellido = reader.ReadString();

                    // Check if this record matches the condition
                    if (!EvaluateCondition(id, nombre, apellido, condition))
                    {
                        // If it does not match, keep the record
                        remainingRecords.Add($"{id};{nombre};{apellido}");
                    }
                }
            }

            // Write the remaining records back to the file
            using (FileStream stream = File.Open(tablePath, FileMode.Create))
            using (BinaryWriter writer = new(stream))
            {
                foreach (var record in remainingRecords)
                {
                    var fields = record.Split(';');
                    writer.Write(int.Parse(fields[0])); // ID
                    writer.Write(fields[1]);             // Nombre
                    writer.Write(fields[2]);             // Apellido
                }
            }

            return OperationStatus.Success;
        }

    }
}
