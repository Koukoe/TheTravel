# README

## 拓扑图基本介绍

---

![alt text](image.png)

如上图所示，这就是一个最简单的拓扑图，也叫 DAG (即有向无环图)。

而每一个节点表示一个任务节点，一个节点如果想被启用，当且仅当它的所有前置节点都已经被执行完毕。

所以在这里我们使用拓扑图来存储任务节点中的依存关系。
(注意这里的依存关系并不局限于具体任务，只要你希望两个状态是要有先后顺序的，都要用 TaskNode 来存储)

---

## TaskNode

---

![alt text](image-1.png)

看上图是 TaskNode 在 Inspector 中的表现形式，首先我们刚需的便是为每个TaskNode 填入 TaskId，TaskId 是一个唯一的标识符。

然后我们需要在 NextNodesIds 中填入该 TaskNode 的所有后置节点的 TaskId，根据第一张图片的例子，我们要在TaskId为0的TaskNode的NextNodesIds中填入1，如果是在TaskId为2的TaskNode的NextNodesIds中填入3和4。

然后是TaskEffects，这些是在当前TaskNode启动后更改的状态，如图所示

![alt text](image-2.png)

EffectType有三种情况，分别是可交互物体，普通物体，和Actor，这些都会有特定的GUID，GUID也是必须的，因为我们需要通过GUID来找到这些物体，然后更改他们的状态。

后面的三个类只需要根据你选的类型去填即可，建议将你选的类的内容全部填上，不然无法保证会不会出现问题。

注意，所有的TaskEffect都会在任务完成后撤销执行，如果你希望在任务完成后更改物体的状态，请在TaskEndEffect中添加，逻辑和TaskEffect相同。
**特别的，如果你希望使用脚本自动控制一些物体比如调整摄像机位置，播放动画，自动对话等，请在后文的TaskGoals里进行设置**

最后是TaskGoals

![alt text](image-4.png)

TaskGoals被分为五类，分别是TRIGGER,ITEM,ACTOR,DIALOGUE,SCRIPT

TRIGGER，ITEM，ACTOR均是检查对应物体状态来判断任务是否完成，如果物体状态符合要求，则任务完成。

而DIALOGUE则是检查对应对话是否播放完毕，如果对话播放完毕，则任务完成。

注意这些检查均为自动定时检测

最后是SCRIPT，这个是用于自动执行任务逻辑如播放对话等，如果你希望使用此选项，那么你需要写一个脚本，继承自TaskBasic，并实现TaskBasic中的方法，这里的检测方式是在脚本中实现的抽象UniTask方法中更改isDone布尔变量，当isDone为true时，任务完成。(当然你需要在Inspector中拖入这个脚本，建议直接挂载在对应TaskNode下方)

![alt text](image-3.png)

这里提供一个例子，这个脚本会在任务开始时播放一段对话，并在对话播放完毕后自动完成任务。
