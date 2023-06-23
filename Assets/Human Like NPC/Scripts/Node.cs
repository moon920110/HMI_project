using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [field: SerializeField] public LaneType LaneNumber { get; private set; }
}


public enum LaneType { Left=1, Right }
