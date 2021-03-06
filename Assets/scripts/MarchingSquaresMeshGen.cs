﻿using UnityEngine;
using System.Collections.Generic;

public class MarchingSquaresMeshGen : MonoBehaviour {

	public class Node {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node (Vector3 _position)
        {
            position = _position;
        }
    }

    public class ControlNode : Node {
        public bool active;
        public Node aboveNode, rightNode;

        public ControlNode (Vector3 _position, bool _active, float squareSize) : base (_position)
        {
            active = _active;
            aboveNode = new Node (position + Vector3.forward * squareSize / 2f);
            rightNode = new Node (position + Vector3.right * squareSize / 2f);
        }
    }

    public class Square {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square (ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centreTop = topLeft.rightNode;
            centreRight = bottomRight.aboveNode;
            centreBottom = bottomLeft.rightNode;
            centreLeft = bottomLeft.aboveNode;

            if (topLeft.active)
            {
                configuration += 8;
            }
            if (topRight.active)
            {
                configuration += 4;
            }
            if (bottomRight.active)
            {
                configuration += 2;
            }
            if (bottomLeft.active)
            {
                configuration += 1;
            }

        }
    }

    public class SquareGrid {
        public Square [,] squares;

        public SquareGrid (int [,] map, float squareSize)
        {
            int nodeCountX = map.GetLength (0);
            int nodeCountY = map.GetLength (1);

            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode [,] controlNodes = new ControlNode [nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 position = new Vector3 (-mapWidth / 2f + x * squareSize + squareSize / 2f, 0, -mapHeight / 2f + y * squareSize + squareSize / 2f);
                    controlNodes [x, y] = new ControlNode (position, map [x, y] == 1, squareSize);
                }   
            }

