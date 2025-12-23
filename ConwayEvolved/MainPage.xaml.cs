using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using Microsoft.Maui.Controls;

namespace ConwayEvolved
{
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<Cell> _cells;
        private int _rows = 40;
        private int _columns = 40;
        private System.Timers.Timer _timer;
        private bool _running;

        public MainPage()
        {
            InitializeComponent();

            InitializeGrid(_rows, _columns);

            // Configure ItemsLayout programmatically so Span can be dynamic
            CellsView.ItemsLayout = new GridItemsLayout(_columns, ItemsLayoutOrientation.Vertical)
            {
                VerticalItemSpacing = 0,
                HorizontalItemSpacing = 0
            };

            //SizeLabel.Text = $"{_columns}x{_rows}";

            _timer = new System.Timers.Timer(250);
            _timer.Elapsed += (s, e) =>
            {
                if (_running)
                {
                    MainThread.BeginInvokeOnMainThread(() => Step());
                }
            };
        }

        private void InitializeGrid(int rows, int columns)
        {
            _rows = rows;
            _columns = columns;

            _cells = new ObservableCollection<Cell>();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    _cells.Add(new Cell(r, c, false));
                }
            }

            CellsView.ItemsSource = _cells;
            // update layout span in case columns changed
            CellsView.ItemsLayout = new GridItemsLayout(_columns, ItemsLayoutOrientation.Vertical)
            {
                VerticalItemSpacing = 0,
                HorizontalItemSpacing = 0
            }
                ;
        }

        private void OnCellTapped(object sender, EventArgs e)
        {
            // Try to get the Cell from the gesture's CommandParameter first,
            // otherwise fall back to the sender's BindingContext (the Frame).
            Cell cell = null;
            var numberOfTaps = 0;

            if (sender is TapGestureRecognizer tap)
            {
                cell = tap.CommandParameter as Cell;
                numberOfTaps = tap.NumberOfTapsRequired;
            }

            if (cell == null && sender is Element elem)
            {
                cell = elem.BindingContext as Cell;
            }

            if (cell != null)
            {
                cell.IsAlive = !cell.IsAlive;
                if(numberOfTaps >= 3)
                {
                    cell.Color = "0000FF"; 
                }
            }
        }

        private void StartBtn_Clicked(object sender, EventArgs e)
        {
            if (_running) return;
            _running = true;
            _timer.Start();
        }

        private void StopBtn_Clicked(object sender, EventArgs e)
        {
            _running = false;
            _timer.Stop();
        }

        private void StepBtn_Clicked(object sender, EventArgs e)
        {
            Step();
        }

        private void ClearBtn_Clicked(object sender, EventArgs e)
        {
            _running = false;
            _timer.Stop();
            foreach (var cell in _cells)
            {
                cell.IsAlive = false;
            }
        }

        // One tick: compute next generation and update cells
        private void Step()
        {
            var next = new bool[_rows, _columns];
            var nextColor = new string[_rows, _columns];

            foreach (var cell in _cells)
            {
                int r = cell.Row;
                int c = cell.Column;
                int aliveNeighbors = CountAliveNeighbors(r, c);
                bool currentlyAlive = cell.IsAlive;

                if (currentlyAlive)
                {
                    next[r, c] = aliveNeighbors == 2 || aliveNeighbors == 3;
                    nextColor[r, c] = (aliveNeighbors == 2 || aliveNeighbors == 3) && (cell.Color.IndexOf("FF") >= 4) ? "0000FF" : "000000";
                }
                else
                {
                    next[r, c] = aliveNeighbors == 3;
                }
            }

            // apply results
            foreach (var cell in _cells)
            {
                cell.IsAlive = next[cell.Row, cell.Column];
            }
        }

        private int CountAliveNeighbors(int row, int col)
        {
            int count = 0;
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int rr = row + dr;
                    int cc = col + dc;
                    if (rr >= 0 && rr < _rows && cc >= 0 && cc < _columns)
                    {
                        var neighbor = _cells[rr * _columns + cc];
                        if (neighbor.IsAlive) count++;
                    }
                }
            }
            return count;
        }

        // Simple cell model with INotifyPropertyChanged
        private class Cell : INotifyPropertyChanged
        {
            private bool _isAlive;

            private string _color;
            public string Color { 
                get { return _color; }
                set { _color = value; }
            }

            public int Row { get; }
            public int Column { get; }

            public bool IsAlive
            {
                get => _isAlive;
                set
                {
                    if (_isAlive == value) return;
                    _isAlive = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAlive)));
                }
            }

            public Cell(int row, int column, bool alive)
            {
                Row = row;
                Column = column;
                _isAlive = alive;
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}
