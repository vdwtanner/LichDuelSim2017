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
    /// <returns>The button so that you can add functionality to it via delegate methods.</returns>
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
		quad.name = "Button";
        Vector3 loc = new Vector3(localPosition.x, localPosition.y, -.01f);
        quad.transform.localPosition = loc;
        return quad.AddComponent<UIButton>();
    }

	/// <summary>
	/// Add a slider to this window
	/// </summary>
	/// <param name="localPosition">Where to place the slider</param>
	/// <param name="trackScale">The scale/size of the sliderTrack. This is in proportion to the size of the window.</param>
	/// <param name="sliderScale">the scale/size of the slider. This is in proportion to the size of the slider track.</param>
	/// <param name="trackTexture">The texture to apply to the track</param>
	/// <param name="sliderTexture">The texture to apply to the slider</param>
	/// <returns>The slider that was created so that you can attach functionality to it via delegate methods.</returns>
	public UISlider addSlider(Vector2 localPosition, Vector2 trackScale, Vector2 sliderScale, Texture trackTexture, Texture sliderTexture) {
		GameObject sliderTrack = GameObject.CreatePrimitive(PrimitiveType.Quad);
		if (trackTexture != null) {
			Material material = new Material(Shader.Find("Unlit/Texture"));
			material.mainTexture = trackTexture;
			sliderTrack.GetComponent<Renderer>().material = material;
		}
		Object.Destroy(sliderTrack.GetComponent<MeshCollider>());
		sliderTrack.AddComponent<BoxCollider>();
		sliderTrack.name = "SliderTrack";
		sliderTrack.transform.parent = side;
		sliderTrack.transform.localEulerAngles = Vector3.zero;
		Vector3 scale = trackScale;
		scale.z = 1;
		sliderTrack.transform.localScale = scale;
		sliderTrack.tag = "GUI";
		Vector3 loc = new Vector3(localPosition.x, localPosition.y, -.01f);
		sliderTrack.transform.localPosition = loc;
		UISlider uiSlider = sliderTrack.AddComponent<UISlider>();
		//Add Slider to track
		GameObject slider = GameObject.CreatePrimitive(PrimitiveType.Quad);
		if (sliderTexture != null) {
			Material material = new Material(Shader.Find("Unlit/Texture"));
			material.mainTexture = sliderTexture;
			slider.GetComponent<Renderer>().material = material;
		}
		Object.Destroy(slider.GetComponent<MeshCollider>());
		slider.AddComponent<BoxCollider>();
		slider.name = "Slider";
		slider.transform.parent = side;
		slider.transform.localEulerAngles = Vector3.zero;
		slider.transform.parent = sliderTrack.transform;
		scale = sliderScale;
		scale.z = 1;
		slider.transform.localScale = scale;
		
		slider.tag = "GUI";
		loc = new Vector3(localPosition.x, localPosition.y, -.01f);
		slider.transform.localPosition = loc;
		uiSlider.slider = slider.transform;
		UIListener uiListener = slider.AddComponent<UIListener>();
		uiListener.onPointerEnter += uiSlider.OnPointerIn;
		uiListener.onPointerRemain += uiSlider.OnPointerStay;

		return uiSlider;
	}
}