            squares = new Square [nodeCountX - 1, nodeCountY - 1];

            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares [x, y] = new Square (controlNodes [x, y + 1], controlNodes [x + 1, y + 1], controlNodes [x + 1, y], controlNodes [x, y]);
                }   
            }
        }
    }

    public bool is2D;

    public SquareGrid squareGrid;
    private List<Vector3> vertices;
    private List<int> triangles;
    private Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>> ();

    private List<List<int>> outlines = new List<List<int>> ();
    private HashSet<int> checkedVertices = new HashSet<int> ();

    public MeshFilter walls;
    public MeshFilter cave;

    public void GenerateMesh (int [,] map, float squareSize)
    {

        triangleDictionary.Clear ();
        outlines.Clear ();
        checkedVertices.Clear ();

        squareGrid = new SquareGrid (map, squareSize);

        vertices = new List<Vector3> ();
        triangles = new List<int> ();

        for (int x = 0; x < squareGrid.squares.GetLength (0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength (1); y++)
            {
                TriangulateSquare (squareGrid.squares [x, y]);
            }
        }

        Mesh mesh = new Mesh ();
        cave.mesh = mesh;

        mesh.vertices = vertices.ToArray ();
        mesh.triangles = triangles.ToArray ();
        mesh.RecalculateNormals ();

        int tileSize = 10;
        Vector2[] uvs = new Vector2[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp (-map.GetLength (0) / 2f * squareSize, map.GetLength (0) / 2f * squareSize, vertices [i].x) * tileSize;
            float percentY = Mathf.InverseLerp (-map.GetLength (0) / 2f * squareSize, map.GetLength (0) / 2f * squareSize, vertices [i].z) * tileSize;

            uvs [i] = new Vector2 (percentX, percentY);
        }

        mesh.uv = uvs;

        if (is2D)
        {
            cave.gameObject.transform.rotation = Quaternion.Euler (270f, 0f, 0f);
            Generate2DColliders ();
        }
        else
        {
            CreateWallMesh ();
        }


    }

    private void CreateWallMesh ()
    {
        CalculateMeshOutlines ();

        List<Vector3> wallVertices = new List<Vector3> ();
        List<int> wallTriangles = new List<int> ();
        Mesh wallMesh = new Mesh ();
        float wallHeight = 5f;

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                // Left vertex
                wallVertices.Add (vertices [outline [i]]);
                // Right vertex
                wallVertices.Add (vertices [outline [i + 1]]);
                // Bottom left vertex
                wallVertices.Add (vertices [outline [i]] - Vector3.up * wallHeight);
                // Bottom right vertex
                wallVertices.Add (vertices [outline [i + 1]] - Vector3.up * wallHeight);

                // // Top Left
                // wallTriangles.Add (startIndex + 0);
                // // Bottom left
                // wallTriangles.Add (startIndex + 2);
                // // Bottom Right
                // wallTriangles.Add (startIndex + 3);
                // // Bottom Right
                // wallTriangles.Add (startIndex + 3);
                // // Top Right
                // wallTriangles.Add (startIndex + 1);
                // // Top Left
                // wallTriangles.Add (startIndex + 0);

                // Top Left
                wallTriangles.Add (startIndex + 3);
                // Bottom left
                wallTriangles.Add (startIndex + 2);
                // Bottom Right
                wallTriangles.Add (startIndex + 0);
                // Bottom Right
                wallTriangles.Add (startIndex + 0);
                // Top Right
                wallTriangles.Add (startIndex + 1);
                // Top Left
                wallTriangles.Add (startIndex + 3);
            }
        }

        wallMesh.vertices = wallVertices.ToArray ();
        wallMesh.triangles = wallTriangles.ToArray ();

        walls.mesh = wallMesh;

        MeshCollider wallCollider = walls.gameObject.GetComponent<MeshCollider> ();

        if (wallCollider == null)
        {
            wallCollider = walls.gameObject.AddComponent<MeshCollider> ();
        }
        
        wallCollider.sharedMesh = wallMesh;
    }

    private void Generate2DColliders ()
    {
        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D> ();

        for (int i = 0; i < currentColliders.Length; i++)
        {
            Destroy (currentColliders [i]);
        }

        CalculateMeshOutlines ();

        foreach (List<int> outline in outlines)
        {
            EdgeCollider2D edgeCol = gameObject.AddComponent<EdgeCollider2D> ();
            Vector2[] edgePoints = new Vector2[outline.Count];

            for (int i = 0; i < outline.Count; i++)
            {
                edgePoints [i] = new Vector2 (vertices [outline [i]].x, vertices [outline [i]].z);
            }

            edgeCol.points = edgePoints;
        }
    }

    private void TriangulateSquare (Square square)
    {
        switch (square.configuration)
        {
            // 0 points
            case 0:
                break;
            
            // 1 point
            case 1:
                MeshFromPoints (square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints (square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                MeshFromPoints (square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                MeshFromPoints (square.topLeft, square.centreTop, square.centreLeft);
                break;

            // 2 points (same edge)
            case 3:
                MeshFromPoints (square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints (square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints (square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints (square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;

            // 2 points (opposing corners)
            case 5:
                MeshFromPoints (square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints (square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 3 points
            case 7:
                MeshFromPoints (square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints (square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints (square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints (square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 4 points
            case 15:
                MeshFromPoints (square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);

                // Since this will never be part of the cave mesh outline, add them to checked vertices
                checkedVertices.Add (square.topLeft.vertexIndex);
                checkedVertices.Add (square.topRight.vertexIndex);
                checkedVertices.Add (square.bottomRight.vertexIndex);
                checkedVertices.Add (square.bottomLeft.vertexIndex);
                break;
        }
    }

    private void MeshFromPoints (params Node[] points)
    {
        AssignVertices (points);
        if (points.Length >= 3)
        {
            CreateTriangle (points [0], points [1], points [2]);
        }
        if (points.Length >= 4)
        {
            CreateTriangle (points [0], points [2], points [3]);
        }
        if (points.Length >= 5)
        {
            CreateTriangle (points [0], points [3], points [4]);
        }
        if (points.Length >= 6)
        {
            CreateTriangle (points [0], points [4], points [5]);
        }
    }

    private void AssignVertices (Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points [i].vertexIndex == -1)
            {
                points [i].vertexIndex = vertices.Count;
                vertices.Add (points [i].position);
            }
        }
    }

    private void CreateTriangle (Node a, Node b, Node c)
    {
        triangles.Add (a.vertexIndex);
        triangles.Add (b.vertexIndex);
        triangles.Add (c.vertexIndex);

        Triangle triangle = new Triangle (a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary (triangle.vertexIndexA, triangle);
        AddTriangleToDictionary (triangle.vertexIndexB, triangle);
        AddTriangleToDictionary (triangle.vertexIndexC, triangle);
    }

    private void AddTriangleToDictionary (int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey (vertexIndexKey))
        {
            triangleDictionary [vertexIndexKey].Add (triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle> ();
            triangleList.Add (triangle);
            triangleDictionary.Add (vertexIndexKey, triangleList);
        }
    }

    private bool IsOutlineEdge (int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary [vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA [i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }

        return sharedTriangleCount == 1;
    }

    private int GetConnectedOutlineVertex (int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex [i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle [j];
                
                if (vertexB != vertexIndex && !checkedVertices.Contains (vertexB))
                {
                    if (IsOutlineEdge (vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }
        
        return -1;
    }

    private void CalculateMeshOutlines ()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains (vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex (vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add (newOutlineVertex);

                    List<int> newOutline = new List<int> ();
                    newOutline.Add (vertexIndex);
                    outlines.Add (newOutline);

                    FollowOutline (newOutlineVertex, outlines.Count - 1);
                    outlines [outlines.Count - 1].Add (vertexIndex);
                }
            }
        }
    }

    private void FollowOutline (int vertexIndex, int outlineIndex)
    {
        outlines [outlineIndex].Add (vertexIndex);
        checkedVertices.Add (vertexIndex);

        int nextVertexIndex = GetConnectedOutlineVertex (vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline (nextVertexIndex, outlineIndex);
        }
    }

    struct Triangle {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        private int [] vertices;

        public Triangle (int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int [3];
            vertices [0] = a;
            vertices [1] = b;
            vertices [2] = c;
        }

        public int this [int i]
        {
            get {
                return vertices [i];
            }
        }

        public bool Contains (int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

}
