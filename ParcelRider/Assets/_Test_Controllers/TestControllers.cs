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

    [Button]public void RiderGetUnassignedOrders()
    {
        LogEvent();
        //RiderOrderController.Do_UpdateAll();
    }

    private void LogEvent([CallerMemberName]string methodName = null) => Debug.Log($"{methodName}: Invoke!");
}
