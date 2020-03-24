using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMParticleSpawner : MonoBehaviour
{
    public GameObject particlePrefab;

    public Transform Target;

    public int ParticleAmount = 5;

    public int ParticleInstance = 20;

    public BezierCurve Curve;

    public bool OnClick = false;

    List<FMParticleMoveToPoint> particles = new List<FMParticleMoveToPoint>();
    int particleIndex = 0;

    private void Start()
    {
        //GameObject particlePrefab = Resources.Load("Prefabs/CoinParticle") as GameObject;

        GameObject particlesObt = new GameObject("particles");
        particlesObt.transform.parent = transform;
        particlesObt.gameObject.layer = gameObject.layer;//LayerMask.NameToLayer("UI");

        for (int i = 0; i < ParticleInstance; i++)
        {
            GameObject particle = Instantiate(particlePrefab, transform);
            var particleScript = particle.AddComponent<FMParticleMoveToPoint>();
            particleScript.Destination = Target;
            particle.gameObject.layer = gameObject.layer;//LayerMask.NameToLayer("UI");
            particleScript.SetBezierCurve(Curve);
            particles.Add(particleScript);
            particle.transform.parent = particlesObt.transform;
        }
    }

    void Update()
    {
        if (OnClick && Input.GetMouseButtonDown(0))
        {
            SpawnParticles(UICamera.mainCamera.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    public void SpawnParticles(Vector3 startPosition)
    {
        SpawnParticles(startPosition, ParticleAmount);
    }

    public void SpawnParticles(Vector3 startPosition, int amount)
    {
        startPosition.z = startPosition.z != 0 ? 0 : startPosition.z;
        for (int i = 0; i < amount; i++)
        {
            var p = particles[particleIndex];
            p.InitPosition(startPosition);
            particleIndex++;
            if (particleIndex >= ParticleInstance)
            {
                particleIndex = 0;
            }
        }
    }

    /// <summary>
    /// sets the destination of the particles in case the script is being instantiated by code
    /// </summary>
    /// <param name="target"></param>
    public void SetTarget(Transform target)
    {
        Target = target;
        particles.ForEach(x => x.Destination = Target);
    }
}
