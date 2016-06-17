using UnityEngine;
using System.Collections;
using System;

public class AddRemoveHeightTool : EditorTool {

    public override void OnSelection() {
        if(hController != null)
            hController.enableLaserPointer(false);
    }

    public override void BrushAltFire() {
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
        float brushOpacity = editor.getAltBrushOpacity();

        for (int i = imgOffsetX; i < width; i++) {
            for (int j = imgOffsetY; j < height; j++) {
                // for some reason height and width are switched in the array returned by getHeights
                int x = i - imgOffsetX;
                int y = j - imgOffsetY;
                heights[y, x] -= ((pixels[i * texWidth + j].a / 255.0f) / 100) * brushOpacity;
                //Don't want to have negative terrain height
                heights[y, x] = Mathf.Max(0, heights[y, x]);
            }
        }

        getHitTerrain().terrainData.SetHeightsDelayLOD(heightmapOffsetX, heightmapOffsetY, heights);
    }

    public override void BrushAltFireUp() {
        //Debug.Log("AddRemoveHeightTool::BrushAltFireUp does nothing");
    }

    public override void BrushPrimaryFire(){
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
                heights[y, x] += ((pixels[i*texWidth + j].a / 255.0f) / 100) * brushOpacity;
            }
        }

        getHitTerrain().terrainData.SetHeightsDelayLOD(heightmapOffsetX, heightmapOffsetY, heights);
    }
}
