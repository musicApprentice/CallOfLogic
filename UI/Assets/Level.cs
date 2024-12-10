using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.UIElements.UxmlAttributeDescription;

enum TileType
{
    WALL = 0,
    FLOOR = 1,
    NPC = 2,
}
// public static class GlobalVariables
// {
//     public static int playerScore = 0;
// }


public class PriorityQueue<T>
{
    SortedSet<PriorityQueueObject<T>> queue;


    int a = 0;

    public PriorityQueue() { queue = new SortedSet<PriorityQueueObject<T>>(); }

    public void Add(T obj, int priority)
    {
        queue.Add(new PriorityQueueObject<T>(obj, priority, a++));
    }

    public T Peak()
    {
        var o = queue.ElementAt(0);
        return o.obj;
    }
    public T Pop()
    {
        var o = queue.ElementAt(0);
        queue.Remove(o);
        return o.obj;
    }

    public int Count()
    {
        return queue.Count;
    }

}


public class PriorityQueueObject<T> : IComparable<PriorityQueueObject<T>>
{
    public T obj { get; set; }
    public int priority { get; set; }
    public int ex { get; set; }

    public PriorityQueueObject(T obj, int priority, int ex)
    {
        this.obj = obj;
        this.priority = priority;
        this.ex = ex;
    }

    public int CompareTo(PriorityQueueObject<T> other)
    {
        int order = priority.CompareTo(other.priority);
        if (order == 0) return ex.CompareTo(other.ex);
        return order;

    }
}

public class Level: MonoBehaviour
{
    public int currentLevel;
    public int width = 16;   // size of level (default 16 x 16 blocks)
    public int length = 16;
    public float storey_height = 2.5f;   // height of walls
    public float npc_speed = 3.0f;     // npc velocity
    public GameObject claire_prefab;
    public GameObject npc_prefab;
    //public GameObject target_prefab;
    public GameObject text_box;
    public Button freezeButton;
    internal bool frozen;
    public Button invinceButton;
    internal bool invincible;
    internal bool player_entered_house;

    // fields/variables accessible from other scripts
    internal GameObject fps_player_obj;   // instance of FPS template
    public GameObject house_prefab;

    // fields/variables needed only from this script
    private Bounds bounds;                   // size of ground plane in world space coordinates 
    // private float timestamp_last_msg = 0.0f; // timestamp used to record when last message on GUI happened (after 7 sec, default msg appears)
    private int function_calls = 0;          // number of function calls during backtracking for solving the CSP
    private int num_npcs = 0;             // number of npcs in the level
    private List<int[]> pos_npcs;         // stores their location in the grid
    private List<TileType>[,] grid;


