using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinearProgrammingSolver.Tables;

namespace LinearProgrammingSolver.Utils
{
    // Enum for file loading options
    public enum FileLoadOption
    {
        LoadDefault = 1,
        ChooseFile = 2,
        EnterPath = 3,
        BackToMain = 4
    }

    public class FileManager
    {
        private FileReader fileReader;
        private CanonicalFormConverter canonicalConverter;

        public FileManager()
        {
            fileReader = new FileReader();
            canonicalConverter = new CanonicalFormConverter();
        }

        // Main file loading menu handler
        public (ProblemType problemType, Table rawTable, string filePath, NLPProblem nlpProblem) HandleLoadFile()
        {
            Console.Clear();
            
            bool backToMain = false;
            (ProblemType problemType, Table rawTable, string filePath, NLPProblem nlpProblem) result = (ProblemType.LinearProgramming, null, "", null);
            
            while (!backToMain)
            {
                DisplayFileLoadMenu();
                
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice >= 1 && choice <= 4)
                    {
                        var selectedOption = (FileLoadOption)choice;
                        
                        switch (selectedOption)
                        {
                            case FileLoadOption.LoadDefault:
                                result = LoadDefaultFile();
                                backToMain = true;
                                break;
                            case FileLoadOption.ChooseFile:
                                result = ChooseFileFromExplorer();
                                backToMain = true;
                                break;
                            case FileLoadOption.EnterPath:
                                result = EnterFilePathManually();
                                backToMain = true;
                                break;
                            case FileLoadOption.BackToMain:
                                backToMain = true;
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid option. Please try again.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }
                
                if (!backToMain && result.rawTable == null)
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
            
            return result;
        }

        // Displays file loading menu
        private void DisplayFileLoadMenu()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                            LOAD INPUT FILE                                   ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("║  1. Load Default File        - Load data/input.txt (LP/IP)                   ║");
            Console.WriteLine("║  2. Choose File              - Browse with file explorer (LP/IP/NLP)         ║");
            Console.WriteLine("║  3. Enter File Path          - Type file path manually (LP/IP/NLP)           ║");
            Console.WriteLine("║  4. Back to Main Menu        - Return to main menu                           ║");
            Console.WriteLine("║                                                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.Write("Select an option (1-4): ");
        }

        // Loads the default input.txt file
        private (ProblemType, Table, string, NLPProblem) LoadDefaultFile()
        {
            Console.Clear();
            Console.WriteLine("Loading default file (data/input.txt)...\n");
            
            try
            {
                string inputPath = FindDefaultInputFile();
                return ProcessInputFile(inputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading default file: {ex.Message}");
                return (ProblemType.LinearProgramming, null, "", null);
            }
        }

        // Opens file explorer to choose a file
        private (ProblemType, Table, string, NLPProblem) ChooseFileFromExplorer()
        {
            Console.Clear();
            Console.WriteLine("Attempting to open file explorer dialog...\n");
            
            try
            {
                // Initialize Windows Forms application for console app
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // Create and configure the file dialog
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    openFileDialog.Title = "Select Input File - Linear Programming Solver";
                    openFileDialog.InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "data");
                    openFileDialog.RestoreDirectory = true;
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.CheckPathExists = true;
                    openFileDialog.Multiselect = false;
                    
                    Console.WriteLine("File dialog launching...");
                    Console.WriteLine("If you don't see the dialog window:");
                    Console.WriteLine("- Check behind other windows or on other monitors");
                    Console.WriteLine("- Press Alt+Tab to cycle through open windows");
                    Console.WriteLine("- Look for the dialog in your taskbar");
                    Console.WriteLine();
                    Console.WriteLine("Press any key to cancel and return to the file menu if the dialog doesn't appear...");
                    Console.WriteLine();
                    
                    // Start the dialog in a separate task
                    var dialogTask = Task.Run(() => openFileDialog.ShowDialog());
                    
                    // Wait for either the dialog to complete or user to press a key
                    while (!dialogTask.IsCompleted)
                    {
                        if (Console.KeyAvailable)
                        {
                            Console.ReadKey(true); // Consume the key press
                            Console.WriteLine("Dialog cancelled by user. Returning to file menu...");
                            return (ProblemType.LinearProgramming, null, "", null);
                        }
                        Thread.Sleep(100); // Check every 100ms
                    }
                    
                    DialogResult result = dialogTask.Result;
                    
                    if (result == DialogResult.OK)
                    {
                        string selectedFile = openFileDialog.FileName;
                        Console.WriteLine($"File selected: {selectedFile}\n");
                        return ProcessInputFile(selectedFile);
                    }
                    else
                    {
                        Console.WriteLine("File selection cancelled.");
                        return (ProblemType.LinearProgramming, null, "", null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with file dialog: {ex.Message}");
                Console.WriteLine("\nThe file explorer dialog could not be opened.");
                Console.WriteLine("This might be due to:");
                Console.WriteLine("- Running in a restricted environment");
                Console.WriteLine("- Missing Windows Forms components");
                Console.WriteLine("- System permission issues");
                Console.WriteLine();
                Console.WriteLine("Please use option 3 'Enter File Path' to manually specify your file location.");
                return (ProblemType.LinearProgramming, null, "", null);
            }
        }

        // Allows manual entry of file path
        private (ProblemType, Table, string, NLPProblem) EnterFilePathManually()
        {
            Console.Clear();
            Console.WriteLine("Enter file path manually:\n");
            Console.Write("Please enter the full path to your input file: ");
            
            string userPath = Console.ReadLine();
            
            if (!string.IsNullOrWhiteSpace(userPath))
            {
                try
                {
                    return ProcessInputFile(userPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading file: {ex.Message}");
                    return (ProblemType.LinearProgramming, null, "", null);
                }
            }
            else
            {
                Console.WriteLine("No file path entered. Operation cancelled.");
                return (ProblemType.LinearProgramming, null, "", null);
            }
        }

        // Finds the default input.txt file
        private string FindDefaultInputFile()
        {
            // Use robust path resolution that works from any working directory
            string currentDir = Directory.GetCurrentDirectory();
            string projectDir = currentDir;
            
            // If running from debugger, find the project directory
            while (!File.Exists(Path.Combine(projectDir, "data", "input.txt")) && 
                   Directory.GetParent(projectDir) != null)
            {
                projectDir = Directory.GetParent(projectDir).FullName;
            }
            
            string inputPath = Path.Combine(projectDir, "data", "input.txt");
            
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"Default input file not found at: {inputPath}");
            }
            
            return inputPath;
        }

        // Processes input file and creates table structure
        private (ProblemType, Table, string, NLPProblem) ProcessInputFile(string inputPath)
        {
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"File not found: {inputPath}");
            }
            
            // Clear any existing tables from previous runs
            TableCache.ClearAllTables();
            
            Console.WriteLine($"Processing: {inputPath}");
            Console.WriteLine();
            
            try
            {
                // Step 1: FileReader detects file type and parses accordingly
                var (problemType, data) = fileReader.ParseFile(inputPath);
            
                if (problemType == ProblemType.LinearProgramming)
                {
                    // Handle LP/IP problems
                    var (matrix, rowLabels, columnLabels, optimizationType, constraintOperators) = 
                        ((double[,], List<string>, List<string>, OptimizationType, Dictionary<string, ConstraintOperator>))data;
                    
                    // Step 2: Create Table object
                    var rawTable = new Table("t-raw", matrix, rowLabels, columnLabels, optimizationType, "Raw", constraintOperators);
                    
                    // Step 3: Store Table in cache
                    TableCache.StoreTable(rawTable);
                    Console.WriteLine("Raw table created and cached");
                    
                    // Step 4: Convert to canonical form
                    var canonicalTable = canonicalConverter.ConvertToCanonicalForm(rawTable);
                    TableCache.StoreTable(canonicalTable);
                    Console.WriteLine("Canonical table created and cached");
                    
                    Console.WriteLine();
                    Console.WriteLine("Linear Programming file loaded successfully!");
                    Console.WriteLine($"Problem type: {optimizationType}");
                    Console.WriteLine($"Variables: {rawTable.GetVariableCount()}");
                    Console.WriteLine($"Constraints: {rawTable.GetRowCount() - 1}");
                    
                    return (problemType, rawTable, inputPath, null);
                }
                else if (problemType == ProblemType.NonLinearProgramming)
                {
                    // Handle NLP problems
                    var nlpProblem = (NLPProblem)data;
                    
                    Console.WriteLine("NLP problem parsed");
                    Console.WriteLine();
                    Console.WriteLine("Non-Linear Programming file loaded successfully!");
                    Console.WriteLine($"Function: {nlpProblem.Function}");
                    Console.WriteLine($"Starting point: ({nlpProblem.StartingPoint.x}, {nlpProblem.StartingPoint.y})");
                    
                    // Store NLP problem for algorithm selection
                    // Note: NLP doesn't use table structure, so we return null for rawTable
                    return (problemType, null, inputPath, nlpProblem);
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine("File Format Error:");
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine("Please ensure your file follows one of these formats:");
                Console.WriteLine("1. Linear Programming: First line starts with 'max' or 'min'");
                Console.WriteLine("2. Non-Linear Programming: First line starts with 'F(x,y) = ...'");
            }
            
            return (ProblemType.LinearProgramming, null, "", null);
        }
    }
}