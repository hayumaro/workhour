using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
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
        public required string Case { get; init; }
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
                // 1行ごとデータベースに登録する
            }
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e) {
            if (recording) {
                _end = AdjustEndTime(DateTime.Now);
                if (_begin.Hour == _end.Hour && _begin.Minute == _end.Minute) {
                    StatusText.Text = "工数が0のため記録しません";
                } else {
                    var workCase = WorkCasePanel.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked == true)!;
                    Works.Add(new Work {
                        Content = WorkContent.Text,
                        Begin = _begin,
                        End = _end,
                        Hour = CalcWorkHour(_begin, _end),
                        Case = workCase.Content.ToString()
                    });
                    workCase.IsChecked = false;
                    StatusText.Text = WorkContent.Text + "を" + _end.ToShortTimeString() + "に終了しました";
                }
                WorkCasePanel.IsEnabled = true;
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
                if (WorkCasePanel.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked == true) is null) {
                    MessageBox.Show("作業区分を入力してください");
                    return;
                }
                WorkCasePanel.IsEnabled = false;
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

        private void ShortcutButtons_Click(object sender, RoutedEventArgs e) {
            if (recording) {
                StatusText.Text = "作業中のため開始できません";
                return;
            }
            if (sender is not Button button) {
                return;
            }
            WorkContent.Text = ((Button)sender).Content.ToString() switch {
                "事務" => "事務作業",
                "部会" => "部内会議",
                "PC" => "PCセットアップ",
                "点検" => "点検",
                "問い合わせ" => "問い合わせ対応",
            };
            var workCaseName = ((Button)sender).Content.ToString() switch {
                "事務" => "事務",
                "部会" => "打ち合わせ",
                "PC" => "ハード管理",
                "点検" => "定例",
                "問い合わせ" => "問い合わせ",
            };
            var workCase = WorkCasePanel.Children.OfType<RadioButton>().FirstOrDefault(r => r.Content.ToString() == workCaseName)!;
            workCase.IsChecked = true;
            RecordButton_Click(sender, e);
        }
    }
}
