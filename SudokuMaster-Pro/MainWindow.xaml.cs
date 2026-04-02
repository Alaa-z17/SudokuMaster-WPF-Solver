using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SudokuMaster_Pro.Core;

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
    }
}