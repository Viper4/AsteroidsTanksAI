using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tank : MonoBehaviour
{
    public CustomLayers customLayers;
    public MeshRenderer[] meshRenderers;
    public Collider tankCollider;
    Rigidbody _rigidbody;

    [SerializeField] bool player;

    public Transform turret;
    [SerializeField] Transform barrel;

    [SerializeField] float speed;

    [SerializeField] float sensitivity = 10;
    [SerializeField] Vector2 pitchMinMax;
    float yaw;
    float pitch;

    bool canShoot = true;
    [SerializeField] float shootDelay = 0.25f;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawn;
    public Transform bulletParent;

    public int kills;
    public bool layerActive = true;
    public UnityEvent<bool> onShow;

    public void Init()
    {
        customLayers = GetComponent<CustomLayers>();
        _rigidbody = GetComponent<Rigidbody>();
        pitch = turret.eulerAngles.x;
        yaw = turret.eulerAngles.y;
    }

    void Start()
    {
        if(player)
        {
            Init();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            for(int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].enabled = false;
            }
        }
    }

    void Update()
    {
        if(player)
        {
            // Camera rotation
            Rotate(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
            Camera.main.transform.eulerAngles = new Vector3(pitch, yaw, transform.eulerAngles.z);
            Camera.main.transform.position = turret.position;

            Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            Move(inputDir);

            if (Input.GetKeyDown(KeyCode.Mouse0))
                Shoot();
        }
    }

    public void Rotate(float pitch, float yaw)
    {
        this.yaw += yaw * sensitivity;
        this.pitch += pitch * sensitivity;
        this.pitch = Mathf.Clamp(this.pitch, pitchMinMax.x, pitchMinMax.y);
        turret.eulerAngles = new Vector3(turret.eulerAngles.x, this.yaw, turret.eulerAngles.z);
        barrel.localEulerAngles = new Vector3(this.pitch, barrel.localEulerAngles.y, barrel.localEulerAngles.z);
    }

    public void Move(Vector2 input)
    {
        if(input != Vector2.zero)
        {
            float targetY = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + yaw;
            transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, targetY, transform.eulerAngles.z));
        }

        float targetSpeed = speed * 0.5f * input.magnitude;

        _rigidbody.velocity = targetSpeed * transform.forward;
    }

    public void Shoot()
    {
        if(canShoot)
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
        bulletMeshRenderer.material.color = meshRenderers[0].material.color;
        bulletMeshRenderer.material.SetColor("_EmissionColor", meshRenderers[0].material.color);

        TrailRenderer bulletTrailRenderer = bullet.GetComponent<TrailRenderer>();
        bulletTrailRenderer.enabled = layerActive;
        bulletTrailRenderer.material.color = meshRenderers[0].material.color;
        bulletTrailRenderer.material.SetColor("_EmissionColor", meshRenderers[0].material.color);

        bullet.GetComponent<CustomLayers>().gameLayer = customLayers.gameLayer;
        yield return new WaitForSeconds(shootDelay);
        canShoot = true;
    }

    public void Show(bool value)
    {
        layerActive = value;
        onShow?.Invoke(value);
        for(int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].enabled = value;
        }
    }

    public void TogglePhysics(bool value)
    {
        tankCollider.enabled = value;
        _rigidbody.isKinematic = !value;
    }
}
