using System;
using System.Collections.Generic;
using System.Linq;
using LinearProgrammingSolver.Tables;
using LinearProgrammingSolver.Utils;

namespace LinearProgrammingSolver.Algorithms.Implementations.IP
{
    public class KnapsackAlgorithm
    {
        private List<Table> _allSubproblems;
        private List<string> _processingOrder;
        private Dictionary<string, string> _fathomReasons;
        private Table _bestIntegerSolution;
        private const double TOLERANCE = 1e-6;
        
        public KnapsackAlgorithm()
        {
            _allSubproblems = new List<Table>();
            _processingOrder = new List<string>();
            _fathomReasons = new Dictionary<string, string>();
            _bestIntegerSolution = null;
        }

        public Table SolveKnapsack(Table inputTable)
        {
            if (inputTable == null)
            {
                Console.WriteLine("Error: Input table is null");
                return null;
            }

            Console.WriteLine("Starting Knapsack Branch & Bound...");
            Console.WriteLine();

            // Extract problem data from table
            var problemData = ExtractKnapsackData(inputTable);
            if (problemData == null) return null;

            // Display initial problem setup
            DisplayProblemSetup(problemData);
            
            // Calculate and display ratio table
            CalculateRatios(problemData);
            DisplayRatioTable(problemData);

            // Solve using branch and bound
            var solution = SolveBranchAndBound(problemData);
            
            if (solution != null)
            {
                _bestIntegerSolution = CreateSolutionTable(solution, problemData);
                TableCache.StoreTable(_bestIntegerSolution);
            }

            return _bestIntegerSolution;
        }

