using UnityEngine;
using UnityEngine.EventSystems;
//using UnityEngine.Networking;
public class SwipeDetector : MonoBehaviour {
    [SerializeField]
    SteamVR_TrackedObject trackedObj;
    private const int mMessageWidth = 200;
    private const int mMessageHeight = 64;

    private readonly Vector2 mXAxis = new Vector2(1, 0);
    private readonly Vector2 mYAxis = new Vector2(0, 1);
    private bool trackingSwipe = false;
    private bool checkSwipe = false;
    private Controller controller;

    // The angle range for detecting swipe
    private const float mAngleRange = 30;

    // To recognize as swipe user should at lease swipe for this many pixels
    private const float mMinSwipeDist = 0.2f;

    // To recognize as a swipe the velocity of the swipe
    // should be at least mMinVelocity
    // Reduce or increase to control the swipe speed
    private const float mMinVelocity = 4.0f;

    private Vector2 mStartPosition;
    private Vector2 endPosition;

    private float mSwipeStartTime;

    void Start() {
        controller = GetComponent<Controller>();
    }

    // Update is called once per frame
    void Update() {
        //var device = SteamVR_Controller.Input((int)trackedObj.index);
        // Touch down, possible chance for a swipe
        //Debug.Log("test");
        if ((int)trackedObj.index != -1 && controller.getTouchDown("touchpad")) {
            trackingSwipe = true;
            // Record start time and position
            mStartPosition = controller.getAxis("touchpad");
            mSwipeStartTime = Time.time;
            Debug.Log("Tracking Start");
        }
        // Touch up , possible chance for a swipe
        else if (controller.getTouchUp("touchpad")) {
            trackingSwipe = false;
            trackingSwipe = true;
            checkSwipe = true;
            Debug.Log("Tracking Finish");
        } else if (trackingSwipe) {
            endPosition = controller.getAxis("touchpad");
            //Debug.Log("swiping");
        }

        if (checkSwipe) {
            checkSwipe = false;
            float deltaTime = Time.time - mSwipeStartTime;
            Vector2 swipeVector = endPosition - mStartPosition;

            float velocity = swipeVector.magnitude / deltaTime;
            Debug.Log(velocity);
            if (velocity > mMinVelocity &&
                swipeVector.magnitude > mMinSwipeDist) {
                // if the swipe has enough velocity and enough distance


                swipeVector.Normalize();

                float angleOfSwipe = Vector2.Dot(swipeVector, mXAxis);
                angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;

                // Detect left and right swipe
                if (angleOfSwipe < mAngleRange) {
                    ExecuteEvents.Execute<SwipeListener>(gameObject, null, (x, y) => x.OnSwipeRight(velocity));
                    //OnSwipeRight();
                } else if ((180.0f - angleOfSwipe) < mAngleRange) {
                    ExecuteEvents.Execute<SwipeListener>(gameObject, null, (x, y) => x.OnSwipeLeft(velocity));
                    //OnSwipeLeft();
                } else {
                    // Detect top and bottom swipe
                    angleOfSwipe = Vector2.Dot(swipeVector, mYAxis);
                    angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
                    if (angleOfSwipe < mAngleRange) {
                        ExecuteEvents.Execute<SwipeListener>(gameObject, null, (x, y) => x.OnSwipeUp(velocity));
                        //OnSwipeTop();
                    } else if ((180.0f - angleOfSwipe) < mAngleRange) {
                        ExecuteEvents.Execute<SwipeListener>(gameObject, null, (x, y) => x.OnSwipeDown(velocity));
                        //OnSwipeBottom();
                    } else {
                        //mMessageIndex = 0;
                    }
                }
            }
        }

    }

    /*private void OnSwipeLeft() {
        mMessageIndex = 1;
    }

    private void OnSwipeRight() {
        mMessageIndex = 2;
    }

    private void OnSwipeTop() {
        mMessageIndex = 3;
    }

    private void OnSwipeBottom() {
        mMessageIndex = 4;
    }*/
}