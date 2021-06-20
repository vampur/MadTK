using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerDoor : MonoBehaviour
{
    public PowerPort powerPort;
    public Transform trnfDestination;
    public float flSmooth = 10f;

    Vector3 vecOrigin;
    Vector3 vecTarget;
    Vector3 vecDestination;

    // Start is called before the first frame update
    void Start()
    {
       vecOrigin = transform.position;
       vecTarget = trnfDestination.position;
    }

    void FixedUpdate()
    {
        vecDestination = powerPort.bPowered ? vecTarget : vecOrigin;
        Vector3 vecDelta = vecDestination - transform.position;
        vecDelta /= flSmooth;
        transform.position += vecDelta;
    }
}