    // a helper function that UnityEngine.Randomly shuffles the elements of a list (useful to UnityEngine.Randomize the solution to the CSP)
    private void Shuffle<T>(ref List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {   
        if (currentLevel == 2)
        {
            width = 20;
            length = 20;
        }

        // freezeButton.onClick.AddListener(freeze);
        // freezeButton.gameObject.SetActive(true);
        // frozen = false;

        // invinceButton.onClick.AddListener(invince);
        // invinceButton.gameObject.SetActive(true);
        // invincible = false;

        player_entered_house = false;

        // initialize internal/private variables
        bounds = GetComponent<Collider>().bounds;
        //timestamp_last_msg = 0.0f;
        function_calls = 0;
        num_npcs = 0;

        // initialize 2D grid
        grid = new List<TileType>[width, length];
        // useful to keep variables that are unassigned so far
        List<int[]> unassigned = new List<int[]>();

        num_npcs = width * length / 25 + 1; // at least one virus will be added
        pos_npcs = new List<int[]>();

        // create the wall perimeter of the level, and let the interior as unassigned
        // then try to assign variables to satisfy all constraints
        // *rarely* it might be impossible to satisfy all constraints due to initialization
        // in this case of no success, we'll restart the UnityEngine.Random initialization and try to re-solve the CSP
        bool success = false;
        while (!success)
        {
            for (int v = 0; v < num_npcs; v++)
            {
                while (true) // try until virus placement is successful (unlikely that there will no places)
                {
                    // try a UnityEngine.Random location in the grid
                    int wr = UnityEngine.Random.Range(1, width - 1);
                    int lr = UnityEngine.Random.Range(1, length - 1);

                    // if grid location is empty/free, place it there
                    if (grid[wr, lr] == null)
                    {
                        grid[wr, lr] = new List<TileType> { TileType.NPC };
                        pos_npcs.Add(new int[2] { wr, lr });
                        break;
                    }
                }
            }

            for (int w = 0; w < width; w++)
                for (int l = 0; l < length; l++)
                    if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                        grid[w, l] = new List<TileType> { TileType.WALL };
                    else
                    {
                        if (grid[w, l] == null) // does not have virus already or some other assignment from previous run
                        {
                            // CSP will involve assigning variables to one of the following four values (VIRUS is predefined for some tiles)
                            List<TileType> candidate_assignments = new List<TileType> { TileType.WALL, TileType.FLOOR, TileType.NPC };
                            Shuffle<TileType>(ref candidate_assignments);
                            
                            grid[w, l] = candidate_assignments;
                            unassigned.Add(new int[] { w, l });
                        }
                    }

            // YOU MUST IMPLEMENT this function!!!
            success = BackTrackingSearch(grid, unassigned);
            if (!success)
            {
                Debug.Log("Could not find valid solution - will try again");
                unassigned.Clear();
                grid = new List<TileType>[width, length];
                function_calls = 0;
            }
        }

        DrawDungeon(grid);
    }

    // one type of constraint already implemented for you
    bool DoWeHaveTooManyInteriorWalls(List<TileType>[,] grid)
    {
        int[] number_of_assigned_elements = new int[] { 0, 0, 0 };
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                if (grid[w, l].Count == 1)
                    number_of_assigned_elements[(int)grid[w, l][0]]++;
            }

        if ((number_of_assigned_elements[(int)TileType.WALL] > num_npcs * 10))
            return true;
        else
            return false;
    }

    // another type of constraint already implemented for you
    bool DoWeHaveTooFewWalls(List<TileType>[,] grid)
    {
        int[] number_of_potential_assignments = new int[] { 0, 0, 0 };
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                for (int i = 0; i < grid[w, l].Count; i++)
                    number_of_potential_assignments[(int)grid[w, l][i]]++;
            }

