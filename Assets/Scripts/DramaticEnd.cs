using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DramaticEnd : MonoBehaviour
{
    MeshRenderer meshRenderer;
    public PowerPort powerPort;
    public List<GameObject> lstOnPowerActivate;
    
    public List<GameObject> lstOnTriggerActivate;
    public List<GameObject> lstOnTriggerDeactivate;    

    bool bPrevState = true;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().bBlockInput = true;
            foreach(var gobj in lstOnTriggerActivate)
                gobj.SetActive(true);
            foreach(var gobj in lstOnTriggerDeactivate)
                gobj.SetActive(false);
            UnityEngine.SceneManagement.SceneManager.LoadScene("EndScene");
        }
    }

    void Update()
    {
        if(powerPort.bPowered == bPrevState)
            return;
        bPrevState = powerPort.bPowered;

        meshRenderer.enabled = powerPort.bPowered;
        foreach(var gobj in lstOnPowerActivate)
            gobj.SetActive(powerPort.bPowered);
    }
}
