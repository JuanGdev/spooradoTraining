using UnityEngine;

public class ballManager : MonoBehaviour
{
    public FourWallsAgent agent; // Reference to the agent script
    public MeshRenderer floorRenderer; // Reference to the floor renderer
    public Material loseMaterial; // Material to show when the agent loses
    public Material winMaterial; // Material to show when the agent wins
    private void OnCollisionEnter(Collision other)
    {
        // Check if the ball hits any wall
        if (other.gameObject.transform.parent.CompareTag("Walls"))
        {
            // Determine if the hit wall is the correct one
            bool hitCorrectWall = agent.CheckIfCorrectWall(other.gameObject);

            if (hitCorrectWall)
            {
                agent.AddReward(1+(1f/agent.totalBallTouches));
                // Reward for hitting the correct wall
                Debug.Log("Hit the correct wall!");
                floorRenderer.material = winMaterial; // Change the floor material
                
                agent.RestartEpisode(); // Restart the episode
            }
            else
            {
                floorRenderer.material = loseMaterial; // Change the floor material
                Debug.Log("WRONG WALL!");
                agent.AddReward(-agent.totalBallTouches-1); // Penalize for hitting the wrong wall
                agent.RestartEpisode();
            }
        }
    }
}