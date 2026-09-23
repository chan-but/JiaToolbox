# 部署指南

## 推送到 GitHub

### 1. 确认仓库状态

```bash
cd f:\VScode_work
git status
git log --oneline -5
```

当前已完成：
- ✅ Git 仓库初始化
- ✅ 初始代码提交（commit: dba29b7）
- ✅ 按钮布局优化提交（commit: fca7dea）
- ✅ 配置远程仓库 origin → https://github.com/chan-but/JiaToolbox.git
- ✅ Git 用户配置（chan-but）

### 2. 推送到 GitHub

**首次推送主分支：**

```bash
cd f:\VScode_work
git push -u origin main
```

如果遇到身份验证问题，需要配置 GitHub 凭据（二选一）：

**选项 A：使用 Personal Access Token (推荐)**

1. 访问 https://github.com/settings/tokens
2. 生成新 token，勾选 `repo` 权限
3. 推送时用 token 作为密码

**选项 B：使用 SSH Key**

```bash
# 生成 SSH 密钥
ssh-keygen -t ed25519 -C "your_email@example.com"

# 添加到 SSH Agent
ssh-add ~/.ssh/id_ed25519

# 将公钥添加到 GitHub
cat ~/.ssh/id_ed25519.pub

# 修改远程地址为 SSH
git remote set-url origin git@github.com:chan-but/JiaToolbox.git
git push -u origin main
```

### 3. 创建 Release

推送成功后，在 GitHub 上创建第一个 Release：

1. 访问 https://github.com/chan-but/JiaToolbox/releases
2. 点击「Draft a new release」
3. 填写：
   - Tag: `v1.2.0`
   - Title: `JiaToolBox v1.2.0 - 嘉工具箱装机助手`
   - Description: 见下方模板
4. 上传编译好的程序：`ToolBoxApp.exe` 及依赖文件

**Release 描述模板：**

```markdown
## ✨ 新功能

- 📦 支持 7 大分类的工具管理（软件、插件、数据、设置、笔记、脚本、其他）
- 🔍 批量扫描功能：一键识别文件夹中的所有安装包
- 🤖 智能识别：自动提取程序名称、版本号等信息
- ☁️ 多种存储方式：内嵌文件、云盘链接、直链下载
- 🚀 开机自启动管理：自动同步安装并注册启动项
- 🎨 分类管理：可自定义增删改分类标签
- 💾 配置导入导出：JSON 格式，方便备份迁移
- 🔄 GitHub 自动更新检查

## 🛠️ 技术栈

- .NET 8.0 + WPF
- 深色主题 UI 设计
- 静默安装参数支持（NSIS、Inno Setup、MSI 等）

## 📦 安装说明

1. 下载 `ToolBoxApp.zip`
2. 解压到任意目录
3. 运行 `ToolBoxApp.exe`

## 📋 系统要求

- Windows 10/11 (64-bit)
- .NET 8.0 Runtime

## 🔗 相关链接

- [GitHub 仓库](https://github.com/chan-but/JiaToolbox)
- [使用文档](https://github.com/chan-but/JiaToolbox/blob/main/README.md)
- [问题反馈](https://github.com/chan-but/JiaToolbox/issues)
```

## 打包发布程序

### 方式一：单文件发布

```bash
cd f:\VScode_work\ToolBoxApp

# 发布为单文件可执行程序（包含 .NET 运行时）
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# 输出目录：bin\Release\net8.0-windows\win-x64\publish\
```

### 方式二：依赖框架发布（体积更小）

```bash
# 需要用户安装 .NET 8.0 Runtime
dotnet publish -c Release -r win-x64 --self-contained false

# 输出目录：bin\Release\net8.0-windows\win-x64\publish\
```

### 创建发布压缩包

```powershell
# 压缩发布文件
cd f:\VScode_work\ToolBoxApp\bin\Release\net8.0-windows\win-x64\publish
Compress-Archive -Path * -DestinationPath ..\..\..\..\..\..\JiaToolBox_v1.2.0.zip -Force
```

## 后续维护

### 添加新功能后更新

```bash
# 1. 提交代码
git add -A
git commit -m "feat: 新功能描述"

# 2. 推送到 GitHub
git push origin main

# 3. 创建新的 tag 和 release
git tag v1.3.0
git push origin v1.3.0
```

### 更新版本号

编辑 `ToolBoxApp.csproj`：

```xml
<PropertyGroup>
    <Version>1.3.0</Version>
    <AssemblyVersion>1.3.0.0</AssemblyVersion>
    <FileVersion>1.3.0.0</FileVersion>
</PropertyGroup>
```

## 注意事项

1. **不要提交敏感信息**：检查 `.gitignore` 是否正确排除了 bin/、obj/、Downloads/ 等目录
2. **Assets 目录**：内嵌文件会被提交到仓库，注意文件大小
3. **配置文件**：`config.json` 包含个人设置，建议加入 `.gitignore`
4. **图标文件**：确保 `JiaToolBox_128x128.ico` 已提交到仓库
5. **README 截图**：建议创建 `docs/` 目录存放截图，更新 README 中的图片链接
