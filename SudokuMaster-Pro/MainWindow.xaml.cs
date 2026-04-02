using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SudokuMaster_Pro.Core;
using System.Windows.Media.Animation;


namespace SudokuMaster_Pro
{
    public partial class MainWindow : Window
    {
        // English: Instance of our fast backtracking engine
        private clsSudokuEngine _engine = new clsSudokuEngine();

        // English: 2D Array to keep reference to our 81 TextBoxes
        private TextBox[,] _cellTextBoxes = new TextBox[9, 9];

        public MainWindow()
        {
            InitializeComponent();
            GenerateSudokuGrid();
        }

        // English: Dynamically generates the 9x9 grid in the UI
        private void GenerateSudokuGrid()
        {
            MainSudokuGrid.Children.Clear();

            // English: Create the 9 major 3x3 blocks
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    // English: We use a Border control to hold the sub-grid and give it thick borders
                    Border subGridBorder = new Border
                    {
                        BorderBrush = new SolidColorBrush(Color.FromRgb(44, 60, 80)), // English: Dark bold borders
                        BorderThickness = new Thickness(1.5)
                    };

                    Grid subGrid = new Grid();

                    // English: Put the grid inside the border
                    subGridBorder.Child = subGrid;

                    // English: Define 3 rows and 3 columns for each sub-grid
                    for (int i = 0; i < 3; i++)
                    {
                        subGrid.RowDefinitions.Add(new RowDefinition());
                        subGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    }

                    // English: Create the actual cells (TextBoxes) inside this 3x3 block
                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            int actualRow = blockRow * 3 + r;
                            int actualCol = blockCol * 3 + c;

                            TextBox cell = new TextBox
                            {
                                // English: Apply the style we defined in XAML
                                Style = (Style)this.Resources["SudokuCell"],
                                Tag = new Tuple<int, int>(actualRow, actualCol) // English: Store row and col indices
                            };

                            // English: Attach event to validate input as the user types
                            cell.TextChanged += Cell_TextChanged;

                            _cellTextBoxes[actualRow, actualCol] = cell;

                            Grid.SetRow(cell, r);
                            Grid.SetColumn(cell, c);
                            subGrid.Children.Add(cell);
                        }
                    }

                    // English: Set the row and column for the BORDER (not the grid) in the main grid
                    Grid.SetRow(subGridBorder, blockRow);
                    Grid.SetColumn(subGridBorder, blockCol);
                    MainSudokuGrid.Children.Add(subGridBorder);
                }
            }
        }

        // English: Restricts input to numbers 1-9 only
        private void Cell_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string text = textBox.Text;
                if (text.Length > 0)
                {
                    char c = text[0];
                    if (c < '1' || c > '9')
                    {
                        textBox.Text = ""; // English: Clear invalid input
                    }
                }
            }
        }

        // English: Event handler for the Solve button
        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            int[,] board = new int[9, 9];

            // English: 1. Read values from the UI TextBoxes into the 2D array
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    string text = _cellTextBoxes[r, c].Text;
                    if (string.IsNullOrEmpty(text))
                    {
                        board[r, c] = 0;
                    }
                    else
                    {
                        board[r, c] = int.Parse(text);
                    }
                }
            }

            // English: 2. Call the engine to solve the board
            if (_engine.Solve(board))
            {
                // English: 3. If solved, display the results back on the UI
                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        // English: Only animate and color the newly solved cells
                        if (string.IsNullOrEmpty(_cellTextBoxes[r, c].Text))
                        {
                            _cellTextBoxes[r, c].Text = board[r, c].ToString();
                            _cellTextBoxes[r, c].Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219)); // English: Blue color for generated answers

                            // English: Apply smooth fade-in animation
                            ApplyFadeInAnimation(_cellTextBoxes[r, c]);
                        }
                    }
                }
            }
            else
            {
                // English: Show error if the puzzle is unsolvable
                MessageBox.Show("This Sudoku puzzle cannot be solved! Please check your input.", "No Solution", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // English: Smoothly fades in the solved numbers
        private void ApplyFadeInAnimation(TextBox textBox)
        {
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.6))
            };

            textBox.BeginAnimation(TextBox.OpacityProperty, fadeIn);
        }

        // English: Clears all text boxes on the board
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    _cellTextBoxes[r, c].Text = "";
                    _cellTextBoxes[r, c].Foreground = Brushes.Black; // English: Reset color
                }
            }
        }

        // English: Placeholder for generating new puzzles (can be expanded later)
        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Puzzle generation feature will be implemented soon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}