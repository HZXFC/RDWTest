# AudioListener 警告问题说明

## 问题原因

Unity场景中有**4个AudioListener组件**同时启用，但Unity要求场景中**只能有一个AudioListener**处于活动状态。

### 场景中的AudioListener位置：

1. ✅ **Main Camera** - 主相机（保留启用）
2. ❌ **Real Top View Cam** - 顶部视图相机（已禁用）
3. ❌ **3rd Person View** - 第三人称视图相机（已禁用）
4. ❌ **1st Person View** - 第一人称视图相机（已禁用）

## 为什么会出现多个AudioListener？

在Unity中，当创建Camera时，默认会自动添加AudioListener组件。如果场景中有多个Camera（例如用于不同视角的观察相机），每个Camera都会带有AudioListener，导致警告。

## 解决方案

**已自动修复**：已禁用其他3个相机的AudioListener组件，只保留Main Camera的AudioListener。

## 如何手动修复（如果将来遇到类似问题）

1. 在Hierarchy窗口中找到所有带有Camera的GameObject
2. 检查每个Camera的Inspector
3. 找到AudioListener组件
4. 对于非主相机，取消勾选AudioListener组件的**Enabled**复选框
5. 只保留主相机（Main Camera）的AudioListener启用

## 验证

运行场景后，Console中应该不再出现"多个AudioListener"的警告。

## 注意事项

- 如果使用XR（VR/AR），通常XR Origin中的Main Camera应该保留AudioListener
- 其他用于观察、调试的相机应该禁用AudioListener
- 如果需要在运行时切换相机，确保在切换时也管理AudioListener的启用/禁用状态
