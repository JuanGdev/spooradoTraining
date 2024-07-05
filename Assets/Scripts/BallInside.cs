using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BallInside : MonoBehaviour
{
    public AgentScript agentScript;
    public MeshRenderer floorRenderer;
    public Material winMaterial;
    public Material loseMaterial;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            agentScript.isBallInside(1f);
            floorRenderer.material = winMaterial;
            agentScript.EndEpisode();
        }
        else if(other.gameObject.CompareTag("Agent"))
        {
            agentScript.isBallInside(-1f);
            floorRenderer.material = loseMaterial;
            agentScript.EndEpisode();
        }
    }
}
