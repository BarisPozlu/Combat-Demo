using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static bool facingRight = true;
    [HideInInspector] public bool died = false;

    private bool attacking = false;
    private bool blocking = false;
    private bool vulnerable = false;

    public float maxHealth = 10;
    public float maxStructure = 7;

    public float health;
    public float structure;

    private float attackDamage = 1;

    private float attackRate = 4;
    private float blockRate = 4;
    private float structureRefreshRate = 0.5f;

    private float nextAttack = 0;
    private float nextBlock = 0;

    [SerializeField] private float swordRangeRadius;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject swordRangeCircle;

    private Animator animator;
    private AudioSource blockSound;
    private HealthStructureBarHandler healthStructureBarHandler;

    // Start is called before the first frame update
    private void Start()
    {
        blockSound = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        healthStructureBarHandler = GetComponent<HealthStructureBarHandler>();

        health = maxHealth;
        structure = maxStructure;
    }

    // Update is called once per frame
    private void Update()
    {
        if (died || vulnerable) return;
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (structure < maxStructure && !vulnerable)
        {
            RefreshStructure(structureRefreshRate / 50);
        }
    }

    private void Die()
    {
        healthStructureBarHandler.DestroyBars();
        animator.SetTrigger("Died");
        died = true;
    }

    public void AttackComplete()
    {
        attacking = false;
        nextAttack = Time.time + 1 / attackRate;
    }

    public void Attack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(swordRangeCircle.transform.position, swordRangeRadius, enemyLayer);
        foreach (Collider2D enemy in enemies)
        {
            enemy.gameObject.GetComponent<Enemy>().GetAttacked(attackDamage);
        }
    }

    public void GetAttacked(float attackDamage, Enemy enemy)
    {
        if (died) return;

        if (vulnerable)
        {
            attackDamage *= 2;
        }

        if (blocking && facingRight != enemy.facingRight)
        {
            blockSound.Play();

            if (facingRight)
            {
                transform.position += new Vector3(-1.2f, 0, 0);
            }

            else
            {
                transform.position += new Vector3(1.2f, 0, 0);
            }
        }

        else
        {
            GetHealthDamage(attackDamage);
        }

        GetStructureDamage(attackDamage);
    }

    private void GetStructureDamage(float damage)
    {
        if (damage >= structure)
        {
            BecomeVulnerable();
            Invoke(nameof(RecoverFromVulnerable), 2);
        }

        else
        {
            structure -= damage;
        }
        healthStructureBarHandler.UpdateStructure(maxStructure, structure);
    }

    private void GetHealthDamage(float damage)
    {
        if (damage >= health)
        {
            health = 0;
            Die();
        }

        else
        {
            health -= damage;
        }

        animator.SetTrigger("Damaged");
        healthStructureBarHandler.UpdateHealth(health);
    }

    private void BecomeVulnerable()
    {
        structure = 0;
        vulnerable = true;
        animator.SetBool("Vulnerable", true);

        // Player might have been blocking before becoming vulnerable
        blocking = false;
        animator.SetBool("Block", false);
    }

    private void RecoverFromVulnerable()
    {
        vulnerable = false;
        animator.SetBool("Vulnerable", false);
        RefreshStructure(3);
    }

    private void RefreshStructure(float value)
    {
        structure += value;
        healthStructureBarHandler.UpdateStructure(maxStructure, structure);
    }

    private bool CanAttack()
    {
        return (Time.time >= nextAttack && !blocking && !attacking);
    }

    private bool CanBlock()
    {
        return (Time.time >= nextBlock && !blocking && !attacking && structure > 0);
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && CanAttack())
        {
            attacking = true;
            animator.SetTrigger("Attack");
        }

        if (Input.GetKey(KeyCode.Mouse1) && CanBlock())
        {
            blocking = true;
            animator.SetBool("Block", true);
        }

        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            blocking = false;
            animator.SetBool("Block", false);
            nextBlock = Time.time + 1 / blockRate;
        }
    }
}