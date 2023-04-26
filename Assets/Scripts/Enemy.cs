using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float maxHealth = 8;
    public float maxStructure = 3;

    private float normalSpeed = 3;
    private float triggeredSpeed = 1.5f;
    private float structureRefreshRate = 0.5f;

    private float attackDamage = 3;

    [HideInInspector] public float health;
    [HideInInspector] public float structure;
    [HideInInspector] public bool died = false;

    private bool onTriggerRange = false;
    private bool vulnerable = false;
    private bool blocking = false;
    public bool facingRight = false;
    private bool attacking = false;

    private float nextAttack = 0;

    private Animator animator;
    private HealthStructureBarHandler healthStructureBarHandler;

    [SerializeField] private float swordRangeRadius;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GameObject swordRangeCircle;
    [SerializeField] private Player player;

    // Start is called before the first frame update
    private void Start()
    {
        healthStructureBarHandler = GetComponent<HealthStructureBarHandler>();
        animator = GetComponent<Animator>();
        health = maxHealth;
        structure = maxStructure;
    }

    private void Update()
    {
        if (died || player.died || vulnerable) return;

        RotateToPlayer();

        if (!onTriggerRange && !OnAttackRange())
        {
            MoveTowardsPlayer(normalSpeed);
            animator.SetBool("Block", false);
            animator.SetBool("Walk", true);
        }

        else if (onTriggerRange)
        {
            WhenTriggered();
        }

        else
        {
            WhenInAttackRange();
        }
    }

    private void FixedUpdate()
    {
        if (structure < maxStructure && !vulnerable)
        {
            RefreshStructure(structureRefreshRate / 50);
        }
    }

    private void RotateToPlayer()
    {
        if ((transform.position.x > player.transform.position.x && facingRight) || (transform.position.x < player.transform.position.x && !facingRight))
        {
            transform.Rotate(Vector2.up, 180);
            facingRight = !facingRight; // wtf
        }
    }

    private void MoveTowardsPlayer(float speed)
    {
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
    }

    private void WhenTriggered()
    {
        if (CanBlock())
        {
            blocking = true;
            animator.SetBool("Block", true);
            MoveTowardsPlayer(triggeredSpeed);
        }

        else
        {
            animator.SetBool("Walk", false);
        }
    }

    private void WhenInAttackRange()
    {
        if (Time.time >= nextAttack && !attacking)
        {
            animator.SetBool("Block", false);
            blocking = false;
            animator.SetTrigger("Attack");
            attacking = true;
        }
    }

    public void Attack()
    {
        Collider2D player = Physics2D.OverlapCircle(swordRangeCircle.transform.position, swordRangeRadius, playerLayer);
        if (player == null) return;
        player.GetComponent<Player>().GetAttacked(attackDamage, this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.Equals("Player"))
        {
            onTriggerRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name.Equals("Player"))
        {
            onTriggerRange = false;
        }
    }

    private void RefreshStructure(float value)
    {
        structure += value;
        healthStructureBarHandler.UpdateStructure(maxStructure, structure);
    }

    private bool CanBlock()
    {
        return structure > 0;
    }

    public void AttackComplete()
    {
        nextAttack = Time.time + Random.Range(2, 3);
        attacking = false;
        if (structure > 0)
        {
            blocking = true;
            animator.SetBool("Block", true);
        }
    }

    private bool OnAttackRange()
    {
        Collider2D player = Physics2D.OverlapCircle(swordRangeCircle.transform.position, swordRangeRadius, playerLayer);
        return (player != null);
    }

    private void Die()
    {
        healthStructureBarHandler.DestroyBars();
        animator.SetTrigger("Died");
        died = true;
    }

    public void GetAttacked(float AttackDamage)
    {
        if (died) return;

        if (vulnerable)
        {
            health = 0;
            Die();
            return;
        }

        if (blocking)
        {
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
            GetHealthDamage(AttackDamage);
        }

        GetStructureDamage(AttackDamage);
    }

    private void GetStructureDamage(float damage)
    {
        if (damage >= structure)
        {
            structure = 0;
            vulnerable = true;
            animator.SetBool("Vulnerable", true);
            blocking = false;
            animator.SetBool("Block", false);
            animator.SetBool("Walk", false);
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

    private void RecoverFromVulnerable()
    {
        vulnerable = false;
        animator.SetBool("Vulnerable", false);
        RefreshStructure(maxStructure);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(swordRangeCircle.transform.position, swordRangeRadius);
    }
}