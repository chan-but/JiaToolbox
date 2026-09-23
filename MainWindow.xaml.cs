using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using ToolBoxApp.Models;
using ToolBoxApp.Services;
using ToolBoxApp.Views;

namespace ToolBoxApp;

public partial class MainWindow : Window
{
    private AppConfig _config = new();
    private string _currentCategory = "all";
    private readonly AutoStartSyncService _syncService = new();

    public MainWindow()
    {
        InitializeComponent();
        _config = ConfigService.Load();
        _syncService.Log += msg => Dispatcher.Invoke(() => AppendLog(msg));
        Loaded += async (_, _) =>
        {
            RenderCategories();
            RenderTools();
            RenderAutoStartItems();
            LoginSyncCheckBox.Checked -= LoginSyncCheckBox_Changed;
            LoginSyncCheckBox.Unchecked -= LoginSyncCheckBox_Changed;
            LoginSyncCheckBox.IsChecked = await ScheduledTaskService.IsRegisteredAsync();
            LoginSyncCheckBox.Checked += LoginSyncCheckBox_Changed;
            LoginSyncCheckBox.Unchecked += LoginSyncCheckBox_Changed;
        };
    }

    private void AppendLog(string msg)
    {
        SyncLogText.Text += $"\n[{DateTime.Now:HH:mm:ss}] {msg}";
    }

    private void SaveConfig() => ConfigService.Save(_config);

