namespace SudokuMaster_Pro.Core
{
    public class clsSudokuEngine
    {
        private const int Size = 9;

        private Random _random = new Random();

        // Bitmasks to check used numbers in rows, cols, and 3x3 grids in O(1)
        private int[] rowMask = new int[Size];
        private int[] colMask = new int[Size];
        private int[,] gridMask = new int[3, 3];
        private void InitializeMasks(int[,] board, int[] rowMask, int[] colMask, int[,] gridMask)
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (board[r, c] != 0)
                    {
                        int num = board[r, c];
                        // Set the bit corresponding to the number
                        rowMask[r] |= (1 << num);
                        colMask[c] |= (1 << num);
                        gridMask[r / 3, c / 3] |= (1 << num);
                    }
                }
            }
        }

        private bool SolveBacktrack(int[,] board, int row, int col, int[] rowMask, int[] colMask, int[,] gridMask)
        {
            // If we reach the end of the row, move to the next row
            if (col == Size)
            {
                row++;
                col = 0;
            }

            // If we processed all rows, the puzzle is solved
            if (row == Size) return true;

            // Skip cells that are already filled
            if (board[row, col] != 0)
                return SolveBacktrack(board, row, col + 1, rowMask, colMask, gridMask);

            for (int num = 1; num <= 9; num++)
            {
                int mask = 1 << num;

                // O(1) Check: if the bit is already set in row, col, or grid, it's not safe
                if ((rowMask[row] & mask) == 0 &&
                    (colMask[col] & mask) == 0 &&
                    (gridMask[row / 3, col / 3] & mask) == 0)
                {
                    // Place the number and update masks
                    board[row, col] = num;
                    rowMask[row] |= mask;
                    colMask[col] |= mask;
                    gridMask[row / 3, col / 3] |= mask;

                    // Recurse to the next cell
                    if (SolveBacktrack(board, row, col + 1,rowMask,colMask,gridMask)) return true;

                    // Backtrack: remove number and reset masks
                    board[row, col] = 0;
                    rowMask[row] &= ~mask;
                    colMask[col] &= ~mask;
                    gridMask[row / 3, col / 3] &= ~mask;
                }
            }

            return false; // Triggers backtracking
        }

        public bool Solve(int[,] board)
        {
            int[] rowMask = new int[Size];
            int[] colMask = new int[Size];
            int[,] gridMask = new int[3, 3];
            InitializeMasks(board, rowMask, colMask, gridMask);
            return SolveBacktrack(board, 0, 0, rowMask, colMask, gridMask);
        }

        //  Generates a new Sudoku puzzle based on difficulty
        //  difficulty level represents the number of cells to clear
        public int[,] GeneratePuzzle(int difficulty)
        {
            int[,] fullBoard = new int[Size, Size];
            FillDiagonal(fullBoard);
            Solve(fullBoard);  // الآن fullBoard مكتمل
            int[,] puzzle = (int[,])fullBoard.Clone();
            RemoveKDigits(puzzle, difficulty);
            // إذا تعددت الحلول، نعيد المحاولة (بسيطة لكنها فعالة)
            int attempts = 0;
            while (CountSolutions(puzzle) != 1 && attempts < 10)
            {
                puzzle = (int[,])fullBoard.Clone();
                RemoveKDigits(puzzle, difficulty);
                attempts++;
            }
            return puzzle;
        }

        private void FillDiagonal(int[,] board)
        {
            for (int i = 0; i < Size; i += 3)
            {
                FillBox(board, i, i);
            }
        }

        private void FillBox(int[,] board, int row, int col)
        {
            int num;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    do
                    {
                        num = _random.Next(1, 10);
                    }
                    while (IsInBox(board, row, col, num));

                    board[row + i, col + j] = num;
                }
            }
        }

        private bool IsInBox(int[,] board, int row, int col, int num)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[row + i, col + j] == num)
                        return true;

            return false;
        }

        private void RemoveKDigits(int[,] board, int count)
        {
            int removed = 0;
            while (removed < count)
            {
                int cellId = _random.Next(0, 81);
                int r = cellId / 9;
                int c = cellId % 9;

                if (board[r, c] != 0)
                {
                    board[r, c] = 0;
                    removed++;
                }
            }
        }
        private int CountSolutions(int[,] board, int limit = 2)
        {
            int[,] copy = (int[,])board.Clone();
            int[] rowMask = new int[Size];
            int[] colMask = new int[Size];
            int[,] gridMask = new int[3, 3];
            InitializeMasks(copy, rowMask, colMask, gridMask);
            int solutions = 0;
            CountSolutionsBacktrack(copy, 0, 0, rowMask, colMask, gridMask, ref solutions, limit);
            return solutions;
        }

        private void CountSolutionsBacktrack(int[,] board, int row, int col, int[] rowMask, int[] colMask, int[,] gridMask, ref int solutions, int limit)
        {
            if (solutions >= limit) return;
            if (col == Size) { row++; col = 0; }
            if (row == Size) { solutions++; return; }
            if (board[row, col] != 0)
            {
                CountSolutionsBacktrack(board, row, col + 1, rowMask, colMask, gridMask, ref solutions, limit);
                return;
            }
            for (int num = 1; num <= 9; num++)
            {
                int mask = 1 << num;
                if ((rowMask[row] & mask) == 0 && (colMask[col] & mask) == 0 && (gridMask[row / 3, col / 3] & mask) == 0)
                {
                    board[row, col] = num;
                    rowMask[row] |= mask;
                    colMask[col] |= mask;
                    gridMask[row / 3, col / 3] |= mask;
                    CountSolutionsBacktrack(board, row, col + 1, rowMask, colMask, gridMask, ref solutions, limit);
                    board[row, col] = 0;
                    rowMask[row] &= ~mask;
                    colMask[col] &= ~mask;
                    gridMask[row / 3, col / 3] &= ~mask;
                }
            }
        }
    }
}