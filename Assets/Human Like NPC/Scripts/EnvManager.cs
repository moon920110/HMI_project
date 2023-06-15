using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnvManager : MonoBehaviour
{
    [Header("Background")]
    public Transform terrain;
    public Transform[] roads;
    private Vector3 relativePos;

    [Header("NPC")]
    public Transform[] cars;

    [Header("Player")]
    public Transform player;
    private int prevIndex;

    // Start is called before the first frame update
    void Start()
    {
        relativePos = terrain.position;
        prevIndex = CurrentIndex;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MoveTerrain();
        MoveRoad();
        MoveCar();

        // for debug
        player.position = new Vector3(player.position.x - 0.1f, player.position.y, player.position.z);
    }

    int CurrentIndex
    {
        get
        {
            int nearestIndex = -1;
            float nearestDistance = Mathf.Infinity;
            for (int i = 0; i < roads.Length; i++)
            {
                float distance = Vector3.Distance(roads[i].position + new Vector3(-35, 0, 0), player.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }
    }

    void MoveTerrain()
    {
        terrain.position = new Vector3(player.position.x, 0, player.position.z) + relativePos;
    }

    void MoveRoad()
    {
        if (prevIndex != CurrentIndex)
        {
            prevIndex = CurrentIndex;

            // Move Road position
            int pastIdx = CurrentIndex - 2;
            if (pastIdx < 0) pastIdx += roads.Length;
            int farthestIdx = (pastIdx + roads.Length - 1) % roads.Length;
            roads[pastIdx].position = roads[farthestIdx].position + new Vector3(-70, 0, 0);
        }
    }

    void MoveCar()
    {
        int pastIdx = CurrentIndex - 1;
        if (pastIdx < 0) pastIdx += roads.Length;
        int farthestIdx = (pastIdx + roads.Length - 1) % roads.Length;
        foreach (Transform car in cars)
        {
            bool isNeedReset = false;

            // If fall out
            if (car.position.y < -10)
            {
                isNeedReset = true;
            }
            // If reached end of Roads
            else if (car.position.x < (roads[farthestIdx].position.x - 70 + 20))
            {
                isNeedReset = true;
            }
            else
            {
                // if destination has wrong angle
                Transform target = car.GetComponent<CustomCarAI>().CustomDestination;
                Vector3 distance = (car.position - target.position).normalized;
                Vector3 direction = car.forward;
                float cosAngle = Vector3.Dot(distance, direction);
                float angle = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;
                if (angle < 60)
                {
                    isNeedReset = true;
                }
            }

            // Reset car
            if (isNeedReset)
            {
                car.GetComponent<NPCController>().Reset(player.position);
            }
        }
    }
}
