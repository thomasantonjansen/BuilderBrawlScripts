using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace BuilderBrawl
{
	public class PlayerInteraction : MonoBehaviour
	{
		public GameObject target;
		Tile tile;
		Building building;
		PhotonView photonView;	

		//interaction object references
		public KeyCode interactKey;

		//prefab child holding other resource GFX
		public CubeBox cubeBox;

		//tap button script
		public TapBtn tapBtn;

		//resources collected by player
		[SerializeField]
		private Resource resource;
		[SerializeField]
		private Resource resourceEmpty;

		//properties to set the selection material of interacted objects
		private MaterialPropertyBlock block;
		public Shader shaderOld;
		public Shader shaderNew;
		Color oldColor;
		Color newColor;

		public void Start()
		{
			photonView = PhotonView.Get(this);
			if(photonView.IsMine)
			{
				tapBtnObject = GameObject.Find("TapBtn");
				tapBtn = tapBtnObject.GetComponent<TapBtn>();
			}
		}

		//handles player interaction
		private void Update()
		{
			if(photonView.IsMine)
			{
				//check input
				if (Input.GetKeyDown(KeyCode.Space) || tapBtn.tapPressed)
				{
					if(target == null)
					{
						Debug.Log("empty target");
						return;
					}

					//resource interact script
					Resource resource = target.GetComponent<Resource>();
					if (resource != null)
					{
						//Debug.Log(resource.resource.GetName());
						resource.Interact(resource, this);
					}

					//tile interact script
					Tile tile = target.GetComponent<Tile>();
					if (tile != null)
					{
						tile.Interact(resource, this);
					}

					//tile interact script
					Building building = target.GetComponent<Building>();
					if (building != null)
					{
						building.Interact(resource, this);
					}
					tapBtn.tapPressed=false;
				}
			}
		}

		//destroy resource the player is holding
		public void destroyResource()
		{
			SetResource(resourceEmpty);
		}

		//set resource to player
		public void SetResource(Resource c)
		{
			resource = c;
			DisplayInventory();
		}

		//method to handle player resource inventory
		void DisplayInventory ()
		{
			if (resource.HasResource())
			{
				cubeBox.SetCubeIcon(resource.GetResourceCube());
			}
			else
			{
				cubeBox.CloseCube();
			}
		}

		//assign correct target
		private void OnTriggerEnter(Collider col)
		{
			if(photonView.IsMine)
			{
				if (target != col.gameObject && target != null)
				{
					Deselect();
				}
				if(col.gameObject != null && col.gameObject.tag=="Resource" || col.gameObject.tag=="Tile" || col.gameObject.tag=="Warehouse" || col.gameObject.tag=="University" || col.gameObject.tag=="Farm" || col.gameObject.tag=="House" || col.gameObject.tag=="Wall" || col.gameObject.tag=="Artillery")
				{
					//als het een tile is die een resource heeft, maar niet buildable is, en je hebt zelf geen resource, dan geen selection.
					if(col.gameObject.tag=="Tile")
					{
						//als wel buildable, en je hebt geen resource, dan wel target.
						if(col.gameObject.GetComponent<Tile>().visualBuildable.activeInHierarchy && resource.HasResource() == false)
							target = col.gameObject;
						else if(resource.HasResource())
							target = col.gameObject;
					}
					else
					{
						target = col.gameObject;
					}
					if(target!=null)//!resource.HasResource() && target != null)
					{
						selectionTextConvert(target.name);
					}
				}
			}
			if(target != null)
			{
				//check if player has tile
				if(target.tag == "Tile" && resource.HasResource() == false && target.GetComponent<Tile>().resource.HasResource() == false)
				{
				}
				else
				{
					//activate tap button
					if(target.tag == "Resource" || target.tag == "Tile" || target.tag == "Warehouse" || target.tag == "University")
						tapBtnObject.SetActive(true);

					//change shader
					MeshRenderer[] mrs = target.GetComponentsInChildren<MeshRenderer>();
					foreach (MeshRenderer mr in mrs)
					{
						newColor = mr.material.GetColor("_BaseColor") + new Color(0.6f, 0.6f, 0.53f, 1);
						block = new MaterialPropertyBlock();
						block.SetColor("_BaseColor", newColor);
						mr.SetPropertyBlock(block);
					}
				}
			}
		}
		//assign correct target
		private void OnTriggerStay(Collider col)
		{
			if(photonView.IsMine)
			{
				if(!target)
				{
					OnTriggerEnter(col);
				}
			}
		}
		//assign correct target
		private void OnTriggerExit(Collider col)
		{
			if(photonView.IsMine)
			{
				if(target == col.gameObject)
				{
					Deselect();
					target = null;
				}
			}
		}
		//assign correct target
		void Deselect()
		{
			dropImg.SetActive(false);
			MeshRenderer[] mrs = target.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer mr in mrs)
			{
				block = new MaterialPropertyBlock();
				block.SetColor("_BaseColor", mr.material.GetColor("_BaseColor"));
				mr.SetPropertyBlock(block);
			}
		}

	}
}