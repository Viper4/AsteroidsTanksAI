using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Ship : MonoBehaviour
{
    public CustomLayers customLayers;
    public MeshRenderer meshRenderer;
    Rigidbody _rigidbody;

    [SerializeField] bool player;

    [SerializeField] float moveSpeed = 1;
    [SerializeField] float rotateSpeed = 5;

    bool canShoot = true;
    [SerializeField] float shootDelay = 0.1f;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawn;

    public int asteroidsDestroyed;
    public bool layerActive = true;
    public UnityEvent<bool> onShow;


    public Transform bulletParent;

    public void Init()
    {
        customLayers = GetComponent<CustomLayers>();
        meshRenderer = GetComponent<MeshRenderer>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (player)
            Init();
    }

    void Update()
    {
        if (player)
        {
            if (Input.GetKey(KeyCode.W))
            {
                Move(1);
            }

            float rotateValue = 0;
            if (Input.GetKey(KeyCode.A))
            {
                rotateValue -= 1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                rotateValue += 1;
            }
            Rotate(rotateValue);

            if (Input.GetKeyDown(KeyCode.Mouse0))
                Shoot();
        }
    }

    public void Move(float value)
    {
        _rigidbody.AddForce(Mathf.Clamp01(value) * moveSpeed * Time.deltaTime * transform.forward, ForceMode.Force);
    }

    public void Rotate(float value)
    {
        transform.Rotate(0, value * rotateSpeed * Time.deltaTime, 0);
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CustomLayers>().gameLayer == customLayers.gameLayer)
            Destroy(gameObject);
    }

    public void Shoot()
    {
        if(canShoot)
            StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        canShoot = false;
        ShipBullet bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation, bulletParent).GetComponent<ShipBullet>();
        bullet.owner = this;
        onShow.AddListener(bullet.Show);

        MeshRenderer bulletMeshRenderer = bullet.GetComponent<MeshRenderer>();
        bulletMeshRenderer.enabled = layerActive;
        bulletMeshRenderer.material.color = meshRenderer.material.color;
        bulletMeshRenderer.material.SetColor("_EmissionColor", meshRenderer.material.color);

        TrailRenderer bulletTrailRenderer = bullet.GetComponent<TrailRenderer>();
        bulletTrailRenderer.enabled = layerActive;
        bulletTrailRenderer.material.color = meshRenderer.material.color;
        bulletTrailRenderer.material.SetColor("_EmissionColor", meshRenderer.material.color);

        bullet.GetComponent<CustomLayers>().gameLayer = customLayers.gameLayer;
        yield return new WaitForSeconds(shootDelay);
        canShoot = true;
    }

    public void Show(bool value)
    {
        layerActive = value;
        onShow?.Invoke(value);
        GetComponent<MeshRenderer>().enabled = value;
    }

    public void TogglePhysics(bool value)
    {
        GetComponent<Collider>().enabled = value;
        _rigidbody.isKinematic = !value;
    }
}
