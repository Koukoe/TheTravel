using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class TaskBasic : MonoBehaviour
{
    public bool isDone;
    public abstract UniTask TaskIEnumerator();
}
