using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// This will detect swipes if it is attached to the same controller that the swipe is made on, assuming that it also has a swipe detector script on it.
/// </summary>
public interface SwipeListener : IEventSystemHandler {

    void OnSwipeRight(float velocity);
    void OnSwipeLeft(float velocity);
    void OnSwipeUp(float velocity);
    void OnSwipeDown(float velocity);
}
