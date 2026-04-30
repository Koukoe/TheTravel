using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Runtime.Serialization;
using Microsoft.VisualBasic;

public class BookPanel : BasePanel
{

    public override void OnResume()
    {
        base.OnResume();
        InputManager.Instance.SwitchUIMode(false);
    }
}