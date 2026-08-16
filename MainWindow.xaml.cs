using DocChange.Models.Change;
using DocChange.Services.Change;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Forms = System.Windows.Forms;

namespace DocChange;

public partial class MainWindow : Window
{
    private CheckResult? _lastCheckResult;

    public MainWindow()
    {
        InitializeComponent();

        ExecuteButton.IsEnabled = false;
    }

    private void AddReplacementRow()
    {
        var row =
            new Grid
            {
                Margin =
                    new Thickness(0, 0, 0, 10)
            };


        row.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width =
                    new GridLength(
                        1,
                        GridUnitType.Star)
            });


        row.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width =
                    new GridLength(20)
            });


        row.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width =
                    new GridLength(
                        1,
                        GridUnitType.Star)
            });


        row.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width =
                    new GridLength(40)
            });


        // =========================================
        // OLD VALUE
        // =========================================

        var oldTextBox =
            new System.Windows.Controls.TextBox
            {
                Height = 40,
                Padding =
                    new Thickness(8),
                FontSize = 14
            };
        oldTextBox.TextChanged +=
        OldTextBox_TextChanged;

        Grid.SetColumn(oldTextBox, 0);


        // =========================================
        // ARROW
        // =========================================

        var arrow =
            new TextBlock
            {
                Text = "→",
                FontSize = 20,
                HorizontalAlignment =
                    System.Windows.HorizontalAlignment.Center,
                VerticalAlignment =
                    System.Windows.VerticalAlignment.Center
            };

        Grid.SetColumn(arrow, 1);


        // =========================================
        // NEW VALUE
        // =========================================

        var newTextBox =
            new System.Windows.Controls.TextBox
            {
                Height = 40,
                Padding =
                    new Thickness(8),
                FontSize = 14
            };
        newTextBox.TextChanged +=
        NewTextBox_TextChanged;

        Grid.SetColumn(newTextBox, 2);


        // =========================================
        // DELETE BUTTON
        // =========================================

        var deleteButton =
            new System.Windows.Controls.Button
            {
                Content = "×",
                Width = 30,
                Height = 30,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(0),
                HorizontalAlignment =
                    System.Windows.HorizontalAlignment.Center,
                VerticalAlignment =
                    VerticalAlignment.Center
            };

        Grid.SetColumn(deleteButton, 3);

        deleteButton.Tag = row;

        deleteButton.Click +=
            (sender, e) =>
            {
                if (sender is System.Windows.Controls.Button button &&
                    button.Tag is Grid targetRow)
                {
                    ReplacementPanel.Children.Remove(
                        targetRow);

                    InvalidateCheckResult();
                }
            };


        // =========================================
        // ADD ELEMENTS
        // =========================================

        row.Children.Add(oldTextBox);
        row.Children.Add(arrow);
        row.Children.Add(newTextBox);
        row.Children.Add(deleteButton);


        ReplacementPanel.Children.Add(row);
    }
    private void AddReplacementButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        AddReplacementRow();

        InvalidateCheckResult();
    }

    // =====================================================
    // SELECT FOLDER
    // =====================================================

    private string? SelectFolder()
    {
        using var dialog =
            new Forms.FolderBrowserDialog();

        dialog.Description =
            "Chọn thư mục";

        dialog.ShowNewFolderButton = true;


        if (dialog.ShowDialog() ==
            Forms.DialogResult.OK)
        {
            return dialog.SelectedPath;
        }


        return null;
    }


    // =====================================================
    // INPUT FOLDER
    // =====================================================

    private void InputBrowseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string? folder =
            SelectFolder();


        if (folder == null)
        {
            return;
        }

        InputFolderTextBox.Text =
            folder;

        InvalidateCheckResult();

        StatusText.Text =
            "Trạng thái: Đã chọn thư mục đầu vào";
    }


    // =====================================================
    // OUTPUT FOLDER
    // =====================================================

    private void OutputBrowseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string? folder =
            SelectFolder();

        if (folder == null)
        {
            return;
        }

        OutputFolderTextBox.Text =
            folder;

        InvalidateCheckResult();

        StatusText.Text =
            "Trạng thái: Đã chọn thư mục đầu ra";
    }
    private List<ReplacementItem> GetReplacements()
    {
        var replacements =
            new List<ReplacementItem>();


        foreach (var child in
                 ReplacementPanel.Children)
        {
            if (child is not Grid row)
            {
                continue;
            }


            System.Windows.Controls.TextBox? oldTextBox = null;
            System.Windows.Controls.TextBox? newTextBox = null;


            foreach (var element in row.Children)
            {
                if (element is System.Windows.Controls.TextBox textBox)
                {
                    if (Grid.GetColumn(textBox) == 0)
                    {
                        oldTextBox = textBox;
                    }
                    else if (Grid.GetColumn(textBox) == 2)
                    {
                        newTextBox = textBox;
                    }
                }
            }


            if (oldTextBox == null ||
                newTextBox == null)
            {
                continue;
            }


            replacements.Add(
                new ReplacementItem
                {
                    OldValue =
                        oldTextBox.Text,

                    NewValue =
                        newTextBox.Text
                });
        }


        return replacements;
    }
    private void CheckButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        // =========================================
        // INPUT FOLDER
        // =========================================

        string inputFolder =
            InputFolderTextBox.Text.Trim();


        if (string.IsNullOrWhiteSpace(
                inputFolder))
        {
            System.Windows.MessageBox.Show(
                "Vui lòng chọn Input folder.",
                "DocChange",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }


        if (!Directory.Exists(inputFolder))
        {
            System.Windows.MessageBox.Show(
                "Input folder không tồn tại.",
                "DocChange",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }


        // =========================================
        // REPLACEMENTS
        // =========================================

        List<ReplacementItem> replacements =
            GetReplacements();


        if (replacements.Count == 0)
        {
            System.Windows.MessageBox.Show(
                "Vui lòng thêm ít nhất một nội dung cần thay đổi.",
                "DocChange",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }


        // =========================================
        // VALIDATION
        // =========================================

        try
        {
            StatusText.Text =
                "Trạng thái: Đang kiểm tra...";


            var validator =
                new ValidationService();


            CheckResult result =
                validator.CheckFiles(
                    inputFolder,
                    replacements);

            _lastCheckResult = result;

            if (result.ValidFiles > 0)
            {
                ExecuteButton.IsEnabled = true;
            }
            else
            {
                ExecuteButton.IsEnabled = false;
            }


            // =====================================
            // RESULT
            // =====================================

            StatusText.Text =
                $"Có thể xử lý: " +
                $"{result.ValidFiles}/{result.TotalFiles}";


            if (result.InvalidFiles > 0)
            {
                DetailsButton.Visibility =
                    Visibility.Visible;
            }
            else
            {
                DetailsButton.Visibility =
                    Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            StatusText.Text =
                "Trạng thái: Lỗi";


            System.Windows.MessageBox.Show(
                $"Không thể kiểm tra:\n\n{ex.Message}",
                "DocChange",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
    private void DetailsButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (_lastCheckResult == null)
        {
            return;
        }


        var message =
            new System.Text.StringBuilder();


        foreach (var file in
                 _lastCheckResult.FileResults)
        {
            if (file.IsValid)
            {
                continue;
            }


            message.AppendLine(
                $"❌ {file.FileName}");


            foreach (var missing in
                     file.MissingValues)
            {
                message.AppendLine(
                    $"   - Thiếu: {missing}");
            }


            message.AppendLine();
        }


        System.Windows.MessageBox.Show(
            message.ToString(),
            "Chi tiết kiểm tra",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    private void ExecuteButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        // =========================================
        // CHECK RESULT
        // =========================================

        if (_lastCheckResult == null)
        {
            System.Windows.MessageBox.Show(
                "Vui lòng kiểm tra tài liệu trước khi thực hiện.",
                "DocChange",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }


        // =========================================
        // INPUT
        // =========================================

        string inputFolder =
            InputFolderTextBox.Text.Trim();


        if (string.IsNullOrWhiteSpace(
                inputFolder) ||
            !Directory.Exists(inputFolder))
        {
            System.Windows.MessageBox.Show(
                "Thư mục đầu vào không hợp lệ.",
                "DocChange",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }


        // =========================================
        // OUTPUT
        // =========================================

        string outputFolder =
            OutputFolderTextBox.Text.Trim();


        if (string.IsNullOrWhiteSpace(
                outputFolder))
        {
            System.Windows.MessageBox.Show(
                "Vui lòng chọn Thư mục đầu ra.",
                "DocChange",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }


        try
        {
            Directory.CreateDirectory(
                outputFolder);

            var existingFiles =
                GetExistingOutputFiles(
                inputFolder,
                outputFolder,
                _lastCheckResult);
            bool overwriteExisting = false;


            if (existingFiles.Count > 0)
            {
                string overwriteMessage =
                    $"Có {existingFiles.Count} tài liệu " +
                    $"đã tồn tại trong Thư mục đầu ra.\n\n" +
                    "Bạn có muốn ghi đè các tài liệu này không?";


                MessageBoxResult result =
                    System.Windows.MessageBox.Show(
                        overwriteMessage,
                        "DocChange",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);


                if (result == MessageBoxResult.Yes)
                {
                    overwriteExisting = true;
                }
            }


            // =====================================
            // REPLACEMENTS
            // =====================================

            List<ReplacementItem> replacements =
                GetReplacements();


            if (replacements.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    "Chưa có nội dung cần thay đổi.",
                    "DocChange",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            // =====================================
            // PROCESS
            // =====================================

            StatusText.Text =
                "Trạng thái: Đang thực hiện...";


            var processor =
                new BatchProcessor();


            ProcessResult processResult =
                processor.ProcessFiles(
                inputFolder,
                outputFolder,
                replacements,
                _lastCheckResult,
                overwriteExisting);


            StatusText.Text =
                $"Đã xử lý: " +
                $"{processResult.ProcessedFiles}";


            string message =
                $"Đã xử lý: {processResult.ProcessedFiles}\n" +
                $"Bỏ qua: {processResult.SkippedFiles}\n" +
                $"Lỗi: {processResult.FailedFiles}";


            System.Windows.MessageBox.Show(
                message,
                "Kết quả thực hiện",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusText.Text =
                "Trạng thái: Lỗi";


            System.Windows.MessageBox.Show(
                $"Không thể thực hiện:\n\n{ex.Message}",
                "DocChange",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
    private List<string> GetExistingOutputFiles(
    string inputFolder,
    string outputFolder,
    CheckResult checkResult)
    {
        var existingFiles =
            new List<string>();


        foreach (var fileResult in
                 checkResult.FileResults)
        {
            if (!fileResult.IsValid)
            {
                continue;
            }


            string inputFile =
                Path.Combine(
                    inputFolder,
                    fileResult.FileName);


            if (!File.Exists(inputFile))
            {
                continue;
            }


            string outputFile =
                Path.Combine(
                    outputFolder,
                    fileResult.FileName);


            if (File.Exists(outputFile))
            {
                existingFiles.Add(
                    fileResult.FileName);
            }
        }


        return existingFiles;
    }

    private void InvalidateCheckResult()
    {
        _lastCheckResult = null;

        DetailsButton.Visibility =
            Visibility.Collapsed;

        ExecuteButton.IsEnabled = false;

        StatusText.Text =
            "Status: Chưa kiểm tra";
    }

    private void OldTextBox_TextChanged(
    object sender,
    TextChangedEventArgs e)
    {
        InvalidateCheckResult();
    }


    private void NewTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        InvalidateCheckResult();
    }
}