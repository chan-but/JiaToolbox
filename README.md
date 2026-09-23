# 嘉工具箱 / JiaToolBox

一个轻量级的 Windows 装机工具箱，帮助你快速部署常用软件、插件、配置和数据。

## ✨ 功能特性

- 📦 **工具管理**：支持软件、插件、数据、设置、笔记、脚本等多种分类
- 🔍 **批量扫描**：一键扫描文件夹，自动识别安装包并添加到工具箱
- 🤖 **智能识别**：自动提取程序名称、版本号等信息
- ☁️ **多种存储**：支持内嵌文件、云盘链接（蓝奏云等）、直链下载
- 🚀 **开机自启动**：维护自启动程序清单，登录时自动同步安装
- 🎨 **分类管理**：可自定义分类标签，灵活组织工具
- 💾 **配置导入导出**：JSON 格式配置，方便备份和迁移
- 🔄 **自动更新检查**：一键检查 GitHub 最新版本

## 🖼️ 截图

![嘉工具箱主界面](docs/screenshot.png)

## 🚀 快速开始

### 下载使用

1. 前往 [Releases](https://github.com/chan-but/JiaToolbox/releases/latest) 下载最新版本
2. 解压后运行 `ToolBoxApp.exe`
3. 首次使用建议先在「设置」中配置默认安装目录和下载目录

### 从源码构建

需要 .NET 8.0 SDK

```bash
git clone https://github.com/chan-but/JiaToolbox.git
cd JiaToolbox/ToolBoxApp
dotnet build --configuration Release
```

生成的程序位于 `bin/Release/net8.0-windows/`

## 📖 使用说明

### 添加工具

**方式一：手动添加**
1. 点击「➕ 添加工具」按钮
2. 填写工具信息（简称、程序名称、版本、分类等）
3. 选择存储方式：
   - 内嵌文件：小文件放在 Assets 目录下
   - 云盘/直链：填写下载链接和提取码

**方式二：批量扫描**
1. 点击「🔍 扫描」按钮
2. 选择包含安装包的文件夹
3. 程序自动识别所有 .exe/.msi/.zip/.rar/.7z 文件
4. 确认后批量添加到工具箱

### 安装工具

1. 在工具卡片上点击「安装」或「下载并安装」
2. 对于云盘链接，会自动打开浏览器，提取码已复制到剪贴板
3. 手动下载完成后，在编辑工具中填写「已下载路径」
4. 程序自动匹配安装包并执行静默安装

### 开机自启动

1. 切换到「🚀 开机自启动」标签
2. 添加需要开机启动的程序清单
3. 勾选「开机登录时自动后台同步」
4. 登录 Windows 时自动检测、下载、安装、注册启动项

## 🛠️ 技术栈

- .NET 8.0 + WPF
- JSON 配置存储
- Windows 任务计划程序集成
- 静默安装参数支持（NSIS、Inno Setup、MSI 等）

## 📂 项目结构

```
ToolBoxApp/
├── Models/              数据模型
│   ├── AppConfig.cs     全局配置
│   ├── ToolItem.cs      工具条目
│   └── AutoStartItem.cs 自启动条目
├── Services/            业务逻辑
│   ├── ConfigService.cs          配置读写
│   ├── SoftwareDetectionService.cs  安装包识别
│   ├── InstallService.cs         静默安装
│   ├── DownloadService.cs        文件下载
│   └── AutoStartSyncService.cs   自启动同步
├── Views/               对话框
│   ├── ToolEditDialog.*         工具编辑
│   ├── SettingsDialog.*         设置界面
│   └── ...
├── Assets/              内嵌资源目录
└── MainWindow.*         主窗口
```

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📄 开源协议

MIT License

## 🔗 相关链接

- [GitHub 仓库](https://github.com/chan-but/JiaToolbox)
- [问题反馈](https://github.com/chan-but/JiaToolbox/issues)
- [最新版本](https://github.com/chan-but/JiaToolbox/releases/latest)

## 📝 更新日志

### v1.2.0 (2024-01-XX)

- ✨ 新增批量扫描功能，一键识别文件夹中的所有安装包
- ✨ 新增分类管理功能，可自由增删改分类
- 🎨 重新设计添加工具界面和设置界面，增加间距优化布局
- 🔄 新增 GitHub 自动更新检查
- 🔗 完全移除 ZXC 浏览器集成，改为系统默认浏览器
- 🐛 修复版本号识别时误删产品名开头字符的问题
- 📦 应用图标更换为 JiaToolBox.ico

### v1.1.0 (2024-01-XX)

- ✨ 新增开机自启动管理功能
- ✨ 新增自动识别功能，自动提取程序名称和版本号
- 🎨 优化用户界面，采用深色主题
- 📦 支持内嵌文件和云盘链接两种存储方式

### v1.0.0 (2024-01-XX)

- 🎉 初始版本发布
- 📦 基础工具管理功能
- 💾 配置导入导出
- ☁️ 云盘链接支持

---

Made with ❤️ by [chan-but](https://github.com/chan-but)
