using UnityEngine;
using System.Collections;

public class Hex{

    public Vector3 position; // position of the lowerleft corner of the hex

    private float mSize;
    private bool mValid;

    public Hex(float x, float y, float z, float size) {
        position.x = x;
        position.y = y;
        position.z = z;
        mSize = size;

    }

    public void setValid(bool isValid) {
        mValid = isValid;
    }

    public bool isValid() {
        return mValid;
    }
}
