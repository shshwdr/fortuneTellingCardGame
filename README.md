# Fortune Telling Card Game - Demo Setup Guide

## 概述
这是一个2D占卜卡牌剧情游戏的最小可行demo。玩家扮演占卜师，为顾客进行塔罗牌占卜。

## ⚠️ 快速开始（解决编译错误）

如果遇到编译错误，请按以下步骤操作：

### 方法1：使用最小测试版本
1. 在Unity中创建空场景
2. 添加空物体，挂载 `GameManager` 脚本
3. 确保 `GameManager` 的 `useMinimalTest` 选项为 `true`
4. 运行游戏，在Console查看测试结果

### 方法2：如果有DOTween相关错误
- 将 `GameUI.cs` 中的 `CardUI[]` 改为 `CardUISimple[]`
- 或者从Unity Package Manager导入DOTween

### 方法3：完全独立测试
- 直接使用 `MinimalGameTest` 组件进行核心功能测试
- 添加到任意GameObject上即可运行

## 游戏机制
- **顾客属性**: 每个顾客有4个属性 - 财富(wealth)、关系(rela)、理智(sanity)、力量(power)
- **卡牌系统**: 每次为顾客占卜时从卡牌库随机抽取4张卡
- **正逆位**: 玩家可以点击卡牌使其翻转，改变效果
- **满意度**: 如果占卜结果提升了顾客想要的属性，顾客会支付金钱

## 当前实现功能

### 1. 核心系统
- ✅ CSV配置系统（卡牌、顾客、日程、符文、符印、升级）
- ✅ 卡牌抽取和效果计算
- ✅ 顾客管理和满意度判断
- ✅ 游戏状态管理（天数、金钱、进度）

### 2. 特殊卡牌效果
- ✅ 月亮卡 - 正位减半负面效果，逆位减半正面效果
- ✅ 其他卡牌 - 属性数值修改

### 3. UI界面
- ✅ 基础UI组件（CardUI, GameUI）
- ✅ 卡牌翻转动画（使用DOTween）
- ✅ 调试界面和控制

## 文件结构

### CSV配置文件 (Assets/Resources/csv/)
- `card.csv` - 卡牌数据（已有5张卡）
- `customer.csv` - 顾客数据（已有3个顾客）
- `day.csv` - 每日顾客安排
- `rune.csv` - 符文配置
- `sigil.csv` - 符印配置
- `upgrade.csv` - 升级配置

### 核心脚本 (Assets/Scripts/)
- `GameData.cs` - 数据结构定义
- `CSVLoader.cs` - 配置加载器（已扩展）
- `GameSystem.cs` - 游戏状态管理
- `CardSystem.cs` - 卡牌系统
- `GameUI.cs` - UI管理器
- `CardUI.cs` - 卡牌UI组件
- `TestGameSetup.cs` - 测试和调试工具
- `GameManager.cs` - 主游戏管理器

## 如何在Unity中设置

### 1. 场景设置
1. 创建新场景
2. 添加GameManager对象，挂载GameManager脚本
3. 可以添加TestGameSetup对象用于调试

### 2. UI设置（可选）
如果要使用完整UI：
1. 创建Canvas
2. 添加GameUI对象，挂载GameUI脚本
3. 创建UI元素并分配到GameUI的公共字段
4. 为卡牌创建CardUI预制体

### 3. 测试控制
当前可以通过以下方式测试：
- **键盘控制**（在GameManager中）：
  - `N` - 下一个顾客
  - `R` - 重新开始游戏
- **键盘控制**（在TestGameSetup中）：
  - `1-4` - 翻转对应卡牌
  - `Space` - 执行占卜
- **调试按钮**（在GameManager的OnGUI中）：
  - "Perform Divination" - 执行占卜
  - "Skip Customer" - 跳过当前顾客

## 游戏流程示例

1. 游戏开始，第1天，第1个顾客进入
2. 从卡牌库抽取4张卡
3. 玩家可以点击卡牌翻转（改变正逆位）
4. 执行占卜，计算效果
5. 判断顾客满意度，获得金钱
6. 下一个顾客，重复流程
7. 一天结束后进入下一天

## 当前CSV数据示例

### 卡牌效果
- **太阳(sun)**: 正位 财富+3,理智+1 | 逆位 财富+1,理智-4
- **恋人(lover)**: 正位 关系+2 | 逆位 关系-2
- **月亮(moon)**: 正位 减半负面效果 | 逆位 减半正面效果

### 顾客需求
- **富人(rich)**: 想要提升财富
- **少女(girl)**: 想要提升关系
- **恶魔(devil)**: 想要提升理智

## 扩展计划

这个demo为后续扩展提供了良好的基础：
- 符文和符印系统（已有数据结构）
- 商店和升级系统
- 更复杂的卡牌效果
- 剧情和对话系统（可与Naninovel集成）
- 更丰富的UI和动画

## 调试信息

游戏运行时会在Console中输出调试信息，包括：
- 占卜结果
- 属性变化
- 顾客满意度
- 金钱变化

在TestGameSetup中还提供了屏幕调试信息显示。