        if ((number_of_potential_assignments[(int)TileType.WALL] < (width * length) / 4))
            return true;
        else
            return false;
    }

    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // must return true if there are three (or more) interior consecutive wall blocks either horizontally or vertically
    // by interior, we mean walls that do not belong to the perimeter of the grid
    // e.g., a grid configuration: "FLOOR - WALL - WALL - WALL - FLOOR" is not valid
    bool TooLongWall(List<TileType>[,] grid)
    {
        /*** implement the rest ! */
        // Check horizontal walls
        for (int w = 1; w < width - 1; w++)
        {
            int consecutive = 0;
            for (int l = 1; l < length - 1; l++)
            {
                if (grid[w, l][0] == TileType.WALL)
                    consecutive++;
                else
                    consecutive = 0;

                if (consecutive >= 3)
                    return true;
            }
        }

        // Check vertical walls
        for (int l = 1; l < length - 1; l++)
        {
            int consecutive = 0;
            for (int w = 1; w < width - 1; w++)
            {
                if (grid[w, l][0] == TileType.WALL)
                    consecutive++;
                else
                    consecutive = 0;

                if (consecutive >= 3)
                    return true;
            }
        }

        return false; // No consecutive walls found
    }

    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // must return true if there is no WALL adjacent to a virus 
    // adjacency means left, right, top, bottom, and *diagonal* blocks
    bool NoWallsCloseToNPCS(List<TileType>[,] grid)
    {
        /*** implement the rest ! */
        for (int i = 0; i < pos_npcs.Count; i++)
        {
            int[] curPos = pos_npcs[i];
            if (grid[curPos[0] + 1, curPos[1]][0] == TileType.WALL
                || grid[curPos[0] + 1, curPos[1] + 1][0] == TileType.WALL
                || grid[curPos[0] + 1, curPos[1] - 1][0] == TileType.WALL
                || grid[curPos[0], curPos[1] + 1][0] == TileType.WALL
                || grid[curPos[0], curPos[1] - 1][0] == TileType.WALL
                || grid[curPos[0] - 1, curPos[1] + 1][0] == TileType.WALL
                || grid[curPos[0] - 1, curPos[1]][0] == TileType.WALL
                || grid[curPos[0] - 1, curPos[1] - 1][0] == TileType.WALL)
            {
                return false;
            }
        }
        return true;
    }


    // check if attempted assignment is consistent with the constraints or not
    bool CheckConsistency(List<TileType>[,] grid, int[] cell_pos, TileType t)
    {
        int w = cell_pos[0];
        int l = cell_pos[1];

        List<TileType> old_assignment = new List<TileType>();
        old_assignment.AddRange(grid[w, l]);
        grid[w, l] = new List<TileType> { t };

        // note that we negate the functions here i.e., check if we are consistent with the constraints we want
        bool areWeConsistent = !DoWeHaveTooFewWalls(grid) && !DoWeHaveTooManyInteriorWalls(grid)
                            && !TooLongWall(grid) && !NoWallsCloseToNPCS (grid);

        grid[w, l] = new List<TileType>();
        grid[w, l].AddRange(old_assignment);
        return areWeConsistent;
    }


    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // implement backtracking 
    bool BackTrackingSearch(List<TileType>[,] grid, List<int[]> unassigned)
    {
        // if there are too many recursive function evaluations, then backtracking has become too slow (or constraints cannot be satisfied)
        // to provide a reasonable amount of time to start the level, we put a limit on the total number of recursive calls
        // if the number of calls exceed the limit, then it's better to try a different initialization
        if (function_calls++ > 100000)
            return false;

        // we are done!
        if (unassigned.Count == 0)
            return true;

        int[] cell = unassigned[unassigned.Count - 1]; // Pick the last unassigned cell
        unassigned.RemoveAt(unassigned.Count - 1);
        function_calls++;

        /*** implement the rest ! */
        foreach (TileType t in grid[cell[0], cell[1]]) // Try all possible assignments
        {
            if (CheckConsistency(grid, cell, t)) // Check if consistent
            {
                grid[cell[0], cell[1]] = new List<TileType> { t }; // Assign tile
                if (BackTrackingSearch(grid, unassigned)) // Recur
                    return true;
            }
        }

        unassigned.Add(cell); // Backtrack
        //grid[cell[0], cell[1]].Clear(); // Clear assignment
        return false;
    }

    string toStr(int[] coord)
    {
        return "" + coord[0] + " " + coord[1];
    }

    List<int[]> dijkstra(List<TileType>[,] grid, int[] start, int[] dst)
    {
        int[,] dist = new int[grid.GetLength(0), grid.GetLength(1)];
        bool[,] explored = new bool[grid.GetLength(0), grid.GetLength(1)];
        Dictionary<string, int[]> parent = new Dictionary<string, int[]>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < length; j++)
            {
                dist[i, j] = int.MaxValue;
                explored[i, j] = false;
            }
        }

        PriorityQueue<int[]> frontier = new PriorityQueue<int[]>();
        frontier.Add(new int[] { start[0], start[1] }, 0);
        dist[start[0], start[1]] = 0;
        while (frontier.Count() > 0)
        {
            int[] curElem = frontier.Pop();
            if (explored[curElem[0], curElem[1]])
            {
                continue;
            }
            //parent[toStr(new int[] { curElem[0], curElem[1] })] = new int[] { curElem[2], curElem[3] };
            if (curElem[0] == dst[0] && curElem[1] == dst[1])
            {
                Debug.Log("Destination found");
                break;
            }
            explored[curElem[0], curElem[1]] = true;
            for (int di = -1; di <= 1; ++di)
            {
                for (int dj = -1; dj <= 1; ++dj)
                {
                    if (Mathf.Abs(di) == Mathf.Abs(dj))
                    {
                        continue;
                    }
                    int[] neighbor = new int[] { curElem[0] + di, curElem[1] + dj };
                    if (neighbor[0] < 0 || neighbor[1] < 0 || neighbor[0] > width - 1 || neighbor[1] > length - 1)
                    {
                        continue;
                    }
                    //if (curElem[0] == start[0] && curElem[1] == start[1])
                    //{
                    //    Debug.Log(toStr(neighbor));
                    //}
                    if (explored[neighbor[0], neighbor[1]])
                    {
                        continue;
                    }
                    int cost = 0;
                    if (grid[neighbor[0], neighbor[1]][0] == TileType.WALL)
                    {
                        cost = 1;
                        if (neighbor[0] == 0 || neighbor[0] == width - 1 || neighbor[1] == 0 || neighbor[1] == length - 1)
                        {
                            cost = 100;
                        }
                    }
                    int currentDist = dist[curElem[0], curElem[1]] + cost;
                    if (currentDist < dist[neighbor[0], neighbor[1]])
                    {
                        dist[neighbor[0], neighbor[1]] = currentDist;
                        parent[toStr(neighbor)] = new int[] { curElem[0], curElem[1] };
                        //if (curElem[0] == start[0] && curElem[1] == start[1])
                        //{
                        //    Debug.Log(toStr(neighbor));
                        //    Debug.Log(toStr(parent[toStr(neighbor)]));
                        //}
                        frontier.Add(new int[] { neighbor[0], neighbor[1] }, dist[curElem[0], curElem[1]] + cost);
                    }
                }
            }
        }
        //Debug.Log(toStr(dst));
        Debug.Log("START");
        List<int[]> route = new List<int[]>();
        int[] curNode = dst;
        while (curNode[0] != start[0] || curNode[1] != start[1])
        {
            Debug.Log(toStr(curNode));
            route.Add(curNode);
            curNode = parent[toStr(curNode)];
        }
        //Debug.Log("mark" + toStr(parent[toStr(route.Last())]));
        //Debug.Log("mark" + toStr(parent[toStr(parent[ toStr( route.Last() )])]));
        //Debug.Log(toStr(start));
        return route;
    }

    void DrawDungeon(List<TileType>[,] solution)
    {
        GetComponent<Renderer>().material.color = Color.grey; // ground plane will be grey

        // place character at UnityEngine.Random position (wr, lr) in terms of grid coordinates (integers)
        // make sure that this UnityEngine.Random position is a FLOOR tile (not wall, drug, or virus)
        int wr = 0;
        int lr = 0;
        while (true) // try until a valid position is sampled
        {
            wr = UnityEngine.Random.Range(1, width - 1);
            lr = UnityEngine.Random.Range(1, length - 1);

            if (solution[wr, lr][0] == TileType.FLOOR)
            {
                float x = bounds.min[0] + (float)wr * (bounds.size[0] / (float)width);
                float z = bounds.min[2] + (float)lr * (bounds.size[2] / (float)length);
                fps_player_obj = Instantiate(claire_prefab);
                fps_player_obj.name = "PLAYER";
                // character is placed above the level so that in the beginning, he appears to fall down onto the maze
                fps_player_obj.transform.position = new Vector3(x + 0.5f, 2.0f * storey_height, z + 0.5f);
                break;
            }
        }

        // place an exit from the maze at location (wee, lee) in terms of grid coordinates (integers)
        // destroy the wall segment there - the grid will be used to place a house
        // the exist will be placed as far as away from the character (yet, with some UnityEngine.Randomness, so that it's not always located at the corners)
        int max_dist = -1;
        int wee = -1;
        int lee = -1;
        while (true) // try until a valid position is sampled
        {
            if (wee != -1)
                break;
            for (int we = 0; we < width; we++)
            {
                for (int le = 0; le < length; le++)
                {
                    // skip corners
                    if (we == 0 && le == 0)
                        continue;
                    if (we == 0 && le == length - 1)
                        continue;
                    if (we == width - 1 && le == 0)
                        continue;
                    if (we == width - 1 && le == length - 1)
                        continue;

                    if (we == 0 || le == 0 || wee == length - 1 || lee == length - 1)
                    {
                        // UnityEngine.Randomize selection
                        if (UnityEngine.Random.Range(0.0f, 1.0f) < 0.1f)
                        {
                            int dist = System.Math.Abs(wr - we) + System.Math.Abs(lr - le);
                            if (dist > max_dist) // must be placed far away from the player
                            {
                                wee = we;
                                lee = le;
                                max_dist = dist;
                            }
                        }
                    }
                }
            }
        }


        // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION  ***
        // implement an algorithm that checks whether
        // all paths between the player at (wr,lr) and the exit (wee, lee)
        // are blocked by walls. i.e., there's no way to get to the exit!
        // if this is the case, you must guarantee that there is at least 
        // one accessible path (any path) from the initial player position to the exit
        // by removing a few wall blocks (removing all of them is not acceptable!)
        // this is done as a post-processing step after the CSP solution.
        // It might be case that some constraints might be violated by this
        // post-processing step - this is OK.

        /*** implement what is described above ! */
        // use dijkstra algorithms
        int debugcount = 0;
        List<int[]> path = dijkstra(solution, new int[] { wr, lr }, new int[] { wee, lee });
        foreach (int[] block in path)
        {
            //Debug.Log(toStr(block));
            if (solution[block[0], block[1]][0] == TileType.WALL)
            {
                debugcount++;
                solution[block[0], block[1]][0] = TileType.FLOOR;
            }
        }
        //Debug.Log(debugcount);
        //Debug.Log(wr + " " + lr);

        // the rest of the code creates the scenery based on the grid state 
        // you don't need to modify this code (unless you want to replace the virus
        // or other prefabs with something else you like)
        int w = 0;
        for (float x = bounds.min[0]; x < bounds.max[0]; x += bounds.size[0] / (float)width - 1e-6f, w++)
        {
            int l = 0;
            for (float z = bounds.min[2]; z < bounds.max[2]; z += bounds.size[2] / (float)length - 1e-6f, l++)
            {
                if ((w >= width) || (l >= width))
                    continue;

                float y = bounds.min[1];
                //Debug.Log(w + " " + l + " " + h);
                if ((w == wee) && (l == lee)) // this is the exit
                {
                    GameObject house = Instantiate(house_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    house.name = "HOUSE";
                    house.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                    if (l == 0)
                        house.transform.Rotate(0.0f, 270.0f, 0.0f);
                    else if (w == 0)
                        house.transform.Rotate(0.0f, 0.0f, 0.0f);
                    else if (l == length - 1)
                        house.transform.Rotate(0.0f, 90.0f, 0.0f);
                    else if (w == width - 1)
                        house.transform.Rotate(0.0f, 180.0f, 0.0f);

                    house.AddComponent<BoxCollider>();
                    house.GetComponent<BoxCollider>().isTrigger = true;
                    house.GetComponent<BoxCollider>().size = new Vector3(3.0f, 3.0f, 3.0f);
                    house.AddComponent<House>();
                }
                else if (solution[w, l][0] == TileType.WALL)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "WALL";
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, storey_height, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + 0.5f, y + storey_height / 2.0f, z + 0.5f);
                    cube.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f);
                    // cube.GetComponent<Renderer>().material.color = new Color(0.6f, 0.8f, 0.8f);
                }
                else if (solution[w, l][0] == TileType.NPC)
                {
                    GameObject npcs = Instantiate(npc_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    npcs.name = "NPC";
                    npcs.transform.position = new Vector3(x + 0.5f, y + UnityEngine.Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);

                    //GameObject virus = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //virus.GetComponent<Renderer>().material.color = new Color(0.5f, 0.0f, 0.0f);
                    //virus.name = "ENEMY";
                    //virus.transform.position = new Vector3(x + 0.5f, y + UnityEngine.Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);
                    //virus.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    //virus.AddComponent<BoxCollider>();
                    //virus.GetComponent<BoxCollider>().size = new Vector3(1.2f, 1.2f, 1.2f);
                    //virus.AddComponent<Rigidbody>();
                    //virus.GetComponent<Rigidbody>().useGravity = false;

                    npcs.AddComponent<NPC>();
                    npcs.GetComponent<Rigidbody>().mass = 10000;
                }
                
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(currentLevel);
        // if (player_entered_house)
        // {
        //     //end current level
        // if (currentLevel == 1)
        // {
        //     SceneManager.LoadScene("SecondLevel");
        // }
            // and do for rest of levels for level 5 go to final scene
        // }
    }

    private void freeze()
    {
        if (!invincible)// make sure not using both power ups at once
        {
            freezeButton.gameObject.SetActive(false);
            frozen = true;
            StartCoroutine(FreezeCoroutine(10.0f));
        }
    }
    private void invince()
    {
        if (!frozen)// make sure not using both power ups at once
        {
            invinceButton.gameObject.SetActive(false);
            invincible = true;
            StartCoroutine(InvinceCoroutine(10.0f));
        }
        
    }
    private IEnumerator FreezeCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        frozen = false;
    }
    private IEnumerator InvinceCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        invincible = false;
    }
}
