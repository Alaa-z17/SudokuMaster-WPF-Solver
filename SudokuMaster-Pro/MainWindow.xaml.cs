using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SudokuMaster_Pro.Core;
using System.Windows.Media.Animation;
using System.Windows.Threading; // Required for DispatcherTimer
using SudokuMaster_Pro.Properties;

namespace SudokuMaster_Pro
{
    public partial class MainWindow : Window
    {
        // Instance of our fast backtracking engine
        private clsSudokuEngine _engine = new clsSudokuEngine();

        //  2D Array to keep reference to our 81 TextBoxes
        private TextBox[,] _cellTextBoxes = new TextBox[9, 9];

        //  Initialize inline to satisfy C# strict null-checks
        private DispatcherTimer _timer = new DispatcherTimer();
        private int _secondsElapsed;

        private int _bestScore;

        //  Initialize Timer in Constructor
        private void InitTimer()
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _secondsElapsed++;
            TimeSpan time = TimeSpan.FromSeconds(_secondsElapsed);
            txtTimer.Text = time.ToString(@"mm\:ss");
        }

        //  Call this when a new game starts
        private void StartGame()
        {
            _secondsElapsed = 0;
            _timer.Start();
        }

        // English: Call this when the user solves it manually or clicks Solve
        private void EndGame()
        {
            _timer.Stop();
            if (_secondsElapsed < _bestScore)
            {
                _bestScore = _secondsElapsed;
                TimeSpan time = TimeSpan.FromSeconds(_bestScore);
                txtBestScore.Text = "Best: " + time.ToString(@"mm\:ss");
                MessageBox.Show("New Record! 🎉");
            }
            Properties.Settings.Default.BestTimeSeconds = _bestScore;
            Properties.Settings.Default.Save();
        }


        public MainWindow()
        {
            InitializeComponent();

            _bestScore = Properties.Settings.Default.BestTimeSeconds;
            if (_bestScore == 0) _bestScore = int.MaxValue;
            else
            {
                TimeSpan time = TimeSpan.FromSeconds(_bestScore);
                txtBestScore.Text = "Best: " + time.ToString(@"mm\:ss");
            }

            InitTimer();           // Added this line
            GenerateSudokuGrid();
        }

