using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onoff : MonoBehaviour
{
    public GameObject VA;
    public GameObject CAR;


    private void FixedUpdate()
    {
        eHMIONOFF();
    }

    private void eHMIONOFF()
    {
        
        if (CAR.transform.position.z <717)
        {
            VA.SetActive(true);
        }
        else
        {
            VA.SetActive(false);
        }
    }
}
