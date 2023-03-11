using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 15f;
    public float jumpPower = 15f;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] Grenades;
    public int hasGrenade;
    public GameObject grenadeObj;
    public Camera followrCamera;

    public int ammo;
    public int coin;
    public int health;

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    float hAxis;
    float vAxis;
    float fireDelay;

    bool wDown;
    bool jDown;
    bool fDown;
    bool gDown;
    bool rDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isFireReady = true;
    bool isBorder;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rb;
    Animator animator;

    GameObject nearObj;
    Weapon equipWeapon;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();
        Grenade();
        Ineteraction();
        Swap();
        Attack();
        Reload();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
        {
            moveVec = dodgeVec;
        }

        if (isSwap || !isFireReady || isReload)
        {
            moveVec = Vector3.zero;
        }

        if (!isBorder)
        {
            transform.position += moveVec * speed * (wDown ? 0.1f : 1f) * Time.deltaTime;
        }

        animator.SetBool("isRun", moveVec != Vector3.zero);
        animator.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        //#1 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        //#2 마우스에 의한 회전
        if (fDown)
        {
            Ray ray = followrCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }

        }
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            animator.SetBool("isJump", true);
            animator.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Grenade()
    {
        if (hasGrenade == 0)
        {
            return;
        }

        if (gDown && !isReload && !isSwap)
        {
            Ray ray = followrCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rbGrenade = instantGrenade.GetComponent<Rigidbody>();
                rbGrenade.AddForce(nextVec, ForceMode.Impulse);
                rbGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenade--;
                Grenades[hasGrenade].SetActive(false);
            }
        }
    }

    void Attack()
    {
        if (equipWeapon == null)
        {
            return;
        }

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            animator.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if (equipWeapon == null) { return; }
        if (equipWeapon.type == Weapon.Type.Melee) { return; }
        if (ammo == 0) { return; }
        if (rDown && !isJump && !isDodge && !isSwap && isFireReady)
        {
            animator.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 3f);// Reload time
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap)
        {
            dodgeVec = moveVec;
            speed *= 2f;
            animator.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.7f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void Swap()
    {
        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge && hasWeapons[weaponIndex] && equipWeapon != weapons[weaponIndex])
        {
            if (equipWeapon != null) { equipWeapon.gameObject.SetActive(false); }

            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            animator.SetTrigger("doSwap");
            isSwap = true;

            Invoke("SwapOut", 0.5f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Ineteraction()
    {
        if (iDown && nearObj != null && !isJump && !isDodge)
        {
            if (nearObj.CompareTag("Weapon"))
            {
                Item item = nearObj.GetComponent<Item>();
                int weaponsIndex = item.value;
                hasWeapons[weaponsIndex] = true;

                Destroy(nearObj);
            }

        }
    }

    void FreezeRotation()
    {
        rb.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 7, Color.red);
        isBorder = Physics.Raycast(transform.position, transform.forward, 7, LayerMask.GetMask("Wall"));
    }
     void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            animator.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                    {
                        ammo = maxAmmo;
                    }
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                    {
                        coin = maxCoin;
                    }
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                    {
                        health = maxHealth;
                    }
                    break;
                case Item.Type.Grenade:
                    if (hasGrenade == maxHasGrenades)
                        return;
                    Grenades[hasGrenade].SetActive(true);
                    hasGrenade += item.value;
                    break;

            }
            Destroy(other.gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            nearObj = other.gameObject;
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Weapon"))
            nearObj = null;
    }
}
