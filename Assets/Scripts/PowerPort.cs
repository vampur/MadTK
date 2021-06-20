using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPort : MonoBehaviour
{
    public GameObject gobjPowerOn, gobjPowerOff;

    [HideInInspector] public bool bPowered;

    List<Collider> lstPowerCubes = new List<Collider>();

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Battery")
            if(!lstPowerCubes.Contains(other))
                lstPowerCubes.Add(other);

    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Battery")
            if(lstPowerCubes.Contains(other))
                lstPowerCubes.Remove(other);
    }

    // Update is called once per frame
    void Update()
    {
        bPowered = 0 < lstPowerCubes.Count;

        gobjPowerOn.SetActive(bPowered);
        gobjPowerOff.SetActive(!bPowered);
    }
}
