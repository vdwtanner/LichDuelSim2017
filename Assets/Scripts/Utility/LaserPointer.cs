using UnityEngine;
using System.Collections;

public class LaserPointer : MonoBehaviour {
    public bool active = true;
    public Color color;
    public float thickness = 0.002f;
    public GameObject holder;
    public GameObject pointer;
    bool isActive = false;
    public bool addRigidBody = false;
    public bool showLaserOnStart = true;
    public Transform reference;
    public event PointerEventHandler PointerIn;
    public event PointerEventHandler PointerStay;
    public event PointerEventHandler PointerOut;
	public Vector3 collisionPoint;
    /// <summary>
    /// The RaycastHit that is used by the laser pointer
    /// </summary>
    public RaycastHit hit;
    Transform previousContact = null;

    // Use this for initialization
    void Start() {
        holder = new GameObject();
        holder.transform.parent = this.transform;
        holder.transform.localPosition = Vector3.zero;
        holder.transform.localEulerAngles = Vector3.zero;

        pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer.transform.parent = holder.transform;
        pointer.name = "Laser Pointer";
        pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
        pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
        pointer.transform.localEulerAngles = Vector3.zero;
        BoxCollider collider = pointer.GetComponent<BoxCollider>();
        if (addRigidBody) {
            if (collider) {
                collider.isTrigger = true;
            }
            Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
        } else {
            if (collider) {
                Object.Destroy(collider);
            }
        }
        Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", color);
        pointer.GetComponent<MeshRenderer>().material = newMaterial;
        pointer.GetComponent<Renderer>().enabled = showLaserOnStart;
    }

    public virtual void OnPointerIn(PointerEventArgs e) {
        //Debug.Log(e.target.name);
        if (PointerIn != null)
            PointerIn(this, e);
    }

    public virtual void OnPointerOut(PointerEventArgs e) {
        if (PointerOut != null)
            PointerOut(this, e);
    }

    public virtual void OnPointerStay(PointerEventArgs e) {
        if (PointerStay != null)
            PointerStay(this, e);
    }


    // Update is called once per frame
    void Update() {
        if (!isActive) {
            isActive = true;
            this.transform.GetChild(0).gameObject.SetActive(true);
        }

        float dist = 100f;

        SteamVR_TrackedController controller = GetComponent<SteamVR_TrackedController>();

        Ray raycast = new Ray(transform.position, transform.forward);

        bool bHit = Physics.Raycast(raycast, out hit);
        if (previousContact && previousContact != hit.transform) {
            PointerEventArgs args = new PointerEventArgs();
            if (controller != null) {
                args.controllerIndex = controller.controllerIndex;
            }
            args.distance = 0f;
            args.flags = 0;
            args.target = previousContact;
            OnPointerOut(args);
            previousContact = null;
        }
        if (bHit && previousContact != hit.transform) {
            PointerEventArgs argsIn = new PointerEventArgs();
            if (controller != null) {
                argsIn.controllerIndex = controller.controllerIndex;
            }
            argsIn.distance = hit.distance;
            argsIn.flags = 0;
            argsIn.target = hit.transform;
            OnPointerIn(argsIn);
            previousContact = hit.transform;
        }else if (bHit && previousContact == hit.transform) {//Add functionality for when the pointer is staying in an object
            PointerEventArgs argsIn = new PointerEventArgs();
            if (controller != null) {
                argsIn.controllerIndex = controller.controllerIndex;
            }
            argsIn.distance = hit.distance;
            argsIn.flags = 0;
            argsIn.target = hit.transform;
            OnPointerStay(argsIn);
            previousContact = hit.transform;
        }
        if (!bHit) {
            previousContact = null;
		}
        if (bHit && hit.distance < 100f) {
            dist = hit.distance;
			collisionPoint = hit.point;
        }

        if (controller != null && controller.triggerPressed) {
            pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);
        } else {
            pointer.transform.localScale = new Vector3(thickness, thickness, dist);
        }
        pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
    }
}
