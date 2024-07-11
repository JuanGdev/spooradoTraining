using UnityEngine;

public class ballManager : MonoBehaviour
{
    public FourWallsAgent agent; // Reference to the agent script
    
    private void OnCollisionEnter(Collision other)
    {
        // Check if the ball hits any wall
        if (other.gameObject.transform.parent.CompareTag("Walls"))
        {
            // Determine if the hit wall is the correct one
            bool hitCorrectWall = agent.CheckIfCorrectWall(other.gameObject);

            if (hitCorrectWall)
            {
                agent.AddReward(1f);
                Debug.Log("Hit the correct wall!");
                // Add reward for hitting the correct wall
                agent.RestartEpisode(); // Restart the episode
            }
            else
            {
                Debug.Log("WRONG WALL!");
                agent.AddReward(-1f); // Penalize for hitting the wrong wall
                agent.RestartEpisode();
            }
        }
    }
}