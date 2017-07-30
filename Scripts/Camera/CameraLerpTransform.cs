using UnityEngine;
using System.Collections;

public class CameraLerpTransform : Photon.MonoBehaviour {

	float speed = 1f;
    public float zoomSpeed = 3f;

	public Transform target;
	public Vector2 targetOffset = new Vector2(0,0.5f);

    static CameraLerpTransform instance;

    Camera c;
    float minZoom = 4, maxZoom = 12;

    void Awake()
    {
        instance = this;
        c = GetComponent<Camera>();
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Z))
        {
            float newZoom = c.orthographicSize;
            newZoom -= zoomSpeed * Time.deltaTime;
            newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
            c.orthographicSize = newZoom;
        }
        if(Input.GetKey(KeyCode.X))
        {
            float newZoom = c.orthographicSize;
            newZoom += zoomSpeed * Time.deltaTime;
            newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
            c.orthographicSize = newZoom;
        }
    } 

    void LateUpdate () 
	{
		if (target == null)
			return;

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance < 1f)
            return;
		Vector3 newPosition = Vector2.Lerp (transform.position, target.position + (Vector3)targetOffset, (speed + (distance/5) ) * Time.deltaTime);
		newPosition.z = transform.position.z;
		transform.position = newPosition;
	}

    [PunRPC]
    void SetTargetRPC (int viewID)
    {
        if(PhotonView.Find(viewID) != null)
        {
            target = PhotonView.Find(viewID).transform;
        }
    }

    public static void SetCameraTarget (int viewID)
    {
        instance.photonView.RPC("SetTargetRPC", PhotonTargets.All, viewID);
    }

    public static void SetLocalCameraTarget (Transform target)
    {
        if(instance != null)
            instance.target = target;
    }
}
