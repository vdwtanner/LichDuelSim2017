using UnityEngine;
using System.Collections.Generic;
using System;

/*
    Attach this script to a camera or gameobject to give it terrain editing tools at runtime.
*/
public class TerrainEditor : MonoBehaviour, SwipeListener {

    [Header("Brush Settings")]
    [Range(0, 100)] public int size = 10;
    [Range(0.0f, 1.0f)] public float opacity = 1.0f;
    [Range(0.0f, 1.0f)]public float timeBetweenBrush = 0.1f;
    public GameObject brushCursorPrefab;
	public Texture2D defaultBrushTex;
	public int defaultBrushSize = 15;

	public enum EditorMode{ TERRAIN_HEIGHT, TERRAIN_TEXTURE, HEX_VALIDATION};

    //private AddRemoveHeightTool mAddRemoveTool;
    //private PaintHeightTool mPaintHeightTool;
    //private SmoothHeightTool mSmoothTool;
    private HexValidationTool mHexValidationTool;
    //private TerrainTool[] mTools;
	private Dictionary<EditorMode, List<EditorTool>> mTools;
    private int mToolIndex;
	private EditorMode mEditorMode;

    private EditorTool mActiveTool;

    private GameObject mCursorInstance;
    private Texture2D mBrushTexture;

    private bool mLODsdone=true;

    private float mTimer;

    private float mLastBrushX;
    private float mLastBrushY;

    private int mSizeBeforeResize = -1;//invalid state

    private Controller controller;

	// Use this for initialization
	void Start () {
        // create the instance of our cursor
        mCursorInstance = (GameObject)Instantiate(brushCursorPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.Euler(90, 0, 0));
		setBrushTexture(defaultBrushTex);
		setBrushSize(defaultBrushSize);
        // add all tool scripts to the object we're on
        /*mAddRemoveTool = new AddRemoveHeightTool();
        mAddRemoveTool.Initialize(this);
        mPaintHeightTool = new PaintHeightTool();
        mPaintHeightTool.Initialize(this);
        mSmoothTool = new SmoothHeightTool();
        mSmoothTool.Initialize(this);*/
        //mHexValidationTool = new HexValidationTool();
        //mHexValidationTool.Initialize(this);
        //mActiveTool = mAddRemoveTool;
        //mActiveTool = mSmoothTool;
        mTimer = timeBetweenBrush;
        controller = GetComponent<Controller>();
        if(controller != null)
            controller.showScrollWheel(true);
        mLODsdone = true;
		//Allows for quick tool swap
		//mTools = new TerrainTool[4];
		mTools = new Dictionary<EditorMode, List<EditorTool>>();
		mTools.Add(EditorMode.TERRAIN_HEIGHT, new List<EditorTool>());
		mTools.Add(EditorMode.TERRAIN_TEXTURE, new List<EditorTool>());
		mTools.Add(EditorMode.HEX_VALIDATION, new List<EditorTool>());
		List<EditorTool> tools;
		mTools.TryGetValue(EditorMode.TERRAIN_HEIGHT, out tools);
		tools.Add(new AddRemoveHeightTool());
		tools.Add(new PaintHeightTool());
		tools.Add(new SmoothHeightTool());
        tools.Add(new RampTool());
		foreach(EditorTool tool in tools) {
			tool.Initialize(this);
		}
		mTools.TryGetValue(EditorMode.HEX_VALIDATION, out tools);
		tools.Add(new HexValidationTool());
		tools.Add(new HexInvalidationTool());
		tools.Add(new HexClearValidationTool());
		foreach (EditorTool tool in tools) {
			tool.Initialize(this);
		}
		mEditorMode = EditorMode.TERRAIN_HEIGHT;
		mToolIndex = 0;
		mActiveTool = getActiveTool();

        setBrushSize(size);
	}
	
	// Update is called once per frame
	void Update () {
        mTimer -= Time.deltaTime;
        if (mTimer < 0.0f)
            mTimer = 0.0f;

        bool addremoveIn = Input.GetKeyUp(KeyCode.Z);
        bool paintHeightIn = Input.GetKeyUp(KeyCode.X);
        bool smoothIn = Input.GetKeyUp(KeyCode.V);
        bool rampIn = Input.GetKeyUp(KeyCode.B);
        bool leftMouseClick = Input.GetMouseButton(0);
        bool rightMouseClick = Input.GetMouseButton(1);

        //TODO make better button
        
        bool controllerTriggerPressed = (controller == null) ? false : controller.getAxis("trigger").x>0.025f;
        bool controllerGripsPressed = (controller == null) ? false : controller.getButtonPressed("grip");
        bool controllerGripsUp = (controller == null) ? false : controller.getButtonUp("grip");

        if (addremoveIn) {
            mToolIndex = 0;
            mActiveTool = getActiveTool();
            getActiveTool().OnSelection();
		}
        if (paintHeightIn) {
            mToolIndex = 1;
            mActiveTool = getActiveTool();
            getActiveTool().OnSelection();
		}
        if (smoothIn) {
            mToolIndex = 2;
            mActiveTool = getActiveTool();
            getActiveTool().OnSelection();
        }
        if (rampIn) {
            mToolIndex = 3;
            mActiveTool = getActiveTool();
            getActiveTool().OnSelection();
        }

        if(controllerGripsUp || Input.GetMouseButtonUp(1)) {
			//This is the same as mActiveTool, but is a step towards removing that extra variable and making it more scalable
			getActiveTool().BrushAltFireUp();
        }

        if ((controllerTriggerPressed || leftMouseClick) && (mLastBrushX != mActiveTool.getHit().point.x || mLastBrushY != mActiveTool.getHit().point.y) && mTimer == 0.0f) {
            mActiveTool.BrushPrimaryFire();
            mLODsdone = false;
            mTimer = timeBetweenBrush;
        }
        if ((rightMouseClick || controllerGripsPressed) && mTimer == 0.0f) {
            mActiveTool.BrushAltFire();
            //Debug.Log("Alt Fire!");
            mLODsdone = false;
            mTimer = timeBetweenBrush;
        }
        mLastBrushX = mActiveTool.getHit().point.x;
        mLastBrushY = mActiveTool.getHit().point.y;

        if (!mLODsdone && !leftMouseClick && !controllerTriggerPressed && !controllerGripsPressed && mActiveTool.getHitTerrain() != null) {
            mActiveTool.getHitTerrain().ApplyDelayedHeightmapModification();
            TerrainHexGrid grid = mActiveTool.getHitTerrain().gameObject.GetComponent<TerrainHexGrid>();
            if (grid != null)
                grid.TerrainModified();
            mLODsdone = true;
        }
	}

