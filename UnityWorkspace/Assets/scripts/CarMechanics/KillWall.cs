using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

namespace UnitySharpNEAT
{
    public class KillWall : MonoBehaviour
    {
        public GameObject theWall;
        public PathCreator pathCreator;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 20f;
        public float headStart = 1f;
        float distanceTravelled;

        public bool car = false;

        private void Start()
        {
            if (pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;
            }
        }

        public void ConnectToAlgoritm(NeatSupervisorCustom _neatSupervisor)
        {
            _neatSupervisor.EvolutionAlgorithm.UpdateEvent += new EventHandler(HandleUpdateEvent);
        }


        void HandleUpdateEvent(object sender, EventArgs e)
        {
            //reset wall
            StopAllCoroutines();
            theWall.SetActive(false);
            StartCoroutine("NewGen");
        }

        public IEnumerator NewGen()
        {
            distanceTravelled = 0f;
            if (car)
            {
                theWall.transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                theWall.transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
                theWall.SetActive(true);
            }
            else
            {
                theWall.SetActive(false);
            }
            yield return new WaitForSeconds(headStart);

            theWall.transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
            theWall.transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
            theWall.SetActive(true);

            while (true)
            {
                if (pathCreator != null)
                {
                    distanceTravelled += speed * 0.05f;
                    theWall.transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                    theWall.transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
                }
                yield return new WaitForSeconds(0.05f);
            }
        }

        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path
        void OnPathChanged()
        {
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }

        public void StopWall()
        {
            StopAllCoroutines();
        }
    }
}