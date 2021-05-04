using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BuilderBrawl
{
    public class Birds : MonoBehaviour
    {
        //attached to bird prefab, makes a bird appear and fly

        public GameObject birds;
        int dirZ;
        int posZ;
        int y = 10;
        Vector3 target;
        float speed = 7.5f;


        // Start is called before the first frame update
        void Start()
        {
            target = new Vector3(-50, 10, 0);
            birds.SetActive(false);
            StartCoroutine(Fly());
        }

        //function which makes a bird appear and gives it a target
        IEnumerator Fly()
        {
            while(true)
            {
                int R=Random.Range(2,5);
                yield return new WaitForSeconds(R);
                //wait between 30-40 seconds
                //activate bird, reset pos
                dirZ = Random.Range(-40,40);
                posZ = Random.Range(-40,40);
                target = new Vector3(-50, y, dirZ);
                birds.SetActive(true);
            }
        }

        // Update is called once per frame
        void Update()
        {
            //if active, fly
            //iff at pos, deactivate
            if(birds.activeInHierarchy)
            {
                if(birds.transform.position.x > -40)
                {
                    float step =  speed * Time.deltaTime;
                    birds.transform.position = Vector3.MoveTowards(birds.transform.position, target, step);
                    //birds.transform.position += new Vector3(-6,0,0) * Time.deltaTime;
                }
                else
                {
                    birds.transform.position = new Vector3(40,y,posZ);
                    birds.SetActive(false);
                }
            }
        }
    }
}