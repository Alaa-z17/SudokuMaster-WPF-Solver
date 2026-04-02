using SudokuMaster_Pro.Core;
using SudokuMaster_Pro.Properties;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SudokuMaster_Pro
{
    public partial class MainWindow : Window
    {
        // Instance of our fast backtracking engine
        private clsSudokuEngine _engine = new clsSudokuEngine();

        // 2D Array to keep reference to our 81 TextBoxes
        private TextBox[,] _cellTextBoxes = new TextBox[9, 9];

        // Timer and game state
        private DispatcherTimer _timer = new DispatcherTimer();
        private int _secondsElapsed;
        private int _bestScore;
        private bool _musicEnabled = true;

        // Constructor
        public MainWindow()
        {
            InitializeComponent();
            LoadBestScore();
            SetupMusic();
            InitTimer();
            GenerateSudokuGrid();
            // Note: StartGame() is not called here because the board is empty; user must generate a puzzle.
        }

        // Load saved best score from settings
        private void LoadBestScore()
        {
            _bestScore = Properties.Settings.Default.BestTimeSeconds;
            if (_bestScore == 0)
                _bestScore = int.MaxValue;
            else
            {
                TimeSpan time = TimeSpan.FromSeconds(_bestScore);
                txtBestScore.Text = "Best: " + time.ToString(@"mm\:ss");
            }
        }

        // Setup background music (optional, fails silently if file missing)
        private void SetupMusic()
        {
            try
            {
                string musicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "background_music.mp3");
                if (System.IO.File.Exists(musicPath))
                {
                    bgMusic.Source = new Uri(musicPath);
                    bgMusic.LoadedBehavior = MediaState.Manual;
                    bgMusic.Volume = 0.5;
                    bgMusic.Play();
                    bgMusic.MediaEnded += bgMusic_MediaEnded;
                    _musicEnabled = true;
                    musicIcon.Text = "🔊";
                }
                else
                {
                    _musicEnabled = false;
                    musicIcon.Text = "🔇";
                }
            }
            catch
            {
                _musicEnabled = false;
                musicIcon.Text = "🔇";
            }
        }

        // Initialize the timer
        private void InitTimer()
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        // Timer tick event
        private void Timer_Tick(object? sender, EventArgs e)
        {
            _secondsElapsed++;
            TimeSpan time = TimeSpan.FromSeconds(_secondsElapsed);
            txtTimer.Text = time.ToString(@"mm\:ss");
        }

        // Start a new game: reset timer and start counting
        private void StartGame()
        {
            _secondsElapsed = 0;
            _timer.Start();
        }

        // End the current game: stop timer and check for new record
        private void EndGame()
        {
            _timer.Stop();
            if (_secondsElapsed < _bestScore)
            {
                _bestScore = _secondsElapsed;
                TimeSpan time = TimeSpan.FromSeconds(_bestScore);
                txtBestScore.Text = "Best: " + time.ToString(@"mm\:ss");
                MessageBox.Show("New Record! 🎉", "Congratulations", MessageBoxButton.OK, MessageBoxImage.Information);
                Properties.Settings.Default.BestTimeSeconds = _bestScore;
                Properties.Settings.Default.Save();
            }
        }

        // Dynamically generates the 9x9 grid in the UI
        private void GenerateSudokuGrid()
        {
            MainSudokuGrid.Children.Clear();

            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    Border subGridBorder = new Border
                    {
                        BorderBrush = new SolidColorBrush(Color.FromRgb(44, 60, 80)),
                        BorderThickness = new Thickness(1.5)
                    };

                    Grid subGrid = new Grid();
                    subGridBorder.Child = subGrid;

                    for (int i = 0; i < 3; i++)
                    {
                        subGrid.RowDefinitions.Add(new RowDefinition());
                        subGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    }

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            int actualRow = blockRow * 3 + r;
                            int actualCol = blockCol * 3 + c;

                            TextBox cell = new TextBox
                            {
                                Style = (Style)this.Resources["SudokuCell"],
                                Tag = new Tuple<int, int>(actualRow, actualCol)
                            };

                            cell.TextChanged += Cell_TextChanged;
                            _cellTextBoxes[actualRow, actualCol] = cell;

                            Grid.SetRow(cell, r);
                            Grid.SetColumn(cell, c);
                            subGrid.Children.Add(cell);
                        }
                    }

                    Grid.SetRow(subGridBorder, blockRow);
                    Grid.SetColumn(subGridBorder, blockCol);
                    MainSudokuGrid.Children.Add(subGridBorder);
                }
            }
        }

        // Restricts input to numbers 1-9 only
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
                        textBox.Text = "";
                    }
                }
            }
        }

        // Validate board for duplicate numbers (rows, columns, boxes)
        private bool IsBoardValid(int[,] board)
        {
            // Check rows
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
            // Check columns
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
            // Check 3x3 boxes
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

        // Event handler for Solve button
        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            int[,] board = new int[9, 9];

            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    string text = _cellTextBoxes[r, c].Text;
                    board[r, c] = string.IsNullOrEmpty(text) ? 0 : int.Parse(text);
                }

            if (!IsBoardValid(board))
            {
                MessageBox.Show("The board contains duplicate numbers in a row, column, or box. Please fix them first.", "Invalid Board", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_engine.Solve(board))
            {
                for (int r = 0; r < 9; r++)
                    for (int c = 0; c < 9; c++)
                    {
                        if (string.IsNullOrEmpty(_cellTextBoxes[r, c].Text))
                        {
                            _cellTextBoxes[r, c].Text = board[r, c].ToString();
                            _cellTextBoxes[r, c].Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219));
                            ApplyFadeInAnimation(_cellTextBoxes[r, c]);
                        }
                    }
                EndGame();
            }
            else
            {
                MessageBox.Show("This Sudoku puzzle cannot be solved! Please check your input.", "No Solution", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Smooth fade-in animation for solved cells
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

        // Clear the entire board
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    TextBox cell = _cellTextBoxes[r, c];
                    cell.Text = "";
                    cell.Foreground = Brushes.Black;
                    cell.IsReadOnly = false;
                    cell.Background = Brushes.White;
                }
       
            _timer.Stop();
            _secondsElapsed = 0;
            txtTimer.Text = "00:00";
        }

        // Generate a new puzzle based on selected difficulty
        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            int difficulty = 45;
            if (cmbDifficulty.SelectedItem is ComboBoxItem item)
                difficulty = Convert.ToInt32(item.Tag);

            int[,] newBoard = _engine.GeneratePuzzle(difficulty);

            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    TextBox cell = _cellTextBoxes[r, c];
                    cell.Text = "";
                    if (newBoard[r, c] != 0)
                    {
                        cell.Text = newBoard[r, c].ToString();
                        cell.Foreground = Brushes.Black;
                        cell.IsReadOnly = true;
                        cell.Background = new SolidColorBrush(Color.FromRgb(245, 247, 250));
                    }
                    else
                    {
                        cell.Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219));
                        cell.IsReadOnly = false;
                        cell.Background = Brushes.White;
                    }
                }

            StartGame(); // Start timer for the new puzzle
            MessageBox.Show("New Puzzle Generated Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Music event handlers
        private void bgMusic_MediaEnded(object sender, RoutedEventArgs e)
        {
            bgMusic.Position = TimeSpan.Zero;
            bgMusic.Play();
        }

        private void bgMusic_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            _musicEnabled = false;
            // Do not show message to avoid annoying user
        }

        // Check button: validates current board and detects completion
        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            int[,] board = new int[9, 9];
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    string txt = _cellTextBoxes[r, c].Text;
                    board[r, c] = string.IsNullOrEmpty(txt) ? 0 : int.Parse(txt);
                }

            if (IsBoardValid(board))
            {
                bool isComplete = true;
                for (int r = 0; r < 9; r++)
                    for (int c = 0; c < 9; c++)
                        if (board[r, c] == 0) { isComplete = false; break; }

                if (isComplete)
                {
                    EndGame(); // User solved the puzzle manually
                    MessageBox.Show("Congratulations! You solved the puzzle!", "Check", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("The board has no duplicates so far. Keep going!", "Check", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Invalid board: duplicate numbers exist.", "Check", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Hint feature: returns a random empty cell and its correct number
        private (int row, int col, int number)? GetHint()
        {
            int[,] board = new int[9, 9];
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    string txt = _cellTextBoxes[r, c].Text;
                    board[r, c] = string.IsNullOrEmpty(txt) ? 0 : int.Parse(txt);
                }

            int[,] solvedBoard = (int[,])board.Clone();
            if (!_engine.Solve(solvedBoard))
                return null;

            List<(int row, int col)> emptyCells = new List<(int, int)>();
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    if (board[r, c] == 0)
                        emptyCells.Add((r, c));

            if (emptyCells.Count == 0)
                return null;

            Random random = new Random();
            var (row, col) = emptyCells[random.Next(emptyCells.Count)];
            int correctNumber = solvedBoard[row, col];
            return (row, col, correctNumber);
        }

        // Hint button click
        private void btnHint_Click(object sender, RoutedEventArgs e)
        {
            var hint = GetHint();
            if (hint == null)
            {
                MessageBox.Show("No hint available. Either the board is full or unsolvable.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var (row, col, number) = hint.Value;
            TextBox cell = _cellTextBoxes[row, col];

            if (!string.IsNullOrEmpty(cell.Text))
                return;

            cell.Text = number.ToString();
            cell.Foreground = new SolidColorBrush(Color.FromRgb(155, 89, 182)); // Purple
            ApplyFadeInAnimation(cell);
        }
        // Title bar dragging
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        // Minimize button
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Close button
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Toggle music on/off
        private void btnMusicToggle_Click(object sender, RoutedEventArgs e)
        {
            if (_musicEnabled && bgMusic.Source != null)
            {
                if (bgMusic.IsLoaded && bgMusic.Volume > 0)
                {
                    bgMusic.Volume = 0;
                    musicIcon.Text = "🔇";
                }
                else
                {
                    bgMusic.Volume = 0.5;
                    musicIcon.Text = "🔊";
                }
            }
            else
            {
                MessageBox.Show("Music file not found. Please add background_music.mp3 to the Assets folder.", "Music Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}