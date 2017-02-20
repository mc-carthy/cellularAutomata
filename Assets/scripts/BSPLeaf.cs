using UnityEngine;

public class BSPLeaf {

    private const int MIN_LEAF_SIZE = 8;

    public BSPLeaf parent;
    public BSPLeaf firstChild;
    public BSPLeaf secondChild;
    public int width;
    public int height;
    public int x;
    public int y;
    public bool hasRoom;
    public Vector2 roomSize;
    public Vector2 roomPos;
    public Room room;
    public bool isFirstChild;
    public Vector3 centre;
    public Vector2 centre2D;

    private GameObject quad;

    public BSPLeaf (int _x, int _y, int _width, int _height, BSPLeaf _parent)
    {
        x = _x;
        y = _y;
        width = _width;
        height = _height;
        parent = _parent;
        hasRoom = false;
        centre = new Vector3 (x + width / 2f, 0, y + height / 2f);
        centre2D = new Vector2 (x + width / 2f, y + height / 2f);

        quad = GameObject.CreatePrimitive (PrimitiveType.Quad);
        quad.transform.position = new Vector3 (x + (width * 0.5f), -1f, y + (height * 0.5f));
        quad.transform.localScale = new Vector3 (width, height, 1f);
        quad.transform.Rotate (new Vector3 (90f, 0, 0));
        quad.GetComponent<Renderer> ().material.color = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f), 0.25f);
        quad.gameObject.name = x + " - " + y;
        quad.gameObject.transform.parent = GameObject.Find ("debugQuads").transform;

        // Debug.DrawLine (new Vector3 (x + (width * 0.5f), -1f, y + (height * 0.5f)), Vector3.zero, Color.red, 100f);

        if (parent != null)
        {
            // Debug.DrawLine (centre, parent.centre, Color.red, 100f);
            Corridor corridor = new Corridor (new Vector2 (Mathf.Abs (centre2D.x - parent.centre2D.x), Mathf.Abs (centre2D.y - parent.centre2D.y)), (centre2D + parent.centre2D) / 2f, 1f);
        }
    }

    public bool Split ()
    {
        // If this leaf already has children, skip it
        if (firstChild != null || secondChild != null)
        {
            return false;
        }

        // 50:50 chance of splitting leaf horizontally
        bool splitH = Random.Range (0f, 1f) < 0.5f;

        // If the width is >25% larger than height, we split vertically
        // If the height is >25% larger than the width, we split horizontally
        if (width > height && width / height >= 1.25f)
        {
            splitH = false;
        }
        else if (height > width && height / width >= 1.25f)
        {
            splitH = true;
        }

        // Determine the max height/width of child leaf
        int max = (splitH ? height : width) - MIN_LEAF_SIZE;

        // If we can't generate a large enough room, break
        if (max <= MIN_LEAF_SIZE)
        {
            return false;
        }

        // Generate split
        int split = Random.Range (MIN_LEAF_SIZE, max);
        if (splitH)
        {
            firstChild = new BSPLeaf (x, y, width, split, this);
            secondChild = new BSPLeaf (x, y + split, width, height - split, this);
        }
        else
        {
            firstChild = new BSPLeaf (x, y, split, height, this);
            secondChild = new BSPLeaf (x + split, y, width - split, height, this);
        }


        if (quad != null)
        {
            GameObject.Destroy(quad);
            quad = null;
        }

        // We've successfully split the leaf
        return true;
    }

    public void CreateRooms ()
    {
        if (firstChild != null || secondChild != null)
        {
            if (firstChild != null)
            {
                firstChild.CreateRooms ();
            }
            if (secondChild != null)
            {
                secondChild.CreateRooms ();
            }
            hasRoom = false;
        }
        else
        {
            roomSize = new Vector2 (Random.Range (3, width - 2), Random.Range (3, height - 2));
            roomPos = new Vector2 (Random.Range (2, width - roomSize.x - 2), Random.Range (2, height - roomSize.y - 2));
            Room room = new Room (roomSize, roomPos + new Vector2 (x + (roomSize.x / 2f), y + (roomSize.y / 2f)), this);
            this.room = room;
            hasRoom = true;
        }
    }

    public class Room {

        public Vector2 size;
        public Vector2 pos;
        public BSPLeaf parent;

        public Room (Vector2 _size, Vector2 _pos, BSPLeaf _parent)
        {
            size = _size;
            pos = _pos;
            parent = _parent;

            GameObject quad = GameObject.CreatePrimitive (PrimitiveType.Quad);
            quad.transform.position = new Vector3 (pos.x, 0, pos.y);
            quad.transform.localScale = new Vector3 (size.x, size.y, 1f);
            quad.transform.Rotate (new Vector3 (90f, 0f, 0f));

            quad.GetComponent<Renderer> ().material.color = Color.white;
            quad.gameObject.name = "Room";
            quad.gameObject.transform.parent = GameObject.Find("bspTree").transform;
        }
    }

    public class Corridor {

        public Vector2 size;
        public Vector2 pos;

        public Corridor (Vector2 _size, Vector2 _pos, float width)
        {
            size = _size;
            pos = _pos;

            if (size.x > size.y)
            {
                size.y = width;
            }
            else
            {
                size.x = width;
            }

            GameObject quad = GameObject.CreatePrimitive (PrimitiveType.Quad);
            quad.transform.position = new Vector3 (pos.x, 0, pos.y);
            quad.transform.localScale = new Vector3 (size.x, size.y, 1f);
            quad.transform.Rotate (new Vector3 (90f, 0f, 0f));
            quad.GetComponent<Renderer> ().material.color = Color.white;
            quad.name = "Corridor";
            quad.gameObject.transform.parent = GameObject.Find("bspTree").transform;
        }

    }
}