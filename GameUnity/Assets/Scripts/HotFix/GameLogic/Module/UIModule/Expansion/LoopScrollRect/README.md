# LoopScrollRect DGame 接入说明

项目同时保留 LoopScrollRect 1.1.5 与 SuperScrollView。LoopScrollRect 运行时代码编译到
可热更新的 `GameLogic` 程序集，自定义 Inspector 则保留在 `Assets/Plugins` 下。

## 单模板列表

先定义具体的 Item Widget 类型，使列表项接入 DGame UI 生命周期：

```csharp
public sealed class BagLoopItem : SelectItemBase, IListDataItem<ItemData>
{
    public void SetItemData(ItemData data)
    {
        // 根据数据刷新 Item 节点。
    }
}

private UILoopScrollWidget<BagLoopItem, ItemData> m_loopList;

protected override void OnCreate()
{
    m_loopList = CreateWidget<UILoopScrollWidget<BagLoopItem, ItemData>>("m_tfLoopList");
    m_loopList.BaseItemPrefab = m_goItemTemplate;
}

protected override void OnRefresh()
{
    m_loopList.SetDatas(m_items);
}
```

目标节点需要挂载 `LoopVerticalScrollRect` 或 `LoopHorizontalScrollRect` 组件。
固定 Grid 列表同样使用这个封装，并在 Content 节点配置 LoopScrollRect 支持的
`GridLayoutGroup` 约束。

## 多模板列表

多模板列表使用 `UILoopScrollMultiWidget<TItem, TData>`。调用 `SetDatas` 前，需要先通过
`SetItemPrefabs` 设置模板集合，再通过 `SetItemPrefabIndexProvider` 提供“逻辑 Item 索引
到模板索引”的选择函数。

两个封装的 `GetItem(index)` 都按当前可见 Item 的逻辑索引查询，不使用对象池中的物理
顺序。池中的物理 Item 仍然是 DGame 子 Widget，会在父 UI 销毁时统一销毁。
