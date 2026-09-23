# 装机工具箱（桌面版）

一个真正的 Windows 桌面程序（.NET 8 WPF），非网页实现，具备真实的文件下载、静默安装、开机自启动管理能力。

## 目录结构

```
ToolBoxApp/
├── Models/                  数据模型
│   ├── ToolItem.cs           工具箱条目
│   ├── AutoStartItem.cs      开机自启动条目
│   └── AppConfig.cs          整体配置（含分类）
├── Services/                 核心服务
│   ├── ConfigService.cs      配置加载/保存/导入导出
│   ├── DownloadService.cs    真实 HTTP 下载（带进度）
│   ├── InstallService.cs     执行安装程序（支持静默参数）
│   ├── AutoStartService.cs   注册表开机自启动（HKCU Run 键）
│   ├── ScheduledTaskService.cs 登录时后台同步的计划任务管理
│   └── AutoStartSyncService.cs 核心编排：检测->下载安装->登记自启
├── Views/                    对话框
│   ├── ToolEditDialog.*       工具箱条目编辑
│   ├── AutoStartEditDialog.*  自启动条目编辑
│   └── ProgressWindow.*       下载进度窗口
├── MainWindow.xaml(.cs)       主窗口（工具箱 Tab + 开机自启动 Tab）
└── App.xaml(.cs)              程序入口，支持 --sync-autostart 静默模式
```

## 数据存放位置

- 配置文件：`%LocalAppData%\ToolBoxApp\config.json`
- 内嵌文件目录：`%LocalAppData%\ToolBoxApp\Assets`（把你的安装包/脚本放这里，编辑条目时填相对路径）
- 下载缓存目录：`%LocalAppData%\ToolBoxApp\Downloads`

## 开机自启动清单是如何工作的

「开机自启动」Tab 维护一个可增删改的清单，每一项包含：

- **安装检测路径**：判断程序是否已经安装到指定目录的可执行文件路径
- **来源方式**：云盘/直链下载 或 内嵌安装包
- **静默安装参数**：如 `/S`、`/VERYSILENT`、`/quiet`
- **启动路径/参数**：真正用于开机启动的可执行文件

点击"同步"（单项或全部）时：
1. 检测 `安装检测路径` 是否存在
2. 不存在 -> 按来源方式下载/取内嵌包 -> 静默执行安装 -> 再次确认路径存在
3. 存在（或刚安装完成）-> 写入注册表 `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`，登记开机自启动
4. 关闭"启用"开关的条目会被从启动项移除，但不会卸载已安装的程序

清单支持随时添加、编辑、删除，不限制数量。

## "开机登录时自动后台同步"开关

勾选后会通过 `schtasks.exe` 注册一个登录时触发的计划任务，静默运行 `ToolBoxApp.exe --sync-autostart`，这样即使不打开主界面，每次开机登录也会自动跑一遍上述同步逻辑。

**已知限制**：在部分受限的执行环境（如某些沙箱/远程会话）下，`schtasks /Create` 可能返回"拒绝访问"。这不影响核心的注册表自启动功能——只是这个"登录自动同步"的便捷开关可能需要在真实的本机用户会话下才能注册成功。如果勾选失败，程序会提示原因并保持未勾选状态，不影响其他功能使用。

## 使用步骤

1. 编译运行：`dotnet build` 后运行 `bin\Debug\net8.0-windows\ToolBoxApp.exe`，或用 `dotnet publish` 打包成独立 exe
2. 切到"🚀 开机自启动"Tab，点击"➕ 添加自启动项"
3. 填写名称、安装检测路径、来源（下载链接或内嵌包）、静默安装参数
4. 点击"🔄 同步"验证效果，或点"🔄 同步全部"批量处理
5. 需要开机自动跑，勾选"开机登录时自动后台同步"

## 打包为独立可执行文件

```powershell
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

生成的单文件 exe 位于 `bin\Release\net8.0-windows\win-x64\publish\`。
