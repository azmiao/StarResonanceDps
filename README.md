# 🌟 星痕共鸣工具箱

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-brightgreen.svg)](https://www.gnu.org/licenses/agpl-3.0.txt)

[🇺🇸 English README](Markdown/README.en-US.md)

本项目关键数据抓取与分析部分基于 [StarResonanceDamageCounter](https://github.com/dmlgzs/StarResonanceDamageCounter) 项目移植而来，感谢原作者对于本项目的帮助。

该工具**无需修改游戏客户端**，**不违反游戏服务条款**。该工具旨在帮助玩家更好地理解战斗数据，减少无效提升，提升游戏体验。使用该工具前，请确保**不会将数据结果用于战力歧视等破坏游戏社区环境的行为**。

![Moe-counter](https://ipacel.cc/+/MoeCounter2/?name=StarResonanceToolBox)

## 🚀 快速开始

### 前置要求

- SDK
  - [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) 或者
  - [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- Windows 10 以上

### 构建与运行

1. 克隆本项目:

   ```bash
   git clone https://github.com/anying1073/StarResonanceDps.git
   cd StarResonanceDps
   ```

2. 构建:

   ```bash
   dotnet build -c Release
   ```

3. 运行:

   ```bash
   dotnet run --project StarResonanceDpsAnalysis.WPF
   ```

### 功能特性

- ✅ 实时战斗数据分析
- 📊 伤害追踪与可视化
- 🔍 各职业专属伤害细分
- ⚙️ 可自定义用户界面和图表设置
- 💾 本地数据存储，用于长期趋势分析

### 截图

![Screenshot](Markdown/image/screenshot.jpg)

---

## 📄 许可证

[![AGPLv3](https://www.gnu.org/graphics/agplv3-with-text-162x68.png)](LICENSE.txt)

本项目采用 **[GNU AFFERO GENERAL PUBLIC LICENSE version 3](LICENSE.txt)** 许可证。

使用本项目即表示您同意遵守该许可证的条款。

**不欢迎**某些**不遵守**本许可证的人。**不欢迎**某些修改或翻译了开源代码却做闭源、开源一更新**闭源**就跟进的人。

---

## 👥 贡献

欢迎提交 [Issue](../../issues) 和 [Pull Request](../../pulls) 来改进项目！

## ⭐ 支持

如果这个项目对您有帮助，请给它一个 **Star ⭐**

---

## ⚠️ 免责声明

本工具仅用于游戏数据分析学习目的，不得用于任何违反游戏服务条款的行为，使用者需自行承担相关风险。
项目开发者不对任何他人使用本工具的恶意战力歧视行为负责，请在使用前确保遵守游戏社区的相关规定和道德标准。
