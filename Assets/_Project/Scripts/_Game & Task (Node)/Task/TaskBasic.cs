using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TaskBasic : MonoBehaviour
{
    public bool isDone;
    public abstract IEnumerator TaskIEnumerator();
}
