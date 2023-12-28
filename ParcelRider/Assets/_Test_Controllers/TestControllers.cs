using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AOT.Controllers;
using AOT.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestControllers : MonoBehaviour
{
    private RiderOrderController RiderOrderController => App.GetController<RiderOrderController>();
    private UserOrderController UserOrderController => App.GetController<UserOrderController>();

    [Button]public void SynchronizeOrders()
    {
        CheckSupport();
        RiderOrderController.Do_Sync_History();
    }

    private void CheckSupport([CallerMemberName] string methodName = null)
    {
        LogEvent(methodName);
        if(App.IsTestMode) Debug.LogError("不支持测试模式!");
        if(!Application.isPlaying) Debug.LogError("请运行App!");
    }

    private void LogEvent([CallerMemberName]string methodName = null) => Debug.Log($"{methodName}: Invoke!");
}
