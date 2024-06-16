using UnityEngine;
using System;
using scsmmedia;

public class FlightStickController : MonoBehaviour
{
    [SerializeField] public SSCInputBridge inputBridge;

    void Update()
    {
        
    }

    void Awake()
    {
        if (inputBridge == null)
        {
            Debug.LogError("FlightStickController: inputBridge is not set.");
        }

        if (Settings.Instance == null)
        {
            Debug.LogError("FlightStickController: Settings.Instance is null.");
        }

        if (Settings.Instance.IsLeverOn())
        {
            inputBridge.inputAxisX = SSCInputBridge.InputAxis.Roll;
            inputBridge.inputAxisZ = SSCInputBridge.InputAxis.Pitch;
            inputBridge.inputAxisXInt = SSCInputBridge.InputAxisRollInt;
            inputBridge.inputAxisZInt = SSCInputBridge.InputAxisPitchInt;
        }
        else
        {
            inputBridge.inputAxisX = SSCInputBridge.InputAxis.None;
            inputBridge.inputAxisZ = SSCInputBridge.InputAxis.None;
            inputBridge.inputAxisXInt = SSCInputBridge.InputAxisNoneInt;
            inputBridge.inputAxisZInt = SSCInputBridge.InputAxisNoneInt;
        }
    }
}