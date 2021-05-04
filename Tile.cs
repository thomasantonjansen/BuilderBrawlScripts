using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BuilderBrawl
{
//script is attached to each tile prefab, collects neighbour tiles and checks continuously if there is an opportunity for building, if so handles visualization.

	public class Tile : MonoBehaviour
	{
		public GameObject[] tilePrefabs;
		MeshRenderer renderer;
		GUIcontroller gui;
		public CubeBox cubeBox;
		public Resource resource;
		public Resource cempty;
		public List<GameObject> neighbours = new List<GameObject> ();
		public CapsuleCollider col2d;	

		//dit laat voor deze tile zien of er gebouwd kan worden, en welk gebouw. puur voor visualisatie
		[SerializeField]
		public string[,] Buildables = new string[2, 6];

		//dit is de visual op de tile
		public GameObject visualBuildable;
		public GameObject visualBuildableWell;
		public GameObject visualBuildableCottage;
		public GameObject visualBuildableGranary;
		public GameObject visualBuildableWarehouse;
		public GameObject visualBuildableInn;
		public GameObject visualBuildableChapel;

		//deze ints laten zien of er meerdere opties zijn. Zo ja, dan moet je forms laten zien
		public int countWell = 0;
		public int countCottage = 0;
		public int countGranary = 0;
		public int countWarehouse = 0;
		public int countInn = 0;
		public int countChapel = 0;

		//belangrijkste lijst, die laat zien voor de huidige tile, welke gebouwen en gekoppelde tiles er mogelijk zijn.
		//dit wordt gebruikt als je de tile aanroept, en op "build" klikt, zodat je kan weten welke opties er getoont moeten worden.
		//dit wordt ook gebruikt om, als er een keuze wordt gemaakt, de relevante tiles aan te roepen en juiste data te verwijdren.
		public List<BuildableTile> tilesByBuildables = new List<BuildableTile> ();

		void Awake()
		{
			renderer = GetComponent<MeshRenderer>();
			int rand= Random.Range(0,2);
			GameObject GO = Instantiate(tilePrefabs[rand], transform.position, Quaternion.identity);
			GO.transform.parent = this.transform;
			//GO.transform.position += new Vector3(0,-0.8f,0);
			for(int i = 0; i < 6; i++)
			{
				Buildables[1,i] = "No";
			}
			Buildables[0,0] = "Well";
			Buildables[0,1] = "Cottage";
			Buildables[0,2] = "Granary";
			Buildables[0,3] = "Warehouse";
			Buildables[0,4] = "Inn";
			Buildables[0,5] = "Chapel";
			FindNeighbours ();
		}
		void Start()
		{
			col2d.transform.gameObject.SetActive(false);
			gui = GameObject.Find("Canvas").GetComponent<GUIcontroller>();
		}

		public void Interact(Resource c, PlayerInteraction player)
		{
			//als tile geen resource heeft dan is resource c
			if(resource.HasResource() == false && c != null)
			{
				player.SetResource(resource);
				resource = c;
				cubeBox.SetCubeIcon(resource.GetResourceCube());
				//GameManager.Instance.SetResource(resource.GetName(), this.gameObject.name, transform.parent.name);
				CheckBuilds();
			}
			else
			{
				return;
			}
		}

		//als tile buildable is, update array Buildables
		public void updateTileBuildables(string Building)
		{
			for(int i = 0; i < 6; i++)
			{
				if(Buildables[0,i] == Building)
				{
					Buildables[1,i] = "Yes";
				}
			}
			//Debug.Log(Buildables[1,0]);
			updateTileVisual();
		}

		//delete resources, arrays, buildables for each related tile. this tile is the tile that's not chosen but has its resource taken
		public void deleteRelatedTiles()
		{
			resource = cempty;
			for(int i = 0; i < 6; i++)
			{
				Buildables[1,i] = "No";
			}
			cubeBox.CloseCube();
			updateTileVisual();

			//related tiles is alle direct gelinkte tiles. Van die tiles moet je als buur verwijderen.
			foreach(GameObject go in neighbours)
			{
				go.GetComponent<Tile>().CheckBuilds();
			}
		}

		public void removeMeAsNeighbour(GameObject t, bool related)
		{
			foreach(GameObject n in neighbours)
			{
				if(n == t)
				{
					neighbours.Remove(n);
					return;
				}
			}
		}

		public void VisualizeForms(string Building)
		{
			//voor elke unieke buildingoptie roep je de GUIfunctie aan, waar je een lijst met alle voorgaande tiles meegeeft.
			//dan haal je die lijst gelijk leeg, en ga je verder totdat je weer nieuwe optie tegenkomt.
			List<GameObject> optionGO = new List<GameObject> ();
			int Counter = 0 ;
			for (int z = 0; z < tilesByBuildables.Count; z++)// bt in tilesByBuildables)
			{
				if(tilesByBuildables[z].building == Building)
				{
					//als geen match, maak dan counter hetzelfde als number, zodat volgende wel match is. zet huidige lijst door naar GUI, maak leeg en voeg de huidige GO toe.
					if(Counter != tilesByBuildables[z].number)
					{
						Counter = tilesByBuildables[z].number;
						gui.VisualizeFormsGUI(optionGO, Building);
						optionGO.Clear();
						optionGO.Add(tilesByBuildables[z].tile);
					}
					//als wel match, voeg toe en ga door. hij gaat dan door tot geen match meer
					else
					{
						optionGO.Add(tilesByBuildables[z].tile);
					}
				}
				if(tilesByBuildables[z].number > 0 && z==tilesByBuildables.Count-1)
				{
					gui.VisualizeFormsGUI(optionGO, Building);
					optionGO.Clear();
				}
			}
			Debug.Log(Building);
			gui.DeleteFormDupes(Building);
		}

		//build is geklikt, buildable form data is gemaakt, nu moeten de mogelijke building forms worden gevisualiseerd
		public void updateTileVisualizeBuildables()
		{
			foreach(BuildableTile bt in tilesByBuildables)
			{
				//Debug.Log(bt.building + "  gebouw, en tile is " + bt.tile + " en number is " + bt.number);
				if(bt.building == "Well")
				{
					countWell++;
				}
				if(bt.building == "Cottage")
				{
					countCottage++;
				}
				if(bt.building == "Granary")
				{
					countGranary++;
				}
				if(bt.building == "Warehouse")
				{
					countWarehouse++;
				}
				if(bt.building == "Inn")
				{
					countInn++;
				}
				if(bt.building == "Chapel")
				{
					countChapel++;
				}
			}
			if(countWell != 0)
				countWell = countWell / 2;
			if(countCottage != 0)
				countCottage = countCottage / 3;
			if(countGranary != 0)
				countGranary = countGranary / 4;
			if(countWarehouse != 0)
				countWarehouse = countWarehouse / 4;
			if(countInn != 0)
				countInn = countInn / 3;
			if(countChapel != 0)
				countChapel = countChapel / 4;

			gui.enableBuildButtons(countWell, countInn, countChapel, countCottage, countWarehouse, countGranary);
		}

		//update voor alles in array Builables de visual van de tile
		public void updateTileVisual()
		{
			bool OneActive = false;
			for(int i = 0; i < 6; i++)
			{
				if(Buildables[1,i] == "Yes")
				{
					OneActive = true;
				}
			}
			if(OneActive)
			{
				visualBuildable.SetActive(true);
				gui.activateBuildIcon(this);
			}
			else
			{
				visualBuildable.SetActive(false);
			}
		}

		//doorloop voor deze tile en elke buildable in de array "Buildables" welke tiles exact voor elk gebouw buildable zijn.
		//dit vanwege de tijdelijkheid; we hebben het enkel nodig als Build button is aangeroepen om te visualiseren.
		public void CheckBuildableForms()
		{
			if(Buildables[1,0] == "Yes") //Well
			{
				//for this buildable, check if there are neighbours with succes on each side, so max 4 times
				int z = 0;
				foreach(GameObject neighbour in neighbours)
				{

					Tile neighbourtile = neighbour.GetComponent<Tile>();
					if(neighbourtile.Buildables[1,0]=="Yes")
					{
						tilesByBuildables.Add(new BuildableTile("Well", z, neighbour));
						tilesByBuildables.Add(new BuildableTile("Well", z, this.gameObject));
						//succes! ga naar volgende
						z = z + 1;
					}
				}
			}
			if(Buildables[1,1] == "Yes") //Cottage
			{
				int z = 0;
				//for this buildable, check if there are neighbours 
				foreach(GameObject neighbour in neighbours)
				{
					Tile neighbourtile = neighbour.GetComponent<Tile>();
					if(neighbourtile.Buildables[1,1]=="Yes" && neighbourtile.resource.GetName() != resource.GetName())
					{
						//VANAF BLAUWE MIDDEN
						foreach(GameObject neighbour2 in neighbours)
						{
							Tile neighbour2tile = neighbour2.GetComponent<Tile>();
							//nog een andere buur, check of nog een andere kleur en buildable cottage
							//Debug.Log(neighbour2tile.resource.GetName()+neighbourtile.resource.GetName()+resource.GetName());
							if(neighbour2tile.Buildables[1,1]=="Yes" && neighbour2tile.resource.GetName() != neighbourtile.resource.GetName() && neighbour2tile.resource.GetName() != resource.GetName())
							{
								tilesByBuildables.Add(new BuildableTile("Cottage", z, neighbour2));
								tilesByBuildables.Add(new BuildableTile("Cottage", z, neighbour));
								tilesByBuildables.Add(new BuildableTile("Cottage", z, this.gameObject));
								//succes! ga naar volgende
								z = z + 1;
							}
						}
						//VANAF ZIJKANT
						foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
						{
							Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
							//nog een andere buur, check of nog een andere kleur en buildable cottage
							//Debug.Log(neighbour2tile.resource.GetName()+neighbourtile.resource.GetName()+resource.GetName());
							if(neighbourneighbourtile.Buildables[1,1]=="Yes" && neighbourneighbourtile.resource.GetName() != neighbourtile.resource.GetName() && neighbourneighbourtile.resource.GetName() != resource.GetName())
							{
								Debug.Log("yes n2");
								tilesByBuildables.Add(new BuildableTile("Cottage", z, neighbourneighbour));
								tilesByBuildables.Add(new BuildableTile("Cottage", z, neighbour));
								tilesByBuildables.Add(new BuildableTile("Cottage", z, this.gameObject));
								//succes! ga naar volgende
								z = z + 1;
							}
						}
					}
				}
			}
			if(Buildables[1,2] == "Yes") //Granary
			{
				int z = 0;
				//for this buildable, check if there are neighbours 
				foreach(GameObject neighbour in neighbours)
				{
					//i = i + 1;
					Tile neighbourtile = neighbour.GetComponent<Tile>();
					if(neighbourtile.Buildables[1,2]=="Yes" && neighbourtile.resource.GetName() != resource.GetName())
					{
						foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
						{
							Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
							//buur van buur, check of diagonaal van origineel
							if(neighbourneighbourtile.Buildables[1,2]=="Yes" && neighbourneighbour.transform.position.x != transform.position.x && neighbourneighbour.transform.position.z != transform.position.z)
							{
								foreach(GameObject neighbour2 in neighbours)
								{
									Tile neighbour2tile = neighbour2.GetComponent<Tile>();
									//nog een andere buur, check of niet dezelfde als eerdere directe buur
									if(neighbour2tile.Buildables[1,2]=="Yes" && neighbour2.transform.position.x != neighbour.transform.position.x && neighbour2.transform.position.z != neighbour.transform.position.z)
									{
										//check op vierkant: check origineel x of z hetzelfde, neighbourneighbour z of x hetzelfde
										if(neighbour2.transform.position.x == transform.position.x || neighbour2.transform.position.z == transform.position.z)
										{
											if(neighbour2.transform.position.x == neighbourneighbour.transform.position.x || neighbour2.transform.position.z == neighbourneighbour.transform.position.z)
											{
												//nu is het een vierkant. laatste check op 4 kleuren:
												string[] kleuren = new string[4];
												int wheatCounts = 0;
												int woodCounts = 0;
												int fireCounts = 0;
												kleuren[0] = resource.GetName();
												kleuren[1] = neighbourtile.resource.GetName();
												kleuren[2] = neighbourneighbourtile.resource.GetName();
												kleuren[3] = neighbour2tile.resource.GetName();
												for (int j = 0;j<4;j++)
												{
													if (kleuren[j] == "WheatResource")
														wheatCounts++;
													if (kleuren[j] == "FireResource")
														fireCounts++;
													if (kleuren[j] == "WoodResource")
														woodCounts++;
												}
												if(wheatCounts == 2 && woodCounts == 1 && fireCounts == 1)
												{
													//Debug.Log("het is de " + z + "e keer succes!");
													tilesByBuildables.Add(new BuildableTile("Granary", z, neighbourneighbour));
													tilesByBuildables.Add(new BuildableTile("Granary", z, neighbour));
													tilesByBuildables.Add(new BuildableTile("Granary", z, neighbour2));
													tilesByBuildables.Add(new BuildableTile("Granary", z, this.gameObject));
													//succes! ga naar volgende
													//continue;
													z = z + 1;
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if(Buildables[1,3] == "Yes") //Warehouse
			{
				int z = 0;
				foreach(GameObject neighbour in neighbours)
				{
					Tile neighbourtile = neighbour.GetComponent<Tile>();
					if(neighbourtile.Buildables[1,3]=="Yes" && neighbourtile.resource.GetName() != resource.GetName())
					{
						//VANAF ROOD HOEK
						foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
						{
							Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
							//check of 3 buren andere kleur zijn
							if(neighbourneighbourtile.Buildables[1,3]=="Yes" && neighbourneighbourtile.resource.GetName() != neighbourtile.resource.GetName() && neighbourneighbourtile.resource.GetName() != resource.GetName())
							{
								//check of buurbuur diagonaal zit
								if(neighbourneighbour.transform.position.x != transform.position.x && neighbourneighbour.transform.position.z != transform.position.z)//" && neighbourneighbourtile.resource.GetName() != neighbourtile.resource.GetName() && neighbourneighbourtile.resource.GetName() != resource.GetName())
								{
									//check of buurbuurbuur in lijn met buur en buurbuur
									foreach(GameObject neighbourneighbourneighbour in neighbourneighbourtile.neighbours)
									{
										Tile neighbourneighbourneighbourtile = neighbourneighbourneighbour.GetComponent<Tile>();
										//check of buur 3 en origineel = rood en diagonaal
										if(neighbourneighbourneighbourtile.Buildables[1,3]=="Yes" && neighbourneighbourneighbourtile.resource.GetName() == "FireResource" && neighbourneighbourneighbour.transform.position.x != transform.position.x && neighbourneighbourneighbour.transform.position.z != transform.position.z)
										{
											Debug.Log("yes warehouse loop 1");
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, neighbourneighbour));
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, neighbour));
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, neighbourneighbourneighbour));
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, this.gameObject));
											//succes! ga naar volgende
											z = z + 1;
										}
									}
								}

							}
						}

					//VANAF LIJN
						//VANAF ZIJKANT (geel of rood)
						foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
						{
							Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
							//check of 3 buren andere kleur zijn
							if(neighbourneighbourtile.Buildables[1,3]=="Yes" && neighbourneighbourtile.resource.GetName() != neighbourtile.resource.GetName() && neighbourneighbourtile.resource.GetName() != resource.GetName())
							{
								//check of 3 in 1 lijn zit
								if((neighbourneighbour.transform.position.x == transform.position.x && neighbourneighbour.transform.position.x == neighbour.transform.position.x) || (neighbourneighbour.transform.position.z == transform.position.z && neighbourneighbour.transform.position.z == neighbour.transform.position.z))
								{

									//ze zitten in 1 lijn, origineel is de uiteinden, dus check of buur2 rood en diagonaal van midden zit, 

									//of buurbuurbuur rood en diagonaal van origineel zit
									foreach(GameObject neighbourneighbourneighbour in neighbourneighbourtile.neighbours)
									{
										Tile neighbourneighbourneighbourtile = neighbourneighbourneighbour.GetComponent<Tile>();
										if(neighbourneighbourneighbourtile.Buildables[1,3]=="Yes" && neighbourneighbourneighbourtile.resource.GetName() == "FireResource" && neighbourneighbourneighbour.transform.position.x != transform.position.x && neighbourneighbourneighbour.transform.position.z != transform.position.z) 								
										{
											//uiteinde is rood en diagonaal, dus goed
											Debug.Log("yes warehouse loop 2");
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, neighbourneighbour));
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, neighbour));
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, this.gameObject));
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, neighbourneighbourneighbour));
											//succes! ga naar volgende
											z = z + 1;
											
										}
									}
									//of buur2 rood en diagonaal van origineel zit
									foreach(GameObject neighbour2 in neighbours)
									{
										Tile neighbour2tile = neighbour2.GetComponent<Tile>();
										if(neighbour2tile.Buildables[1,3]=="Yes" && neighbour2tile.resource.GetName() == "FireResource" && neighbourneighbour.transform.position.x != neighbour2.transform.position.x && neighbourneighbour.transform.position.z != neighbour2.transform.position.z) 								
										{
											//uiteinde is rood en diagonaal, dus goed
											Debug.Log("yes warehouse loop 3");
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, neighbourneighbour));
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, neighbour));
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, this.gameObject));
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, neighbour2));
											//succes! ga naar volgende
											z = z + 1;
											
										}
									}
								}
							}
						}
						//VANAF MIDDEN (bruin)
						foreach(GameObject neighbour2 in neighbours)
						{
							Tile neighbour2tile = neighbour2.GetComponent<Tile>();
							//check of 3 buren andere kleur zijn
							if(neighbour2tile.Buildables[1,3]=="Yes" && neighbour2tile.resource.GetName() != neighbourtile.resource.GetName() && neighbour2tile.resource.GetName() != resource.GetName())
							{
								//check of 3 in 1 lijn zit
								if((neighbour2.transform.position.x == transform.position.x && neighbour2.transform.position.x == neighbour.transform.position.x) || (neighbour2.transform.position.z == transform.position.z && neighbour2.transform.position.z == neighbour.transform.position.z))
								{
									//check of er een diagonale aan uiteinden zit, die rood is. van nieghbour2.
									foreach(GameObject neighbourneighbour2 in neighbour2tile.neighbours)
									{
										Tile neighbourneighbour2tile = neighbourneighbour2.GetComponent<Tile>();
										if(neighbourneighbour2tile.Buildables[1,3]=="Yes" && neighbourneighbour2tile.resource.GetName() == "FireResource" && neighbourneighbour2.transform.position.z != transform.position.z && neighbourneighbour2.transform.position.x != transform.position.x)
										{
											//diagonaal, rood en buidable, dus goed
											Debug.Log("yes warehouse loop 4");
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, neighbour2));
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, neighbour));
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, this.gameObject));
											tilesByBuildables.Add(new BuildableTile("Warehouse", z, neighbourneighbour2));
											//succes! ga naar volgende
											z = z + 1;
										}
									}
								}
							}
						}
					}
				}
			}
			if(Buildables[1,4] == "Yes") //Inn
			{
				int z = 0;
				foreach(GameObject neighbour in neighbours)
				{
					Tile neighbourtile = neighbour.GetComponent<Tile>();
					if(neighbourtile.Buildables[1,4]=="Yes" && neighbourtile.resource.GetName() != resource.GetName())
					{
						//VANAF ZIJKANT MIDDEN
						foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
						{
							Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
							//check of 3 buren andere kleur zijn
							if(neighbourneighbourtile.Buildables[1,4]=="Yes" && neighbourneighbourtile.resource.GetName() != neighbourtile.resource.GetName() && neighbourneighbourtile.resource.GetName() != resource.GetName())
							{
								//check of 3 in 1 lijn zit
								if((neighbourneighbour.transform.position.x == transform.position.x && neighbourneighbour.transform.position.x == neighbour.transform.position.x) || (neighbourneighbour.transform.position.z == transform.position.z && neighbourneighbour.transform.position.z == neighbour.transform.position.z))
								{
									Debug.Log("yes n2");
									tilesByBuildables.Add(new BuildableTile("Inn", z, neighbourneighbour));
									tilesByBuildables.Add(new BuildableTile("Inn", z, neighbour));
									tilesByBuildables.Add(new BuildableTile("Inn", z, this.gameObject));
									//succes! ga naar volgende
									z = z + 1;
								}
							}
						}
						//VANAF MIDDEN
						foreach(GameObject neighbour2 in neighbours)
						{
							Tile neighbour2tile = neighbour2.GetComponent<Tile>();
							//check of 3 buren andere kleur zijn
							if(neighbour2tile.Buildables[1,4]=="Yes" && neighbour2tile.resource.GetName() != neighbourtile.resource.GetName() && neighbour2tile.resource.GetName() != resource.GetName())
							{
								//check of 3 in 1 lijn zit
								if((neighbour2.transform.position.x == transform.position.x && neighbour2.transform.position.x == neighbour.transform.position.x) || (neighbour2.transform.position.z == transform.position.z && neighbour2.transform.position.z == neighbour.transform.position.z))
								{
									Debug.Log("yes n2");
									tilesByBuildables.Add(new BuildableTile("Inn", z, neighbour2));
									tilesByBuildables.Add(new BuildableTile("Inn", z, neighbour));
									tilesByBuildables.Add(new BuildableTile("Inn", z, this.gameObject));
									//succes! ga naar volgende
									z = z + 1;
								}
							}
						}
					}
				}
			}
			if(Buildables[1,5] == "Yes") //Chapel
			{
				int z = 0;
				foreach(GameObject neighbour in neighbours)
				{
					Tile neighbourtile = neighbour.GetComponent<Tile>();
					if(neighbourtile.Buildables[1,5]=="Yes" && neighbourtile.resource.GetName() != resource.GetName())
					{
						//VANAF BLAUW HOEK
						foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
						{
							Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
							//check of 3 buren andere kleur zijn
							if(neighbourneighbourtile.Buildables[1,5]=="Yes" && neighbourneighbourtile.resource.GetName() != neighbourtile.resource.GetName() && neighbourneighbourtile.resource.GetName() == resource.GetName())
							{
								//check of buurbuur diagonaal zit
								if(neighbourneighbour.transform.position.x != transform.position.x && neighbourneighbour.transform.position.z != transform.position.z)//" && neighbourneighbourtile.resource.GetName() != neighbourtile.resource.GetName() && neighbourneighbourtile.resource.GetName() != resource.GetName())
								{
									//check of buurbuurbuur in lijn met buur en buurbuur
									foreach(GameObject neighbourneighbourneighbour in neighbourneighbourtile.neighbours)
									{
										Tile neighbourneighbourneighbourtile = neighbourneighbourneighbour.GetComponent<Tile>();
										//check of buur 3 en origineel = rood en diagonaal
										if(neighbourneighbourneighbourtile.Buildables[1,5]=="Yes" && neighbourneighbourneighbourtile.resource.GetName() == neighbourtile.resource.GetName() && neighbourneighbourneighbour.transform.position.x != transform.position.x && neighbourneighbourneighbour.transform.position.z != transform.position.z)
										{
											Debug.Log("yes chapel loop 1");
											tilesByBuildables.Add(new BuildableTile("Chapel", z, neighbourneighbour));
											tilesByBuildables.Add(new BuildableTile("Chapel", z, neighbour));
											tilesByBuildables.Add(new BuildableTile("Chapel", z, neighbourneighbourneighbour));
											tilesByBuildables.Add(new BuildableTile("Chapel", z, this.gameObject));
											//succes! ga naar volgende
											z = z + 1;
										}
									}
								}

							}
						}

						//VANAF ZIJKANT MIDDEN
						foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
						{
							Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
							//check of buren andere kleur zijn, maar uiteinden dezelfde
							if(neighbourneighbourtile.Buildables[1,5]=="Yes" && neighbourneighbourtile.resource.GetName() != neighbourtile.resource.GetName() && neighbourneighbourtile.resource.GetName() == resource.GetName())
							{
								//moet niet dezelfde zijn
								if(neighbourneighbour.transform.position.x == transform.position.x && neighbourneighbour.transform.position.z == transform.position.z)
								{
								}
								else
								{
									//check of 3 in 1 lijn zit
									if((neighbourneighbour.transform.position.x == transform.position.x && neighbourneighbour.transform.position.x == neighbour.transform.position.x) || (neighbourneighbour.transform.position.z == transform.position.z && neighbourneighbour.transform.position.z == neighbour.transform.position.z))
									{
										//nu in 1 lijn, vanaf een blauwe hoek bekeken. nu nog check op uiteinden.
										foreach(GameObject neighbourneighbourneighbour in neighbourneighbourtile.neighbours)
										{
											Tile neighbourneighbourneighbourtile = neighbourneighbourneighbour.GetComponent<Tile>();
											if(neighbourneighbourneighbourtile.Buildables[1,5]=="Yes" && neighbourneighbourneighbourtile.resource.GetName() == "GlassResource" && neighbourneighbourneighbour.transform.position.z != transform.position.z && neighbourneighbourneighbour.transform.position.x != transform.position.x)
											{
												//diagonaal, blauw en buidable, dus goed
												Debug.Log("yes chapel loop 2");
												tilesByBuildables.Add(new BuildableTile("Chapel", z, neighbourneighbour));
												tilesByBuildables.Add(new BuildableTile("Chapel", z, neighbour));
												tilesByBuildables.Add(new BuildableTile("Chapel", z, this.gameObject));
												tilesByBuildables.Add(new BuildableTile("Chapel", z, neighbourneighbourneighbour));
												//succes! ga naar volgende
												z = z + 1;
											}
										}
										//nu nog check op uiteinden van directe buur.
										foreach(GameObject neighbour2 in neighbours)
										{
											Tile neighbour2tile = neighbour2.GetComponent<Tile>();
											//als directe buur glas, en diagonaal van buur, dan ook correct
											if(neighbour2tile.Buildables[1,5]=="Yes" && neighbour2tile.resource.GetName() == "GlassResource" && neighbour2.transform.position.z != neighbour.transform.position.z && neighbour2.transform.position.x != neighbour.transform.position.x)
											{
												//diagonaal, blauw en buidable, dus goed
												Debug.Log("yes warehouse loop 3");
												tilesByBuildables.Add(new BuildableTile("Chapel", z, neighbourneighbour));
												tilesByBuildables.Add(new BuildableTile("Chapel", z, neighbour));
												tilesByBuildables.Add(new BuildableTile("Chapel", z, this.gameObject));
												tilesByBuildables.Add(new BuildableTile("Chapel", z, neighbour2));
												//succes! ga naar volgende
												z = z + 1;
											}
										}
									}
								}

							}
						}
						//VANAF MIDDEN
						foreach(GameObject neighbour2 in neighbours)
						{
							Tile neighbour2tile = neighbour2.GetComponent<Tile>();
							//check of 3 buren andere kleur zijn
							if(neighbour2tile.Buildables[1,5]=="Yes" && neighbour2tile.resource.GetName() == neighbourtile.resource.GetName() && neighbour2tile.resource.GetName() != resource.GetName())
							{
								//moet niet dezelfde zijn
								if(neighbour2.transform.position.x == neighbour.transform.position.x && neighbour2.transform.position.z == neighbour.transform.position.z)
								{
								}
								else
								{
									//check of 3 in 1 lijn zit
									if((neighbour2.transform.position.x == transform.position.x && neighbour2.transform.position.x == neighbour.transform.position.x) || (neighbour2.transform.position.z == transform.position.z && neighbour2.transform.position.z == neighbour.transform.position.z))
									{
										//nu in 1 lijn, vanaf een blauwe bekeken. nu nog check op uiteinden.
										//check of er een diagonale aan uiteinden zit, die blauw is. van nieghbour2.
										foreach(GameObject neighbourneighbour2 in neighbour2tile.neighbours)
										{
											Tile neighbourneighbour2tile = neighbourneighbour2.GetComponent<Tile>();
											if(neighbourneighbour2tile.Buildables[1,5]=="Yes" && neighbourneighbour2tile.resource.GetName() == "GlassResource" && neighbourneighbour2.transform.position.z != transform.position.z && neighbourneighbour2.transform.position.x != transform.position.x)
											{
												//diagonaal, blauw en buidable, dus goed
												Debug.Log("yes chapel loop 4");
												tilesByBuildables.Add(new BuildableTile("Chapel", z, neighbour2));
												tilesByBuildables.Add(new BuildableTile("Chapel", z, neighbour));
												tilesByBuildables.Add(new BuildableTile("Chapel", z, this.gameObject));
												tilesByBuildables.Add(new BuildableTile("Chapel", z, neighbourneighbour2));
												//succes! ga naar volgende
												z = z + 1;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			updateTileVisualizeBuildables();
		}

			//deze functie checkt voor een bepaalde tile of er gebouwt kan worden
			public void CheckBuilds()
			{
				for(int i = 0; i < 6; i++)
				{
					Buildables[1,i] = "No";
				}
				if(resource != null)
				{
				//methode werkt als volgt: case of het een bepaalde resource is. Daarna check op neighbour tiles. 
				//tile heeft 4 directe neighbours. loop daar doorheen totdat er geen optie meer is. break dan loop.
				//Debug.Log(resource.GetName());
				if(resource.GetName() == "WoodResource")
				{
					
					//Check if neighbour is iron >> WELL POSSIBLE
					foreach(GameObject neighbour in neighbours)
					{
						Tile neighbourtile = neighbour.GetComponent<Tile>();
						if(neighbourtile.resource.GetName() == "IronResource")
						{
							//Debug.Log("Well possible!");
							Buildables[1,0] = "Yes";
							neighbourtile.updateTileBuildables("Well");
							//If so, check if neighbour of is blue >> INN POSSIBLE
							foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
							{
								Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
								if((neighbourneighbourtile.resource.GetName() == "GlassResource" && neighbourneighbourtile.transform.position.x == transform.position.x) || (neighbourneighbourtile.resource.GetName() == "GlassResource" && neighbourneighbourtile.transform.position.z == transform.position.z))
								{
									//Debug.Log("Inn possible!");
									Buildables[1,4] = "Yes";
									neighbourneighbourtile.updateTileBuildables("Inn");
									neighbourtile.updateTileBuildables("Inn");
									
								}
							}
						}

						//check for > GRANARY
						if(neighbourtile.resource.GetName() == "WheatResource")
						{
							//Debug.Log("wheatneighbour!!");
							foreach(GameObject neighbour2 in neighbours)
							{
								Tile neighbourtile2 = neighbour2.GetComponent<Tile>();
								if(neighbourtile2.resource.GetName() == "FireResource")
								{
									//als buren rood/geel en ernaast, dan mogelijk>> WAREHOUSE
									if((transform.position.x == neighbourtile.transform.position.x && transform.position.x == neighbourtile2.transform.position.x) || (transform.position.z == neighbourtile.transform.position.z && transform.position.z == neighbourtile2.transform.position.z))
									{
										//zit er naast, dus check wheat neighbour voor rood
										foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
										{
											Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
											if(neighbourneighbourtile.resource.GetName() == "FireResource")
											{
												if(transform.position.x != neighbourneighbour.transform.position.x && transform.position.z != neighbourneighbour.transform.position.z)
												{
													//Debug.Log("Warehouse possible!");
													Buildables[1,3] = "Yes";
													neighbourneighbourtile.updateTileBuildables("Warehouse");
													neighbourtile.updateTileBuildables("Warehouse");
													neighbourtile2.updateTileBuildables("Warehouse");
													
												}
											}
										}
									}

									//als buren rood/geel zijn, en ze zitten niet allebei naast hout, dan moet er dus 1tje boven en 1tje onder zitten.
									//nu moet alleen nog de diagonale ook wheat zijn. loop door neighbours van rood en check of die positie klopt.
									foreach(GameObject neighbourneighbour in neighbourtile2.neighbours)
									{
										Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
										if(neighbourneighbourtile.resource.GetName() == "WheatResource" && neighbourneighbour.transform.position.x == neighbour.transform.position.x && neighbourneighbour.transform.position.z == neighbour2.transform.position.z)
										{
											//Debug.Log("Granary possible!");
											Buildables[1,2] = "Yes";
											neighbourneighbourtile.updateTileBuildables("Granary");
											neighbourtile.updateTileBuildables("Granary");
											neighbourtile2.updateTileBuildables("Granary");
											break;
										}
										else if(neighbourneighbourtile.resource.GetName() == "WheatResource" && neighbourneighbour.transform.position.z == neighbour.transform.position.z && neighbourneighbour.transform.position.x == neighbour2.transform.position.x)
										{
											//Debug.Log("Granary possible!");
											Buildables[1,2] = "Yes";
											neighbourneighbourtile.updateTileBuildables("Granary");
											neighbourtile.updateTileBuildables("Granary");
											neighbourtile2.updateTileBuildables("Granary");
											break;
										}
										else
										{
										}
									}
								}
							}
						}
					}
				}

				if(resource.GetName() == "WheatResource")
				{
					foreach(GameObject neighbour in neighbours)
					{
						//Check if neighbour is blue + diagal is red, >> COTTAGE POSSIBLE
						Tile neighbourtile = neighbour.GetComponent<Tile>();
						if(neighbourtile.resource.GetName() == "GlassResource")
						{
							//If so, check if neighbour of is fire >> COTTAGE POSSIBLE
							foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
							{
								Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
								if(neighbourneighbourtile.resource.GetName() == "FireResource" && neighbourneighbourtile.transform.position.x != transform.position.x && neighbourneighbourtile.transform.position.z != transform.position.z)
								{
									Debug.Log("Cottage possible!");
									neighbourneighbourtile.updateTileBuildables("Cottage");
									neighbourtile.updateTileBuildables("Cottage");
									Buildables[1,1] = "Yes";
									
								}
							}
						}

						//Check if yellow+brown+red >> GRANARY POSSIBLE
						if(neighbourtile.resource.GetName() == "WheatResource")
						{
							foreach(GameObject neighbour2 in neighbours)
							{
								Tile neighbourtile2 = neighbour2.GetComponent<Tile>();
								if(neighbourtile2.resource.GetName() == "FireResource")
								{
									//als buren rood/geel en zitten in 1 lijn, dan niks
									if((transform.position.x == neighbourtile.transform.position.x && transform.position.x == neighbourtile2.transform.position.x) || (transform.position.z == neighbourtile.transform.position.z && transform.position.z == neighbourtile2.transform.position.z))
									{
									}
									//ze zitten dus niet naast elkaar, dus verder
									else
									{
										//Debug.Log("almost possible");
										//nu moet alleen nog de diagonale hout zijn. loop door neighbours van rood en check of die positie klopt.
										foreach(GameObject neighbourneighbour in neighbourtile2.neighbours)
										{
											Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
											if((neighbourneighbourtile.resource.GetName() == "WoodResource" && neighbourneighbour.transform.position.x != transform.position.x && neighbourneighbour.transform.position.z != transform.position.z))
											{
												Debug.Log("Granary possible!");
												neighbourneighbourtile.updateTileBuildables("Granary");
												neighbourtile2.updateTileBuildables("Granary");
												neighbourtile.updateTileBuildables("Granary");
												Buildables[1,2] = "Yes";
												
											}
										}
									}
								}
								if(neighbourtile2.resource.GetName() == "WoodResource")
								{
									//als buren rood/geel en zitten in 1 lijn, dan niks
									if((transform.position.x == neighbourtile.transform.position.x && transform.position.x == neighbourtile2.transform.position.x) || (transform.position.z == neighbourtile.transform.position.z && transform.position.z == neighbourtile2.transform.position.z))
									{
									}
									//ze zitten dus niet naast elkaar, dus verder
									else
									{
										//Debug.Log("almost possible");
										//nu moet alleen nog de diagonale hout zijn. loop door neighbours van rood en check of die positie klopt.
										foreach(GameObject neighbourneighbour in neighbourtile2.neighbours)
										{
											Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
											if((neighbourneighbourtile.resource.GetName() == "FireResource" && neighbourneighbour.transform.position.x != transform.position.x && neighbourneighbour.transform.position.z != transform.position.z))
											{
												Debug.Log("Granary possible!");
												neighbourneighbourtile.updateTileBuildables("Granary");
												neighbourtile2.updateTileBuildables("Granary");
												neighbourtile.updateTileBuildables("Granary");
												Buildables[1,2] = "Yes";
												
											}
										}
									}
								}
							}
						}

						//Check if  red+brown+red >> WAREHOUSE POSSIBLE
						Tile neighbourtileg = neighbour.GetComponent<Tile>();
						//als hout buur is
						if(neighbourtileg.resource.GetName() == "WoodResource")
						{
							foreach(GameObject neighbour2 in neighbours)
							{
								//als fire 2e buur is
								Tile neighbourtile2 = neighbour2.GetComponent<Tile>();
								if(neighbourtile2.resource.GetName() == "FireResource")
								{
									//doorloop buren van hout
									foreach(GameObject neighbourneighbour in neighbourtileg.neighbours)
									{
										//als die vuur is en op zelfde x of z as, dan is warehouse mogelijk!
										Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
										if(neighbourneighbourtile.resource.GetName() == "FireResource")
										{
											if((transform.position.x == neighbourneighbourtile.transform.position.x && transform.position.x != neighbour2.transform.position.x) || (transform.position.z != neighbour2.transform.position.z && transform.position.z == neighbourneighbourtile.transform.position.z))
											{
												Debug.Log("Warehouse possible!");
												neighbourneighbourtile.updateTileBuildables("Warehouse");
												neighbourtile2.updateTileBuildables("Warehouse");
												neighbourtileg.updateTileBuildables("Warehouse");
												Buildables[1,3] = "Yes";
											}
										}
									}
								}
							}
						}
					}
				}
				if(resource.GetName() == "FireResource")
				{
					//Check if neighbour is blue + diagal is yellow, >> COTTAGE POSSIBLE
					foreach(GameObject neighbour in neighbours)
					{
						//Check if neighbour is blue + diagal is red, >> COTTAGE POSSIBLE
						Tile neighbourtile = neighbour.GetComponent<Tile>();
						if(neighbourtile.resource.GetName() == "GlassResource")
						{
							//If so, check if neighbour of is fire >> COTTAGE POSSIBLE
							foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
							{
								Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
								if(neighbourneighbourtile.resource.GetName() == "WheatResource" && neighbourneighbourtile.transform.position.x != transform.position.x && neighbourneighbourtile.transform.position.z != transform.position.z)
								{
									//Debug.Log("Cottage possible!" + neighbourneighbourtile.transform.position + transform.position + resource.GetName() + neighbourneighbourtile.resource.GetName());
									Buildables[1,1] = "Yes";
									neighbourneighbourtile.updateTileBuildables("Cottage");
									neighbourtile.updateTileBuildables("Cottage");
									
								}
							}
						}

						//Check if neighbour is blue + diagal is red, >> WAREHOUSE POSSIBLE
						if(neighbourtile.resource.GetName() == "WoodResource")
						{
							//If so, check if neighbour of is fire >> COTTAGE POSSIBLE
							foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
							{
								Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
								if((neighbourneighbourtile.resource.GetName() == "WheatResource" && neighbourneighbourtile.transform.position.x == transform.position.x) || (neighbourneighbourtile.resource.GetName() == "WheatResource" && neighbourneighbourtile.transform.position.z == transform.position.z))
								{
									//geel zit in 1 lijn, check geel neighbour voor rood en diagonaal
									foreach(GameObject neighbourneighbourneighbour in neighbourneighbourtile.neighbours)
									{
										Tile neighbourneighbourneighbourtile = neighbourneighbourneighbour.GetComponent<Tile>();
										if(neighbourneighbourneighbourtile.resource.GetName() == "FireResource" && neighbourneighbourneighbour.transform.position.x != transform.position.x && neighbourneighbourneighbour.transform.position.z != transform.position.z)
										{
											Debug.Log("Warehouse possible!");
											neighbourneighbourneighbourtile.updateTileBuildables("Warehouse");
											neighbourneighbourtile.updateTileBuildables("Warehouse");
											neighbourtile.updateTileBuildables("Warehouse");
											Buildables[1,3] = "Yes";
											
										}
									}

								}
							}
						}

						//Check if yellow+brown aan weerskanten, en of wheatresource diagonaal zit en grenst aan bruin > GRANARY POSSIBLE
						if(neighbourtile.resource.GetName() == "WheatResource")
						{
							//Debug.Log("wheatneighbour!!");
							foreach(GameObject neighbour2 in neighbours)
							{
								Tile neighbourtile2 = neighbour2.GetComponent<Tile>();
								if(neighbourtile2.resource.GetName() == "WoodResource")
								{
									//als buren rood/geel en ernaast, dan mogelijk>> WAREHOUSE
									if((transform.position.x == neighbourtile.transform.position.x && transform.position.z == neighbourtile2.transform.position.z) || (transform.position.z == neighbourtile.transform.position.z && transform.position.x == neighbourtile2.transform.position.x))
									{
										//zitten aan weerskanten, dus check wheat neighbour voor rood
										foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
										{
											Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
											if(neighbourneighbourtile.resource.GetName() == "WheatResource")
											{
												if(transform.position.x != neighbourneighbour.transform.position.x && transform.position.z != neighbourneighbour.transform.position.z)
												{
													//nu hoeft de gele nog niet te grenzen aan bruin. dus check nog of z of x overeenkomt.
													if(neighbourtile2.transform.position.x == neighbourneighbour.transform.position.x || neighbourtile2.transform.position.z == neighbourneighbour.transform.position.z)
													{
														Debug.Log("Granary possible!");
														Buildables[1,2] = "Yes";
														neighbourtile.updateTileBuildables("Granary");
														neighbourneighbourtile.updateTileBuildables("Granary");
														neighbourtile2.updateTileBuildables("Granary");
														
													}

												}
											}
										}
									}
								}
							}

							//Geel is buur, check of bruin daar buur van is en diagonaal zit, en of rood buur is en diagonaal zit.
							foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
							{
								Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
								if(neighbourneighbourtile.resource.GetName() == "WoodResource" && neighbourneighbour.transform.position.x != transform.position.x && neighbourneighbour.transform.position.z != transform.position.z)
								{
									//nu zit bruin diagonaal, check op rood
									foreach(GameObject neighbourneighbourneighbour in neighbourneighbourtile.neighbours)
									{
										//derde buur. die moet rood zijn en diagonaal zitten met 1e rood
										Tile neighbourneighbourneighbourtile = neighbourneighbourneighbour.GetComponent<Tile>();
										if(neighbourneighbourneighbourtile.resource.GetName() == "FireResource" && neighbourneighbourneighbour.transform.position.x != transform.position.x && neighbourneighbour.transform.position.z != transform.position.z)
										{
											//nu nog in lijn met bruin en geel
											if(neighbourneighbourneighbour.transform.position.x == neighbour.transform.position.x || neighbourneighbourneighbour.transform.position.z == neighbour.transform.position.z)
											{
												Debug.Log("Warehouse possible!");
												neighbourtile.updateTileBuildables("Warehouse");
												neighbourneighbourtile.updateTileBuildables("Warehouse");
												neighbourneighbourneighbourtile.updateTileBuildables("Warehouse");
												Buildables[1,3] = "Yes";
												
											}
										}
									}
								}
							}
						}
					}
				}
				if(resource.GetName() == "IronResource")
				{
					//Brown neighbour > well possible
					foreach(GameObject neighbour in neighbours)
					{
						Tile neighbourtile = neighbour.GetComponent<Tile>();
						if(neighbourtile.resource.GetName() == "WoodResource")
						{
							Debug.Log("Well possible!");
							Buildables[1,0] = "Yes";
							neighbourtile.updateTileBuildables("Well");

							//Also glass neighbour and in same line > INN possible
							foreach(GameObject neighbour2 in neighbours)
							{
								Tile neighbourtile2 = neighbour2.GetComponent<Tile>();
								if(neighbourtile2.resource.GetName() == "GlassResource")
								{
									if((transform.position.x == neighbour2.transform.position.x && transform.position.x == neighbour.transform.position.x) || (transform.position.z == neighbour2.transform.position.z && transform.position.z == neighbour.transform.position.z))
									{
										Debug.Log("Inn possible!");
										neighbourtile.updateTileBuildables("Inn");
										neighbourtile2.updateTileBuildables("Inn");
										Buildables[1,4] = "Yes";
										
									}
								}
							}
						}
						//2 blue 1 iron >> CHAPEL possible
						if(neighbourtile.resource.GetName() == "GlassResource")
						{
							//Also glass neighbour and in same line > INN possible
							foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
							{
								if(neighbourneighbour.transform.position.x == transform.position.x && neighbourneighbour.transform.position.z == transform.position.z)
								{
									//is dezelfde als origineel
								}
								else
								{
									Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
									if(neighbourneighbourtile.resource.GetName() == "IronResource")
									{
										if((transform.position.x == neighbourneighbour.transform.position.x && transform.position.x == neighbour.transform.position.x) || (transform.position.z == neighbourneighbour.transform.position.z && transform.position.z == neighbour.transform.position.z))
										{
											//er zit nu dus in 1 rij: iron, glass, iron. nu zijn er 2 opties: er zit glas aan de ene kan of aan de andere kant. en diagonaal.
											
											//check glas bij de originele buur
											foreach(GameObject neighbour2 in neighbours)
											{
												if(neighbour2.transform.position.x != neighbour.transform.position.x && neighbour2.transform.position.z != neighbour.transform.position.z)
												{
													Tile neighbourtile2 = neighbour2.GetComponent<Tile>();
													if(neighbourtile2.resource.GetName() == "GlassResource")
													{
														Debug.Log("Chapel possible!");
														neighbourneighbourtile.updateTileBuildables("Chapel");
														neighbourtile.updateTileBuildables("Chapel");
														neighbourtile2.updateTileBuildables("Chapel");
														Buildables[1,5] = "Yes";
														
														//hij zit niet in lijn met andere blauw, dus chapel!
													}
												}
											}
											//check glas bij de andere grijze buur
											foreach(GameObject neighbour2 in neighbourneighbourtile.neighbours)
											{
												if(neighbour2.transform.position.x != neighbour.transform.position.x && neighbour2.transform.position.z != neighbour.transform.position.z)
												{
													Tile neighbourtile2 = neighbour2.GetComponent<Tile>();
													if(neighbourtile2.resource.GetName() == "GlassResource")
													{
														Debug.Log("Chapel possible!");
														neighbourneighbourtile.updateTileBuildables("Chapel");
														neighbourtile.updateTileBuildables("Chapel");
														neighbourtile2.updateTileBuildables("Chapel");
														Buildables[1,5] = "Yes";
														
														//hij zit niet in lijn met andere blauw, dus chapel!
													}
												}
											}
										}
									}
								}
							}
						}
					}				
				}
				if(resource.GetName() == "GlassResource")
				{
					
					foreach(GameObject neighbour in neighbours)
					{
						//cottage
						Tile neighbourtile = neighbour.GetComponent<Tile>();
						if(neighbourtile.resource.GetName() == "FireResource")
						{
							foreach(GameObject neighbour2 in neighbours)
							{
								Tile neighbour2tile = neighbour2.GetComponent<Tile>();
								if(neighbour2tile.resource.GetName() == "WheatResource" && neighbour2tile.transform.position.x != neighbourtile.transform.position.x && neighbour2tile.transform.position.z != neighbourtile.transform.position.z)
								{
									//als rood en geel aan weerskanten, cottage!
									Debug.Log("Cottage possible!");
									neighbourtile.updateTileBuildables("Cottage");
									neighbour2tile.updateTileBuildables("Cottage");
									Buildables[1,1] = "Yes";
									
								}
							}
						}

						//inn
						if(neighbourtile.resource.GetName() == "IronResource")
						{
							foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
							{
								Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
								if((neighbourneighbourtile.resource.GetName() == "WoodResource" && neighbourneighbourtile.transform.position.z == neighbourtile.transform.position.z && neighbourneighbourtile.transform.position.z == transform.position.z) || (neighbourneighbourtile.resource.GetName() == "WoodResource" && neighbourneighbourtile.transform.position.x == neighbourtile.transform.position.x && neighbourneighbourtile.transform.position.x == transform.position.x))
								{
									//als rood en geel aan weerskanten, cottage!
									Debug.Log("Inn possible!");
									neighbourneighbourtile.updateTileBuildables("Inn");
									neighbourtile.updateTileBuildables("Inn");
									Buildables[1,4] = "Yes";
									
								}
							}

							//vanaf blauwe middenblokje
							foreach(GameObject neighbour2 in neighbours)
							{
								if(neighbour2.transform.position.x == neighbour.transform.position.x && neighbour2.transform.position.z == neighbour.transform.position.z)
								{
									//is dezelfde
								}
								else
								{
									//check of in 1 lijn
									Tile neighbour2tile = neighbour2.GetComponent<Tile>();
									if((neighbour2tile.resource.GetName() == "IronResource" && neighbour2.transform.position.x == transform.position.x && neighbour2.transform.position.x == neighbour.transform.position.x)||(neighbour2tile.resource.GetName() == "IronResource" && neighbour2.transform.position.z == transform.position.z && neighbour2.transform.position.z == neighbour.transform.position.z))
									{
										//checken of 1e grijze buur een diagonale blauw heeft
										foreach(GameObject neighbourneighbour in neighbour2tile.neighbours)
										{
											Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
											if(neighbourneighbourtile.resource.GetName() == "GlassResource")//;// && neighbourneighbour.transform.position.x != transform.position.x && neighbourneighbour.transform.position.z != transform.position.z);
											{
												//Debug.Log("resource: "+ neighbourneighbourtile.resource.GetName() + "coords: " + neighbourneighbour.name + " naam, coords: " + neighbourneighbour.transform.position.z + ", " + transform.position.z);

												if(neighbourneighbour.transform.position.x != transform.position.x)
												{
													if(neighbourneighbour.transform.position.z != transform.position.z)
													{
														//Debug.Log("coords: " + neighbourneighbour.transform.position.x + ", " + transform.position.x + " en z is:  " + neighbourneighbour.transform.position.z + ", " + transform.position.z);
														Debug.Log("Chapel possible!");
														neighbourneighbourtile.updateTileBuildables("Chapel");
														neighbour2tile.updateTileBuildables("Chapel");
														neighbourtile.updateTileBuildables("Chapel");
														Buildables[1,5] = "Yes";
													}
												}						
											}
										}
									}
								}
							}

							//vanaf blauwe diagonale blokje
							foreach(GameObject neighbourneighbour in neighbourtile.neighbours)
							{
								//check op diagonaal
								if(neighbourneighbour.transform.position.x != transform.position.x && neighbourneighbour.transform.position.z != transform.position.z)
								{
									//check op blauw
									Tile neighbourneighbourtile = neighbourneighbour.GetComponent<Tile>();
									if(neighbourneighbourtile.resource.GetName() == "GlassResource")
									{
										//check of die een grijze heeft in lijn met neighbour
										foreach(GameObject neighbourneighbourneighbour in neighbourneighbourtile.neighbours)
										{
											if(neighbourneighbourneighbour.transform.position.x == neighbour.transform.position.x && neighbourneighbourneighbour.transform.position.z == neighbour.transform.position.z)
											{
												//zelfde als neighbour
											}
											else
											{
												if(neighbourneighbourneighbour.transform.position.x == neighbourneighbour.transform.position.x && neighbourneighbourneighbour.transform.position.x == neighbour.transform.position.x)
												{
													//is in 1 lijn
													Tile neighbourneighbourneighbourtile = neighbourneighbourneighbour.GetComponent<Tile>();
													if(neighbourneighbourneighbourtile.resource.GetName() == "IronResource")
													{
														Debug.Log("Chapel possible!");
														Buildables[1,5] = "Yes";
														neighbourneighbourneighbourtile.updateTileBuildables("Chapel");
														neighbourtile.updateTileBuildables("Chapel");
														neighbourneighbourtile.updateTileBuildables("Chapel");
														break;
													}
												}
												else if(neighbourneighbourneighbour.transform.position.z == neighbourneighbour.transform.position.z && neighbourneighbourneighbour.transform.position.z == neighbour.transform.position.z)
												{
													//is in 1 lijn
													Tile neighbourneighbourneighbourtile = neighbourneighbourneighbour.GetComponent<Tile>();
													if(neighbourneighbourneighbourtile.resource.GetName() == "IronResource")
													{
														Debug.Log("Chapel possible!");
														neighbourneighbourneighbourtile.updateTileBuildables("Chapel");
														neighbourtile.updateTileBuildables("Chapel");
														neighbourneighbourtile.updateTileBuildables("Chapel");
														Buildables[1,5] = "Yes";
														break;
													}
												}
											}

										}
									}
								}
							}
						}
					}
				}
				}
				updateTileVisual();
			}

			void FindNeighbours(){
				//Tile[] tiles = FindObjectsOfType<Tile> ();
				GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
				foreach (GameObject tile in tiles) {
					if (tile.gameObject.GetInstanceID() != gameObject.GetInstanceID()) {
						//Debug.Log(tile.transform.GetChild(1));
						if (col2d.bounds.Intersects (tile.transform.GetChild(1).gameObject.GetComponent<CapsuleCollider> ().bounds)) {
							if(tile.gameObject.transform.position.x == transform.position.x || tile.gameObject.transform.position.z == transform.position.z)
							{
								//Debug.Log ("[" + gameObject.name + "] found a neighbour: " + tile.gameObject.name);
								neighbours.Add (tile.gameObject);
							}

						}
					}
				}
			}

	}
}