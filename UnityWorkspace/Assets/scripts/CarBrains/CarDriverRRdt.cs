using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpNeat.Phenomes;
using PathCreation;

namespace UnitySharpNEAT
{
    public class CarDriverRRdt : UnitController
    {
        public CarModel carModel;
        public Radar radar;
        public Rigidbody rb;

        public PathCreator pathCreator;
        public int pathPoint = 0;
        public int pointCount = 1;

        private int groundHits = 0;
        [SerializeField]
        private float laneDist = 0f;
        public float laneImportance = 0.5f;

        public GameObject meshParrent;
        public GameObject colliderParrent;

        public float steer = 0.5f;
        public float speed = 0.5f;
        public bool debug = false;

        public float basicReward = 1f;
        public float targetSpeed = 30f;
        public float speedImportance = 1f;
        public float finnalDistanceImportance = 0f;
        public float stayOnRoadImportance = 0.5f;
        private float speedDiff = 0f;

        public float maxDistance = 10f;


        public LayerMask ground;
        public LayerMask road;
        public LayerMask props;

        protected override void UpdateBlackBoxInputs(ISignalArray inputSignalArray)
        {
            // Called by the base class on FixedUpdate

            // Feed inputs into the Neural Net (IBlackBox) by modifying its InputSignalArray
            // The size of the input array corresponds to NeatSupervisor.NetworkInputCount

            //inputSignalArray[0] = someSensorValue;
            //inputSignalArray[1] = someOtherSensorValue;
            //...
            if (pathCreator != null)
            {
                Vector3 currPoint = pathCreator.path.GetPoint(pathPoint % pointCount);
                float controlPoint = Vector3.Distance(transform.position, currPoint);

                if(controlPoint > maxDistance)
                {
                    IsActive = false;
                }

                float nextPoint = Vector3.Distance(transform.position, pathCreator.path.GetPoint((pathPoint + 1) % pointCount));
                float prewPoint = 1000f;
                if(pathPoint != 0)
                {
                    prewPoint = Vector3.Distance(transform.position, pathCreator.path.GetPoint((pathPoint - 1) % pointCount));
                }

                if (nextPoint < controlPoint)
                {
                    pathPoint++;

                    if(rb.velocity.magnitude - targetSpeed > 0)
                        speedDiff += rb.velocity.magnitude - targetSpeed;

                    laneDist += Mathf.Clamp(controlPoint - 0.5f, 0f,10f);
                }
                else
                {
                    if (prewPoint < controlPoint)
                    {
                        pathPoint--;
                        if (rb.velocity.magnitude - targetSpeed > 0)
                            speedDiff += rb.velocity.magnitude - targetSpeed;
                        laneDist += Mathf.Clamp(controlPoint-0.5f, 0f, 10f);
                    }


                }

                Vector3 localTarget;
                float targetAngle;
                if (pathPoint < pointCount + 10)
                {
                    localTarget = transform.InverseTransformPoint(pathCreator.path.GetPoint((pathPoint + 10) % pointCount));
                    targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
                    inputSignalArray[0] = targetAngle / 180f;
                    inputSignalArray[6] = Vector3.Distance(Vector3.zero, localTarget) / maxDistance;
                }
                else
                {
                    inputSignalArray[0] = 0f;
                    inputSignalArray[6] = -1f;
                }

                if (pathPoint < pointCount + 20)
                {
                    localTarget = transform.InverseTransformPoint(pathCreator.path.GetPoint((pathPoint + 20) % pointCount));
                    targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
                    inputSignalArray[1] = targetAngle / 180f;
                    inputSignalArray[7] = Vector3.Distance(Vector3.zero, localTarget) / maxDistance;
                }
                else
                {
                    inputSignalArray[1] = 0f;
                    inputSignalArray[7] = -1f;
                }

                if (pathPoint < pointCount + 30)
                {
                    localTarget = transform.InverseTransformPoint(pathCreator.path.GetPoint((pathPoint + 30) % pointCount));
                    targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
                    inputSignalArray[2] = targetAngle / 180f;
                    inputSignalArray[8] = Vector3.Distance(Vector3.zero, localTarget) / maxDistance;
                }
                else
                {
                    inputSignalArray[2] = 0f;
                    inputSignalArray[8] = -1f;
                }


            }
            else
            {
                inputSignalArray[0] = 0f;
                inputSignalArray[1] = 0f;
                inputSignalArray[2] = 0f;
                inputSignalArray[6] = 0f;
                inputSignalArray[7] = 0f;
                inputSignalArray[8] = 0f;
            }

            inputSignalArray[3] = (steer - 0.5f) * 2f;
            inputSignalArray[4] = (speed - 0.5f) * 2f;
            inputSignalArray[5] = -(rb.velocity.magnitude - targetSpeed)/ 100;

            int i = 9;
            if (radar != null)
                foreach (float ray in radar.Scan())
                {
                    inputSignalArray[i] = ((1 - ray));
                    i++;
                }


        }

