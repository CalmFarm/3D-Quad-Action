using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    Rigidbody rb;
    BoxCollider boxCollider;
    Material material;
    NavMeshAgent nav;
    Animator anim;

    public Transform target;
    public int maxHealth;
    public int curHealth;
    public bool isChase;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        material = GetComponentInChildren<MeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        Invoke("ChaseStart", 2);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }
    void Update()
    {
        if (isChase)
        {
            nav.SetDestination(target.position);
        }
    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

        }
    }

    void FixedUpdate()
    {
        FreezeVelocity();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVector = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactVector, false));
        }
        else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVector = transform.position - other.transform.position;

            Destroy(other.gameObject);
            StartCoroutine(OnDamage(reactVector, false));
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVector = transform.position - explosionPos;

        StartCoroutine(OnDamage(reactVector, true));
    }

    IEnumerator OnDamage(Vector3 reactVector, bool isGrenade)
    {
        material.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            material.color = Color.white;
        }
        else
        {
            material.color = Color.gray;
            gameObject.layer = 12;
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");

            if (isGrenade)
            {
                reactVector = reactVector.normalized;
                reactVector += Vector3.up * 3;

                rb.freezeRotation = false;
                rb.AddTorque(reactVector * 5, ForceMode.Impulse);
                rb.AddForce(reactVector * 5, ForceMode.Impulse);
            }
            else
            {
                reactVector = reactVector.normalized;
                reactVector += Vector3.up;
                rb.AddForce(reactVector * 5, ForceMode.Impulse);
            }

            Destroy(gameObject, 3f);
        }
    }
}
