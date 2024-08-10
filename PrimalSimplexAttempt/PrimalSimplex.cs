using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG381_Project
{
    internal class PrimalSimplex
    {
        private double[,] tableau;

        public PrimalSimplex(double[,] tableau)
        {
            this.tableau = tableau;
        }

        public void Solve()
        {
            bool optimal = false;

            while (!optimal)
            {
                PrintTableau();

                int pivotColumn = FindPivotColumn();
                if (pivotColumn == -1)
                {
                    Console.WriteLine("Optimal solution found.");
                    break;
                }

                int pivotRow = FindPivotRow(pivotColumn);
                if (pivotRow == -1)
                {
                    Console.WriteLine("The solution is unbounded.");
                    return;
                }

                Pivot(pivotRow, pivotColumn);
            }

            Console.WriteLine("Final tableau (Optimal Solution):");
            PrintTableau();
        }

        private int FindPivotColumn()
        {
            int pivotColumn = -1;
            double minValue = 0;

            for (int j = 0; j < tableau.GetLength(1) - 1; j++)
            {
                if (tableau[0, j] < minValue)
                {
                    minValue = tableau[0, j];
                    pivotColumn = j;
                }
            }

            return pivotColumn;
        }

        private int FindPivotRow(int pivotColumn)
        {
            int pivotRow = -1;
            double minRatio = double.PositiveInfinity;

            for (int i = 1; i < tableau.GetLength(0); i++)
            {
                double rhs = tableau[i, tableau.GetLength(1) - 1];
                double coefficient = tableau[i, pivotColumn];

                if (coefficient > 0)
                {
                    double ratio = rhs / coefficient;
                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        pivotRow = i;
                    }
                }
            }

            return pivotRow;
        }

        private void Pivot(int pivotRow, int pivotColumn)
        {
            double pivotValue = tableau[pivotRow, pivotColumn];

            for (int j = 0; j < tableau.GetLength(1); j++)
            {
                tableau[pivotRow, j] /= pivotValue;
            }

            for (int i = 0; i < tableau.GetLength(0); i++)
            {
                if (i != pivotRow)
                {
                    double factor = tableau[i, pivotColumn];
                    for (int j = 0; j < tableau.GetLength(1); j++)
                    {
                        tableau[i, j] -= factor * tableau[pivotRow, j];
                    }
                }
            }
        }

        private void PrintTableau()
        {
            int rows = tableau.GetLength(0);
            int cols = tableau.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write($"{tableau[i, j],8:F2}");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}