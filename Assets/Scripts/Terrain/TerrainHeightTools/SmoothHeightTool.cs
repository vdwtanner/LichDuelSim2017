﻿using UnityEngine;
using System.Collections;
using System;

public class SmoothHeightTool : EditorTool {

    public override void OnSelection() {
        if(hController != null)
            hController.enableLaserPointer(false);
    }

	public override void OnUnselect() {

	}

    public override void BrushAltFire() {
        Debug.Log("SmoothHeightTool::BrushAltFire does nothing");
    }

    public override void BrushAltFireUp() {
        Debug.Log("SmoothHeightTool::BrushAltFireUp does nothing");
    }

    public override void BrushPrimaryFire() {
        if (getHit().collider == null)
            return;
        if (getHit().collider.gameObject.GetComponent<Terrain>() == null)
            return;
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
        TerrainEditor editor = getEditor();
        float brushOpacity = editor.getBrushOpacity();

        for (int i = imgOffsetX; i < width; i++) {
            for (int j = imgOffsetY; j < height; j++) {
                // for some reason height and width are switched in the array returned by getHeights
                int x = i - imgOffsetX;
                int y = j - imgOffsetY;
                float defVal = heights[y, x];
                float hm00 = getHeightForGaussian(heights, x - 1, y - 1, defVal);
                float hm01 = getHeightForGaussian(heights, x, y - 1, defVal) * 2;
                float hm02 = getHeightForGaussian(heights, x + 1, y - 1, defVal);

                float hm10 = getHeightForGaussian(heights, x - 1, y, defVal) * 2;
                float hm11 = defVal * 4;
                float hm12 = getHeightForGaussian(heights, x + 1, y, defVal) * 2;

                float hm20 = getHeightForGaussian(heights, x - 1, y + 1, defVal);
                float hm21 = getHeightForGaussian(heights, x, y + 1, defVal) * 2;
                float hm22 = getHeightForGaussian(heights, x + 1, y + 1, defVal);

                float newHeight = hm00 + hm01 + hm02 + hm10 + hm11 + hm12 + hm20 + hm21 + hm22;
                newHeight /= 16.0f;
                float diffHeight = newHeight - heights[y, x];
                diffHeight *= pixels[i * texWidth + j].a / 255.0f;
                diffHeight *= brushOpacity;
                heights[y, x] += diffHeight;  

            }
        }

        getHitTerrain().terrainData.SetHeightsDelayLOD(heightmapOffsetX, heightmapOffsetY, heights);
    }

    float getHeightForGaussian(float[,] heights, int x, int y, float defaultVal) {
        if (x < 0)
            return defaultVal;
        if (x >= heights.GetLength(1))
            return defaultVal;
        if (y < 0)
            return defaultVal;
        if (y >= heights.GetLength(0))
            return defaultVal;
        return heights[y, x];
        
    }
}
