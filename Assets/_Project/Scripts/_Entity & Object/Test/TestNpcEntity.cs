using UnityEngine;

public class TestNpcEntity : PoolStateEntity<ActorState>
{
    [SerializeField, Tooltip("测试用 NPC GUID, 对话效果通过此 GUID 索引")]
    private string testGuid = string.Empty;

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(testGuid))
        {
            Debug.LogWarning($"[TestNpcEntity] {name} 未配置 testGuid, 跳过自动绑定");
            return;
        }

        // BindState 内部通过 GetState<ActorState> 自行创建/获取 DataArchive 中的 state
        BindState(testGuid);
    }

    protected override void OnStateBound()
    {
        Debug.Log($"[TestNpcEntity] {_state.name}({_guid}) 已绑定, 位置={transform.position}, 可见={gameObject.activeSelf}");
    }
}