using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// functionality of virus
// animates color and size of the virus
// and attacks the player if the player is near the virus (check the code)
public class NPC : MonoBehaviour
{
    private GameObject fps_player_obj;
    private Level level1;
    private float radius_of_search_for_player;
    //private float virus_speed;

	void Start ()
    {
        //GameObject level_obj = GameObject.FindGameObjectWithTag("FirstLevel");
        //level1 = level_obj.GetComponent<FirstLevel>();
        //if (level == null)
        //{
        //    Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
        //    return;
        //}
        //fps_player_obj = level1.fps_player_obj;
        //Bounds bounds = level1.GetComponent<Collider>().bounds;
        //radius_of_search_for_player = (bounds.size.x + bounds.size.z) / 10.0f;
        //virus_speed = level1.virus_speed;
    }

    // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION TO ANIMATE THE VIRUS ***
    // so that it moves towards the player when the player is within radius_of_search_for_player
    // a simple strategy is to update the position of the virus
    // so that it moves towards the direction d=v/||v||, where v=(fps_player_obj.transform.position - transform.position)
    // with rate of change (virus_speed * Time.deltaTime)
    // make also sure that the virus y-coordinate position does not go above the wall height
    void Update()
    {
        // TO IMPLEMENT FREEZE AND INVINCIBLE LOGIC
        // if(level1.frozen)
        // {
        //     return;
        // }
        // if(level1.invincible)
        // {
        //     //implement slowing down
        // }
        // if(level1.currentLevel == 1)
        // {
        //     //change ai settings
        // }
        // else if (level1.currentLevel == 2)
        // {
        //      //change ai settings
        // }
        //if (level.player_health < 0.001f || level.player_entered_house)
        //    return;
        //Color redness = new Color
        //{
        //    r = Mathf.Max(1.0f, 0.25f + Mathf.Abs(Mathf.Sin(2.0f * Time.time)))
        //};
        //if ( transform.childCount > 0)
        //    transform.GetChild(0).GetComponent<MeshRenderer>().material.color = redness;
        //else
        //    transform.GetComponent<MeshRenderer>().material.color = redness;
        //transform.localScale = new Vector3(
        //                       0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time)), 
        //                       0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time)), 
        //                       0.9f + 0.2f * Mathf.Abs(Mathf.Sin(4.0f * Time.time))
        //                       );
        ///*** implement the rest ! */
        //Vector3 curDif = fps_player_obj.transform.position - transform.position;
        //Vector3 newpos = transform.position;
        //if (curDif.magnitude <= radius_of_search_for_player)
        //{
        //    Vector3 dir = curDif.normalized;
        //    newpos += virus_speed * dir * Time.deltaTime;
        //}
        //newpos.y = Mathf.Min(newpos.y, level.storey_height);
        //transform.position = newpos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if(level1.invincible)
        // {
        //     //implement no effect on touching
        // }

        //if (collision.gameObject.name == "PLAYER")
        //{
        //    if (level.source.isPlaying)
        //    {
        //        level.source.Stop();
        //    }
        //    level.source.PlayOneShot(level.collide_with_virus);
        //    if (!level.virus_landed_on_player_recently)
        //        level.timestamp_virus_landed = Time.time;
        //    level.num_virus_hit_concurrently++;
        //    level.virus_landed_on_player_recently = true;
        //    Destroy(gameObject);
        //}
    }
    
}
