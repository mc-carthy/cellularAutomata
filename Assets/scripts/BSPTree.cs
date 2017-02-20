using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BSPTree : MonoBehaviour {

    public Vector2 treeSize;

    private BSPLeaf root;
    private List<BSPLeaf> leaves = new List<BSPLeaf> ();
	private int[,] grid;

    private void Start ()
    {
        CreateRooms ();
        GameObject.Find ("debugQuads").gameObject.SetActive (false);
    }

    private void Update ()
    {
        if (Input.GetKeyDown (KeyCode.Space))
        {
            SceneManager.LoadScene (SceneManager.GetActiveScene ().name, LoadSceneMode.Single);
        }
    }

    private void CreateRooms ()
    {
        root = new BSPLeaf (0, 0, (int) treeSize.x, (int) treeSize.y, null);
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
        GameObject baseQuad = GameObject.CreatePrimitive (PrimitiveType.Quad);
        baseQuad.transform.position = new Vector3 (treeSize.x / 2f, -2f, treeSize.y / 2f);
        baseQuad.transform.localScale = new Vector3 (treeSize.x, treeSize.y, 0f);
        baseQuad.transform.Rotate (new Vector3 (90f, 0f, 0f));
        baseQuad.GetComponent<Renderer> ().material.color = Color.black;
    }

}
