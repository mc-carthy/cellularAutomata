using UnityEngine;

public class BSPLeaf {

    private const int MIN_LEAF_SIZE = 8;

    public BSPLeaf firstChild;
    public BSPLeaf secondChild;
    public int width;
    public int height;
    public int x;
    public int y;
    public bool hasRoom;
    public Vector2 roomSize;
    public Vector2 roomPos;

    private GameObject quad;

    public BSPLeaf (int _x, int _y, int _width, int _height)
    {
        x = _x;
        y = _y;
        width = _width;
        height = _height;
        hasRoom = false;

        quad = GameObject.CreatePrimitive (PrimitiveType.Quad);
        quad.transform.position = new Vector3 (x + (width * 0.5f), -1f, y + (height * 0.5f));
        quad.transform.localScale = new Vector3 (width, height, 1f);
        quad.transform.Rotate (new Vector3 (90f, 0, 0));
        quad.GetComponent<Renderer> ().material.color = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f), 0.25f);
        quad.gameObject.name = x + " - " + y;
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
            firstChild = new BSPLeaf (x, y, width, split);
            secondChild = new BSPLeaf (x, y + split, width, height - split);
        }
        else
        {
            firstChild = new BSPLeaf (x, y, split, height);
            secondChild = new BSPLeaf (x + split, y, width - split, height);
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
            Room room = new Room (roomSize, roomPos + new Vector2 (x + (roomSize.x / 2f), y + (roomSize.y / 2f)));
            hasRoom = true;
        }
    }

    class Room {

        public Vector2 size;
        public Vector2 pos;

        public Room (Vector2 _size, Vector2 _pos)
        {
            size = _size;
            pos = _pos;

            GameObject quad = GameObject.CreatePrimitive (PrimitiveType.Quad);
            quad.transform.position = new Vector3 (pos.x, 0, pos.y);
            quad.transform.localScale = new Vector3 (size.x, size.y, 1f);
            quad.transform.Rotate (new Vector3 (90f, 0f, 0f));

            quad.GetComponent<Renderer> ().material.color = Color.white;
        }
    }
}