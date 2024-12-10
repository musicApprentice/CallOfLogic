using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.UIElements;

// functionality of virus
// animates color and size of the virus
// and attacks the player if the player is near the virus (check the code)
public class Virus : MonoBehaviour
{
    private GameObject fps_player_obj;
    private Level level;
    private float radius_of_search_for_player;
    private float virus_speed;

    private Utils u;
	void Start ()
    {
        u = new Utils();
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
        fps_player_obj = level.fps_player_obj;
        Bounds bounds = level.GetComponent<Collider>().bounds;
        radius_of_search_for_player = (bounds.size.x + bounds.size.z) / 10.0f;
        virus_speed = level.virus_speed;
    }

    // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION TO ANIMATE THE VIRUS ***
    // so that it moves towards the player when the player is within radius_of_search_for_player
    // a simple strategy is to update the position of the virus
    // so that it moves towards the direction d=v/||v||, where v=(fps_player_obj.transform.position - transform.position)
    // with rate of change (virus_speed * Time.deltaTime)
    // make also sure that the virus y-coordinate position does not go above the wall height
    void Update()
    {
        if (level.player_health < 0.001f || level.player_entered_house)
            return;
        Color redness = new Color
        {
            r = Mathf.Max(1.0f, 0.25f + Mathf.Abs(Mathf.Sin(2.0f * Time.time)))
        };
        if ( transform.childCount > 0)
            transform.GetChild(0).GetComponent<MeshRenderer>().material.color = redness;
        else
            transform.GetComponent<MeshRenderer>().material.color = redness;
        transform.localScale = new Vector3(
                               0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time)), 
                               0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time)), 
                               0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time))
                               );
        // /*** implement the rest ! */
        // Vector3 direction = fps_player_obj.transform.position - transform.position;
        // Vector3 newPos = transform.position;
        // if (direction.magnitude <= radius_of_search_for_player) {
        //     newPos += direction.normalized * virus_speed * Time.deltaTime;
        // }
        // newPos.y = Mathf.Min(newPos.y, level.storey_height);  // Constrain y-coordinate
        // transform.position = newPos;
        // Debug.Log("Position debug Start");
        // Debug.Log(u.pos_to_grid(level.bounds, fps_player_obj.transform.position.x, fps_player_obj.transform.position.z, level.width, level.length));
        // Debug.Log($"{fps_player_obj.transform.position.x}, {fps_player_obj.transform.position.z}");
        // Debug.Log("Position debug paused");
        (int npcx, int npcy) = u.pos_to_grid(level.bounds, transform.position.x, transform.position.z, level.width, level.length);
        (int playerx, int playery) = u.pos_to_grid(level.bounds, fps_player_obj.transform.position.x, fps_player_obj.transform.position.y, level.width, level.length);
        List<int[]> path = u.shortestPath(level.previous_grid, new int[] {npcx, npcy}, new int[] {playerx, playery});


        if (path.Count == 0) {
            return;
        }
        // foreach(int[] pos in path) {
        //     Debug.Log("" + pos[0] + ", " + pos[1]);
        // }
        // Debug.Log("End: " + u.coord2Str(new int[]{playerx, playery}));
        
        (float x, float z) = u.grid_to_pos(level.bounds, path[1][0], path[1][1], level.width, level.length);
        Vector3 next_grid_pos = new Vector3();
        next_grid_pos.x = x;
        next_grid_pos.z = z;
        next_grid_pos.y = fps_player_obj.transform.position.y;
        

        Vector3 direction = (next_grid_pos - transform.position).normalized;
        // Debug.Log(u.coord2Str(path[1]));

        transform.position = transform.position + direction * virus_speed * Time.deltaTime;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "PLAYER")
        {
            if (!level.virus_landed_on_player_recently)
                level.timestamp_virus_landed = Time.time;
            level.num_virus_hit_concurrently++;
            level.virus_landed_on_player_recently = true;
            level.source.PlayOneShot(level.hitSound);
            Destroy(gameObject);
        }
    }
    
}
