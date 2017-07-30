using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(TileObject))]
public class TileEditor : Editor {

    static float arrowSize = 0.2f;

    [MenuItem("Crackmage/420")]
    public static void TEMP_DEBUG ()
    {
        Debug.Log("<color=green>420</color>");
    }

    [MenuItem("Hacks/Kill-all")]
    public static void Kill_All()
    {
        foreach (PlayerHealth p in FindObjectsOfType<PlayerHealth>())
            p.TakeDamage(-1, 999);
    }

    [MenuItem("Hacks/Kill all opponents")]
    public static void Kill_All_Opponents()
    {
        foreach (PlayerHealth p in FindObjectsOfType<PlayerHealth>())
        {
            if(!p.photonView.isMine || !p.GetComponent<Player>().playerControlled)
                p.TakeDamage(-1, 999);
        }
    }

    [MenuItem("Hacks/Sudden death")]
    public static void SuddenDeath()
    {
        foreach (PlayerHealth p in FindObjectsOfType<PlayerHealth>())
            p.TakeDamage(-1, p.getHealth-1);
    }

    [MenuItem("Hacks/Random Push")]
    public static void RandomPush()
    {
        foreach (PlayerHealth p in FindObjectsOfType<PlayerHealth>())
            p.TakeDamage(-1, 0, Vector2.up * Random.Range(1f,5f) + Vector2.right * Random.Range(-5f,5f));
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TileObject script = (TileObject)target;

        if(GUILayout.Button("Update"))
        {
            Refresh();
        }

        if (GUILayout.Button("Destroy Tiles"))
        {
            DestroyTiles();
            script.width = 0;
            script.height = 0;
        }

        if (GUILayout.Button("Generate Collider"))
        {
            if(!script.GetComponent<BoxCollider2D>())
                script.gameObject.AddComponent<BoxCollider2D>();
            script.GetComponent<BoxCollider2D>().size = new Vector2(script.width, script.height);
        }
    }

    void OnSceneGUI ()
    {
        TileObject script = (TileObject)target;

        if (script.width == 0 || script.height == 0)
            return;

        Handles.color = Color.green;
        ExtendButton(Vector3.right);
        ExtendButton(-Vector3.right);
        ExtendButton(Vector3.up);
        ExtendButton(-Vector3.up);

        Handles.color = Color.red;
        ReduceButton(Vector3.right);
        ReduceButton(-Vector3.right);
        ReduceButton(Vector3.up);
        ReduceButton(-Vector3.up);
    }

    void ExtendButton (Vector3 direction)
    {
        TileObject script = (TileObject)target;

        Vector3 pos = script.transform.position + (script.transform.right * (script.width / 2f)) * direction.x + (script.transform.up * (script.height / 2f)) * direction.y;
        Quaternion rot = Quaternion.Euler(-90 * direction.y, 90 * direction.x, 0);
        pos += ((script.transform.up * direction.y + script.transform.right * direction.x) * arrowSize) * HandleUtility.GetHandleSize(pos);
        if (Handles.Button(pos, rot, HandleUtility.GetHandleSize(pos) * arrowSize, HandleUtility.GetHandleSize(pos) * arrowSize, Handles.CubeHandleCap))
        {
            Extend(direction);
        }
    }

    void ReduceButton(Vector3 direction)
    {
        TileObject script = (TileObject)target;

        Vector3 pos = script.transform.position + (script.transform.right * (script.width / 2f)) * -direction.x + (script.transform.up * (script.height / 2f)) * -direction.y;
        Quaternion rot = Quaternion.Euler(-90 * direction.y, 90 * direction.x, 0);
        pos += ((script.transform.up * direction.y + script.transform.right * direction.x) * arrowSize) * HandleUtility.GetHandleSize(pos);
        if (Handles.Button(pos, rot, HandleUtility.GetHandleSize(pos) * arrowSize, HandleUtility.GetHandleSize(pos) * arrowSize, Handles.CubeHandleCap))
        {
            Reduce(direction);
        }
    }
  
    void Reduce(Vector3 direction)
    {
        TileObject script = (TileObject)target;

        script.width -= Mathf.Clamp(script.width,0,Mathf.Abs((int)direction.x));
        script.height -= Mathf.Clamp(script.height,0,Mathf.Abs((int)direction.y));
        script.transform.position = script.transform.position + direction * 0.5f;
        Refresh();
    }

    void Extend (Vector3 direction)
    {
        TileObject script = (TileObject)target;

        script.width += Mathf.Abs((int)direction.x);
        script.height += Mathf.Abs((int)direction.y);
        script.transform.position = script.transform.position + direction * 0.5f;
        Refresh();
    }

    void Refresh ()
    {
        DestroyTiles();
        CreateTiles();
    }

    void CreateTiles ()
    {
        TileObject script = (TileObject)target;

        int i = 0;
        for (int y = 0; y < script.height; y++)
        {
            for (int x = 0; x < script.width; x++)
            {
                GameObject go = new GameObject();
                go.transform.SetParent(script.transform);
                go.name = "Tile" + i;
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = GetSprite(y,x);
                renderer.color = script.spriteInfo.color;
                renderer.sortingLayerID = script.spriteInfo.sortingLayerID;
                renderer.sortingOrder = script.spriteInfo.sortingOrder;
                //renderer.sortingLayerID = SortingLayer.NameToID(script.sorting);
                float offsetX = script.width / 2f - 0.5f;
                float offsetY = script.height / 2f - 0.5f;
                go.transform.localPosition = new Vector2(-offsetX + x, -offsetY + y);

                i++;
            }
        }
        if (script.GetComponent<BoxCollider2D>())
            script.GetComponent<BoxCollider2D>().size = new Vector2(script.width, script.height);
    }

    Sprite GetSprite(int y, int x)
    {
        TileObject script = (TileObject)target;

        if(!script.advancedMode)
            return script.spriteInfo.sprite;

        x = x == script.width-1 ? 2 : x > 0 ? 1 : 0;
        y = y == script.height-1 ? 0 : y > 0 ? 1 : 2;

        Sprite sprite = script.sprites[y].GetSprite(x) != null ? script.sprites[y].GetSprite(x) : script.spriteInfo.sprite;
        return sprite;
    }

    void DestroyTiles ()
    {
        TileObject script = (TileObject)target;

        List<Transform> children = new List<Transform>();
        foreach (Transform child in script.transform)
        {
            children.Add(child);
        }
        children.ForEach(child => DestroyImmediate(child.gameObject));
    }

}