    // ========== 自绘标题栏 ==========
    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            Maximize_Click(sender, e);
            return;
        }
        DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SettingsDialog(_config) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            SaveConfig();
        }
    }

    // ========== 分类渲染 ==========
    private void RenderCategories()
    {
        CategoryPanel.Children.Clear();
        AddCategoryButton("all", "📦 全部");
        foreach (var cat in _config.Categories)
        {
            var count = _config.Tools.Count(t => t.Category == cat.Id);
            if (count > 0) AddCategoryButton(cat.Id, cat.Name);
        }
    }

    private void AddCategoryButton(string id, string name)
    {
        var isActive = _currentCategory == id;
        var btn = new Button
        {
            Content = name,
            Margin = new Thickness(0, 0, 8, 0),
            Padding = new Thickness(12, 6, 12, 6),
            Tag = id,
            Background = isActive ? (Brush)FindResource("AccentCool") : (Brush)FindResource("BgSecondary"),
            Foreground = (Brush)FindResource("TextPrimary"),
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        btn.Click += (_, _) =>
        {
            _currentCategory = id;
            RenderCategories();
            RenderTools();
        };
        CategoryPanel.Children.Add(btn);
    }

    // ========== 工具箱渲染 ==========
    private void RenderTools()
    {
        var tools = _config.Tools.AsEnumerable();
        if (_currentCategory != "all")
            tools = tools.Where(t => t.Category == _currentCategory);

        var keyword = SearchBox.Text?.Trim().ToLower() ?? "";
        if (!string.IsNullOrEmpty(keyword))
        {
            tools = tools.Where(t =>
                t.DisplayName.ToLower().Contains(keyword) ||
                t.Description.ToLower().Contains(keyword) ||
                t.Tags.Any(tag => tag.ToLower().Contains(keyword)));
        }

        ToolsItemsControl.ItemsSource = null;
        ToolsItemsControl.Items.Clear();
        var list = tools.ToList();
        var panelItems = list.Select(BuildToolCard).ToList();
        ToolsItemsControl.ItemsSource = panelItems;
    }

    private Border BuildToolCard(ToolItem tool)
    {
        var card = new Border
        {
            Style = (Style)FindResource("CardBorder"),
            Width = 320,
            Margin = new Thickness(0, 0, 16, 16)
        };

        var stack = new StackPanel();

        var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        headerPanel.Children.Add(new TextBlock { Text = tool.Icon, FontSize = 28, Margin = new Thickness(0, 0, 10, 0) });
        var titleStack = new StackPanel();
        titleStack.Children.Add(new TextBlock { Text = tool.DisplayName, FontSize = 15, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("TextPrimary") });
        if (!string.IsNullOrEmpty(tool.Version))
            titleStack.Children.Add(new TextBlock { Text = tool.Version, FontSize = 11, Foreground = (Brush)FindResource("TextSecondary") });
        headerPanel.Children.Add(titleStack);
        stack.Children.Add(headerPanel);

        if (!string.IsNullOrEmpty(tool.Description))
            stack.Children.Add(new TextBlock
            {
                Text = tool.Description,
                Foreground = (Brush)FindResource("TextSecondary"),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 10)
            });

        var typeTag = new Border
        {
            Background = tool.Type == StorageType.Embedded ? (Brush)FindResource("SuccessColor") : (Brush)FindResource("AccentCool"),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(6, 2, 6, 2),
            Opacity = 0.85,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 10)
        };
        typeTag.Child = new TextBlock
        {
            Text = tool.Type == StorageType.Embedded ? "📦 内嵌" : "☁️ 云盘",
            FontSize = 11,
            Foreground = Brushes.White
        };
        stack.Children.Add(typeTag);

        if (tool.Tags.Count > 0)
        {
            var tagsScroll = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = new Thickness(0, 0, 0, 10),
                Height = 28
            };
            var tagsPanel = new StackPanel { Orientation = Orientation.Horizontal };
            foreach (var tag in tool.Tags)
            {
                var tagBorder = new Border
                {
                    Background = (Brush)FindResource("BgSecondary"),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(8, 3, 8, 3),
                    Margin = new Thickness(0, 0, 6, 0)
                };
                tagBorder.Child = new TextBlock { Text = $"#{tag}", FontSize = 11, Foreground = (Brush)FindResource("AccentCool") };
                tagsPanel.Children.Add(tagBorder);
            }
            tagsScroll.Content = tagsPanel;
            // 鼠标滚轮在标签区域内转为横向滚动
            tagsScroll.PreviewMouseWheel += (s, args) =>
            {
                tagsScroll.ScrollToHorizontalOffset(tagsScroll.HorizontalOffset - args.Delta);
                args.Handled = true;
            };
            stack.Children.Add(tagsScroll);
        }

        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal };

        var actionBtn = new Button
        {
            Content = tool.Type == StorageType.Embedded ? "安装" : "下载并安装",
            Style = (Style)FindResource("PrimaryButton"),
            Width = 100,
            Margin = new Thickness(0, 0, 8, 0)
        };
        actionBtn.Click += async (_, _) => await InstallToolAsync(tool);
        btnPanel.Children.Add(actionBtn);

        var editBtn = new Button { Content = "✏️", Style = (Style)FindResource("SecondaryButton"), Width = 40, Margin = new Thickness(0, 0, 8, 0) };
        editBtn.Click += (_, _) => EditTool(tool);
        btnPanel.Children.Add(editBtn);

        var delBtn = new Button { Content = "🗑️", Style = (Style)FindResource("SecondaryButton"), Width = 40 };
        delBtn.Click += (_, _) => DeleteTool(tool);
        btnPanel.Children.Add(delBtn);

        stack.Children.Add(btnPanel);
        card.Child = stack;
        return card;
    }

    // ========== 工具操作 ==========
    private async Task InstallToolAsync(ToolItem tool)
    {
        try
        {
            string installerPath;

            if (tool.Type == StorageType.Embedded)
            {
                installerPath = Path.Combine(ConfigService.AssetsDir, tool.Path);
                if (!File.Exists(installerPath))
                {
                    MessageBox.Show($"内嵌文件不存在：{installerPath}", "安装失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                // 优先级 1：用户在编辑工具时已通过"自动识别"确定了具体文件
                if (!string.IsNullOrWhiteSpace(tool.LocalFilePath) && File.Exists(tool.LocalFilePath))
                {
                    installerPath = tool.LocalFilePath;
                }
                else
                {
                    // 优先级 2：在"已下载路径"中按程序名称自动匹配
                    var (matchedPath, isArchive) = InstallService.FindByProgramName(tool.DownloadDir, tool.ProgramName);
                    if (matchedPath != null && !isArchive)
                    {
                        installerPath = matchedPath;
                    }
                    else if (matchedPath != null && isArchive)
                    {
                        var openDir = MessageBox.Show(
                            $"在已下载路径中找到压缩包：\n{matchedPath}\n\n需要先解压才能安装，是否现在打开该目录？",
                            "需要先解压", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (openDir == MessageBoxResult.Yes)
                            Process.Start(new ProcessStartInfo("explorer.exe", tool.DownloadDir) { UseShellExecute = true });
                        return;
                    }
                    else if (!string.IsNullOrWhiteSpace(tool.Url) && CloudLinkParser.IsSharePageLink(tool.Url))
                    {
                        // 网盘分享页是网页，无法程序化下载文件本体；打开浏览器由用户手动点击下载
                        if (!string.IsNullOrWhiteSpace(tool.ExtractCode))
                            Clipboard.SetText(tool.ExtractCode);
                        Process.Start(new ProcessStartInfo(tool.Url) { UseShellExecute = true });
                        var codeTip = string.IsNullOrWhiteSpace(tool.ExtractCode)
                            ? "已在浏览器中打开网盘链接，请手动点击下载。\n下载完成后，在编辑工具中填写「已下载路径」，再次点击安装即可自动匹配安装包。"
                            : $"已在浏览器中打开网盘链接，提取码「{tool.ExtractCode}」已复制到剪贴板，粘贴后请手动点击下载。\n下载完成后，在编辑工具中填写「已下载路径」，再次点击安装即可自动匹配安装包。";
                        MessageBox.Show(codeTip, "请在浏览器中手动下载", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else if (!string.IsNullOrWhiteSpace(tool.Url))
                    {
                        // 直链：程序自动下载
                        var progressWin = new ProgressWindow($"正在下载 {tool.DisplayName} ...") { Owner = this };
                        progressWin.Show();
                        var fileName = System.IO.Path.GetFileName(new Uri(tool.Url).LocalPath);
                        if (string.IsNullOrWhiteSpace(fileName)) fileName = $"{tool.DisplayName}.exe";
                        var destDir = string.IsNullOrWhiteSpace(tool.DownloadDir) ? ConfigService.DownloadsDir : tool.DownloadDir;
                        installerPath = Path.Combine(destDir, fileName);
                        var progress = new Progress<int>(p => progressWin.SetProgress(p));
                        await DownloadService.DownloadFileAsync(tool.Url, installerPath, progress);
                        progressWin.Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "未在已下载路径中找到匹配的安装包，且未配置下载链接。\n请先手动下载软件到「已下载路径」目录，或填写下载链接。",
                            "无法安装", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }

            var result = MessageBox.Show(
                $"已获取安装包：\n{installerPath}\n\n静默参数：{(string.IsNullOrWhiteSpace(tool.InstallArgs) ? "(无，将以默认方式打开安装程序)" : tool.InstallArgs)}\n\n是否现在执行安装？",
                "确认安装", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            var exitCode = await InstallService.RunInstallerAsync(installerPath, tool.InstallArgs, tool, _config.DefaultInstallDir);
            MessageBox.Show(exitCode == 0 ? $"{tool.DisplayName} 安装完成！" : $"安装程序退出码：{exitCode}（不代表一定失败，请检查程序是否已安装）",
                "安装结果", MessageBoxButton.OK, exitCode == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"操作失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void EditTool(ToolItem tool)
    {
        var dialog = new ToolEditDialog(tool, _config.Categories) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            SaveConfig();
            RenderCategories();
            RenderTools();
        }
    }

    private void DeleteTool(ToolItem tool)
    {
        if (MessageBox.Show($"确定要删除「{tool.DisplayName}」吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        _config.Tools.Remove(tool);
        SaveConfig();
        RenderCategories();
        RenderTools();
    }

    private void AddTool_Click(object sender, RoutedEventArgs e)
    {
        var newTool = new ToolItem();
        var dialog = new ToolEditDialog(newTool, _config.Categories, isNew: true) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _config.Tools.Add(newTool);
            SaveConfig();
            RenderCategories();
            RenderTools();
        }
    }

    private void ScanTools_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "选择要扫描的目录（将批量识别其中的所有安装包）" };
        if (dialog.ShowDialog() != true) return;

        var dir = dialog.FolderName;
        var results = SoftwareDetectionService.ScanDirectory(dir);
        
        if (results.Count == 0)
        {
            MessageBox.Show("未在该目录中找到任何安装包（.exe/.msi/.zip/.rar/.7z）", "扫描完成", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirmMsg = $"在目录中找到 {results.Count} 个可能的安装包：\n\n{string.Join("\n", results.Take(5).Select(r => $"• {r.ProgramName} {r.Version}"))}" +
                         (results.Count > 5 ? $"\n... 以及其他 {results.Count - 5} 个" : "") +
                         "\n\n是否将它们全部添加到工具箱？";
        
        if (MessageBox.Show(confirmMsg, "确认批量添加", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        foreach (var result in results)
        {
            var newTool = new ToolItem
            {
                ProgramName = result.ProgramName,
                Version = result.Version,
                DownloadDir = dir,
                LocalFilePath = result.FilePath,
                Category = "software",
                Icon = "📦",
                Type = StorageType.Cloud
            };
            _config.Tools.Add(newTool);
        }

        SaveConfig();
        RenderCategories();
        RenderTools();
        MessageBox.Show($"已成功添加 {results.Count} 个工具到工具箱", "批量添加完成", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => RenderTools();

    private void ExportConfig_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = "JSON 配置文件|*.json", FileName = $"toolbox-config-{DateTime.Now:yyyyMMdd}.json" };
        if (dialog.ShowDialog() == true)
        {
            ConfigService.ExportTo(_config, dialog.FileName);
            MessageBox.Show("配置已导出", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ImportConfig_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "JSON 配置文件|*.json" };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                if (MessageBox.Show("导入将覆盖当前所有配置，确定继续吗？", "确认导入", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;
                _config = ConfigService.ImportFrom(dialog.FileName);
                SaveConfig();
                _currentCategory = "all";
                RenderCategories();
                RenderTools();
                RenderAutoStartItems();
                MessageBox.Show("配置已导入", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // ========== 开机自启动清单渲染 ==========
    private void RenderAutoStartItems()
    {
        var cards = _config.AutoStartItems.Select(BuildAutoStartCard).ToList();
        AutoStartItemsControl.ItemsSource = cards;
    }

    private Border BuildAutoStartCard(AutoStartItem item)
    {
        var card = new Border
        {
            Style = (Style)FindResource("CardBorder"),
            Margin = new Thickness(0, 0, 0, 12)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var infoStack = new StackPanel();
        var titleRow = new StackPanel { Orientation = Orientation.Horizontal };
        titleRow.Children.Add(new TextBlock { Text = item.Name, FontSize = 15, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("TextPrimary"), Margin = new Thickness(0, 0, 10, 0) });

        var registered = AutoStartService.IsRegistered(item.Name);
        var statusTag = new Border
        {
            Background = registered ? (Brush)FindResource("SuccessColor") : (Brush)FindResource("BgSecondary"),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(6, 1, 6, 1)
        };
        statusTag.Child = new TextBlock { Text = registered ? "✓ 已加入自启" : "未加入自启", FontSize = 11, Foreground = registered ? Brushes.White : (Brush)FindResource("TextSecondary") };
        titleRow.Children.Add(statusTag);

        if (!item.Enabled)
        {
            var disabledTag = new Border { Background = (Brush)FindResource("BgSecondary"), CornerRadius = new CornerRadius(4), Padding = new Thickness(6, 1, 6, 1), Margin = new Thickness(8, 0, 0, 0) };
            disabledTag.Child = new TextBlock { Text = "已禁用", FontSize = 11, Foreground = (Brush)FindResource("TextSecondary") };
            titleRow.Children.Add(disabledTag);
        }
        infoStack.Children.Add(titleRow);

        if (!string.IsNullOrEmpty(item.Description))
            infoStack.Children.Add(new TextBlock { Text = item.Description, Foreground = (Brush)FindResource("TextSecondary"), FontSize = 12, Margin = new Thickness(0, 4, 0, 0) });

        infoStack.Children.Add(new TextBlock { Text = $"检测路径：{item.InstallCheckPath}", Foreground = (Brush)FindResource("TextSecondary"), FontSize = 11, Margin = new Thickness(0, 4, 0, 0) });
        infoStack.Children.Add(new TextBlock { Text = $"状态：{item.LastStatus}", Foreground = (Brush)FindResource("TextSecondary"), FontSize = 11, Margin = new Thickness(0, 2, 0, 0) });

        Grid.SetColumn(infoStack, 0);
        grid.Children.Add(infoStack);

        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

        var enableCheck = new CheckBox { Content = "启用", IsChecked = item.Enabled, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
        enableCheck.Checked += (_, _) => { item.Enabled = true; SaveConfig(); RenderAutoStartItems(); };
        enableCheck.Unchecked += (_, _) => { item.Enabled = false; SaveConfig(); RenderAutoStartItems(); };
        btnPanel.Children.Add(enableCheck);

        var syncBtn = new Button { Content = "🔄 同步", Style = (Style)FindResource("SecondaryButton"), Margin = new Thickness(0, 0, 8, 0) };
        syncBtn.Click += async (_, _) => await SyncOneAsync(item);
        btnPanel.Children.Add(syncBtn);

        var editBtn = new Button { Content = "✏️", Style = (Style)FindResource("SecondaryButton"), Width = 40, Margin = new Thickness(0, 0, 8, 0) };
        editBtn.Click += (_, _) => EditAutoStartItem(item);
        btnPanel.Children.Add(editBtn);

        var delBtn = new Button { Content = "🗑️", Style = (Style)FindResource("SecondaryButton"), Width = 40 };
        delBtn.Click += (_, _) => DeleteAutoStartItem(item);
        btnPanel.Children.Add(delBtn);

        Grid.SetColumn(btnPanel, 1);
        grid.Children.Add(btnPanel);

        card.Child = grid;
        return card;
    }

    private async Task SyncOneAsync(AutoStartItem item)
    {
        try
        {
            var (success, message) = await _syncService.SyncOneAsync(item);
            SaveConfig();
            RenderAutoStartItems();
            AppendLog($"[{item.Name}] {(success ? "✓" : "✗")} {message}");
        }
        catch (Exception ex)
        {
            AppendLog($"[{item.Name}] ✗ 同步出错：{ex.Message}");
        }
    }

    private async void SyncAllAutoStart_Click(object sender, RoutedEventArgs e)
    {
        SyncLogText.Text = "开始同步全部自启动项...";
        var results = await _syncService.SyncAllAsync(_config.AutoStartItems);
        SaveConfig();
        RenderAutoStartItems();
        foreach (var (item, success, message) in results)
            AppendLog($"[{item.Name}] {(success ? "✓" : "✗")} {message}");
        AppendLog("全部同步完成");
    }

    private void EditAutoStartItem(AutoStartItem item)
    {
        var dialog = new AutoStartEditDialog(item) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            SaveConfig();
            RenderAutoStartItems();
        }
    }

    private void DeleteAutoStartItem(AutoStartItem item)
    {
        if (MessageBox.Show($"确定要删除自启动项「{item.Name}」吗？\n（仅移除清单和启动项登记，不会卸载已安装的程序）", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        AutoStartService.Unregister(item.Name);
        _config.AutoStartItems.Remove(item);
        SaveConfig();
        RenderAutoStartItems();
    }

    private void AddAutoStartItem_Click(object sender, RoutedEventArgs e)
    {
        var newItem = new AutoStartItem { Name = "新程序" };
        var dialog = new AutoStartEditDialog(newItem, isNew: true) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _config.AutoStartItems.Add(newItem);
            SaveConfig();
            RenderAutoStartItems();
        }
    }

    private async void LoginSyncCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (LoginSyncCheckBox.IsChecked == true)
        {
            var (success, message) = await ScheduledTaskService.RegisterAsync();
            AppendLog(message);
            if (!success)
            {
                MessageBox.Show(message, "注册失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginSyncCheckBox.Checked -= LoginSyncCheckBox_Changed;
                LoginSyncCheckBox.IsChecked = false;
                LoginSyncCheckBox.Checked += LoginSyncCheckBox_Changed;
            }
        }
        else
        {
            var (success, message) = await ScheduledTaskService.UnregisterAsync();
            AppendLog(message);
        }
    }
}

