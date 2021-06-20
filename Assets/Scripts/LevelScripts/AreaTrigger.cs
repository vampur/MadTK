using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    public string szTrigger;
    public LayerMask lyrTarget;
    public bool bOnceOff;

    bool bDone;

    void OnTriggerEnter(Collider other)
    {
        if(bDone)
            return;
        if((lyrTarget.value & 1 << other.gameObject.layer) != 0)
        {
            GetComponentInParent<Animator>().SetTrigger(szTrigger);
            bDone = bOnceOff;
        }
    }
}
