using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentScript : Agent
{
     //   Observations variables
     public Rigidbody ballRigidbody;
     private Rigidbody agentRigidbody;
     public Transform ballTransform;
     public Transform holeTransform;
     public MeshRenderer floorRenderer;
     public Material loseMaterial;
     public float maxSpeed = 10f;
     public Vector3[] initialBallPosition = new Vector3[4];
     private void Start()
     {
          agentRigidbody = GetComponent<Rigidbody>();
          initialBallPosition[0] = new Vector3(6, 0.25f, 5);
          initialBallPosition[1] = new Vector3(-6, 0.25f, 5);
          initialBallPosition[2] = new Vector3(6, 0.25f, -5);
          initialBallPosition[3] = new Vector3(-6, 0.25f, -5);
     }
     public void isBallInside(float reward)
     {
          AddReward(reward);
     }

     public override void OnEpisodeBegin()
     {
          // If the Agent fell, zero its momentum
          if (transform.localPosition.y < 0)
          {
               agentRigidbody.angularVelocity = Vector3.zero;
               agentRigidbody.velocity = Vector3.zero;
               // Move the target to a new spot
               transform.localPosition = new Vector3(UnityEngine.Random.value * 4 ,
                    0.35f,
                    UnityEngine.Random.value * 4);         
          }
          
          // Move the ball to a new spot
          if (transform.localPosition != ballTransform.localPosition)
          {
               // Array de posibles posiciones
               // [6,0.25,5]
               // [-6,0.25,5]
               // [6,0.25,-5]
               // [-6,0.25,-5]
               ballRigidbody.angularVelocity = Vector3.zero;
               ballRigidbody.velocity = Vector3.zero;
               ballTransform.localPosition = initialBallPosition[UnityEngine.Random.Range(0, 4)];
          }
     }

     public override void CollectObservations(VectorSensor sensor)
     {
          // Agent, Ball and Hole positions => 9 values
          sensor.AddObservation(this.transform.localPosition);
          sensor.AddObservation(ballTransform.localPosition);
          sensor.AddObservation(holeTransform.localPosition);
          
          // Agent velocity => 6 values
          sensor.AddObservation(agentRigidbody.velocity);
          sensor.AddObservation(ballRigidbody.velocity);
          
          // Calculate direction vectors
          Vector3 directionFromBallToHole = (holeTransform.position - ballTransform.position);
          //Debug.Log("Direction from ball to hole: " + directionFromBallToHole);
          sensor.AddObservation(directionFromBallToHole);
          
          float distanceFromBallToHole = Vector3.Distance(holeTransform.position, ballTransform.position);
          //Debug.Log("Distance from ball to hole: " + distanceFromBallToHole);
          sensor.AddObservation(distanceFromBallToHole);
          
          Vector3 directionFromAgentToBall = (ballTransform.position - this.transform.position);
          //Debug.Log("Direction from agent to ball: " + directionFromAgentToBall);
          sensor.AddObservation(directionFromAgentToBall);
          
          // Calculate angles using the forward vector as a reference
          float angleFromHoleVecToBallDirVec = Vector3.Angle(ballRigidbody.velocity, directionFromBallToHole);
          //Debug.Log("Angle From Hole to Ball: " + angleFromHoleVecToBallDirVec);
          sensor.AddObservation(angleFromHoleVecToBallDirVec);
     }
     
     public float forceMultiplier = 10;
     public override void OnActionReceived(ActionBuffers actions)
     {
          // Actions, size = 2
          Vector3 controlSignal = Vector3.zero;
          controlSignal.x = actions.ContinuousActions[0];
          controlSignal.z = actions.ContinuousActions[1];
          if (agentRigidbody.velocity.magnitude < maxSpeed)
          {
               agentRigidbody.AddForce(controlSignal * forceMultiplier);
          }
     }

     private void OnTriggerEnter(Collider other)
     {
          if (other.gameObject.CompareTag("Walls"))
          {
               agentRigidbody.angularVelocity = Vector3.zero;
               agentRigidbody.velocity = Vector3.zero;
               
               // Move the target to a new spot
               transform.localPosition = new Vector3(UnityEngine.Random.value * 6 ,
                    0.35f,
                    UnityEngine.Random.value * 6);
               SetReward(-0.5f);
               floorRenderer.material = loseMaterial;
               EndEpisode();
          }
     }

     private void OnCollisionEnter(Collision other)
     {
          if(other.gameObject.CompareTag("Ball"))
          {
               AddReward(0.7f);
          }
     }

     public override void Heuristic(in ActionBuffers actionsOut)
     {
          var continuousActions = actionsOut.ContinuousActions;
          float moveX = Input.GetAxis("Horizontal");
          float moveZ = Input.GetAxis("Vertical");

          Vector3 force = new Vector3(moveX, 0, moveZ) * 10; // Adjust the multiplier as needed for desired force strength
          agentRigidbody.AddForce(force);
     }
}