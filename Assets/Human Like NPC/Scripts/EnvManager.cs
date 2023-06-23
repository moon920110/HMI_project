using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class EnvManager : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform player;
    public Transform playerGoal;
    [field: SerializeField] public int PlayerLaneNumber { get; private set; }

    [Header("Path Settings")]
    public List<Node> Nodes = new List<Node>();

    [Header("NPC Settings")]
    public GameObject rightCarPrefab;
    public GameObject leftCarPrefab;
    [Tooltip("The index of left node to spawn")]
    public List<int> spawnLeftIndex = new List<int>();
    [Tooltip("The index of right node to spawn")]
    public List<int> spawnRightIndex = new List<int>();
    private List<Transform> cars = new List<Transform>();

    public static EnvManager Instance { get; private set; }

    private void Awake() => Instance = this;

    private void Start()
    {
        SpawnCars();
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RescuePlayer();
        }

        CheckTermination();
        CheckLaneNumber();
    }

    private void SpawnCars()
    {
        foreach (int i in spawnLeftIndex)
        {
            SpawnNPC(LaneType.Left, i, null);
        }
        foreach (int i in spawnRightIndex)
        {
            SpawnNPC(LaneType.Right, i, null);
        }

        void SpawnNPC(LaneType laneType, int nodeIdx, int? speed)
        {
            List<Node> nodes = Nodes.FindAll(node => node.LaneNumber == laneType);

            // Instatiate NPC
            GameObject carPrefab = (laneType == LaneType.Right) ? rightCarPrefab : leftCarPrefab;
            Transform car = Instantiate(carPrefab, transform).transform;
            for (int i = 0; i < nodes.Count; i++)
            {
                car.GetComponent<NPCController>().nodes.Add(nodes[i].transform);
            }

            car.position = nodes[nodeIdx].transform.position;
            car.LookAt(nodes[nodeIdx + 1].transform.position);
            if (speed != null)
                car.GetComponent<CustomCarAI>().MaxRPM = (int)speed;
            cars.Add(car);
        }
    }

    private void CheckTermination()
    {
        // check if player's position entered the mesh renderer of playerGoal
        if (playerGoal.GetComponent<MeshRenderer>().bounds.Contains(player.position))
        {
            EndGame();
        }
    }

    private int FindClosestNode()
    {
        // find the closest node
        float minDistance = Mathf.Infinity;
        int minIndex = -1;
        for (int i = 0; i < Nodes.Count; i++)
        {
            float distance = Vector3.Distance(player.position, Nodes[i].transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                minIndex = i;
            }
        }
        return minIndex;
    }

    private void CheckLaneNumber()
    {
        int closestIdx = FindClosestNode();
        PlayerLaneNumber = (int)Nodes[closestIdx].LaneNumber;
    }

    private void EndGame()
    {
        CarEngine carEngine = player.GetComponent<CarEngine>();
        if (carEngine == null)
        {
            return;
        }

        // Ghostify player
        {
            carEngine.enabled = false;

            // deactivate player rigidbody
            player.GetComponent<Rigidbody>().isKinematic = true;

            // deactivate player collider
            player.GetComponentsInChildren<Collider>().ToList().ForEach(collider => collider.enabled = false);
        }

        // Show end message
        {
            // find canvas
            Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

            // show end message
            GameObject endMessage = new GameObject("EndMessage");
            endMessage.transform.parent = canvas.transform;
            endMessage.AddComponent<RectTransform>();
            endMessage.AddComponent<UnityEngine.UI.Text>();
            endMessage.GetComponent<UnityEngine.UI.Text>().text = "Congratulations on completing the track!";
            endMessage.GetComponent<UnityEngine.UI.Text>().font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            endMessage.GetComponent<UnityEngine.UI.Text>().fontSize = 40;
            endMessage.GetComponent<UnityEngine.UI.Text>().color = Color.red;
            endMessage.GetComponent<UnityEngine.UI.Text>().alignment = TextAnchor.MiddleCenter;
            endMessage.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 100);
            endMessage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }

        // deactivate this script
        this.enabled = false;
    }

    public void RescuePlayer()
    {
        int closestIdx = FindClosestNode();

        // until there is no NPC nearby the closest node
        while (cars.Any(car => Vector3.Distance(player.position, Nodes[closestIdx].transform.position) < 10))
        {
            closestIdx = (closestIdx - 1 + Nodes.Count) % Nodes.Count;
        }

        player.position = Nodes[closestIdx].transform.position;
        player.LookAt(Nodes[closestIdx + 1].transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw spawning positions of NPC
        List<Node> nodeLefts = Nodes.FindAll(node => node.LaneNumber == LaneType.Left);
        foreach (int i in spawnLeftIndex)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(nodeLefts[i].transform.position, 1f);
        }
        List<Node> nodeRights = Nodes.FindAll(node => node.LaneNumber == LaneType.Right);
        foreach (int i in spawnRightIndex)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(nodeRights[i].transform.position, 1f);
        }
    }
}
