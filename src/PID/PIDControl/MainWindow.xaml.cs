using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Threading;

namespace PIDControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private double _mousewheelInterval = 0.1;
        private bool _isAutoRefresh = false;
        private PID _pid = new PID(0, 0, 0);
        private IControl _control = new CustomControl();

        public ObservableCollection<ISeries> Series { get; set; } = new ObservableCollection<ISeries>();
        public ObservableCollection<RectangularSection> Sections { get; set; } = new ObservableCollection<RectangularSection>();
        public ObservableCollection<double> Values = new ObservableCollection<double>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Series.CollectionChanged += Series_CollectionChanged;
            Sections.CollectionChanged += Series_CollectionChanged;
            Series.Add(new LineSeries<double>
            {
                Values = Values,
                Fill = null,
                GeometryFill = null,
                GeometrySize = 0,
                LineSmoothness = 1,
            });
        }

        private void Series_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Series)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Sections)));
        }

        private void TextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (double.TryParse(textBox.Text, out double val))
                {
                    double interval = _mousewheelInterval * (e.Delta > 0 ? 1 : -1);
                    textBox.Text = (val += interval).ToString("0.######");
                    e.Handled = true;
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag?.ToString() == "PID")
            {
                string p = PART_TextBox_P?.Text ?? "0";
                string i = PART_TextBox_I?.Text ?? "0";
                string d = PART_TextBox_D?.Text ?? "0";
                if (double.TryParse(p, out double dp) && double.TryParse(i, out double di) && double.TryParse(d, out double dd))
                {
                    _pid.KP = dp;
                    _pid.KI = di;
                    _pid.KD = dd;
                }
            }
            if (_isAutoRefresh)
            {
                RefreshAsync();
            }
        }

        private void PART_TextBox_MouseWheelInterval_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (double.TryParse(textBox.Text, out double val))
                {
                    double interval = 0.01 * (e.Delta > 0 ? 1 : -1);
                    textBox.Text = (val += interval).ToString("0.######");
                    e.Handled = true;
                }
            }
        }

        private void PART_TextBox_MouseWheelInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && double.TryParse(textBox.Text, out double value))
            {
                _mousewheelInterval = value;
            }
        }

        private void CheckBox_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                _isAutoRefresh = checkBox.IsChecked ?? false;
            }
        }

        private void RadioButton_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked == true && Enum.TryParse(radioButton.Tag?.ToString(), out PID.ControlType type))
            {
                _pid.PIDType = type;
                _pid.Reset();
            }
        }


        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshAsync();
        }

        private CancellationTokenSource _cts = null;
        private object _refreshLock = new object();

        private async void RefreshAsync()
        {
            lock (_refreshLock)
            {
                if (_cts != null)
                {
                    if (!_cts.IsCancellationRequested)
                    {
                        _cts.Cancel();
                    }
                    _cts.Dispose();
                }
                _cts = new CancellationTokenSource();

                _control.Reset();
                _pid.Reset();
                Values.Clear();
                Sections.Clear();
                Values.Add(_control.ActualValue);
                Sections.Add(new RectangularSection
                {
                    Yi = _control.TargetValue,
                    Yj = _control.TargetValue,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.Red,
                        StrokeThickness = 3,
                        PathEffect = new DashEffect(new float[] { 6, 6 })
                    }
                });
            }
            string countText = PART_TextBox_Count?.Text ?? "0";
            await Task.Run(() =>
            {
                if (int.TryParse(countText, out int count))
                {
                    for (int i = 0; i < count && !_cts.IsCancellationRequested; i++)
                    {
                        double output = _pid.Calculate(_control.TargetValue, _control.ActualValue);
                        if (_pid.PIDType == PID.ControlType.Positional)
                        {
                            _control.Output(output);
                        }
                        else
                        {
                            _control.Output(_control.OutputValue + output);
                        }
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Values.Add(_control.ActualValue);
                        });
                    }
                }
            }, _cts.Token);
        }
    }
}