        private KnapsackProblemData ExtractKnapsackData(Table table)
        {
            try
            {
                // Assuming standard knapsack format: max cx subject to wx <= capacity
                int numVars = table.GetColumnCount() - 1; // Exclude RHS column
                
                // Get objective coefficients (first row, excluding RHS)
                double[] values = new double[numVars];
                for (int j = 0; j < numVars; j++)
                {
                    values[j] = table.GetElement(0, j);
                }

                // Get constraint coefficients (assuming one constraint row)
                double[] weights = new double[numVars];
                double capacity = 0;
                
                // Find the constraint row (skip objective row)
                for (int i = 1; i < table.GetRowCount(); i++)
                {
                    bool hasNonZero = false;
                    for (int j = 0; j < numVars; j++)
                    {
                        double coeff = table.GetElement(i, j);
                        if (Math.Abs(coeff) > TOLERANCE)
                        {
                            weights[j] = coeff;
                            hasNonZero = true;
                        }
                    }
                    if (hasNonZero)
                    {
                        capacity = table.GetElement(i, numVars); // RHS
                        break;
                    }
                }

                return new KnapsackProblemData
                {
                    NumItems = numVars,
                    Values = values,
                    Weights = weights,
                    Capacity = capacity,
                    Items = new List<KnapsackItem>()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting knapsack data: {ex.Message}");
                return null;
            }
        }

        private void DisplayProblemSetup(KnapsackProblemData data)
        {
            Console.WriteLine("Knapsack Problem Setup:");
            Console.WriteLine($"Capacity: {data.Capacity}");
            Console.WriteLine($"Number of items: {data.NumItems}");
            Console.WriteLine();
            
            Console.WriteLine("Items:");
            for (int i = 0; i < data.NumItems; i++)
            {
                Console.WriteLine($"x{i+1}: Value = {data.Values[i]}, Weight = {data.Weights[i]}");
            }
            Console.WriteLine();
        }

        private void CalculateRatios(KnapsackProblemData data)
        {
            data.Items.Clear();
            
            for (int i = 0; i < data.NumItems; i++)
            {
                double ratio = data.Weights[i] > TOLERANCE ? data.Values[i] / data.Weights[i] : 0;
                data.Items.Add(new KnapsackItem
                {
                    Index = i,
                    Value = data.Values[i],
                    Weight = data.Weights[i],
                    Ratio = ratio
                });
            }

            // Sort by ratio (highest first)
            data.Items = data.Items.OrderByDescending(item => item.Ratio).ToList();
            
            // Assign ranks
            for (int i = 0; i < data.Items.Count; i++)
            {
                data.Items[i].Rank = i + 1;
            }
        }

        private void DisplayRatioTable(KnapsackProblemData data)
        {
            Console.WriteLine("Ratio Table (sorted by Value/Weight ratio):");
            Console.WriteLine("Rank\tItem\tValue\tWeight\tRatio");
            Console.WriteLine("----\t----\t-----\t------\t-----");
            
            foreach (var item in data.Items)
            {
                Console.WriteLine($"{item.Rank}\tx{item.Index + 1}\t{item.Value:F1}\t{item.Weight:F1}\t{item.Ratio:F3}");
            }
            Console.WriteLine();
        }

        private KnapsackSolution SolveBranchAndBound(KnapsackProblemData data)
        {
            var bestSolution = new KnapsackSolution
            {
                Variables = new double[data.NumItems],
                ObjectiveValue = double.NegativeInfinity,
                IsInteger = false
            };

            Console.WriteLine("Branch & Bound Tree:");
            Console.WriteLine();

            // Start with fractional knapsack solution
            var rootSolution = SolveFractionalKnapsack(data, new Dictionary<int, int>());
            
            Console.WriteLine($"Root problem (fractional solution):");
            DisplaySolution(rootSolution, data, "Root");
            
            if (IsIntegerSolution(rootSolution))
            {
                Console.WriteLine("Root solution is already integer - optimal!");
                return rootSolution;
            }

            // Initialize branch and bound
            var queue = new Queue<BranchNode>();
            queue.Enqueue(new BranchNode
            {
                FixedVariables = new Dictionary<int, int>(),
                Solution = rootSolution,
                NodeId = "1",
                Depth = 0
            });

            int nodeCounter = 1;

            while (queue.Count > 0)
            {
                var currentNode = queue.Dequeue();
                _processingOrder.Add($"Processing node {currentNode.NodeId}");

                Console.WriteLine($"Node {currentNode.NodeId}: {GetNodeDescription(currentNode, data)}");

                // Check if we can prune
                if (currentNode.Solution.ObjectiveValue <= bestSolution.ObjectiveValue + TOLERANCE)
                {
                    _fathomReasons[currentNode.NodeId] = "Bound worse than current best";
                    Console.WriteLine($"  -> PRUNED: Bound {currentNode.Solution.ObjectiveValue:F3} <= best {bestSolution.ObjectiveValue:F3}");
                    continue;
                }

                // Check if integer solution
                if (IsIntegerSolution(currentNode.Solution))
                {
                    if (currentNode.Solution.ObjectiveValue > bestSolution.ObjectiveValue + TOLERANCE)
                    {
                        bestSolution = currentNode.Solution;
                        Console.WriteLine($"  -> NEW BEST INTEGER SOLUTION: {bestSolution.ObjectiveValue:F3}");
                    }
                    continue;
                }

                // Find first fractional variable for branching
                int branchVar = FindBranchingVariable(currentNode.Solution, data);
                if (branchVar == -1) continue;

                Console.WriteLine($"  -> Branching on x{data.Items[branchVar].Index + 1}");

                // Create two child nodes
                // Child 1: x_i = 0
                nodeCounter++;
                var child0 = CreateChildNode(currentNode, branchVar, 0, $"{currentNode.NodeId}.1", data);
                if (child0.Solution != null)
                {
                    queue.Enqueue(child0);
                    Console.WriteLine($"    Child {child0.NodeId}: x{data.Items[branchVar].Index + 1} = 0, bound = {child0.Solution.ObjectiveValue:F3}");
                }

                // Child 2: x_i = 1  
                nodeCounter++;
                var child1 = CreateChildNode(currentNode, branchVar, 1, $"{currentNode.NodeId}.2", data);
                if (child1.Solution != null)
                {
                    queue.Enqueue(child1);
                    Console.WriteLine($"    Child {child1.NodeId}: x{data.Items[branchVar].Index + 1} = 1, bound = {child1.Solution.ObjectiveValue:F3}");
                }
                Console.WriteLine();
            }

            Console.WriteLine($"Optimal integer solution found: {bestSolution.ObjectiveValue:F3}");
            DisplayFinalSolution(bestSolution, data);

            return bestSolution;
        }

        private KnapsackSolution SolveFractionalKnapsack(KnapsackProblemData data, Dictionary<int, int> fixedVars)
        {
            var solution = new KnapsackSolution
            {
                Variables = new double[data.NumItems],
                ObjectiveValue = 0,
                IsInteger = true
            };

            double remainingCapacity = data.Capacity;

            // Apply fixed variables first
            foreach (var fix in fixedVars)
            {
                int origIndex = data.Items[fix.Key].Index;
                solution.Variables[origIndex] = fix.Value;
                if (fix.Value == 1)
                {
                    remainingCapacity -= data.Items[fix.Key].Weight;
                    solution.ObjectiveValue += data.Items[fix.Key].Value;
                }
            }

            if (remainingCapacity < -TOLERANCE)
            {
                return null; // Infeasible
            }

            // Greedy fill remaining capacity
            for (int i = 0; i < data.Items.Count; i++)
            {
                if (fixedVars.ContainsKey(i)) continue; // Already fixed

                var item = data.Items[i];
                int origIndex = item.Index;

                if (item.Weight <= remainingCapacity + TOLERANCE)
                {
                    // Take full item
                    solution.Variables[origIndex] = 1;
                    solution.ObjectiveValue += item.Value;
                    remainingCapacity -= item.Weight;
                }
                else if (remainingCapacity > TOLERANCE)
                {
                    // Take fractional part
                    double fraction = remainingCapacity / item.Weight;
                    solution.Variables[origIndex] = fraction;
                    solution.ObjectiveValue += item.Value * fraction;
                    solution.IsInteger = false;
                    remainingCapacity = 0;
                    break;
                }
            }

            return solution;
        }

        private bool IsIntegerSolution(KnapsackSolution solution)
        {
            return solution.Variables.All(x => Math.Abs(x - Math.Round(x)) < TOLERANCE);
        }

        private int FindBranchingVariable(KnapsackSolution solution, KnapsackProblemData data)
        {
            for (int i = 0; i < data.Items.Count; i++)
            {
                int origIndex = data.Items[i].Index;
                double value = solution.Variables[origIndex];
                if (Math.Abs(value - Math.Round(value)) > TOLERANCE)
                {
                    return i; // Return index in sorted items list
                }
            }
            return -1;
        }

        private BranchNode CreateChildNode(BranchNode parent, int branchVar, int value, string nodeId, KnapsackProblemData data)
        {
            var childFixed = new Dictionary<int, int>(parent.FixedVariables);
            childFixed[branchVar] = value;

            var childSolution = SolveFractionalKnapsack(data, childFixed);

            return new BranchNode
            {
                FixedVariables = childFixed,
                Solution = childSolution,
                NodeId = nodeId,
                Depth = parent.Depth + 1
            };
        }

        private string GetNodeDescription(BranchNode node, KnapsackProblemData data)
        {
            if (node.FixedVariables.Count == 0)
                return "Root";

            var constraints = node.FixedVariables.Select(kv => 
                $"x{data.Items[kv.Key].Index + 1}={kv.Value}").ToArray();
            return string.Join(", ", constraints);
        }

        private void DisplaySolution(KnapsackSolution solution, KnapsackProblemData data, string label)
        {
            Console.WriteLine($"{label} solution:");
            Console.WriteLine($"  Objective: {solution.ObjectiveValue:F3}");
            Console.WriteLine($"  Variables: [{string.Join(", ", solution.Variables.Select(x => x.ToString("F3")))}]");
            Console.WriteLine($"  Integer: {IsIntegerSolution(solution)}");
        }

        private void DisplayFinalSolution(KnapsackSolution solution, KnapsackProblemData data)
        {
            Console.WriteLine();
            Console.WriteLine("=== FINAL KNAPSACK SOLUTION ===");
            Console.WriteLine($"Objective Value: {solution.ObjectiveValue:F3}");
            Console.WriteLine();
            Console.WriteLine("Item\tTake\tValue\tWeight\tContribution");
            Console.WriteLine("----\t----\t-----\t------\t------------");
            
            double totalValue = 0, totalWeight = 0;
            for (int i = 0; i < data.NumItems; i++)
            {
                double take = solution.Variables[i];
                if (take > TOLERANCE)
                {
                    double contribution = data.Values[i] * take;
                    double weightUsed = data.Weights[i] * take;
                    Console.WriteLine($"x{i+1}\t{take:F0}\t{data.Values[i]:F1}\t{data.Weights[i]:F1}\t{contribution:F1}");
                    totalValue += contribution;
                    totalWeight += weightUsed;
                }
            }
            
            Console.WriteLine($"TOTAL\t\t{totalValue:F1}\t{totalWeight:F1}");
            Console.WriteLine($"Capacity utilization: {totalWeight:F1}/{data.Capacity:F1}");
        }

        private Table CreateSolutionTable(KnapsackSolution solution, KnapsackProblemData data)
        {
            // Create a simple result table
            var result = TableCache.CreateAndStoreTable(
                "knapsack-optimal",
                new double[,] { { solution.ObjectiveValue } },
                new List<string> { "Z" },
                new List<string> { "Value" },
                OptimizationType.Maximize,
                "Optimal_Integer"
            );
            
            result.Status = "Optimal_Integer";
            return result;
        }

        // Public methods for integration with adapter
        public List<Table> GetAllSubproblems() => _allSubproblems;
        public List<string> GetProcessingOrder() => _processingOrder;
        public Dictionary<string, string> GetFathomReasons() => _fathomReasons;
        public Table GetBestIntegerSolution() => _bestIntegerSolution;
    }

    // Helper classes
    public class KnapsackProblemData
    {
        public int NumItems { get; set; }
        public double[] Values { get; set; }
        public double[] Weights { get; set; }
        public double Capacity { get; set; }
        public List<KnapsackItem> Items { get; set; }
    }

    public class KnapsackItem
    {
        public int Index { get; set; }      // Original variable index
        public double Value { get; set; }
        public double Weight { get; set; }
        public double Ratio { get; set; }   // Value/Weight
        public int Rank { get; set; }
    }

    public class KnapsackSolution
    {
        public double[] Variables { get; set; }
        public double ObjectiveValue { get; set; }
        public bool IsInteger { get; set; }
    }

    public class BranchNode
    {
        public Dictionary<int, int> FixedVariables { get; set; }
        public KnapsackSolution Solution { get; set; }
        public string NodeId { get; set; }
        public int Depth { get; set; }
    }
}