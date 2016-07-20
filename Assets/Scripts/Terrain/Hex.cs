﻿using UnityEngine;
using System.Collections;

public class Hex{

    public Vector3 position; // position of the lowerleft corner of the hex (Y is equal to chunk Y)

	public Vector3 worldPosition { get; set; }

    private float mSize;
    private bool mValid;
    private bool mIgnoreAutoValidation;
    private Rect mUVRect;
	private HexChunk h_chunk;

    public Hex(float x, float y, float z, float size, Rect uvRect, HexChunk chunk) {
        position.x = x;
        position.y = y;
        position.z = z;
        mSize = size;
        mUVRect = uvRect;
		h_chunk = chunk;
    }

    public void setValid(bool isValid, bool ignoreAutoValidation) {
        mValid = isValid;
        mIgnoreAutoValidation = ignoreAutoValidation;
    }

    public bool isValid() {
        return mValid;
    }

    public bool ignoreAutoValidate() {
        return mIgnoreAutoValidation;
    }

    public void setUVRect(Rect r) {
        mUVRect = r;
    }

    public Rect getUVRect() {
        return mUVRect;
    }
}
