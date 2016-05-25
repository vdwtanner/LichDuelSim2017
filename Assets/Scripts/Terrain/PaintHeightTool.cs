﻿using UnityEngine;
using System.Collections;

public class PaintHeightTool : TerrainTool {

    float mSampleHeight = -1;

    public override void BrushAltFire() {
        // sample height of terrain
        if (getHitTerrain() != null) {
            Vector3 heightmapScale = getHitTerrain().terrainData.heightmapScale;
            int heightmapOffsetX = (int)((getHit().point.x - getHitTerrain().GetPosition().x) / heightmapScale.x);
            int heightmapOffsetY = (int)((getHit().point.z - getHitTerrain().GetPosition().z) / heightmapScale.z);

            mSampleHeight = getHitTerrain().terrainData.GetHeights(heightmapOffsetX, heightmapOffsetY, 1, 1)[0, 0];
            Debug.Log("Sample Height is" + mSampleHeight);
        } else {
            Debug.Log("PaintHeightTool::BrushAltFire terrain is null");
            mSampleHeight = -1;
        }
    }

    public override void ModifyTerrain(){
        if (mSampleHeight == -1) {
            Debug.Log("PaintHeightTool::ModifyTerrain sample height was not set");
            return;
        }

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
                // logic here is that the pixels are 0 to 1 in value, but so are the heightmap points.
                // we need a divisor apart from opacity to weaken the brush effects to a managable level
                // for some reason height and width are switched in the array returned by getHeights
                // TODO replace getpixel with getpixels32 for optimization
                int x = i - imgOffsetX;
                int y = j - imgOffsetY;
                if (heights[y, x] < mSampleHeight) {
                    heights[y, x] += (editor.PixelToGrayScale(pixels[i * texWidth + j]) / 100) * brushOpacity;
                    if (heights[y, x] > mSampleHeight) {
                        heights[y, x] = mSampleHeight;
                    }
                } else if (heights[y, x] > mSampleHeight) {
                    heights[y, x] -= (editor.PixelToGrayScale(pixels[i * texWidth + j]) / 100) * brushOpacity;
                    if (heights[y, x] < mSampleHeight) {
                        heights[y, x] = mSampleHeight;
                    }
                }
            }
        }

        getHitTerrain().terrainData.SetHeightsDelayLOD(heightmapOffsetX, heightmapOffsetY, heights);

    }
}
