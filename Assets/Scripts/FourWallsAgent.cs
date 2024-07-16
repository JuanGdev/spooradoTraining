using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class FourWallsAgent : Agent
{
    private Vector3[] initialAgentPosition = new Vector3[4];
    public Material loseMaterial;
    public Material winMaterial;
    public GameObject[] walls = new GameObject[4];
    public float moveSpeed = 10f;
    public Rigidbody ballRigidbody;

    public int totalBallTouches = 0;
    private int indexWall;
    private Rigidbody agentRigidbody;
    //  TO DO:
    //  4 posiciones donde el agente esta alineado con la bola y debe empujarla hacia alguna pared que se encuentre en su dirección
    //  x: 0, z: 7      x: 0, z: -7     x: -7, z:0      x:7, z: 0
    private void Start()
    {
        //  Setup initial Positions
        initialAgentPosition[0] = new Vector3(0f, 0, 7f);    // => Back Wall     0
        initialAgentPosition[1] = new Vector3(0f, 0, -7f);   // => Front Wall    1
        initialAgentPosition[2] = new Vector3(-7f, 0, 0f);   // => Right Wall    2
        initialAgentPosition[3] = new Vector3(7f, 0, 0f);    // => Left Wall     3
        agentRigidbody = GetComponent<Rigidbody>();
        
        indexWall = Random.Range(0, 4);
        transform.position = initialAgentPosition[indexWall];
    }
    
    public override void OnEpisodeBegin()
    {
        //  Reset agent position
        if(transform.localPosition.y < 0)
        {
            agentRigidbody.angularVelocity = Vector3.zero;
            agentRigidbody.velocity = Vector3.zero;
            
            // Calculate the direction to the ball and rotate the agent to face the ball
            Vector3 directionToBall = (ballRigidbody.transform.localPosition - transform.localPosition).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToBall);
            transform.rotation = lookRotation;
            
            indexWall = Random.Range(0, 4);    
            transform.localPosition = initialAgentPosition[indexWall];
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.transform.localPosition = new Vector3(-0.140000001f,0,-2.25999999f);
        }
        
        if (Mathf.Approximately(transform.localPosition.z, 7f))
        {
            //  agent goes to back wall     0
            indexWall = 0;
            
            //  Paint the back wall with win material and all others with lose materials
            walls[0].GetComponent<MeshRenderer>().material = winMaterial;
            walls[1].GetComponent<MeshRenderer>().material = loseMaterial;
            walls[2].GetComponent<MeshRenderer>().material = loseMaterial;
            walls[3].GetComponent<MeshRenderer>().material = loseMaterial;
        } else if (Mathf.Approximately(transform.localPosition.z, -7f))
        {
            //  agent goes to front wall    1
            indexWall = 1;
            
            //  Paint the front wall with win material and all others with lose materials
            walls[0].GetComponent<MeshRenderer>().material = loseMaterial;
            walls[1].GetComponent<MeshRenderer>().material = winMaterial;
            walls[2].GetComponent<MeshRenderer>().material = loseMaterial;
            walls[3].GetComponent<MeshRenderer>().material = loseMaterial;
        } else if (Mathf.Approximately(transform.localPosition.x, -7f))
        {
            //  agent goes to right wall    2
            indexWall = 2;
            
            //  Paint the right wall with win material and all others with lose materials
            walls[0].GetComponent<MeshRenderer>().material = loseMaterial;
            walls[1].GetComponent<MeshRenderer>().material = loseMaterial;
            walls[2].GetComponent<MeshRenderer>().material = winMaterial;
            walls[3].GetComponent<MeshRenderer>().material = loseMaterial;
        }
        else
        {
            //  agent goes to left wall     3
            indexWall = 3;
            
            //  Paint the left wall with win material and all others with lose materials
            walls[0].GetComponent<MeshRenderer>().material = loseMaterial;
            walls[1].GetComponent<MeshRenderer>().material = loseMaterial;
            walls[2].GetComponent<MeshRenderer>().material = loseMaterial;
            walls[3].GetComponent<MeshRenderer>().material = winMaterial;
        } 
    }
    public bool CheckIfCorrectWall(GameObject wall)
    {
        // Assuming walls array is ordered as [Back, Front, Right, Left]
        // and corresponds to indexWall values 0, 1, 2, 3 respectively
        return wall == walls[indexWall];
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(walls[indexWall].transform.localPosition);
        sensor.AddObservation(agentRigidbody.velocity);
        sensor.AddObservation(ballRigidbody.velocity);
        // Calculate the direction to the ball and rotate the agent to face the ball
        Vector3 directionToBall = (ballRigidbody.transform.localPosition - transform.localPosition).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToBall);
        sensor.AddObservation(lookRotation);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];
        agentRigidbody.MovePosition(transform.position + transform.forward * moveForward*moveSpeed*Time.deltaTime);
        transform.Rotate(0f, moveRotate*(moveSpeed/3f),0f,Space.Self);
    }
    
    private void OnCollisionEnter(Collision other)
    {
        // Check if the collision is with the ball
        if (other.gameObject.CompareTag("Ball"))
        {
            totalBallTouches++;
        }
        else
        {
            // Check if the collision is with the correct wall
            bool hitCorrectWall = false;
            switch (indexWall)
            {
                case 0: hitCorrectWall = other.gameObject.CompareTag("Back Wall"); break;
                case 1: hitCorrectWall = other.gameObject.CompareTag("Front Wall"); break;
                case 2: hitCorrectWall = other.gameObject.CompareTag("Right Wall"); break;
                case 3: hitCorrectWall = other.gameObject.CompareTag("Left Wall"); break;
            }
            if (hitCorrectWall)
            {
                Debug.Log("Agent in the correct wall");
                // Do not restart the episode immediately to allow for observation of the correct action
                RestartEpisode();
            }
            else
            {
                Debug.Log("Agent in the wrong wall");
                RestartEpisode();
            }
        }
    }

    public void RestartEpisode()
    {
        agentRigidbody.angularVelocity = Vector3.zero;
        agentRigidbody.velocity = Vector3.zero;
        indexWall = Random.Range(0, 4);
        transform.localPosition = initialAgentPosition[indexWall];
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.transform.localPosition = new Vector3(-0.140000001f, 0, -2.25999999f);
        totalBallTouches = 0;
        
        // Calculate the direction to the ball and rotate the agent to face the ball
        Vector3 directionToBall = (ballRigidbody.transform.localPosition - transform.localPosition).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToBall);
        transform.rotation = lookRotation;
        
        EndEpisode(); // Restart the episode
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }
}
