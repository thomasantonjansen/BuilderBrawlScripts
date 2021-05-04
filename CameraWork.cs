using UnityEngine;

namespace BuilderBrawl
{
	/// Camera work. Follow a target
	public class CameraWork : MonoBehaviour
	{
	    [Tooltip("The distance in the local x-z plane to the target")]
	    [SerializeField]
	    private float distance = 10.0f;
	    
	    [Tooltip("The height we want the camera to be above the target")]
	    [SerializeField]
	    private float height = 15.0f;

	    [SerializeField]
	    private float offset = 25.0f;

	    [Tooltip("Allow the camera to be offseted vertically from the target, for example giving more view of the sceneray and less ground.")]
	    [SerializeField]
	    private Vector3 centerOffset = Vector3.zero;

	    [Tooltip("Set this as false if a component of a prefab being instanciated by Photon Network, and manually call OnStartFollowing() when and if needed.")]
	    [SerializeField]
	    private bool followOnStart = false;

	    [Tooltip("The Smoothing for the camera to follow the target")]
	    [SerializeField]
	    private float smoothSpeed = 0.125f;

        // cached transform of the target
        Transform cameraTransform;

		// maintain a flag internally to reconnect if target is lost or camera is switched
		bool isFollowing;
		
		// Cache for camera offset
		Vector3 cameraOffset = Vector3.zero;
		
        void Start()
		{
			// Start following the target if wanted.
			if (followOnStart)
			{
				OnStartFollowing();
			}
		}

		void LateUpdate()
		{
			// The transform target may not destroy on level load, 
			// so we need to cover corner cases where the Main Camera is different everytime we load a new scene, and reconnect when that happens
			if (cameraTransform == null && isFollowing)
			{
				OnStartFollowing();
			}

			// only follow is explicitly declared
			if (isFollowing) {
				Follow ();
			}
		}

		public void OnStartFollowing()
		{	      
			cameraTransform = Camera.main.transform;
			isFollowing = true;
			// we don't smooth anything, we go straight to the right camera shot
			Cut();
		}

		/// Follow the target smoothly
		void Follow()
		{
			if(GameManager.TopBotCam == true)
			{
				cameraTransform.position = new Vector3(this.transform.position.x, 0, transform.position.z) + new Vector3(0,16.5f,12.5f); 
			}
			else
			{
				cameraTransform.position = this.transform.position + new Vector3(0,16.5f,-12.5f); 
			}
		    cameraTransform.LookAt(this.transform.position + centerOffset);
	    }

		void Cut()
		{
			cameraTransform.position = this.transform.position + new Vector3(0,10,-5); 
			cameraTransform.LookAt(this.transform.position + centerOffset);
		}
	}
}