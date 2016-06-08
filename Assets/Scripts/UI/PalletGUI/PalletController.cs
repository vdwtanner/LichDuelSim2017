using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class PalletController : MonoBehaviour, SwipeListener {
    Transform pallet;
    public Texture2D test;
    private LevelEditorController levelEditorController;
    private Controller controller;

    //Rotation variables
    public float rotationSpeed = 1.0f;
    private int goalState = 0;
    public Vector3 toEuler;
    public Quaternion to;
    public float axis;
    public bool eulerEqual;
    private Quaternion[] rotations;

    //UI
    private PalletUIWindow[] uiWindows = new PalletUIWindow[4];

    // Use this for initialization
    void Start () {
        pallet = transform.FindChild("Pallet");
        controller = GetComponent<Controller>();
        rotations = new Quaternion[4];
        rotations[0] = Quaternion.Euler(0, 0, 0);
        rotations[1] = Quaternion.Euler(0, 0, 90);
        rotations[2] = Quaternion.Euler(0, 0, 180);
        rotations[3] = Quaternion.Euler(0, 0, 270);
        to = rotations[goalState];
        levelEditorController = controller.vrHelper.dominantHand.GetComponent<LevelEditorController>();
        initUIWindows();
    }

    public LevelEditorController getLevelEditorController() {
        if(levelEditorController == null) {
            levelEditorController = controller.vrHelper.dominantHand.GetComponent<LevelEditorController>();
        }
        return levelEditorController;
    }

    void initUIWindows() {
        for(int x = 0; x<4; x++) {
            uiWindows[x] = new PalletUIWindow(pallet.FindChild("Side " + x));
        }
        //Brushes
        string directory = Path.Combine(Application.streamingAssetsPath, "Level Editor/Brushes/");
        byte[] fileData;
        int index = 0;
        List<string> ext = new List<string> { ".jpg", ".png" };
        IEnumerable<string> files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).Where(s => ext.Any(e => s.EndsWith(e)));
        foreach (string filePath in files) {
            Debug.Log(filePath);
            fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            texture.wrapMode = TextureWrapMode.Clamp;
            float y = ((index % 3) - 1) * .3f;
            float x = .3f - .15f * (index / 3);
            UIButton uiButton = uiWindows[1].addButton(new Vector2(x, y), new Vector2(.14f, .28f), texture);
            uiButton.onTriggerDown += setBrushTexture;
            index++;
        }
		//Slider for scaling player
		UISlider playerScale = uiWindows[0].addSlider(new Vector2(0, 0), new Vector2(.6f, .08f), new Vector2(.04f, 2f), null, null);
		playerScale.min = .2f;
		playerScale.max = 25f;
		playerScale.setCalculatedValue(5);
		playerScale.onTriggerUp += setPlayerScale;
    }

	void setPlayerScale(UISlider slider) {
		float scale = slider.calcValue();
		transform.parent.localScale = new Vector3(scale, scale, scale);
	}

    void setBrushTexture(UIButton button) {
        levelEditorController.terrainEditor.setBrushTexture((Texture2D)button.GetComponent<Renderer>().material.GetTexture("_MainTex"));
    }
	
	// Update is called once per frame
	void Update () {
        toEuler = to.eulerAngles;

        if (pallet.localEulerAngles != toEuler) {
            pallet.localRotation = Quaternion.Lerp(pallet.localRotation, to, Time.deltaTime * rotationSpeed);
        }
	}

    public void OnSwipeDown(float velocity) {
        //throw new NotImplementedException();
    }

    public void OnSwipeLeft(float velocity) {
        goalState = (goalState + 1) % 4;
        to = rotations[goalState];
    }

    public void OnSwipeRight(float velocity) {
        goalState -= 1;
        if (goalState == -1) {
            goalState = 3;
        }
        to = rotations[goalState];
    }

    public void OnSwipeUp(float velocity) {
        //throw new NotImplementedException();
    }
}
