# Troubleshooting Guide - 编译错误解决指南

## 🔧 常见编译错误及解决方案

### 1. DOTween 相关错误
**错误信息**: `DG.Tweening` 命名空间不存在

**解决方案**:
- 方案A: 删除或注释掉 `CardUI.cs` 中的 `using DG.Tweening;`，使用 `CardUISimple.cs` 代替
- 方案B: 从 Unity Package Manager 安装 DOTween
- 方案C: 使用 `MinimalGameTest` 进行无UI测试

### 2. Singleton 类未找到
**错误信息**: `Singleton<T>` 类型不存在

**解决方案**:
- 确保 `Assets/utils/Singleton.cs` 文件存在
- 检查文件路径是否正确
- 在Unity中刷新项目 (Ctrl+R)

### 3. CSV 相关错误
**错误信息**: `Sinbad` 命名空间或 `CsvUtil` 不存在

**解决方案**:
- 确保 `Assets/utils/CSVUtil/CsvUtil.cs` 文件存在
- 检查命名空间声明
- 确保所有CSV文件在 `Assets/Resources/csv/` 目录下

### 4. 数据类未定义错误
**错误信息**: `CustomerInfo`, `DayInfo` 等类型不存在

**解决方案**:
- 确保 `GameData.cs` 文件存在且包含所有数据类定义
- 检查类的访问修饰符 (应该是 `public`)
- 重新编译项目

## 🚀 快速验证步骤

### 步骤1: 基础编译测试
1. 在Unity中创建空场景
2. 添加空GameObject，命名为"CompileTest"
3. 挂载 `CompileTest.cs` 脚本
4. 运行场景，查看Console输出

### 步骤2: 最小系统测试
1. 创建空GameObject，命名为"GameManager"
2. 挂载 `GameManager.cs` 脚本
3. 确保 `useMinimalTest = true`
4. 运行场景，使用键盘控制测试

### 步骤3: 完整系统测试
1. 使用 `QuickUISetup.cs` 创建UI
2. 运行 `CreateBasicUI()` 方法
3. 测试完整的UI交互

## 📋 文件依赖检查清单

### 必需的核心文件:
- ✅ `Assets/utils/Singleton.cs`
- ✅ `Assets/utils/CSVUtil/CsvUtil.cs`
- ✅ `Assets/Scripts/GameData.cs`
- ✅ `Assets/Scripts/CSVLoader.cs`
- ✅ `Assets/Scripts/GameSystem.cs`
- ✅ `Assets/Scripts/CardSystem.cs`

### CSV配置文件:
- ✅ `Assets/Resources/csv/card.csv`
- ✅ `Assets/Resources/csv/customer.csv`
- ✅ `Assets/Resources/csv/day.csv`
- ✅ `Assets/Resources/csv/rune.csv`
- ✅ `Assets/Resources/csv/sigil.csv`
- ✅ `Assets/Resources/csv/upgrade.csv`

### UI相关文件 (可选):
- ⚠️ `Assets/Scripts/CardUI.cs` (需要DOTween)
- ✅ `Assets/Scripts/CardUISimple.cs` (无外部依赖)
- ✅ `Assets/Scripts/GameUI.cs`
- ✅ `Assets/Scripts/QuickUISetup.cs`

### 测试文件:
- ✅ `Assets/Scripts/CompileTest.cs`
- ✅ `Assets/Scripts/MinimalGameTest.cs`
- ✅ `Assets/Scripts/TestGameSetup.cs`

## 🎯 推荐的启动流程

### 对于初次使用:
1. 先运行 `CompileTest` 验证基础编译
2. 再运行 `MinimalGameTest` 测试核心功能
3. 最后搭建UI进行完整测试

### 对于开发调试:
1. 使用 `GameManager` 的调试模式
2. 在Console中查看详细日志
3. 使用键盘快捷键快速测试

## 📞 如果仍有问题

如果按照上述步骤仍有编译错误，请检查:
1. Unity版本兼容性 (推荐2022.3 LTS+)
2. C# 编译器设置
3. 项目设置中的脚本编译顺序
4. 是否有重复的类定义

您可以先使用 `MinimalGameTest` 验证核心游戏逻辑是否正常工作。