    void FixedUpdate() {
        mActiveTool.FixedUpdate();
        if(controller != null) {
            float sizeChange = controller.getAxis("scrollWheel").x / 2.0f;

            mCursorInstance.transform.rotation = controller.transform.rotation;
            if (sizeChange != 0) {
                if (mSizeBeforeResize == -1) {
                    mSizeBeforeResize = getBrushSize();
                }
                setBrushSize(mSizeBeforeResize + Mathf.RoundToInt(sizeChange));
            } else if (mSizeBeforeResize > -1) {
                mSizeBeforeResize = -1;
            }
        }
        
    }

    public GameObject getCursor() {
        return mCursorInstance;
    }

    public int getBrushSize() {
        return size;
    }

    public float getBrushOpacity() {
        return opacity * ((controller == null) ? 1.0f : controller.getAxis("trigger").x);
    }

    public float getAltBrushOpacity() {
        return opacity;
    }

    public Texture2D getBrushTexture() {
        return mBrushTexture;
    }

    public void setBrushTexture(Texture2D tex) {
        mCursorInstance.GetComponent<Projector>().material.SetTexture("_ShadowTex", tex);
        setBrushSize(this.size);
    }

    public void setBrushSize(int size) {
        // update the brush texture
        //Debug.Log("New Size = " + size);
        if (size > 100)
            size = 100;
        if (size < 4)
            size = 4;
        this.size = size;
		Texture2D tex = mCursorInstance.GetComponent<Projector>().material.GetTexture("_ShadowTex") as Texture2D;
		//Texture2D tex = defaultBrushTex;
        Texture2D tCopy = Instantiate(tex);
        TextureScale.Point(tCopy, size, size);
        Color32[] pixels = tCopy.GetPixels32();
        int pWidth = tCopy.width;
        int pHeight = tCopy.height;
        for (int i = 0; i < pWidth; i++) {
            for (int j = 0; j < pHeight; j++) {
                pixels[i * pWidth + j] = PixelToGrayScale(pixels[i * pWidth + j]);
            }
        }
        tCopy.SetPixels32(pixels);
        mBrushTexture = tCopy;
    }

    public Color32 PixelToGrayScale(Color32 color) {
        byte gray = (byte)(((color.r + color.g + color.b) / 3) * (color.a / 255.0f));
        return new Color32(gray, gray, gray, gray);
    }

    public void OnSwipeRight(float velocity) {
        mToolIndex = (mToolIndex + 1) % getActiveToolList().Count;
        mActiveTool = getActiveTool();
		getActiveTool().OnSelection();
        controller.showText(mActiveTool.GetType().ToString(), "base", 2.0f);
        Debug.Log("Current tool: " + mActiveTool.GetType());//Need to create UI to show these things
    }

    public void OnSwipeLeft(float velocity) {
		int toolCount = getActiveToolList().Count;
		mToolIndex = (mToolIndex - 1) % toolCount;
        if (mToolIndex < 0) {
            mToolIndex = toolCount + mToolIndex;
        }
        mActiveTool = getActiveTool();
		getActiveTool().OnSelection();
        controller.showText(mActiveTool.GetType().ToString(), "base", 2.0f);
        Debug.Log("Current tool: " + mActiveTool.GetType());//Need to create UI to show these things
    }

    //Don't use these since it will override the size changer
    public void OnSwipeUp(float velocity) {}
    //Don't use these since it will override the size changer
    public void OnSwipeDown(float velocity) {}


	public EditorTool getActiveTool() {
		List<EditorTool> tools;
		mTools.TryGetValue(mEditorMode, out tools);
		return tools[mToolIndex];
	}

	public List<EditorTool> getActiveToolList() {
		List<EditorTool> tools;
		mTools.TryGetValue(mEditorMode, out tools);
		return tools;
	}

	public void setEditorMode(EditorMode mode) {
		mEditorMode = mode;
	}
}
