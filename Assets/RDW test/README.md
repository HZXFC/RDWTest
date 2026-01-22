# 阈值测定实验脚本说明

本目录包含了用于8-10岁儿童虚拟现实重定向行走感知阈值测定实验的核心脚本。

## 脚本结构

### 核心脚本

1. **StaircaseController.cs** - 1-up/2-down 自适应阶梯法控制器
   - 实现心理物理学阈值测定的自适应阶梯算法
   - 支持catch trial检测
   - 自动收敛于70.7%探测概率阈值点

2. **ThresholdRotationRedirector.cs** - 旋转增益阈值测定专用Redirector
   - 精确控制旋转增益值（gR）
   - 继承自Redirector基类
   - 用于旋转增益阈值测定实验

3. **ThresholdCurvatureRedirector.cs** - 曲率增益阈值测定专用Redirector
   - 基于S2C算法，精确控制曲率半径（r）
   - 继承自SteerToRedirector
   - 用于曲率增益阈值测定实验

4. **ThresholdExperimentManager.cs** - 实验流程管理器
   - 协调staircase、redirector、UI和数据记录
   - 管理实验状态和流程
   - 处理用户响应和试次控制

5. **ThresholdUI.cs** - 2AFC用户界面控制器
   - 实现二选一强制选择界面
   - 儿童友好设计（支持表情图标）
   - 处理用户响应收集

6. **ThresholdDataLogger.cs** - 数据记录器
   - 扩展StatisticsLogger功能
   - 专门记录staircase实验数据
   - 导出CSV格式数据

## 使用说明

### 基本设置步骤

1. **在场景中创建实验管理器GameObject**
   - 创建一个空的GameObject，命名为"ThresholdExperimentManager"
   - 添加 `ThresholdExperimentManager` 组件

2. **配置实验参数**
   - 在Inspector中设置实验条件（Rotation或Curvature）
   - 设置年龄组（0=儿童，1=成人）
   - 设置参与者ID
   - 配置staircase参数（初始增益、步长等）

3. **添加必要的组件引用**
   - StaircaseController: 创建GameObject并添加组件
   - Redirector: 根据条件选择ThresholdRotationRedirector或ThresholdCurvatureRedirector
   - ThresholdDataLogger: 创建GameObject并添加组件
   - ThresholdUI: 创建GameObject并添加组件（需要Canvas）

4. **配置Redirector**
   - 对于旋转任务：添加 `ThresholdRotationRedirector` 到Redirected Avatar
   - 对于曲率任务：添加 `ThresholdCurvatureRedirector` 到Redirected Avatar
   - 在RedirectionManager中选择对应的Redirector类型

5. **设置UI**
   - 创建Canvas（如果还没有）
   - 创建2AFC响应按钮（根据条件显示对应的按钮）
   - 将按钮连接到ThresholdUI组件
   - 设置状态文本显示

### 实验流程

1. **初始化**: 调用 `ThresholdExperimentManager.InitializeExperiment()`
2. **开始试次**: 调用 `ThresholdExperimentManager.StartNextTrial()`
3. **试次完成**: 调用 `ThresholdExperimentManager.OnTrialComplete()`
4. **用户响应**: 通过UI调用 `ThresholdExperimentManager.SubmitUserResponse()`
5. **实验完成**: 自动计算阈值并保存数据

### 数据记录

- 数据保存在 `项目根目录/ThresholdExperimentResults/参与者ID_时间戳/` 目录下
- 每个条件生成一个CSV文件，包含所有试次数据
- 汇总数据保存在summary.csv文件中

## 参数说明

### 旋转增益参数
- 初始增益 (initialGain): 1.5
- 初始步长 (initialStepSize): 0.05
- 精细步长 (fineStepSize): 0.02

### 曲率增益参数
- 初始半径 (initialRadius): 6.0米
- 初始步长 (initialStepSize): 0.04米（对应2.3度/米）
- 精细步长 (fineStepSize): 0.01米（对应0.6度/米）

### Staircase参数
- 最大试次 (maxTrials): 25
- 最小反转次数 (minReversals): 6
- Catch trial概率 (catchTrialProbability): 0.1 (10%)

## 注意事项

1. **Redirector集成**: 需要将ThresholdRedirector添加到RedirectionManager的Redirector选择中
2. **UI依赖**: ThresholdUI需要TextMeshPro支持（项目已包含）
3. **数据记录**: 确保有写入权限，数据会自动保存
4. **实验流程**: 需要手动调用StartNextTrial()来开始试次，并根据任务完成情况调用OnTrialComplete()

## 后续开发建议

1. 创建专门的实验场景
2. 实现自动化的试次控制（根据任务完成自动触发）
3. 添加更多的UI反馈（如进度条、动画等）
4. 实现SSQ量表的集成
5. 添加实验暂停和恢复功能
