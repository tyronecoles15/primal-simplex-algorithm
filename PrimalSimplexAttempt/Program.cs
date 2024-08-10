using System;
using System.IO;
using System.Linq;

namespace PRG381_Project
{
    internal class Program
    {
        public static void TakeInputAndSaveToFile(string filePath)
        {
            Console.WriteLine("Enter the Linear Programming Model in the specified format (Objective Function, Constraints, Sign Restrictions):");

            string input = "";
            while (true)
            {
                string line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    break;
                input += line + Environment.NewLine;
            }

            File.WriteAllText(filePath, input);
        }

        public static double[,] ConvertToCanonicalForm(string inputFilePath, string outputFilePath)
        {
            string[] lines = File.ReadAllLines(inputFilePath);

            // Parse the objective function
            string[] objectiveFunctionLine = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double[] coefficients = new double[objectiveFunctionLine.Length - 1];
            for (int i = 1; i < objectiveFunctionLine.Length; i++)
            {
                try
                {
                    coefficients[i - 1] = -Convert.ToDouble(objectiveFunctionLine[i].Replace("+", "").Trim());
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Invalid coefficient format: {objectiveFunctionLine[i]}");
                    return null;
                }
            }

            // Identify the index where constraints end and sign restrictions begin
            int signRestrictionStartIndex = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("x"))
                {
                    signRestrictionStartIndex = i;
                    break;
                }
            }

            // Parse constraints
            int numConstraints = signRestrictionStartIndex - 1; // Exclude sign restrictions
            double[,] constraints = new double[numConstraints, coefficients.Length];
            double[] rhs = new double[numConstraints];
            string[] operators = new string[numConstraints];

            int constraintIndex = 0;
            for (int i = 1; i < signRestrictionStartIndex; i++)
            {
                string[] constraintParts = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (constraintParts.Length < coefficients.Length + 2)
                {
                    Console.WriteLine($"Invalid constraint format: {lines[i]}");
                    return null;
                }

                for (int j = 0; j < coefficients.Length; j++)
                {
                    try
                    {
                        constraints[constraintIndex, j] = Convert.ToDouble(constraintParts[j].Replace("+", "").Trim());
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Invalid constraint coefficient format: {constraintParts[j]}");
                        return null;
                    }
                }
                operators[constraintIndex] = constraintParts[coefficients.Length];
                try
                {
                    rhs[constraintIndex] = Convert.ToDouble(constraintParts[coefficients.Length + 1].Trim());
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Invalid RHS format: {constraintParts[coefficients.Length + 1]}");
                    return null;
                }
                constraintIndex++;
            }

            // Convert to canonical form by adding slack and surplus variables
            int slackVariableCount = operators.Count(op => op == "<=");
            int totalVariables = coefficients.Length + slackVariableCount;
            double[,] canonicalFormArray = new double[numConstraints + 1, totalVariables + 1]; // +1 for the RHS column

            // Add objective function to the array
            for (int i = 0; i < coefficients.Length; i++)
            {
                canonicalFormArray[0, i] = coefficients[i];
            }

            // Add constraints to the array
            int slackIndex = coefficients.Length;
            for (int i = 0; i < numConstraints; i++)
            {
                for (int j = 0; j < coefficients.Length; j++)
                {
                    canonicalFormArray[i + 1, j] = constraints[i, j];
                }
                if (operators[i] == "<=")
                {
                    canonicalFormArray[i + 1, slackIndex] = 1; // Add slack variable with +1 coefficient
                    slackIndex++;
                }
                canonicalFormArray[i + 1, totalVariables] = rhs[i]; // Add RHS
            }

            // Save and print the canonical form
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                // Objective function with RHS = 0
                for (int i = 0; i < totalVariables; i++)
                {
                    writer.Write($"{(canonicalFormArray[0, i] >= 0 ? "+" : "")}{canonicalFormArray[0, i]} ");
                }
                writer.WriteLine("= 0");

                // Constraints
                for (int i = 1; i <= numConstraints; i++)
                {
                    for (int j = 0; j < totalVariables; j++)
                    {
                        writer.Write($"{(canonicalFormArray[i, j] >= 0 ? "+" : "")}{canonicalFormArray[i, j]} ");
                    }
                    writer.WriteLine($"= {canonicalFormArray[i, totalVariables]}");
                }
            }

            return canonicalFormArray; // Return the 2D array
        }

        static void Main(string[] args)
        {
            string inputFilePath = "lp_model.txt";
            string outputFilePath = "canonical_lp_model.txt";

            TakeInputAndSaveToFile(inputFilePath);
            double[,] canonicalForm = ConvertToCanonicalForm(inputFilePath, outputFilePath);

            PrimalSimplex simplex = new PrimalSimplex(canonicalForm);
            simplex.Solve();
            Console.ReadKey();
        }
    }
}

