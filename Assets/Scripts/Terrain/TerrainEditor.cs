using UnityEngine;
using System.Collections;

/*
    Attach this script to a camera or gameobject to give it terrain editing tools at runtime.
*/
public class TerrainEditor : MonoBehaviour {

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

    private TerrainTool mActiveTool;

    private GameObject mCursorInstance;
    private Texture2D mBrushTexture;

    private bool mLODsdone;

    private float mTimer;

    private float mLastBrushX;
    private float mLastBrushY;

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
        mTimer = timeBetweenBrush;

        mLODsdone = true;

        setBrushSize(size);
	}
	
	// Update is called once per frame
	void Update () {
        mTimer -= Time.deltaTime;
        if (mTimer < 0.0f)
            mTimer = 0.0f;

        bool addremoveIn = Input.GetKeyUp(KeyCode.Z);
        bool paintHeightIn = Input.GetKeyUp(KeyCode.X);
        bool smoothIn = Input.GetKeyUp(KeyCode.C);
        bool leftMouseClick = Input.GetMouseButton(0);
        bool rightMouseClick = Input.GetMouseButton(1);

        //TODO make better button
        Controller controller = GetComponent<Controller>();
        bool controllerTriggered = (controller == null) ? false : controller.getButtonPressed("trigger");

        if (addremoveIn) {
            mActiveTool = mAddRemoveTool;
        }
        if (paintHeightIn) {
            mActiveTool = mPaintHeightTool;
        }
        if (smoothIn) {
            mActiveTool = mSmoothTool;
        }
        if ((controllerTriggered || leftMouseClick) && (mLastBrushX != mActiveTool.getHit().point.x || mLastBrushY != mActiveTool.getHit().point.y) && mTimer == 0.0f) {
            mActiveTool.ModifyTerrain();
            mLODsdone = false;
            mTimer = timeBetweenBrush;
        }
        if ((rightMouseClick) && mTimer == 0.0f) {
            mActiveTool.BrushAltFire();
            Debug.Log("Alt Fire!");
            mLODsdone = false;
            mTimer = timeBetweenBrush;
        }
        mLastBrushX = mActiveTool.getHit().point.x;
        mLastBrushY = mActiveTool.getHit().point.y;

        if (!mLODsdone && !leftMouseClick && !controllerTriggered) {
            mActiveTool.getHitTerrain().ApplyDelayedHeightmapModification();
            mLODsdone = true;
        }
	}

    void FixedUpdate() {
        mActiveTool.FixedUpdate();
    }

    public GameObject getCursor() {
        return mCursorInstance;
    }

    public int getBrushSize() {
        return size;
    }

    public float getBrushOpacity() {
        return opacity;
    }

    public Texture2D getBrushTexture() {
        return mBrushTexture;
    }

    public void setBrushSize(int size) {
        // update the brush texture
        this.size = size;
        Texture2D tex = mCursorInstance.GetComponent<Projector>().material.GetTexture("_ShadowTex") as Texture2D;
        Texture2D tCopy = Instantiate(tex);
        TextureScale.Point(tCopy, size, size);
        mBrushTexture = tCopy;
    }

    public float PixelToGrayScale(Color32 color) {
        return (((color.r + color.g + color.b) / 3) * (color.a/255.0f)) / 255.0f;
    }

}
