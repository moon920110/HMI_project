using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CustomCarAI : MonoBehaviour
{
    [Header("Car Wheels (Wheel Collider)")]// Assign wheel Colliders through the inspector
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider backLeft;
    public WheelCollider backRight;

    [Header("Car Wheels (Transform)")]// Assign wheel Transform(Mesh render) through the inspector
    public Transform wheelFL;
    public Transform wheelFR;
    public Transform wheelBL;
    public Transform wheelBR;

    [Header("Car Front (Transform)")]// Assign a Gameobject representing the front of the car
    public Transform carFront;

    [Header("General Parameters")]// Look at the documentation for a detailed explanation 
    public List<string> NavMeshLayers;
    public int MaxSteeringAngle = 45;
    public int MaxRPM = 150;

    [Header("Debug")]
    public bool ShowGizmos;
    public bool Debugger;

    [Header("Destination Parameters")]// Look at the documentation for a detailed explanation
    public Transform CustomDestination;

    [HideInInspector] public bool move;// Look at the documentation for a detailed explanation

    private Vector3 PostionToFollow = Vector3.zero;
    private int currentWayPoint;
    private float AIFOV = 360; //60;
    private bool allowMovement;
    public List<Vector3> waypoints = new List<Vector3>();
    private float LocalMaxSpeed;
    private float MovementTorque = 1;

    void Awake()
    {
        currentWayPoint = 0;
        allowMovement = true;
        move = true;
    }

    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
    }

    void FixedUpdate()
    {
        UpdateWheels();
        ApplySteering();
        PathProgress();
    }

    private void PathProgress() //Checks if the agent has reached the currentWayPoint or not. If yes, it will assign the next waypoint as the currentWayPoint depending on the input
    {
        wayPointManager();
        Movement();
        ListOptimizer();

        void wayPointManager()
        {
            if (currentWayPoint >= waypoints.Count)
                allowMovement = false;
            else
            {
                PostionToFollow = waypoints[currentWayPoint];
                allowMovement = true;
                if (Vector3.Distance(carFront.position, PostionToFollow) < 2)
                    currentWayPoint++;
            }

            if (currentWayPoint >= waypoints.Count - 3)
                CreatePath();
        }

        void CreatePath()
        {
            if (CustomDestination != null)
                CustomPath(CustomDestination);
        }

        void ListOptimizer()
        {
            if (currentWayPoint > 1 && waypoints.Count > 30)
            {
                waypoints.RemoveAt(0);
                currentWayPoint--;
            }
        }
    }

    public void CustomPath(Transform destination) //Creates a path to the Custom destination
    {
        Vector3 sourcePostion;

        if (waypoints.Count == 0)
        {
            sourcePostion = carFront.position;
            Calculate(destination.position, sourcePostion, carFront.forward);
        }
        else
        {
            sourcePostion = waypoints[waypoints.Count - 1];
            Vector3 direction = (waypoints[waypoints.Count - 1] - waypoints[waypoints.Count - 2]).normalized;
            Calculate(destination.position, sourcePostion, direction);
        }

        void Calculate(Vector3 destination, Vector3 sourcePostion, Vector3 direction)
        {
            if (CheckForAngle(destination, sourcePostion, direction))
            {
                Vector3[] targetPosition = { sourcePostion, destination + new Vector3(0f, 0.3f, 0f) };
                waypoints.AddRange(targetPosition);
            }
        }
    }

    private bool CheckForAngle(Vector3 pos, Vector3 source, Vector3 direction) //calculates the angle between the car and the waypoint 
    {
        Vector3 distance = (pos - source).normalized;
        float CosAngle = Vector3.Dot(distance, direction);
        float Angle = Mathf.Acos(CosAngle) * Mathf.Rad2Deg;

        if (Angle < AIFOV)
            return true;
        else
            return false;
    }

    private void ApplyBrakes() // Apply brake torque 
    {
        frontLeft.brakeTorque = 5000;
        frontRight.brakeTorque = 5000;
        backLeft.brakeTorque = 5000;
        backRight.brakeTorque = 5000;
    }


    private void UpdateWheels() // Updates the wheel's postion and rotation
    {
        ApplyRotationAndPostion(frontLeft, wheelFL);
        ApplyRotationAndPostion(frontRight, wheelFR);
        ApplyRotationAndPostion(backLeft, wheelBL);
        ApplyRotationAndPostion(backRight, wheelBR);
    }

    private void ApplyRotationAndPostion(WheelCollider targetWheel, Transform wheel) // Updates the wheel's postion and rotation
    {
        targetWheel.ConfigureVehicleSubsteps(5, 12, 15);

        Vector3 pos;
        Quaternion rot;
        targetWheel.GetWorldPose(out pos, out rot);
        wheel.position = pos;
        wheel.rotation = rot;
    }

    void ApplySteering() // Applies steering to the Current waypoint
    {
        Vector3 relativeVector = transform.InverseTransformPoint(PostionToFollow);
        float SteeringAngle = (relativeVector.x / relativeVector.magnitude) * MaxSteeringAngle;
        if (SteeringAngle > 15) LocalMaxSpeed = 100;
        else LocalMaxSpeed = MaxRPM;

        frontLeft.steerAngle = SteeringAngle;
        frontRight.steerAngle = SteeringAngle;
    }

    void Movement() // moves the car forward and backward depending on the input
    {
        if (move == true && allowMovement == true)
            allowMovement = true;
        else
            allowMovement = false;

        if (allowMovement == true)
        {
            frontLeft.brakeTorque = 0;
            frontRight.brakeTorque = 0;
            backLeft.brakeTorque = 0;
            backRight.brakeTorque = 0;

            int SpeedOfWheels = (int)((frontLeft.rpm + frontRight.rpm + backLeft.rpm + backRight.rpm) / 4);

            if (SpeedOfWheels < LocalMaxSpeed)
            {
                backRight.motorTorque = 400 * MovementTorque;
                backLeft.motorTorque = 400 * MovementTorque;
                frontRight.motorTorque = 400 * MovementTorque;
                frontLeft.motorTorque = 400 * MovementTorque;
            }
            else if (SpeedOfWheels < LocalMaxSpeed + (LocalMaxSpeed * 1 / 4))
            {
                backRight.motorTorque = 0;
                backLeft.motorTorque = 0;
                frontRight.motorTorque = 0;
                frontLeft.motorTorque = 0;
            }
            else
                ApplyBrakes();

        }
        else
            ApplyBrakes();
    }

    void debug(string text, bool IsCritical)
    {
        if (Debugger)
        {
            if (IsCritical)
                Debug.LogError(text);
            else
                Debug.Log(text);
        }
    }

    private void OnDrawGizmos() // shows a Gizmos representing the waypoints and AI FOV
    {
        if (ShowGizmos == true)
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (i == currentWayPoint)
                    Gizmos.color = Color.blue;
                else
                {
                    if (i > currentWayPoint)
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.green;
                }
                Gizmos.DrawWireSphere(waypoints[i], 2f);
            }
            CalculateFOV();
        }

        void CalculateFOV()
        {
            Gizmos.color = Color.white;
            float totalFOV = AIFOV * 2;
            float rayRange = 10.0f;
            float halfFOV = totalFOV / 2.0f;
            Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
            Vector3 leftRayDirection = leftRayRotation * transform.forward;
            Vector3 rightRayDirection = rightRayRotation * transform.forward;
            Gizmos.DrawRay(carFront.position, leftRayDirection * rayRange);
            Gizmos.DrawRay(carFront.position, rightRayDirection * rayRange);
        }
    }
}
