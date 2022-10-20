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
    public float maxSpeed = 100f;

    public Vector3 centerOfMass;

    private List<Transform> nodes;
    private int currentNode = 0;
    
    
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;

        Transform[] pathTransform = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();
        

        for (int i = 0; i < pathTransform.Length; i++)
        {
            if (pathTransform[i] != path.transform)
            {
                nodes.Add(pathTransform[i]);
            }
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        ApplySteer();
        Drive();
        CheckWaypointDistance();
    }

    private void ApplySteer()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode].position);
        float newsteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;
        //
        //print(relativeVector);
        WheelFL.steerAngle = newsteer;
        WheelFR.steerAngle = newsteer;
    }

    private void Drive()
    {
        currentSpeed = 2 * Mathf.PI * WheelFL.radius * WheelFL.rpm * 60 / 1000;

        if (currentSpeed < maxSpeed)
        {
            WheelFL.motorTorque = maxMotorTorque;
            WheelFR.motorTorque = maxMotorTorque;
        }
        else
        {
            WheelFL.motorTorque = 0;
            WheelFR.motorTorque = 0;

        }
    }

    private void CheckWaypointDistance()
    {
        if (Vector3.Distance(transform.position, nodes[currentNode].position) < 0.5f) {
            if (currentNode == nodes.Count - 1)
            {
                currentNode = 0;
            }
            else
            {
                currentNode++;
            }
        }
    }
}
