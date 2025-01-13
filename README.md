# SimpleAnimation

This is a sample that shows how to use Playable Graphs to animate objects in a manner similar to the Animation Component. 

## This repository is archived. The last compatible version tested is Unity 2018.3, though we expect the code to still be compatible with Unity 2019.3.

---

# 目录结构
- PlayableApiTest ：官方文档中一些playable的示例测试，其中添加了一种上下半身蒙版的的示例（https://docs.unity3d.com/cn/2021.2/Manual/Playables-Examples.html）
- Playable，参考（https://github.com/IrisFenrir/Fenrir-RPG）
- SimpleAnimationComponent，官方示例（https://github.com/Unity-Technologies/SimpleAnimation）
- unity-chan!，官方商城免费模型资源（https://assetstore.unity.com/packages/3d/characters/unity-chan-model-18705?srsltid=AfmBOoq3OkSIU76oukGNnijomCN9oqfh8Gs3z0LR2we7twSSM1v9-UlA）
- Plugins：
    - Sirenix，odin编辑器插件（https://assetstore.unity.com/publishers/3727?srsltid=AfmBOoqCdiSlX8nhxAllwZJV2SNy0Gz3SABzKR-cjWQid-wrvOxK0kve）
    - graph-visualizer，官方可视化插件（https://github.com/Unity-Technologies/graph-visualizer）
    - UnityPlayableGraphMonitorTool-main，第三方可视化插件（https://github.com/SolarianZ/UnityPlayableGraphMonitorTool/tree/main）

#学习记录
- 帧率的加权计算方式 TestUI.cs
- IEnumerable<T> 集合的构建与使用 SimpleAnimation_impl.cs 
- IEquatable<T> PlayableHandle
- CustomEditor，CustomPropertyDrawer，EditorGUI SimpleAnimationEditor.cs
- StateInfo来封装状态字段
- StateManagement List的空闲位添加对象
- 通过 partial 来分散定义类
- bool的 |= 赋值运算 UpdateStates
- 单元测试