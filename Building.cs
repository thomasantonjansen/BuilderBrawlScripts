using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace BuilderBrawl
{
//script attached to a building prefab. On instantiation, gets a definition of building type. 
//Player is able to interact with it, and handles the levels, health, upgrades.
    public class Building : MonoBehaviour
    {
        //needs to be referenced at start
        PhotonView photonView;
        HealthManager healthmgr;
        Transform parent;

        //prefab child objects to be referenced
        public ArtilleryManager artillerymgr;
        public GameObject visualUpgradable;
        public CubeBox cubeBox1;
        public CubeBox cubeBox2;
        public CubeBox cubeBox3;
        public Text leveltext;

        //objects for for building upgrades
        public GameObject part1;
        public GameObject part2;
        public GameObject part3;
        public GameObject lv2;
        public GameObject lv3;

        //resources for warehouse
        public Resource resource1;
        public Resource resource2;
        public Resource resource3;

        //level of the building
        public int level;

        //defines the type of building
        private string type;

        //defines if upgrades are possible for this type
        private int levelEnabled = 1;


        void Start()
        {
            healthmgr = GetComponent<HealthManager>();
            //if parent exists, it is a localbuilding, so do stuff.
            parent = transform.parent;
            //appoint building stats & type tag

            //assign building values
            level = 0;
            type = transform.name;
            if(type == "Well(Clone)")
            {
                type = "House";
                transform.gameObject.tag = "House";
                if(photonView.IsMine)
                    healthmgr.increaseHealth(10);
                upgradeHouse();
            }
            else if(type == "Warehouse(Clone)")
            {
                type = "Warehouse";
                transform.gameObject.tag = "Warehouse";
                if(photonView.IsMine)
                    healthmgr.increaseHealth(20);
                upgradeWarehouse();
            }
            else if(type == "Cottage(Clone)")
            {
                type = "Wall";
                transform.gameObject.tag = "Wall";
                upgradeWall();
            }
            else if(type == "Chapel(Clone)")
            {
                type = "University";
                transform.gameObject.tag = "University";
                if(photonView.IsMine)
                    healthmgr.increaseHealth(50);
                leveltext.text="-";
            }
            else if(type == "Inn(Clone)")
            {
                type = "Artillery";
                transform.gameObject.tag = "Artillery";
                if(photonView.IsMine)
                    healthmgr.increaseHealth(10);
                upgradeArtillery();
            }
            else if(type == "Granary(Clone)")
            {
                type = "Farm";
                transform.gameObject.tag = "Farm";
                if(photonView.IsMine)
                    healthmgr.increaseHealth(10);
                upgradeFarm();
            }
        }
        
        public void upgradeBuilding()
        {
            healthmgr.BuildingUIdisableResource(reqRes);
            reqRes = "nothing";
            //decrease pop
            if(parent)
            {
                GameManager.Instance.population -= 1;
            }

                if(type=="Wall")
                {
                    upgradeWall();
                }
                if(type=="House")
                {
                    upgradeHouse();
                }
                if(type=="Farm")
                {
                    upgradeFarm();
                }
                if(type=="Warehouse")
                {
                    upgradeWarehouse();
                }
                if(type=="Artillery")
                {
                    upgradeArtillery();
                }	
        }

        public void checkIfUpgradable()
        {
            if(photonView.IsMine)
            {
                //check of upgradable level nog aligned is met eigen level. als hoger, dan enabled.
                for(int i = 0; i < 5; i++)
                {
                    if(GameManager.Instance.levelEnabled[0,i] == type)
                    {
                        levelEnabled = int.Parse(GameManager.Instance.levelEnabled[1,i]);
                        break;
                    }
                }
                //Debug.Log(level + "is level en level mogelijk is " + levelEnabled);
                if(level < levelEnabled)
                {
                    visualUpgradable.SetActive(true);
                    generateRequiredResource();
                    healthmgr.BuildingUIenableResource(reqRes);
                }
                else
                {
                    visualUpgradable.SetActive(false);
                }
            }

        }

        private void generateRequiredResource()
        {
            int i = Random.Range(0,5);
            if(i == 0)
            {
                reqRes = "GlassResource";
            }
            else if(i == 1)
            {
                reqRes = "WoodResource";
            }
            else if(i == 2)
            {
                reqRes = "IronResource";
            }
            else if(i == 3)
            {
                reqRes = "WheatResource";
            }
            else if(i == 4)
            {
                reqRes = "FireResource";
            }
        }

        public void upgradeWall()
        {
            level ++;
            leveltext.text=level.ToString();
            if(photonView.IsMine)
                healthmgr.increaseHealth(100);
            checkIfUpgradable();
            if(level == 2)
            {
                part1.SetActive(false);
                part2.SetActive(false);
                part3.SetActive(false);
                lv2.SetActive(true);
            }
            if(level == 3)
            {
                lv2.SetActive(false);
                lv3.SetActive(true);
            }
        }

        //het huis geeft bonus aan population, op basis van level farm
        public void upgradeHouse()
        {
            if(parent)
                GameManager.Instance.houses += 1;
            level ++;
            leveltext.text=level.ToString();
            if(photonView.IsMine)
                healthmgr.increaseHealth(10);
            int populationIncrease = GameManager.Instance.farmlevel + level;
            if(parent)
                GameManager.Instance.population += populationIncrease;
            checkIfUpgradable();
            if(level == 2)
            {
                part1.SetActive(false);
                part2.SetActive(false);
                lv2.SetActive(true);
            }
            if(level == 3)
            {
                lv2.SetActive(false);
                lv3.SetActive(true);
            }
        }

        //de farm die gaat met upgrade bonus geven aan population, op basis van aantal huizen
        public void upgradeFarm()
        {
            level ++;
            leveltext.text=level.ToString();
            if(photonView.IsMine)
                healthmgr.increaseHealth(10);
            if(parent)
                GameManager.Instance.farmlevel = level;
            int populationIncrease = GameManager.Instance.houses;
            if(parent)
                GameManager.Instance.population += populationIncrease;
            checkIfUpgradable();
            if(level == 2)
            {
                part1.SetActive(false);
                part2.SetActive(false);
                lv2.SetActive(true);
            }
            if(level == 3)
            {
                lv2.SetActive(false);
                lv3.SetActive(true);
            }
        }

        public void upgradeWarehouse()
        {  
            level ++ ;
            leveltext.text=level.ToString();
            if(photonView.IsMine)
                healthmgr.increaseHealth(10);
            if(level == 2)
            {
                part1.SetActive(false);
                part2.SetActive(false);
                lv2.SetActive(true);
            }
            if(level == 3)
            {
                lv2.SetActive(false);
                lv3.SetActive(true);
            }

            checkIfUpgradable();
            //enable nog extra storage visual
        }

        //maak interaction mogelijk
        public void upgradeArtillery()
        {
            level++;
            leveltext.text=level.ToString();
            if(photonView.IsMine)
                healthmgr.increaseHealth(10);
            checkIfUpgradable();
            if(level == 2)
            {
                part1.SetActive(false);
                part2.SetActive(false);
                lv2.SetActive(true);
            }
            if(level == 3)
            {
                lv2.SetActive(false);
                lv3.SetActive(true);
            }
        }

        public void InteractWarehouse(Resource c, PlayerInteraction player, string storageName)
        {
            if(storageName == "GetResourceBtn1")
            {
                player.SetResource(resource1);
                resource1 = c;
                cubeBox1.SetCubeIcon(resource1.GetResourceCube());
            }
            else if(storageName == "GetResourceBtn2")
            {
                player.SetResource(resource2);
                resource2 = c;
                cubeBox2.SetCubeIcon(resource2.GetResourceCube());
            }
            else
            {
                player.SetResource(resource3);
                resource3 = c;
                cubeBox3.SetCubeIcon(resource3.GetResourceCube());
            }
        }

        public void Interact(Resource c, PlayerInteraction player)
        {
            Debug.Log("interacted");
            //warehouse eerst nog met 1 blokje, misschien later warehouse met meerdere blokjes
            if(type == "Warehouse")
            {
                if(level == 1)
                {
                    player.SetResource(resource1);
                    resource1 = c;
                    cubeBox1.SetCubeIcon(resource1.GetResourceCube());
                }
            }
            else
            {
                return;
            }
        }
    }
}