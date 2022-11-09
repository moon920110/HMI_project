using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
    public Transform path;
    public float maxSteerAngle=45f;

    public WheelCollider WheelFL;
    public WheelCollider WheelFR;

    public float maxMotorTorque = 80f;
    public float currentSpeed;
    public float maxSpeed = 1000f;

    public Vector3 centerOfMass;

    private List<Transform> nodes;

    private float gasInput;
    private float brakeInput;
    private float steerInput;


    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;
        print("LogiSteeringInitialize: " + LogitechGSDK.LogiSteeringInitialize(false));
    }

    // Update is called once per frame
    private void Update()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected((int)LogitechKeyCodes.FirstIndex))
        {
            steerInput = GetAxis(AxisName.SteeringHorizontal);
            gasInput = GetAxis(AxisName.GasVertical);
            brakeInput = GetAxis(AxisName.BrakeVertical);
        }
        ApplySteer();
        Drive();
    }

    private void ApplySteer()
    {
        float newsteer = steerInput * maxSteerAngle;
        //
        //print(relativeVector);
        WheelFL.steerAngle = newsteer;
        WheelFR.steerAngle = newsteer;
    }

    private void Drive()
    {
        currentSpeed = 2 * Mathf.PI * WheelFL.radius * WheelFL.rpm * 60 / 1000;
        if (currentSpeed < 0)
        {
            currentSpeed = 0f;
        }

        // TODO: brake implementation
        float throttle = (gasInput - brakeInput * 3f) * maxSpeed ;

        if (currentSpeed == 0f && throttle < 0)
        {
            throttle = 0f;
        }
        print($"{currentSpeed}, {throttle}");
        WheelFL.motorTorque = currentSpeed >= 0f ? throttle : 0;
        WheelFR.motorTorque = currentSpeed >= 0f ? throttle : 0;
   }

        private static float GetAxis(AxisName axisName)
    {
        LogitechGSDK.DIJOYSTATE2ENGINES rec;
        rec = LogitechGSDK.LogiGetStateUnity(0);

        switch(axisName)
        {
            case AxisName.SteeringHorizontal: return rec.lX / (float)Int16.MaxValue;
            case AxisName.GasVertical: return (rec.lY - Int16.MaxValue) / (float)Int16.MinValue;
            case AxisName.BrakeVertical: return (rec.rglSlider[0] - Int16.MaxValue) / (float)Int16.MinValue;
        }
        return 0f;
    }
}

enum AxisName
    {
        SteeringHorizontal,
        GasVertical,
        ClutchVertical,
        BrakeVertical,
    }