        protected override void UseBlackBoxOutpts(ISignalArray outputSignalArray)
        {
            // Called by the base class after the inputs have been processed

            // Read the outputs and do something with them
            // The size of the array corresponds to NeatSupervisor.NetworkOutputCount

            //someMoveDirection = outputSignalArray[0];
            //someMoveSpeed = outputSignalArray[1];
            //...
            steer += (float)(outputSignalArray[0]) * 0.05f;
            steer = Mathf.Clamp01(steer);
            carModel.Input.Steer = (steer - 0.5f) * 2f;

            speed += (float)(outputSignalArray[1]) * 0.05f;
            speed = Mathf.Clamp01(speed);

            carModel.Input.Forward = speed / 2;
            //carModel.Input.Brake = 0f;

            //steer = (float)outputSignalArray[0];
            //carModel.Input.Steer = (float)outputSignalArray[0];

            //speed = (float)outputSignalArray[1];
            //carModel.Input.Forward = Mathf.Clamp01(((float)outputSignalArray[1]+1)/2) / 2f;

            carModel.Input.Brake = Mathf.Clamp01((float)outputSignalArray[2])/2f;
        }

        public override float GetFitness()
        {
            // Called during the evaluation phase (at the end of each trail)

            // The performance of this unit, i.e. it's fitness, is retrieved by this function.
            // Implement a meaningful fitness function here

            float finalDist = 0f;
            if (pathCreator != null)
                finalDist = Vector3.Distance(pathCreator.path.GetPoint(pathCreator.path.NumPoints - 1), transform.position);

            float toReturn = 1000 + pathPoint * basicReward + groundHits * stayOnRoadImportance - speedDiff * speedImportance - finalDist * finnalDistanceImportance - laneDist * laneImportance;

            if (toReturn < 0)
                toReturn = 0;
            return toReturn;
        }

        protected override void HandleIsActiveChanged(bool newIsActive)
        {
            // Called whenever the value of IsActive has changed

            // Since NeatSupervisor.cs is making use of Object Pooling, this Unit will never get destroyed. 
            // Make sure that when IsActive gets set to false, the variables and the Transform of this Unit are reset!
            // Consider to also disable MeshRenderers until IsActive turns true again.

            if (newIsActive == true)
            {
                rb.velocity = new Vector3();
                rb.angularVelocity = new Vector3();
                carModel.Input.Steer = 0f;
                carModel.Input.Forward = 0f;
                carModel.Input.Brake = 0f;

                GameObject[] temp = GameObject.FindGameObjectsWithTag("lap");

                if(temp.Length != 0)
                {
                    pathCreator = temp[0].GetComponent<PathCreator>();
                    pointCount = pathCreator.path.NumPoints;
                    pathPoint = 0;
                }


                if (pathCreator != null)
                {
                    transform.SetPositionAndRotation(pathCreator.path.GetPoint(0), Quaternion.Euler(0,-90,0));
                }
                else
                {
                    transform.rotation = new Quaternion();
                    transform.position = new Vector3();
                }




                meshParrent.SetActive(true);
                colliderParrent.SetActive(true);
                rb.useGravity = true;

                pathPoint = 0;
                groundHits = 0;
                speed = 0.0f;
                steer = 0.5f;
                speedDiff = 0f;
                laneDist = 0f;
            }
            else
            {
                meshParrent.SetActive(false);
                rb.velocity = new Vector3();
                rb.angularVelocity = new Vector3();
                carModel.Input.Steer = 0f;
                carModel.Input.Forward = 0f;
                carModel.Input.Brake = 0f;

                rb.useGravity = false;
                colliderParrent.SetActive(false);
                speed = 0f;

                //cmVC.Priority = 1;
            }
        }




        private void Start()
        {
            carModel = GetComponent<CarModel>();
            if (carModel == null)
                Destroy(gameObject);
            rb = GetComponent<Rigidbody>();
            
        }


        private void OnTriggerEnter(Collider other)
        {
            if (IncludesLayer(props, other.gameObject.layer))
            {
                IsActive = false;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(IncludesLayer(props, collision.gameObject.layer))
            {
                IsActive = false;
            }

        }

        private bool IsGrounded()
        {
            return Physics.Raycast(transform.position, -Vector3.up, 0.5f, LayerMask.NameToLayer("Ground"));
        }

        private void OnDrawGizmosSelected()
        {
            if (debug)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(pathCreator.path.GetPoint((pathPoint + 10) % pointCount), 0.25f);
                Gizmos.DrawSphere(pathCreator.path.GetPoint((pathPoint + 20) % pointCount), 0.25f);
                Gizmos.DrawSphere(pathCreator.path.GetPoint((pathPoint + 30) % pointCount), 0.25f);


            }

        }

        public bool IncludesLayer(LayerMask mask, int layer)
        {
            return (mask.value & 1 << layer) > 0;
        }
    }



}
