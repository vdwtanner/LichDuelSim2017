using UnityEngine;
using System.Collections;
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

    [Header("Add/Remove Height Tool")]
    public int Aplaceholder;

    [Header("Paint Height Tool")]
    public int PHplaceholder;

    [Header("Smooth Tool")]
    public int Splaceholder;

    private AddRemoveHeightTool mAddRemoveTool;
    private PaintHeightTool mPaintHeightTool;
    private SmoothHeightTool mSmoothTool;
    private TerrainTool[] mTools;
    private int mToolIndex;

    private TerrainTool mActiveTool;

    private GameObject mCursorInstance;
    private Texture2D mBrushTexture;

    private bool mLODsdone;

    private float mTimer;

    private float mLastBrushX;
    private float mLastBrushY;

    private int mSizeBeforeResize = -1;//invalid state

    private Controller controller;

	// Use this for initialization
	void Start () {
        // create the instance of our cursor
        mCursorInstance = (GameObject)Instantiate(brushCursorPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.Euler(90, 0, 0));

        // add all tool scripts to the object we're on
        mAddRemoveTool = new AddRemoveHeightTool();
        mAddRemoveTool.Initialize(this);
        mPaintHeightTool = new PaintHeightTool();
        mPaintHeightTool.Initialize(this);
        mSmoothTool = new SmoothHeightTool();
        mSmoothTool.Initialize(this);
        mActiveTool = mAddRemoveTool;
        //mActiveTool = mSmoothTool;
        mTimer = timeBetweenBrush;
        controller = GetComponent<Controller>();
        controller.showScrollWheel(true);
        mLODsdone = true;
        //Allows for quick tool swap
        mTools = new TerrainTool[3];
        mTools[0] = mAddRemoveTool;
        mTools[1] = mPaintHeightTool;
        mTools[2] = mSmoothTool;
        mToolIndex = 0;

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
        bool leftMouseClick = Input.GetMouseButton(0);
        bool rightMouseClick = Input.GetMouseButton(1);

        //TODO make better button
        
        bool controllerTriggerPressed = (controller == null) ? false : controller.getAxis("trigger").x>0.025f;
        bool controllerGripsPressed = (controller == null) ? false : controller.getButtonPressed("grip");

        if (addremoveIn) {
            mActiveTool = mAddRemoveTool;
            mToolIndex = 0;
        }
        if (paintHeightIn) {
            mActiveTool = mPaintHeightTool;
            mToolIndex = 1;
        }
        if (smoothIn) {
            mActiveTool = mSmoothTool;
            mToolIndex = 2;
        }
        if ((controllerTriggerPressed || leftMouseClick) && (mLastBrushX != mActiveTool.getHit().point.x || mLastBrushY != mActiveTool.getHit().point.y) && mTimer == 0.0f) {
            mActiveTool.ModifyTerrain();
            mLODsdone = false;
            mTimer = timeBetweenBrush;
        }
        if ((rightMouseClick || controllerGripsPressed) && mTimer == 0.0f) {
            mActiveTool.BrushAltFire();
            Debug.Log("Alt Fire!");
            mLODsdone = false;
            mTimer = timeBetweenBrush;
        }
        mLastBrushX = mActiveTool.getHit().point.x;
        mLastBrushY = mActiveTool.getHit().point.y;

        if (!mLODsdone && !leftMouseClick && !controllerTriggerPressed) {
            mActiveTool.getHitTerrain().ApplyDelayedHeightmapModification();
            mLODsdone = true;
        }
	}

    void FixedUpdate() {
        mActiveTool.FixedUpdate();
        if(controller != null) {
            float sizeChange = controller.getAxis("scrollWheel").x / 2.0f;

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

    public void setBrushSize(int size) {
        // update the brush texture
        Debug.Log("New Size = " + size);
        if (size > 100)
            size = 100;
        if (size < 1)
            size = 1;
        this.size = size;
        Texture2D tex = mCursorInstance.GetComponent<Projector>().material.GetTexture("_ShadowTex") as Texture2D;
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
        mToolIndex = (mToolIndex + 1) % mTools.Length;
        mActiveTool = mTools[mToolIndex];
        Debug.Log("Current tool: " + mActiveTool.GetType());//Need to create UI to show these things
    }

    public void OnSwipeLeft(float velocity) {
        mToolIndex = (mToolIndex - 1) % mTools.Length;
        if (mToolIndex < 0) {
            mToolIndex = mTools.Length + mToolIndex;
        }
        mActiveTool = mTools[mToolIndex];
        Debug.Log("Current tool: " + mActiveTool.GetType());//Need to create UI to show these things
    }

    public void OnSwipeUp(float velocity) {}

    public void OnSwipeDown(float velocity) {}
}
