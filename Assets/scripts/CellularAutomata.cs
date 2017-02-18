using UnityEngine;
using System.Collections.Generic;

[RequireComponent (typeof (MarchingSquaresMeshGen))]
public class CellularAutomata : MonoBehaviour {

    public string seed;
    public bool useRandomSeed;
    public int width;
    public int height;
    [RangeAttribute (1, 10)]
    public int borderSize;
    [RangeAttribute (0, 100)]
    public int randomFillPercent;
    public int smoothingIterations;
    public int wallThresholdSize;
    public int roomThresholdSize;

	private int [,] map;

    private void Start ()
    {
        GenerateMap ();
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

        ProcessMap ();

        int [,] borderedMap = new int [width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength (0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength (1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap [x, y] = map [x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap [x, y] = 1;
                }
            }   
        }
        

        MarchingSquaresMeshGen meshGen = GetComponent<MarchingSquaresMeshGen> ();
        meshGen.GenerateMesh (borderedMap, 1f);
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

    private List<Coord> GetRegionTiles (int startX, int startY)
    {
        List<Coord> tiles = new List<Coord> ();

        int [,] mapFlags = new int [width, height];
        int tileType = map [startX, startY];

        Queue<Coord> queue = new Queue<Coord> ();
        queue.Enqueue (new Coord (startX, startY));
        mapFlags [startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue ();
            tiles.Add (tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange (x, y) && (x == tile.tileX || y == tile.tileY))
                    {
                        if (mapFlags [x, y] != 1 && map [x, y] == tileType)
                        {
                            mapFlags [x, y] = 1;
                            queue.Enqueue (new Coord (x, y));
                        }
                    }
                }                
            }
        }

        return tiles;
    }

    private List<List<Coord>> GetRegions (int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>> ();

        int [,] mapFlags = new int [width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags [x, y] == 0 && map [x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles (x, y);
                    regions.Add (newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags [tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    private void ProcessMap ()
    {
        List<List<Coord>> wallRegions = GetRegions (1);
        
        // Remove wall regions smaller than  wallThresholdSize
        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map [tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions (0);
        
        // Remove wall regions smaller than  wallThresholdSize
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map [tile.tileX, tile.tileY] = 1;
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
                if (IsInMapRange (neighbourX, neighbourY))
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

    private bool IsInMapRange (int x, int y)
    {
        return x > 0 && x < width && y > 0 && y < height;
    }

    struct Coord {
        public int tileX;
        public int tileY;

        public Coord (int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

}
