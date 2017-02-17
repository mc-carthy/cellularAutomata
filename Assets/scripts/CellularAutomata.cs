using UnityEngine;
using System;

public class CellularAutomata : MonoBehaviour {

    public string seed;
    public bool useRandomSeed;
    public int width;
    public int height;
    [RangeAttribute (0, 100)]
    public int randomFillPercent;
    public int smoothingIterations;

	private int [,] map;

    private void Start ()
    {
        GenerateMap ();
        CentreMap ();
    }

    private void Update ()
    {
        if (Input.GetKeyDown (KeyCode.Space))
        {
            GenerateMap ();
        }
    }

    private void GenerateMap ()
    {
        map = new int [width, height];
        RandomFillMap ();

        for (int i = 0; i < smoothingIterations; i++)
        {
            SmoothMap ();
        }

        MarchingSquaresMeshGen meshGen = GetComponent<MarchingSquaresMeshGen> ();
        meshGen.GenerateMesh (map, 1f);
    }

    private void CentreMap ()
    {
        transform.position = new Vector3 (width / 2f, 0, height / 2f);
    }

    private void RandomFillMap ()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString ();
        }

        System.Random prng = new System.Random (seed.GetHashCode ());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Ensure walls surround the grid
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map [x, y] = 1;
                }
                else
                {
                    map [x, y] = (prng.Next (0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    private void SmoothMap ()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount (x, y);

                if (neighbourWallTiles > 4)
                {
                    map [x, y] = 1;
                }
                else if (neighbourWallTiles < 4)
                {
                    map [x, y] = 0;
                }
            }
        }   
    }

    private int GetSurroundingWallCount (int gridX, int gridY)
    {
        int wallCount = 0;

        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map [neighbourX, neighbourY];
                    }
                }
                else
                {
                    // Encourage walls around edge of map
                    wallCount++;
                }
            }   
        }

        return wallCount;
    }

    // private void OnDrawGizmos ()
    // {
    //     if (map != null)
    //     {
    //         for (int x = 0; x < width; x++)
    //         {
    //             for (int y = 0; y < height; y++)
    //             {
    //                 Gizmos.color = (map [x, y] == 1) ? Color.black : Color.white;
    //                 Vector3 pos = new Vector3 (-width / 2 + x + 0.5f, 0, -height / 2 + y + 0.5f);
    //                 Gizmos.DrawCube (pos, Vector3.one);
    //             }
    //         }
    //     }
    // }

}
