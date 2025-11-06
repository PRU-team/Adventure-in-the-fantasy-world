using System.Collections.Generic;
using Enemy;
using UnityEngine;

namespace Combat.Player
{
    public class AttackHitbox : MonoBehaviour
    {
        private List<GameObject> enemies = new List<GameObject>();
        private List<bool> inRange = new List<bool>();

        public void Start()
        {
            enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
            for (int i = 0; i < enemies.Count; i++)
            {
                inRange.Add(false);
            }
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                if (!enemies.Contains(other.gameObject))
                {
                    enemies.Add(other.gameObject);
                    inRange.Add(true);
                }
                else inRange[enemies.IndexOf(other.gameObject)] = true;
            }
        }
    
        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                inRange[enemies.IndexOf(other.gameObject)] = false;
            }
        }

        public void Attack(float attackDamage)
        {
            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.enabled = true;
            Invoke(nameof(DisableCollider), 0.1f); // collider active 0.1s

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, col.radius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    var enemyCtrl = hit.GetComponent<EnemyController>();
                    if (enemyCtrl != null) enemyCtrl.TakeDamage(attackDamage);
                    else hit.GetComponent<DeathBossController>()?.TakeDamage(attackDamage);
                }
            }
        }

        private void DisableCollider()
        {
            GetComponent<CircleCollider2D>().enabled = false;
        }
        public void SetAttackRange(float attackRange)
        {
            GetComponent<CircleCollider2D>().radius = attackRange;
        }
    }
}
