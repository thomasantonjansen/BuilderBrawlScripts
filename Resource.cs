using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Resource
{
	public ResourceAsset asset;

	public ResourceState state;

	public Resource (ResourceAsset a) {
		asset = a;
		state = ResourceState.Boat;
	}

	public bool HasResource()
	{
		if (asset == null)
			return false;
		else
			return true;
	}

	public GameObject GetResourceCube()
	{
		if (asset == null)
			return null;

		switch (state)
		{
			case ResourceState.Boat:
				return asset.boatCube;
		}
		
		return asset.boatCube;
	}

	public string GetName()
	{
		if (asset == null)
			return null;

		return asset.name;
	}
}

public enum ResourceState
{
	Boat,
	Player,
	Tile,
	Warehouse,
	Building
}