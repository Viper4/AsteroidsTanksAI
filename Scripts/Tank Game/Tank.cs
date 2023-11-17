using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tank : MonoBehaviour
{
    public CustomLayers customLayers;
    public MeshRenderer meshRenderer;
    Rigidbody _rigidbody;

    [SerializeField] Transform turret;
    [SerializeField] Transform barrel;

    [SerializeField] float speed;

    [SerializeField] float sensitivity = 10;
    [SerializeField] Vector2 pitchMinMax;
    [SerializeField] float barrelPitchMax;
    float yaw;
    float pitch;

    bool canShoot;
    [SerializeField] float shootDelay = 0.25f;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawn;
    public Transform bulletParent;

    public bool layerActive = true;
    public UnityEvent<bool> onShow;

    public void Init()
    {
        customLayers = GetComponent<CustomLayers>();
        meshRenderer = GetComponent<MeshRenderer>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        // Movement
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (inputDir != Vector2.zero)
        {
            float targetY = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, targetY, transform.eulerAngles.z));
        }

        float targetSpeed = speed * 0.5f * inputDir.magnitude;

        Vector3 velocityDirection = transform.forward;

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit middleHit, 1) && Physics.Raycast(transform.position + transform.forward, -transform.up, out RaycastHit frontHit, 1))
        {
            velocityDirection = frontHit.point - middleHit.point;
        }

        _rigidbody.velocity = targetSpeed * velocityDirection;

        // Camera rotation
        yaw += Input.GetAxis("Mouse X") * sensitivity / 4;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity / 4;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
        Camera.main.transform.eulerAngles = new Vector3(pitch, yaw, transform.eulerAngles.z);
        turret.localEulerAngles = new Vector3(0, yaw, 0);
        barrel.localEulerAngles = new Vector3(pitch, 0, 0);

        if (Input.GetKeyDown(KeyCode.Mouse0))
            Shoot();
    }

    public void Shoot()
    {
        if (canShoot)
            StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        canShoot = false;
        TankBullet bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation, bulletParent).GetComponent<TankBullet>();
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
