using FastCdReplace.Core;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FastCdReplace.Forms
{
    public partial class MainForm : Form
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly CdAlgorithmProcessor _processor = new CdAlgorithmProcessor();

        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isRunning;

        public MainForm()
        {
            InitializeComponent();
            InitializeFormState();
        }

        private void InitializeFormState()
        {
            Text = "CD-Замена";

            txtInputFolderPath.Text = string.Empty;

            chkStatistics.Checked = false;
            chkNoOutputFolder.Checked = false;

            lblElapsedText.Text = "00.00.00 мин/сек/мсек";
            lblAlgorithmText.Text = "00.00.00 алгоритм";
            lblPercentText.Text = "%";

            progressBarMain.Value = 0;

            btnCancel.Enabled = false;

            timerExecution.Stop();
        }

        private void btnSelectInputFolder_Click(object sender, EventArgs e)
        {
            if (_isRunning)
                return;

            using var dialog = new FolderBrowserDialog
            {
                Description = "Выберите папку \"CD-in\"",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtInputFolderPath.Text = dialog.SelectedPath;
                txtInputFolderPath.SelectionLength = 0;
                txtInputFolderPath.SelectionStart = 0;
            }
        }

        private async void btnExecute_Click(object sender, EventArgs e)
        {
            if (_isRunning)
                return;

            string inputFolder = txtInputFolderPath.Text.Trim();

            if (string.IsNullOrWhiteSpace(inputFolder))
            {
                MessageBox.Show(
                    "Сначала выберите папку \"CD-in\".",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(inputFolder))
            {
                MessageBox.Show(
                    "Выбранная папка не существует.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            string[] txtFiles = Directory
                .GetFiles(inputFolder, "*.txt", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (txtFiles.Length == 0)
            {
                MessageBox.Show(
                    "В папке нет текстовых файлов (*.txt).",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (txtFiles.Length > 2)
            {
                MessageBox.Show(
                    "По условию должно быть 1 или 2 текстовых файла.",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var options = new ProcessingOptions
            {
                InputFolderPath = inputFolder,
                CreateOutputFolder = !chkNoOutputFolder.Checked,
                ShowStatistics = chkStatistics.Checked,
                BufferSizeBytes = 4 * 1024 * 1024,
                TextEncoding = new UTF8Encoding(false),
                EnableParallelProcessing = true
            };

            string? outputFolderPath = null;

            if (options.CreateOutputFolder)
            {
                string? parentDirectory = Directory.GetParent(inputFolder)?.FullName;
                if (string.IsNullOrWhiteSpace(parentDirectory))
                {
                    MessageBox.Show(
                        "Не удалось определить родительскую папку для создания \"CD-out\".",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                outputFolderPath = Path.Combine(parentDirectory, "CD-out");
            }

            StartUiProcessingState();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                long totalBytesAllFiles = txtFiles.Sum(path => new FileInfo(path).Length);
                long processedBytesAllFiles = 0;

                var progress = new Progress<FileProgressInfo>(info =>
                {
                    int percent = info.OverallPercent;

                    if (percent < 0) percent = 0;
                    if (percent > 100) percent = 100;

                    progressBarMain.Value = percent;
                    lblPercentText.Text = percent == 100 ? "100%" : $"{percent}%";
                });

                FileProcessResult[] results;

                if (txtFiles.Length == 1 || !options.EnableParallelProcessing)
                {
                    results = new FileProcessResult[txtFiles.Length];

                    for (int i = 0; i < txtFiles.Length; i++)
                    {
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                        results[i] = await _processor.ProcessFileAsync(
                            inputFilePath: txtFiles[i],
                            outputFolderPath: outputFolderPath,
                            createOutputFiles: options.CreateOutputFolder,
                            encoding: options.TextEncoding,
                            progress: progress,
                            totalBytesAllFiles: totalBytesAllFiles,
                            getProcessedBytesAllFiles: () => Interlocked.Read(ref processedBytesAllFiles),
                            addProcessedBytesAllFiles: delta => Interlocked.Add(ref processedBytesAllFiles, delta),
                            cancellationToken: _cancellationTokenSource.Token);
                    }
                }
                else
                {
                    Task<FileProcessResult>[] tasks = txtFiles
                        .Select(path => _processor.ProcessFileAsync(
                            inputFilePath: path,
                            outputFolderPath: outputFolderPath,
                            createOutputFiles: options.CreateOutputFolder,
                            encoding: options.TextEncoding,
                            progress: progress,
                            totalBytesAllFiles: totalBytesAllFiles,
                            getProcessedBytesAllFiles: () => Interlocked.Read(ref processedBytesAllFiles),
                            addProcessedBytesAllFiles: delta => Interlocked.Add(ref processedBytesAllFiles, delta),
                            cancellationToken: _cancellationTokenSource.Token))
                        .ToArray();

                    results = await Task.WhenAll(tasks);
                }

                FinishUiProcessingState(results, options.ShowStatistics);
            }
            catch (OperationCanceledException)
            {
                StopUiProcessingState(resetProgress: true);

                MessageBox.Show(
                    "Операция отменена.",
                    "Отмена",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                StopUiProcessingState(resetProgress: true);

                MessageBox.Show(
                    $"Произошла ошибка:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!_isRunning)
                return;

            _cancellationTokenSource?.Cancel();
        }

        private void timerExecution_Tick(object sender, EventArgs e)
        {
            if (!_isRunning)
                return;

            string formatted = FormatElapsed(_stopwatch.Elapsed);

            lblElapsedText.Text = $"{formatted} мин/сек/мсек";
            lblAlgorithmText.Text = $"{formatted} алгоритм";
        }

        private void StartUiProcessingState()
        {
            _isRunning = true;

            btnSelectInputFolder.Enabled = false;
            btnExecute.Enabled = false;
            btnCancel.Enabled = true;

            chkStatistics.Enabled = false;
            chkNoOutputFolder.Enabled = false;

            progressBarMain.Value = 0;
            lblPercentText.Text = "0%";
            lblElapsedText.Text = "00.00.00 мин/сек/мсек";
            lblAlgorithmText.Text = "00.00.00 алгоритм";

            _stopwatch.Restart();
            timerExecution.Start();
        }

        private void StopUiProcessingState(bool resetProgress)
        {
            _isRunning = false;

            timerExecution.Stop();
            _stopwatch.Stop();

            btnSelectInputFolder.Enabled = true;
            btnExecute.Enabled = true;
            btnCancel.Enabled = false;

            chkStatistics.Enabled = true;
            chkNoOutputFolder.Enabled = true;

            if (resetProgress)
            {
                progressBarMain.Value = 0;
                lblPercentText.Text = "%";
            }
        }

        private void FinishUiProcessingState(FileProcessResult[] results, bool showStatistics)
        {
            timerExecution.Stop();
            _stopwatch.Stop();

            _isRunning = false;

            btnSelectInputFolder.Enabled = true;
            btnExecute.Enabled = true;
            btnCancel.Enabled = false;

            chkStatistics.Enabled = true;
            chkNoOutputFolder.Enabled = true;

            string formatted = FormatElapsed(_stopwatch.Elapsed);

            lblElapsedText.Text = $"{formatted} мин/сек/мсек";
            lblAlgorithmText.Text = $"{formatted} алгоритм";
            lblPercentText.Text = "100%";
            progressBarMain.Value = 100;

            if (showStatistics)
            {
                ShowStatisticsDialog("Статистика", BuildStatisticsText(results));
            }
            else
            {
                MessageBox.Show(
                    "Расчёт выполнен!",
                    "Готово",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private static string BuildStatisticsText(FileProcessResult[] results)
        {
            var sb = new StringBuilder();

            foreach (FileProcessResult result in results.OrderBy(r => r.InputFilePath, StringComparer.OrdinalIgnoreCase))
            {
                string fileName = Path.GetFileName(result.InputFilePath);

                sb.AppendLine($"Файл \"{fileName}\":");
                sb.AppendLine();

                AppendRow(sb, "Число", "Вход", "Выход", "Разница");

                foreach (var row in result.NumberStatistics.OrderByDescending(r => r.Number))
                {
                    AppendRow(
                        sb,
                        row.Number.ToString(),
                        row.InputValue.ToString(),
                        row.OutputValue.ToString(),
                        row.Difference.ToString());
                }

                AppendRow(
                    sb,
                    "Всего",
                    result.InputNumbersSum.ToString(),
                    result.OutputNumbersSum.ToString(),
                    (result.InputNumbersSum - result.OutputNumbersSum).ToString());

                AppendRow(
                    sb,
                    "%",
                    "100",
                    result.OutputNumbersSumPercentOfInput.ToString("F2"),
                    (100.0 - result.OutputNumbersSumPercentOfInput).ToString("F2"));

                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private static void AppendRow(StringBuilder sb, string c1, string c2, string c3, string c4)
        {
            sb.AppendLine(
                c1.PadRight(12) +
                c2.PadLeft(10) +
                c3.PadLeft(10) +
                c4.PadLeft(10));
        }

        private static void ShowStatisticsDialog(string title, string text)
        {
            using var form = new Form();
            using var textBox = new TextBox();
            using var button = new Button();

            form.Text = title;
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.ClientSize = new Size(620, 380);

            textBox.Multiline = true;
            textBox.ReadOnly = true;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.Font = new Font("Consolas", 11F);
            textBox.Location = new Point(12, 12);
            textBox.Size = new Size(596, 320);
            textBox.Text = text;

            button.Text = "OK";
            button.DialogResult = DialogResult.OK;
            button.Location = new Point(508, 340);
            button.Size = new Size(100, 28);

            form.Controls.Add(textBox);
            form.Controls.Add(button);
            form.AcceptButton = button;

            form.ShowDialog();
        }

        private static string FormatElapsed(TimeSpan elapsed)
        {
            return $"{elapsed.Minutes:00}.{elapsed.Seconds:00}.{elapsed.Milliseconds / 10:00}";
        }
    }
}