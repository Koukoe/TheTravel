using System;
using System.Collections.Generic;

public enum ItemType
{
    Book,
    Scene,
}

// 物品属性类（可序列化以便储存）
[Serializable]
public class ItemData
{
    public int id;
    public string itemName;
    public ItemType type;
    public int level;
    public float durability; // 耐久度（状态示例）

    // 构造函数
    public ItemData(int id, string name, ItemType type)
    {
        this.id = id;
        this.itemName = name;
        this.type = type;
    }
}