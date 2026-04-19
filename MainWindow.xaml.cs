using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace workhour {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class Work : INotifyPropertyChanged {
        private string _content = "";
        private DateTime _begin = new();
        private DateTime _end = new();
        private TimeSpan _hour = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        public required string Content {
            get => _content;
            set {
                _content = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Content"));
            }
        }
        public required DateTime Begin {
            get => _begin;
            set {
                _begin = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Begin"));
            }
        }

        public required DateTime End {
            get => _end;
            set {
                _end = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("End"));
            }
        }

        public required TimeSpan Hour {
            get => _hour;
            set {
                _hour = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Hour"));
            }
        }
    }

    public partial class MainWindow : Window {
        private DateTime _begin;
        private DateTime _end;
        private readonly SolidColorBrush _beginButton = new(Color.FromRgb(191, 228, 202));
        private readonly SolidColorBrush _endButton = new(Color.FromRgb(228, 191, 191));
        private bool recording = false;

        public ObservableCollection<Work> _works { get; set; }

        public MainWindow() {
            InitializeComponent();
            _works = [];
            DataContext = _works;
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e) {
            if (recording) {
                _end = AdjustEndTime(DateTime.Now);
                var workHour = CalcWorkHour(_begin, _end);

                _works.Add(new Work {
                    Content = WorkContent.Text,
                    Begin = _begin,
                    End = _end,
                    Hour = workHour
                });

                RecordButton.Background = _beginButton;
                RecordButton.Content = "開始";
                recording = false;

                WorkContent.Clear();
            } else {
                _begin = AdjustBeginTime(DateTime.Now);
                if (_begin < _end) {
                    _begin = _end;
                }
                RecordButton.Background = _endButton;
                RecordButton.Content = "終了";
                recording = true;
            }
        }

        // 作業の開始時刻を決定する。15分未満は切り捨てる。
        // ex: 8:44 => 8:30
        static private DateTime AdjustBeginTime(DateTime begin) {
            return begin.AddMinutes((begin.Minute / 15) * 15 - begin.Minute);
        }

        // 作業の終了時刻を決定する。15分未満は切りあげる。
        // ex: 8:31 => 8:45
        static private DateTime AdjustEndTime(DateTime end) {
            return end.AddMinutes(Math.Round(Math.Ceiling(end.Minute / 15.0)) * 15 - end.Minute);
        }

        static private TimeSpan CalcWorkHour(DateTime begin, DateTime end) {
            return end - begin;
        }
    }
}
