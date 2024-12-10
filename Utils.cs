using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.PlasticSCM.Editor.WebApi;


public class PriorityQueue2<T>
{
    SortedSet<PriorityQueueObject<T>> queue;


    int a = 0;

    public PriorityQueue2() { queue = new SortedSet<PriorityQueueObject<T>>(); }

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


public class Utils {
    
    public string coord2Str(int[] coord) {
        return "" + coord[0] + " " + coord[1];
    }
    
    public int[] str2coord(string str) {
        string[] parts = str.Split(' ');
        if (parts.Length != 2)
            return null;

        int x = int.Parse(parts[0]);
        int y = int.Parse(parts[1]);

        return new int[] { x, y };
    }

    public int ManhanttanDist(int[] init_pos, int[] dst) {
        return Mathf.Abs(init_pos[0] - dst[0]) + Mathf.Abs(init_pos[1] -dst[1]); 
    }

    public List<int[]> shortestPath(List<TileType>[,] solution, int[] init_pos, int[] dst) {
        int width = solution.GetLength(0);
        int length = solution.GetLength(1);
        if (init_pos[0] < 0 || init_pos[1] < 0 || init_pos[0] >= width || init_pos[1] >= length || solution[init_pos[0], init_pos[1]][0] == TileType.WALL) {
            Debug.Log($"Init_pos {coord2Str(init_pos)} invalid");
            // Debug.Log(solution[init_pos[0], init_pos[1]][0] == TileType.WALL);
            return new List<int[]> {};
        }

        if (dst[1] < 0 || dst[0] >= width || dst[1] >= length || solution[dst[0], dst[1]][0] == TileType.WALL) {
            Debug.Log($"Destination {coord2Str(dst)} invalid");

            return new List<int[]> {};
        }

        if (init_pos[0] == dst[0] && init_pos[1] == dst[1]) {
            return new List<int[]> {dst};
        }

        // real script


        PriorityQueue2<string> frontier = new PriorityQueue2<string>();
        Dictionary<string, int[]> parents = new Dictionary<string, int[]>();

        // int[, ] infrontier = new int[width, length]; //Check if a node is in the frontier to prevent repitition
        int[, ] explored = new int[width, length];
        int[, ] dist = new int[width, length];

        for (int i = 0; i < width; ++i) {
            for (int j = 0; j < length; ++j) {
                dist[i, j] = int.MaxValue;
            }
        }

        // Let's first add our player into the graph search frontier
        frontier.Add(coord2Str(init_pos), 0);
        dist[init_pos[0], init_pos[1]] = 0;
        parents[coord2Str(init_pos)] = null;
        // infrontier[init_pos[0], init_pos[1]] = 1;

        while (frontier.Count() != 0) {
            // get an arbitrary node
            var strPos = frontier.Pop();
            int[] pos = str2coord(strPos);

            if (explored[pos[0], pos[1]] == 1) {
                continue; // Handling a repeated push
            }
            int tw = pos[0];
            int tl = pos[1];
            explored[tw, tl] = 1; 
            // infrontier[tw, tl] = 0; // This node is explored and meanwhile nolonger in the frontier
            if (tw == dst[0] && tl == dst[1]) {
                // Destination found
                break;
            }
            // Get all neighbors of our pos

            for (int dx = -1; dx <= 1; ++dx) {
                for (int dy = -1; dy <= 1; ++dy) {
                    if (Mathf.Abs(dx) == Mathf.Abs(dy)) {
                        continue;
                    }
                    int tww = tw + dx;
                    int tll = tl + dy;
                    if (tww < 0 || tll < 0 || tww >= width || tll >= length) {
                        // Invalid neighbors out of bounds
                        continue;
                    }
                    if (explored[tww, tll] == 1) {
                        // Invaiant invalid new neighbors (not valid new neighbors in general)
                        continue;
                    } 
                    if (solution[tww, tll][0] == TileType.WALL && !(tww == dst[0] && tll == dst[1])) {
                        continue; // I wont be allowing any wall traversals
                    }

                    // I allow arbitrary number of travels on the floor but a cost on traveling on the wall
                    // So the Dijkstra's algorithm theoretically finds a path that minimizes the wall-traversing
                    int edge_cost = 1;
                    int distance = dist[tw, tl] + edge_cost;
                    // Now we are certain (tww, tll) is a valid new neighbor
                    int[] neighbor = new int[]{tww, tll};
                    int old_distance = dist[tww, tll];
                    if (distance >= old_distance) {
                        continue; //Dont overwrite in this case
                    }

                    int heuristics = ManhanttanDist(neighbor, dst);
                    frontier.Add(coord2Str(neighbor), distance + heuristics);
                    dist[tww, tll] = distance;
                    // infrontier[tww, tll] = 1;
                    parents[coord2Str(neighbor)] = pos;
                }
            }
        }
        // Reconstructing the path
        // I think there certainly a way to reach the destination if I allow wall traversing
        // try {
        //     Debug.Log(parents[coord2Str(dst)]);
        // }
        if (explored[dst[0], dst[1]] == 0) {
            Debug.Log("Destination not reachable");
            return new List<int[]>();
        }
            

        List<int[]> path = new List<int[]>();
        int[] current = dst;

        
        while (current != null)
        {
            path.Add(current);
            current = parents[coord2Str(current)];
        }

        // Debug.Log(frontier.elementLocations[coord2Str(dst)]);
        path.Reverse();

        return path;
    }

    public (float, float) grid_to_pos(Bounds bounds, int wr, int lr, int width, int length) {
        float x = bounds.min[0] + (float)wr * (bounds.size[0] / (float)width);
        float z = bounds.min[2] + (float)lr * (bounds.size[2] / (float)length);
        return (x, z);
    }

    public (int, int) pos_to_grid(Bounds bounds, float x, float z, int width, int length) {
        int wr = (int)(((x - bounds.min[0]) / (bounds.size[0] / (float)width)) - 0.5);
        int lr = (int)(((z - bounds.min[2]) / (bounds.size[2] / (float)length)) - 0.5);
        return (wr + 1, lr + 1);

    }


//     public (int, int) pos_to_grid(Bounds bounds, float x, float z, int width, int length) {
//     // Calculate the grid index from world position
//     int wr = Mathf.FloorToInt((x - bounds.min[0]) / (bounds.size[0] / (float)width));
//     int lr = Mathf.FloorToInt((z - bounds.min[2]) / (bounds.size[2] / (float)length));

//     // Return the grid indices
//     return (wr, lr);
// }

    // internal List<int[]> shortestPath(List<TileType>[,] grid, (int, int) npc_pos, (float, float) player_pos)
    // {
    //     throw new NotImplementedException();
    // }
}

