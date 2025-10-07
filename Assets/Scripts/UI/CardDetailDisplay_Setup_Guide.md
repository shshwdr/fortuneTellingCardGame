# CardDetailDisplay 预制件设置指南

## 创建CardDetailDisplay预制件的步骤

### 1. 创建主要结构
在Unity中创建以下GameObject结构：

```
CardDetailDisplay (CardDetailDisplay script)
└── DetailPanel (Panel with Image background)
    ├── Background (Image - 半透明黑色背景)
    ├── CardImage (Image - 显示卡牌图片)
    ├── ContentArea (Vertical Layout Group)
    │   ├── CardNameText (TextMeshPro - UI)
    │   ├── CardDescriptionText (TextMeshPro - UI)
    │   ├── CurrentStatusText (TextMeshPro - UI)
    │   ├── EffectsArea (Horizontal Layout Group)
    │   │   ├── UprightEffectText (TextMeshPro - UI)
    │   │   └── ReversedEffectText (TextMeshPro - UI)
```

### 2. 组件设置

**CardDetailDisplay脚本引用：**
- detailPanel = DetailPanel
- cardNameText = CardNameText
- cardDescriptionText = CardDescriptionText
- uprightEffectText = UprightEffectText
- reversedEffectText = ReversedEffectText
- cardImage = CardImage
- currentStatusText = CurrentStatusText

### 3. 布局建议

**DetailPanel设置：**
- Anchor: Center
- Size: 600x400 (可调整)
- 背景色：半透明黑色 (0, 0, 0, 0.8)

**CardImage设置：**
- Size: 150x200
- 保持纵横比
- 位置：面板左侧

**文本组件设置：**
- CardNameText: 字体大小24，加粗
- CardDescriptionText: 字体大小16
- CurrentStatusText: 字体大小18，居中
- UprightEffectText: 字体大小14，绿色调
- ReversedEffectText: 字体大小14，红色调

### 4. 放置位置
将CardDetailDisplay作为Canvas的子对象，确保它在其他UI元素之上。

### 5. 初始状态
DetailPanel应该在开始时设置为不活跃（SetActive(false)），这样游戏开始时不会显示。

## 使用方法

一旦设置完成，CardUISimple组件会自动处理鼠标悬停事件：
- 鼠标移到卡牌上时显示详细信息
- 鼠标离开卡牌时隐藏详细信息
- 显示正位和逆位两种效果
- 高亮显示当前卡牌状态

## 注意事项

1. 确保场景中只有一个CardDetailDisplay实例
2. CardDetailDisplay使用单例模式，会自动查找场景中的实例
3. 如果找不到CardDetailDisplay组件，会在Console中显示警告但不会报错
4. 支持富文本格式，可以使用颜色标签美化显示效果