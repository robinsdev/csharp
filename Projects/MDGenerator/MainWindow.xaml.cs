using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;  // CommonOpenFileDialog 사용을 위해 추가
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Windows.Forms;

namespace MDGenerator
{
    [Target("RichTextBoxTarget")]
    public sealed class RichTextBoxTarget : TargetWithLayout
    {
        public RichTextBox? RichTextBox { get; set; }
        public RichTextBoxTarget() { }
        protected override void Write(LogEventInfo logEvent)
        {
            if (RichTextBox != null)
            {
                var logMessage = this.Layout.Render(logEvent);
                RichTextBox.Dispatcher.Invoke(() =>
                {
                    RichTextBox.AppendText(logMessage + Environment.NewLine);
                    RichTextBox.ScrollToEnd();
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        // private LogWindow logWindow;

        private void setupNLog()
        {
            var config = LogManager.Configuration ?? new LoggingConfiguration();

            var richTextBoxTarget = new RichTextBoxTarget()
            {
                Name = "richTextBox",
                RichTextBox = richtxtLog
            };

            config.AddTarget(richTextBoxTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, richTextBoxTarget);

            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();

        }
        public MainWindow()
        {
            Process.Start("cmd.exe", "/K title NLog Console");

            InitializeComponent();
            
            setupNLog();
        }

        private void btnSelectSource_Click(object sender, RoutedEventArgs e)
        {

            // 로그 기록
            App.Logger.Info("Application started.");
            App.Logger.Warn("This is a warning.");
            App.Logger.Error("This is an error.");

            using (var dlg = new CommonOpenFileDialog())
            {
                dlg.IsFolderPicker = true;
                dlg.DefaultDirectory = txtSelectedSourcePath.Text != "" ? txtSelectedSourcePath.Text : System.Windows.Forms.Application.StartupPath;
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    txtSelectedSourcePath.Text = dlg.FileName;
                }
            }

        }        

        private void btnMakePath_Click(object sender, RoutedEventArgs e)
        {

            // 디렉토리를 돌면서 MD 파일을 링크로 생성해서 파일을 만든다, 공백이 포함된 경우에는 %20으로 변경한다.

            string rootPath = txtSelectedSourcePath.Text; // 검색할 루트 디렉토리 경로

            try
            {
                // 모든 파일 경로를 가져옴
                string[] allFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);

                // 파일 리스트 출력
                foreach (string file in allFiles)
                {
                    Console.WriteLine(file);
                }

                Console.WriteLine($"총 {allFiles.Length}개의 파일이 발견되었습니다.");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"권한이 없는 디렉토리에 접근했습니다: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"디렉토리를 찾을 수 없습니다: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류 발생: {ex.Message}");
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GeneralTransform transform = richtxtLog.TransformToAncestor(this);
            Point absolutePosition = transform.Transform(new Point(0, 0));

            double leftMargin = richtxtLog.Margin.Left;
            double topMargin = richtxtLog.Margin.Top + 10;
            double rightMargin = 10;
            double bottomMargin = 10;

            richtxtLog.Width = e.NewSize.Width - leftMargin - rightMargin - 10;
            double height = e.NewSize.Height - topMargin - bottomMargin - absolutePosition.Y - 35;

            richtxtLog.Height = (height > 20) ? height : 20;

        }
    }
}