using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace workhour {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class Work {
        public required string Content { get; init; }
        public required DateTime Begin { get; init; }
        public required DateTime End { get; init; }
        public required TimeSpan Hour { get; init; }
    }

    public partial class MainWindow : Window {
        private DateTime _begin;
        private DateTime _end;
        private readonly SolidColorBrush _beginButton = new(Color.FromRgb(191, 228, 202));
        private readonly SolidColorBrush _endButton = new(Color.FromRgb(228, 191, 191));
        private bool recording = false;

        public ObservableCollection<Work> Works { get; set; }
        public ObservableCollection<string> WorkCases { get; set; }

        public MainWindow() {
            InitializeComponent();
            WorkCases = [
                "打合せ",
                "総務"
            ];
            Works = [];
            DataContext = this;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e) {
            foreach (var work in Works) {
                // 1行ごとSQLServerに登録する
            }
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e) {
            if (recording) {
                _end = AdjustEndTime(DateTime.Now);
                if (_begin.Hour == _end.Hour && _begin.Minute == _end.Minute) {
                    StatusText.Text = "工数が0のため記録しません";
                } else {
                    Works.Add(new Work {
                        Content = WorkContent.Text,
                        Begin = _begin,
                        End = _end,
                        Hour = CalcWorkHour(_begin, _end)
                    });
                }
                RecordButton.Background = _beginButton;
                RecordButton.Content = "開始";
                WorkContent.IsReadOnly = false;
                recording = false;
                WorkContent.Clear();
            } else {
                if (WorkContent.Text == string.Empty) {
                    MessageBox.Show("作業内容を入力してください");
                    return;
                }
                _begin = AdjustBeginTime(DateTime.Now);
                if (_begin < _end) {
                    _begin = _end;
                }
                RecordButton.Background = _endButton;
                RecordButton.Content = "終了";
                WorkContent.IsReadOnly = true;
                recording = true;
                StatusText.Text = WorkContent.Text + "を" + _begin.ToShortTimeString() + "に開始しました";
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

        private void JimuButton_Click(object sender, RoutedEventArgs e) {
            if (recording) {
                StatusText.Text = "作業中のため開始できません";
                return;
            }
            WorkContent.Text = "事務作業";
            RecordButton_Click(sender, e);
        }

        private void InnerMtgButton_Click(object sender, RoutedEventArgs e) {
            if (recording) {
                StatusText.Text = "作業中のため開始できません";
                return;
            }
            WorkContent.Text = "部内会議";
            RecordButton_Click(sender, e);
        }

        private void PcSetupButton_Click(object sender, RoutedEventArgs e) {
            if (recording) {
                StatusText.Text = "作業中のため開始できません";
                return;
            }
            WorkContent.Text = "PCセットアップ";
            RecordButton_Click(sender, e);
        }
    }
}
