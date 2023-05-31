using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public Transform[] nodes;
    private Vector3 initPos;
    private int prevIdx = 0;

    private CustomCarAI carAI;
    private int initMaxRPM;

    // Start is called before the first frame update
    void Start()
    {
        carAI = GetComponent<CustomCarAI>();
        initMaxRPM = carAI.MaxRPM;
        initPos = transform.position;

        DriveToNextPoint();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DriveToNextPoint();
        KeepDistance();
    }

    int CurrentIndex
    {
        get
        {
            int nearestIndex = -1;
            float nearestDistance = Mathf.Infinity;
            for (int i = 0; i < nodes.Length; i++)
            {
                float distance = Vector3.Distance(nodes[i].position, transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }
    }

    void DriveToNextPoint()
    {
        carAI.CustomDestination = nodes[(CurrentIndex + 1) % nodes.Length];
    }

    void KeepDistance()
    {
        // raycast
        Vector3 source = transform.position;
        source.y = 0.5f;
        Ray ray = new Ray(source, transform.forward);
        RaycastHit hit;

        float raycastDistance = 30f;
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            float distance = hit.distance;

            if (distance <= 4f)
            {
                carAI.MaxRPM = 0;
            }
            else if (distance <= 8f)
            {
                carAI.MaxRPM = Mathf.Max(carAI.MaxRPM - 1, initMaxRPM - 5);
            }
            else
            {
                carAI.MaxRPM = Mathf.Min(carAI.MaxRPM + 1, initMaxRPM);
            }
        }
    }

    public void Reset(Vector3 player)
    {
        gameObject.SetActive(false);

        // reset posistion
        transform.position = new Vector3(player.x + 60f,  0, initPos.z);

        // reset rotation
        transform.rotation = Quaternion.Euler(0, -90, 0);

        // clear waypoints
        carAI.waypoints.Clear();

        // set new destination
        DriveToNextPoint();

        gameObject.SetActive(true);
    }

    private void OnDrawGizmosSelected() // shows a Gizmos representing the waypoints and AI FOV
    {
        // node
        Gizmos.color = Color.yellow;
        for (int i = 0; i < nodes.Length; i++)
        {
            Gizmos.DrawWireSphere(nodes[i].position, 1f);
        }

        // raycast
        Vector3 source = transform.position;
        source.y = 0.5f;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(source, transform.forward * 20f);
    }
}
