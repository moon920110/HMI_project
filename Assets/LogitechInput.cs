using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogitechInput : MonoBehaviour
{
    static LogitechGSDK.DIJOYSTATE2ENGINES rec;

    public static float GetAxis(string axisName)
    {
        rec = LogitechGSDK.LogiGetStateUnity(0);
        switch (axisName)
        {
            case "Steering Horizontal": return rec.lX / 32760f;
            case "Gas Vertical": return rec.lY / -32760f;
            case "Clutch Vertical": return rec.rglSlider[0] / -32760f;
            case "Brake Vertical": return rec.lRz / -32760f;
        }
        return 0f;
    }


} 