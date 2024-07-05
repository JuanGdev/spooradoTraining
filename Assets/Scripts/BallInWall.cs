using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInWall : MonoBehaviour
{
    public AgentScript agentScript;
    public MeshRenderer floorRenderer;
    public Material loseMaterial;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Ball"))
        {
            agentScript.AddReward(-0.1f);
            floorRenderer.material = loseMaterial;
            agentScript.EndEpisode();
        }
    }
}
