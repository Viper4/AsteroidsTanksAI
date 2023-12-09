using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankAgent : Tank
{
    public NeuralNetwork neuralNetwork;
    public TanksTrainer trainer;

    [SerializeField, Range(0.1f, 360f)] float fieldOfView = 80;
    [SerializeField] int numOfRaycasts = 9;
    [SerializeField] float rayDistance = 30;
    float rayBetweenAngle;
    [SerializeField] LayerMask wallLayers;
    [SerializeField] LayerMask enemyLayers;

    Vector3 lastPosition;
    float score = 0;
    float distanceScore = 0;
    float timeAlive = 0;
    public bool dead = false;

    float[] input;

    private void Awake()
    {
        Init();
        input = new float[numOfRaycasts * 2];

        rayBetweenAngle = fieldOfView / numOfRaycasts;
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (!dead)
            timeAlive += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!dead)
        {
            for (int i = 0; i < numOfRaycasts; i++)
            {
                Quaternion rayRotation = Quaternion.AngleAxis(i * rayBetweenAngle - fieldOfView * 0.5f, Vector3.up);

                // Input for driving
                Vector3 tankDirection = rayRotation * transform.forward;
                if (Physics.Raycast(turret.position, tankDirection, out RaycastHit hit, rayDistance, wallLayers, QueryTriggerInteraction.Collide))
                {
                    input[i] = (rayDistance - hit.distance) / rayDistance;
                    Debug.DrawLine(turret.position, hit.point, Color.yellow, Time.fixedDeltaTime);
                }

                // Input for shooting
                Vector3 turretDirection = rayRotation * turret.forward;
                RaycastHit[] hitResults = new RaycastHit[5];
                if(Physics.RaycastNonAlloc(turret.position, turretDirection, hitResults, rayDistance, enemyLayers, QueryTriggerInteraction.Collide) > 0)
                {
                    System.Array.Sort(hitResults, (x, y) => x.distance.CompareTo(y.distance));
                    for(int j = 0; j < hitResults.Length; j++)
                    {
                        if(hitResults[j].transform != null)
                        {
                            if(hitResults[j].transform.TryGetComponent<CustomLayers>(out var hitCustomLayers))
                            {
                                if(hitCustomLayers.gameLayer == customLayers.gameLayer)
                                {
                                    if(hitCustomLayers.TryGetComponent<TankBullet>(out var bullet) && bullet.owner == this)
                                    {
                                        input[i + numOfRaycasts] = 0;
                                        Debug.DrawLine(turret.position, hitResults[j].point, Color.green, Time.fixedDeltaTime);
                                        break;
                                    }
                                    input[i + numOfRaycasts] = (rayDistance - hitResults[j].distance) / rayDistance;
                                    Debug.DrawLine(turret.position, hitResults[j].point, Color.red, Time.fixedDeltaTime);
                                    break;
                                }
                            }
                            else
                            {
                                input[i + numOfRaycasts] = 0;
                                Debug.DrawLine(turret.position, hitResults[j].point, Color.green, Time.fixedDeltaTime);
                                break;
                            }
                        }
                    }
                }

                distanceScore += (lastPosition - transform.position).sqrMagnitude;
                lastPosition = transform.position;
            }

            float[] output = neuralNetwork.Forward(input);

            Move(new Vector2(output[0], output[1]));

            Rotate(0, output[2]);

            if (output[3] > 0)
            {
                Shoot();
            }
        }
    }

    public void SetDeath(bool value)
    {
        dead = value;
        score -= 10;

        if (dead && trainer != null)
        {
            Color deadColor = meshRenderers[0].material.color;
            deadColor.a = 0.25f;
            meshRenderers[0].material.color = deadColor;
            TogglePhysics(false);
            trainer.OnAgentDeath(customLayers.gameLayer);
            CancelInvoke();
        }
    }

    public float UpdateFitness()
    {
        score += distanceScore + timeAlive + kills * 15;
        neuralNetwork.fitness = score;
        return score;
    }

    public string GetStats()
    {
        return $"Score: {(dead ? distanceScore + timeAlive + kills * 15 - 10 : distanceScore + timeAlive + kills * 15)}\nDistance Score: {distanceScore}\nTime Alive: {timeAlive}\nKills: {kills}";
    }
}
