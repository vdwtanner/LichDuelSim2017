using UnityEngine;
using System.Collections;

public class RampTool : EditorTool {

    Vector3 mFirstPoint;
    Vector3 mSecondPoint;
    float mWidth = 20;

    public override void OnSelection() {
        if (hController != null) {
            hController.enableLaserPointer(true);
        }
        mFirstPoint = new Vector3(0, -1, 0);
        mSecondPoint = new Vector3(0, -1, 0);
    }

    public override void BrushAltFire() {

    }

    public override void BrushAltFireUp() {

    }

    public override void ModifyTerrain() {
        if (mFirstPoint.y != -1 && mSecondPoint.y != -1) {
            // set the first point again

        } else if (mFirstPoint.y != -1) {
            // set the second point and paint terrain
            mSecondPoint = getHit().point;
            mSecondPoint.y = 0;

            
        } else {
            // set the first point
            mFirstPoint = getHit().point;
            mFirstPoint.y = 0;
        }
    }

    private void PaintRamp() {
        if (getHit().collider == null)
            return;
        if (getHit().collider.gameObject.GetComponent<Terrain>() == null)
            return;

        // basic idea for this, draw a line from point 1 to point two, extrude that line to a width of mWidth
        // then find an Axis Aligned bounding box for that rectangle
        // loop through the height map within the bounding box, checking if each sample is
        // within the actual box, then change the height based on the equation of the line



    }

}
