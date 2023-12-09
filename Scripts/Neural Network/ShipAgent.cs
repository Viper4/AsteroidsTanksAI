using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipAgent : Ship
{
    public NeuralNetwork neuralNetwork;
    public AsteroidsTrainer trainer;

    [SerializeField] int numOfRaycasts = 8;
    [SerializeField] float rayDistance = 35;
    float rayBetweenAngle;
    [SerializeField] LayerMask ignoreLayers;

    float score = 0;
    float timeAlive = 0;
    public bool dead = false;

    float[] input;

    private void Awake()
    {
        Init(); 
        input = new float[numOfRaycasts];

        rayBetweenAngle = 360 / numOfRaycasts;
    }

    private void Update()
    {
        if(!dead)
            timeAlive += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!dead)
        {
            for (int i = 0; i < numOfRaycasts; i++)
            {
                Vector3 direction = Quaternion.AngleAxis(i * rayBetweenAngle, Vector3.up) * transform.forward;
                RaycastHit[] hitResults = new RaycastHit[5];
                bool noHit = true;
                input[i] = 0;
                if(Physics.RaycastNonAlloc(transform.position, direction, hitResults, rayDistance, ~ignoreLayers, QueryTriggerInteraction.Collide) > 0)
                {
                    System.Array.Sort(hitResults, (x, y) => x.distance.CompareTo(y.distance));
                    for (int j = 0; j < hitResults.Length; j++)
                    {
                        if (hitResults[j].transform != null && hitResults[j].transform.GetComponent<CustomLayers>().gameLayer == customLayers.gameLayer)
                        {
                            input[i] = (rayDistance - hitResults[j].distance) / rayDistance;
                            Debug.DrawLine(transform.position, hitResults[j].point, Color.red, Time.fixedDeltaTime);
                            noHit = false;
                            break;
                        }
                    }
                }

                if(layerActive && noHit)
                    Debug.DrawLine(transform.position, transform.position + direction * rayDistance, Color.green, Time.fixedDeltaTime);
            }

            float[] output = neuralNetwork.Forward(input);

            Move((output[0] + 1) * 0.5f);

            Rotate(output[1]);

            if (output[2] > 0)
            {
                Shoot();
            }
        }
    }

    void SetDeath(bool value)
    {
        dead = value; 
        score -= 10;

        if (dead && trainer != null)
        {
            Color deadColor = meshRenderer.material.color;
            deadColor.a = 0.25f;
            meshRenderer.material.color = deadColor;
            TogglePhysics(false);
            trainer.OnAgentDeath(customLayers.gameLayer);
            CancelInvoke();
        }
    }

    public float UpdateFitness()
    {
        score += timeAlive + asteroidsDestroyed * 2;
        neuralNetwork.fitness = score;
        return score;
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CustomLayers>().gameLayer == customLayers.gameLayer)
            SetDeath(true);
    }

    public string GetStats()
    {
        return $"Score: {(dead ? timeAlive + asteroidsDestroyed * 2 - 10 : timeAlive + asteroidsDestroyed * 2)}\nTime Alive: {timeAlive}\nAsteroids Destroyed: {asteroidsDestroyed}";
    }
}
