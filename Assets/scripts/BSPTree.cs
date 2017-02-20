using UnityEngine;
using System.Collections.Generic;

public class BSPTree : MonoBehaviour {

    public Vector2 treeSize;

    private BSPLeaf root;
    private List<BSPLeaf> leaves = new List<BSPLeaf> ();
	private int[,] grid;

    private void Start ()
    {
        CreateRooms ();
        // InitialiseGrid ();
        // CentreGrid ();
    }

    private void CreateRooms ()
    {
        root = new BSPLeaf (0, 0, (int) treeSize.x, (int) treeSize.y);
        leaves.Add (root);

        bool didSplit = true;
        while (didSplit)
        {
            didSplit = false;

            for (int i = 0; i < leaves.Count; i++)
            {
                // If we haven't already split this leaf
                if (leaves [i].firstChild == null && leaves [i].secondChild == null)
                {
                    // Attempt to split it
                    if (leaves [i].Split ())
                    {
                        // If successful, add it's children to the list of leaves
                        leaves.Add (leaves [i].firstChild);
                        leaves.Add (leaves [i].secondChild);
                        didSplit = true;
                    }
                }
            }
        }

        root.CreateRooms ();
    }

}
