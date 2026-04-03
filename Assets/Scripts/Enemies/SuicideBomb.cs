using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HackedDesign
{
    public class SuicideBomb: MonoBehaviour
    {
        private CharController charController;

        void Awake()
        {
            charController = GetComponentInParent<CharController>();
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if(!charController.IsDead && collision.gameObject.CompareTag(Tags.Player))
            {
                if(collision.gameObject.TryGetComponent<CharController>(out var otherCharacterController))
                {
                    Vector3 closestPoint = collision.ClosestPoint(this.transform.position);
                    otherCharacterController.TakeDamage(200, closestPoint, otherCharacterController.transform.position - closestPoint, true); 
                }

                if(charController.OrNull() != null)
                {
                    charController.TakeDamage(200, Vector3.zero, Vector3.zero, false);
                }
            }
        }
    }
}
