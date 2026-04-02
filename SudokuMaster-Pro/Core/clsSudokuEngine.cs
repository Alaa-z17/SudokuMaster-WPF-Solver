namespace SudokuMaster_Pro.Core
{
    public class clsSudokuEngine
    {
        private const int Size = 9;

        // Bitmasks to check used numbers in rows, cols, and 3x3 grids in O(1)
        private int[] rowMask = new int[Size];
        private int[] colMask = new int[Size];
        private int[,] gridMask = new int[3, 3];
        private void InitializeMasks(int[,] board)
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

        private bool SolveBacktrack(int[,] board, int row, int col)
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
                return SolveBacktrack(board, row, col + 1);

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
                    if (SolveBacktrack(board, row, col + 1)) return true;

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
            // First, initialize masks with the numbers already on the board
            InitializeMasks(board);
            return SolveBacktrack(board, 0, 0);
        }

   
    }
}