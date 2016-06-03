using UnityEngine;
using System.Collections.Generic;

public class PalletUIWindow{
    private Transform side;
    private Dictionary<int, List<GameObject>> pages;

	public PalletUIWindow(Transform side) {
        this.side = side;
    }

    /// <summary>
    /// Add a button to this window. Will automatically page elements that extend past too far up or down
    /// EXAMPLE USAGE (makes a square textured with texture on the levelEditor's pallet):
    ///  UIButton uiButton = uiWindows[2].addButton(new Vector3(-.25f, -.25f, 0), new Vector3(.2f, .1f, .2f), texture);
    /// </summary>
    /// <param name="localPosition">Where to place the new button. Values are portions of the UI window. X = [-.4, 4], Y can be anything (each page limited to [-.4, .4], but will automatically page)</param>
    /// <param name="scale">Scale of the element relative to the window. Remember that the window is longer on the Y axis</param>
    /// <param name="texture">The texture for the button.</param>
    /// <returns></returns>
    public UIButton addButton(Vector2 localPosition, Vector2 scale, Texture texture) {
        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = texture;
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Object.Destroy(quad.GetComponent<MeshCollider>());
        quad.AddComponent<BoxCollider>();
        quad.GetComponent<Renderer>().material = material;
        quad.transform.parent = side;
        quad.transform.localEulerAngles = Vector3.zero;
        quad.transform.localScale = scale;
        quad.tag = "GUI";
        Vector3 loc = new Vector3(localPosition.x, localPosition.y, -.01f);
        quad.transform.localPosition = loc;
        return quad.AddComponent<UIButton>();
    }
}
