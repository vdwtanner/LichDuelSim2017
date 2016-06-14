using UnityEngine;
using System.Collections;

public class HexValidationTool : TerrainTool {

    public enum ValidationSubTool {
        Validate = 0,
        Invalidate = 1,
        Clear = 2
    };

    ValidationSubTool mCurrentTool = ValidationSubTool.Validate;

    public override void OnSelection() {
        if (hController != null) {
            hController.enableLaserPointer(true);
        }
    }

    public override void BrushAltFire() {
        if (hController != null) {
            hController.enableLaserPointer(true);
        }

    }

    public override void BrushAltFireUp() {
        mCurrentTool++;
        if (mCurrentTool > ValidationSubTool.Clear)
            mCurrentTool = ValidationSubTool.Validate;

        Debug.Log("Current Sub Tool = " + mCurrentTool);
    }

    public override void ModifyTerrain() {
        if (hController != null) {
            hController.enableLaserPointer(true);
        }
        if (getHitTerrain() != null) {
            switch (mCurrentTool) {
                case ValidationSubTool.Validate:
                    getHexGrid().SetHexValid(getHit().point, true, true);
                    break;
                case ValidationSubTool.Invalidate:
                    getHexGrid().SetHexValid(getHit().point, false, true);
                    break;
                case ValidationSubTool.Clear:
                    getHexGrid().SetHexValid(getHit().point, false, false);
                    break;
            }
        }
    }

}
