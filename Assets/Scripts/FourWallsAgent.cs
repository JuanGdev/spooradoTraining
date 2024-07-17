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

    public Vector3 startAgentPosition;
    public Vector3 startBallPosition;
    //  TO DO:
    //  4 posiciones donde el agente esta alineado con la bola y debe empujarla hacia alguna pared que se encuentre en su dirección
    //  x: 0, z: 7      x: 0, z: -7     x: -7, z:0      x:7, z: 0
    private void Start()
    {
        //  Setup initial Positions
        initialAgentPosition[0] = new Vector3(Random.Range(0,4), 0, Random.Range(0,4)); 
        initialAgentPosition[1] = new Vector3(Random.Range(0,4), 0, Random.Range(0,4));
        initialAgentPosition[2] = new Vector3(Random.Range(0,4), 0, Random.Range(0,4));
        initialAgentPosition[3] = new Vector3(Random.Range(0,4), 0, Random.Range(0,4)); 
        agentRigidbody = GetComponent<Rigidbody>();
        startAgentPosition = transform.localPosition;
        startBallPosition = ballRigidbody.transform.localPosition;
        indexWall = Random.Range(0, 4);
        transform.position += initialAgentPosition[indexWall];
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
            transform.localPosition += initialAgentPosition[indexWall];
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.transform.localPosition += new Vector3(-0.140000001f,0,-2.25999999f);
        }
        SetRandomCorrectWall();
    }
    
    private void SetRandomCorrectWall()
    {
        // Randomly select one wall as the correct wall
        indexWall = Random.Range(0, walls.Length);

        // Loop through all walls and assign materials based on whether they are the correct wall
        for (int i = 0; i < walls.Length; i++)
        {
            walls[i].GetComponent<MeshRenderer>().material = (i == indexWall) ? winMaterial : loseMaterial;
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
                //RestartEpisode();
            }
            else
            {
                Debug.Log("Agent in the wrong wall");
                AddReward(-0.1f);
                //RestartEpisode();
            }
        }
    }

    public void RestartEpisode()
    {
        initialAgentPosition[0] = new Vector3(Random.Range(0,6), 0, Random.Range(0,6)); 
        initialAgentPosition[1] = new Vector3(Random.Range(0,6), 0, Random.Range(0,6));
        initialAgentPosition[2] = new Vector3(Random.Range(0,6), 0, Random.Range(0,6));
        initialAgentPosition[3] = new Vector3(Random.Range(0,6), 0, Random.Range(0,6)); 
        agentRigidbody.angularVelocity = Vector3.zero;
        agentRigidbody.velocity = Vector3.zero;
        indexWall = Random.Range(0, 4);
        transform.localPosition = startAgentPosition + initialAgentPosition[indexWall];
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.transform.localPosition = startBallPosition+ new Vector3(-0.140000001f, 0, -2.25999999f);
        if (totalBallTouches == 0)
        {
            SetReward(-1);
        }
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
