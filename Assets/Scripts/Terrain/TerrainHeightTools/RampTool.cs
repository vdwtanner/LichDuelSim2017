using UnityEngine;
using System.Collections;

public class RampTool : EditorTool {

    Vector3 mFirstPoint;
    Vector3 mSecondPoint;
    float mWidth = 20;
    Vector3 mPlaneNormal;
    Vector3 mLineVector;
    float mDTerm = 0;

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
        if (getHit().collider == null)
            return;
        if (getHit().collider.gameObject.GetComponent<Terrain>() == null)
            return;

        if (mFirstPoint.y != -1 && mSecondPoint.y != -1) {
            // set the first point again
            mFirstPoint = getHit().point;
			mFirstPoint.y = getHitTerrain().SampleHeight(mFirstPoint) / getHitTerrain().terrainData.heightmapScale.y;
            mSecondPoint.y = -1;
        } else if (mFirstPoint.y != -1) {
            // set the second point and paint terrain
            mSecondPoint = getHit().point;
			mSecondPoint.y = getHitTerrain().SampleHeight(mSecondPoint) / getHitTerrain().terrainData.heightmapScale.y;

            // find slope of line (z in terms of x) projected onto the xz plane 
            float slope;
            Vector3 lineNorm;
            Vector3 v0;
            if (mFirstPoint.y < mSecondPoint.y) {
                slope = (mSecondPoint.z - mFirstPoint.z) / (mSecondPoint.x - mFirstPoint.x);
                lineNorm = (mSecondPoint - mFirstPoint);
                v0 = mFirstPoint;
            } else {
                slope = (mFirstPoint.z - mSecondPoint.z) / (mFirstPoint.x - mSecondPoint.x);
                lineNorm = (mFirstPoint - mSecondPoint);
                v0 = mSecondPoint;
            }
            mLineVector = lineNorm;
            lineNorm.Normalize();
            // find the slope of the line perpendicular to that xz plane line
            // then create a normal from it.
            // this normal is parallel to the terrain plane
            Vector3 parallelNorm = new Vector3(1.0f, 0.0f, -(1.0f / slope));
            parallelNorm.Normalize();

            mPlaneNormal = Vector3.Cross(lineNorm, parallelNorm);
            if (mPlaneNormal.y < 0) {
                mPlaneNormal = Vector3.Cross(parallelNorm, lineNorm);
            }
            mPlaneNormal.Normalize();

            mDTerm = Vector3.Dot(v0, mPlaneNormal);

        } else {
            // set the first point
            mFirstPoint = getHit().point;
			float myvar = getHitTerrain().SampleHeight(mFirstPoint);
			mFirstPoint.y = getHitTerrain().SampleHeight(mFirstPoint) / getHitTerrain().terrainData.heightmapScale.y;
        }


    }

    public override void BrushPrimaryFire() {
        if (getHit().collider == null)
            return;
        if (getHit().collider.gameObject.GetComponent<Terrain>() == null)
            return;
        if (mFirstPoint.y == -1 || mSecondPoint.y == -1)
            return;

        PaintRamp();
    }

    private void PaintRamp() {


        // basic idea for this: paint height tool, but painting height on a
        // non-axis aligned plane instead.

        // get brush texture
        Texture2D tex2D = getEditor().getBrushTexture();

        Vector3 heightmapScale = getHitTerrain().terrainData.heightmapScale;
        int heightmapOffsetX = (int)((getHit().point.x - getHitTerrain().GetPosition().x) / heightmapScale.x);
        int heightmapOffsetY = (int)((getHit().point.z - getHitTerrain().GetPosition().z) / heightmapScale.z);
        heightmapOffsetX -= (tex2D.width / 2);
        heightmapOffsetY -= (tex2D.height / 2);

        // gotta clip our brush texture so painting at the terrain edge works properly.

        int widthDiff = (heightmapOffsetX + tex2D.width) - (getHitTerrain().terrainData.heightmapWidth);
        int width = (widthDiff > 0) ? (tex2D.width - widthDiff) : tex2D.width;
        int heightDiff = (heightmapOffsetY + tex2D.height) - (getHitTerrain().terrainData.heightmapHeight);
        int height = (heightDiff > 0) ? (tex2D.height - heightDiff) : tex2D.height;

        int imgOffsetX = (heightmapOffsetX < 0) ? -heightmapOffsetX : 0;
        int imgOffsetY = (heightmapOffsetY < 0) ? -heightmapOffsetY : 0;

        heightmapOffsetX = (heightmapOffsetX < 0) ? 0 : heightmapOffsetX;
        heightmapOffsetY = (heightmapOffsetY < 0) ? 0 : heightmapOffsetY;

        float[,] heights = getHitTerrain().terrainData.GetHeights(heightmapOffsetX, heightmapOffsetY, width, height);
        Color32[] pixels = tex2D.GetPixels32();
        int texWidth = tex2D.width;
        float brushOpacity = getEditor().getBrushOpacity();

        for (int i = imgOffsetX; i < width; i++) {
            for (int j = imgOffsetY; j < height; j++) {
                // for some reason height and width are switched in the array returned by getHeights
                int x = i - imgOffsetX;
                int y = j - imgOffsetY;
                // find height at that position on plane

				float worldX = (heightmapOffsetX + x) * heightmapScale.x;
				float worldZ = (heightmapOffsetY + y) * heightmapScale.z;

				float maxHeight = (mDTerm - mPlaneNormal.x * worldX - mPlaneNormal.z * worldZ) / mPlaneNormal.y;
                if (heights[y, x] < maxHeight) {
                    heights[y, x] += ((pixels[i * texWidth + j].a / 255.0f) / 100) * brushOpacity;
                    if (heights[y, x] > maxHeight) {
                        heights[y, x] = maxHeight;
                    }
                } else if (heights[y, x] > maxHeight) {
                    heights[y, x] -= ((pixels[i * texWidth + j].a / 255.0f) / 100) * brushOpacity;
                    if (heights[y, x] < maxHeight) {
                        heights[y, x] = maxHeight;
                    }
                }
            }
        }

        getHitTerrain().terrainData.SetHeightsDelayLOD(heightmapOffsetX, heightmapOffsetY, heights);



    }

}