        // Dynamically generates the 9x9 grid in the UI
        private void GenerateSudokuGrid()
        {
            MainSudokuGrid.Children.Clear();

            // : Create the 9 major 3x3 blocks
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    //  We use a Border control to hold the sub-grid and give it thick borders
                    Border subGridBorder = new Border
                    {
                        BorderBrush = new SolidColorBrush(Color.FromRgb(44, 60, 80)), //  Dark bold borders
                        BorderThickness = new Thickness(1.5)
                    };

                    Grid subGrid = new Grid();

                    //  Put the grid inside the border
                    subGridBorder.Child = subGrid;

                    // : Define 3 rows and 3 columns for each sub-grid
                    for (int i = 0; i < 3; i++)
                    {
                        subGrid.RowDefinitions.Add(new RowDefinition());
                        subGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    }

                    // Create the actual cells (TextBoxes) inside this 3x3 block
                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            int actualRow = blockRow * 3 + r;
                            int actualCol = blockCol * 3 + c;

                            TextBox cell = new TextBox
                            {
                                //  Apply the style we defined in XAML
                                Style = (Style)this.Resources["SudokuCell"],
                                Tag = new Tuple<int, int>(actualRow, actualCol) //Store row and col indices
                            };

                            //  Attach event to validate input as the user types
                            cell.TextChanged += Cell_TextChanged;

                            _cellTextBoxes[actualRow, actualCol] = cell;

                            Grid.SetRow(cell, r);
                            Grid.SetColumn(cell, c);
                            subGrid.Children.Add(cell);
                        }
                    }

                    //  Set the row and column for the BORDER (not the grid) in the main grid
                    Grid.SetRow(subGridBorder, blockRow);
                    Grid.SetColumn(subGridBorder, blockCol);
                    MainSudokuGrid.Children.Add(subGridBorder);
                }
            }
        }

        //  Restricts input to numbers 1-9 only
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
                        textBox.Text = ""; //  Clear invalid input
                    }
                }
            }
        }



        private bool IsBoardValid(int[,] board)
        {
            // التحقق من الصفوف
            for (int r = 0; r < 9; r++)
            {
                bool[] seen = new bool[10];
                for (int c = 0; c < 9; c++)
                {
                    int num = board[r, c];
                    if (num != 0)
                    {
                        if (seen[num]) return false;
                        seen[num] = true;
                    }
                }
            }
            // التحقق من الأعمدة
            for (int c = 0; c < 9; c++)
            {
                bool[] seen = new bool[10];
                for (int r = 0; r < 9; r++)
                {
                    int num = board[r, c];
                    if (num != 0)
                    {
                        if (seen[num]) return false;
                        seen[num] = true;
                    }
                }
            }
            // التحقق من المربعات 3x3
            for (int box = 0; box < 9; box++)
            {
                int startRow = (box / 3) * 3;
                int startCol = (box % 3) * 3;
                bool[] seen = new bool[10];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        int num = board[startRow + i, startCol + j];
                        if (num != 0)
                        {
                            if (seen[num]) return false;
                            seen[num] = true;
                        }
                    }
            }
            return true;
        }




        //  Event handler for the Solve button
        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            int[,] board = new int[9, 9];

            //  1. Read values from the UI TextBoxes into the 2D array
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


            if (!IsBoardValid(board))
            {
                MessageBox.Show("The board contains duplicate numbers in a row, column, or box. Please fix them first.", "Invalid Board", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            //  2. Call the engine to solve the board
            if (_engine.Solve(board))
            {
                // : 3. If solved, display the results back on the UI
                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        // Only animate and color the newly solved cells
                        if (string.IsNullOrEmpty(_cellTextBoxes[r, c].Text))
                        {
                            _cellTextBoxes[r, c].Text = board[r, c].ToString();
                            _cellTextBoxes[r, c].Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219)); // English: Blue color for generated answers

                            //  Apply smooth fade-in animation
                            ApplyFadeInAnimation(_cellTextBoxes[r, c]);
                        }
                    }
                }
            }
            else
            {
                //  Show error if the puzzle is unsolvable
                MessageBox.Show("This Sudoku puzzle cannot be solved! Please check your input.", "No Solution", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //  Smoothly fades in the solved numbers
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

        //  Clears all text boxes on the board
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox cell = _cellTextBoxes[r, c];
                    cell.Text = "";
                    cell.Foreground = Brushes.Black;
                    cell.IsReadOnly = false;
                    cell.Background = Brushes.White;
                }
            }
        }


        //  Generates a new random playable puzzle
        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            int difficulty = 45;
            if (cmbDifficulty.SelectedItem is ComboBoxItem item)
            {
                difficulty = Convert.ToInt32(item.Tag);
            }
            int[,] newBoard = _engine.GeneratePuzzle(difficulty);

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox cell = _cellTextBoxes[r, c];
                    cell.Text = ""; // Clear previous text

                    if (newBoard[r, c] != 0)
                    {
                        cell.Text = newBoard[r, c].ToString();
                        cell.Foreground = Brushes.Black; // Fixed numbers are black
                        cell.IsReadOnly = true; // Lock generated numbers
                        cell.Background = new SolidColorBrush(Color.FromRgb(245, 247, 250)); // Slight gray background for locked cells
                    }
                    else
                    {
                        cell.Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219)); // User/Solved numbers will be blue
                        cell.IsReadOnly = false; // Unlock empty cells for user input
                        cell.Background = Brushes.White; //  White background for input cells
                    }
                }
            }

            MessageBox.Show("New Puzzle Generated Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void bgMusic_MediaEnded(object sender, RoutedEventArgs e)
        {
            bgMusic.Position = TimeSpan.Zero;
            bgMusic.Play();
        }
    }
}