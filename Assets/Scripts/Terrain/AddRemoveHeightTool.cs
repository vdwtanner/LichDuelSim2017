using UnityEngine;
using System.Collections;

public class AddRemoveHeightTool : TerrainTool {

	// Use this for initialization
	void Start () {
        base.Initialize();    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public override void ModifyTerrain(){
        // get brush texture
        Texture2D tex2D = getEditor().getBrushTexture();

        // add texture to heightmap
        Vector3 heightmapScale = getHitTerrain().terrainData.heightmapScale;
        int heightmapOffsetX = (int)((getHit().point.x - getHitTerrain().GetPosition().x) / heightmapScale.x);
        int heightmapOffsetY = (int)((getHit().point.z - getHitTerrain().GetPosition().z) / heightmapScale.z);
        heightmapOffsetX -= (tex2D.width / 2);
        heightmapOffsetY -= (tex2D.height / 2);

        float[,] heights = getHitTerrain().terrainData.GetHeights(heightmapOffsetX, heightmapOffsetY, tex2D.width, tex2D.height);
        for (int i = 0; i < tex2D.width; i++) {
            for (int j = 0; j < tex2D.height; j++) {
                // logic here is that the pixels are 0 to 1 in value, but so are the heightmap points.
                // we need a divisor apart from opacity to weaken the brush effects to a managable level
                heights[i, j] += (getEditor().PixelToGrayScale(tex2D.GetPixel(i, j)) / 100) * getEditor().getBrushOpacity();
            }
        }

        getHitTerrain().terrainData.SetHeightsDelayLOD(heightmapOffsetX, heightmapOffsetY, heights);

    }
}
