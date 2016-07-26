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
	private BoardManager h_boardManager;

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
		//Manages the tooltip text area
	public TextMesh tooltipText;
	private string oldText;
	private float timeSinceTextChanged;
	public float timeBeforeTextCleared = 1f;
	[Header("Button Textures")]
	public Texture heightmapToolsTex;
	public Texture hexValidationToolsTex;
	public Texture saveWorldTex;
	public Texture loadWorldTex;


	// Use this for initialization
	void Start () {
        pallet = transform.FindChild("Pallet");
        controller = GetComponent<Controller>();
        rotations = new Quaternion[4];
        rotations[0] = Quaternion.Euler(0, 0, 0);
        rotations[1] = Quaternion.Euler(0, 0, 90);
        rotations[2] = Quaternion.Euler(0, 0, 180);
        rotations[3] = Quaternion.Euler(0, 0, 270);
		h_boardManager = GameObject.FindGameObjectWithTag("GameBoard").GetComponent<BoardManager>();
        to = rotations[goalState];
        levelEditorController = controller.vrHelper.dominantHand.GetComponent<LevelEditorController>();
		tooltipText = transform.FindChild("TooltipTextArea").GetComponent<TextMesh>();
		oldText = tooltipText.text;
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
            float x = ((index % 3) - 1) * .3f;
            float y = .3f - .15f * (index / 3);
            UIButton uiButton = uiWindows[1].addButton(new Vector2(x, y), new Vector2(.28f, .14f), "Brush Texture", texture);
            uiButton.onTriggerDown += setBrushTexture;
            index++;
        }
		//Slider for scaling player
		UISlider playerScale = uiWindows[0].addSlider(new Vector2(0, 0), new Vector2(.08f, .6f), new Vector2(2f, .04f), "Player Scale");
		playerScale.min = .15f;
		playerScale.max = 25f;
		playerScale.setCalculatedValue(Config.godScale);
		playerScale.onTriggerUp += setPlayerScale;
		playerScale.onPointerDrag += updateScaleToolTip;

		//Buttons to chose editor tool
		UIButton terrainHeightButton = uiWindows[2].addButton(new Vector2(-.2f, .3f), new Vector2(.36f, .18f), "Terrain Height Tools", heightmapToolsTex);
		terrainHeightButton.onTriggerDown += setEditMode;
		UIButton hexValidationButton = uiWindows[2].addButton(new Vector2(.2f, .3f), new Vector2(.36f, .18f), "Hex Validation Tools", hexValidationToolsTex);
		hexValidationButton.onTriggerDown += setEditMode;

		//Buttons to save and load worlds
		UIButton saveWorldButton = uiWindows[3].addButton(new Vector2(0, .25f), new Vector2(.8f, .4f), "Save World", saveWorldTex);
		saveWorldButton.onTriggerDown += saveWorld;
		UIButton loadWorldButton = uiWindows[3].addButton(new Vector2(0, -.25f), new Vector2(.8f, .4f), "Load World", loadWorldTex);
		loadWorldButton.onTriggerDown += loadWorld;
	}

	void saveWorld(UIButton button) {
		h_boardManager.Save("VR_World.godmap");
	}

	void loadWorld(UIButton button) {
		h_boardManager.Load("VR_World.godmap");
	}

	void updateScaleToolTip(UISlider slider) {
		int value = Mathf.RoundToInt(slider.calcValue()*100);
		slider.tooltipText = "Player Scale\n" + (value / 100.0f);
	}

    void setPlayerScale(UISlider slider) {
		float scale = slider.calcValue();
		transform.parent.localScale = new Vector3(scale, scale, scale);
	}

    void setBrushTexture(UIButton button) {
        levelEditorController.terrainEditor.setBrushTexture((Texture2D)button.GetComponent<Renderer>().material.GetTexture("_MainTex"));
    }

	void setEditMode(UIButton button) {
		switch (button.tooltipText) {
			case "Terrain Height Tools":
				levelEditorController.terrainEditor.setEditorMode(TerrainEditor.EditorMode.TERRAIN_HEIGHT);
				break;
			case "Hex Validation Tools":
				levelEditorController.terrainEditor.setEditorMode(TerrainEditor.EditorMode.HEX_VALIDATION);
				break;
		}
	}
	
	// Update is called once per frame
	void Update () {
        toEuler = to.eulerAngles;

        if (pallet.localEulerAngles != toEuler) {
            pallet.localRotation = Quaternion.Lerp(pallet.localRotation, to, Time.deltaTime * rotationSpeed);
        }
	}

	void FixedUpdate() {
		if(oldText == tooltipText.text && timeSinceTextChanged <= timeBeforeTextCleared) {
			timeSinceTextChanged += Time.fixedDeltaTime;
			if(timeSinceTextChanged >= timeBeforeTextCleared) {
				tooltipText.text = "";
				oldText = "";
			}
		}else if(oldText != tooltipText.text) {
			timeSinceTextChanged = 0;
			oldText = tooltipText.text;
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
