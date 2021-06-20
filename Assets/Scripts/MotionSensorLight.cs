using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSensorLight : MonoBehaviour
{
    public List<GameObject> lstLightObjs = new List<GameObject>();
    
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
            foreach(var gobjLight in lstLightObjs)
                gobjLight.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
            foreach(var gobjLight in lstLightObjs)
                gobjLight.SetActive(false);
    }
}
