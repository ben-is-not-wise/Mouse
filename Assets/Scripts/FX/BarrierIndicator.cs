using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HackedDesign
{
    public class BarrierIndicator: MonoBehaviour
    {
        public HexIndicator indicator;
        public void Repaint(BarrierInstance barrier, float currentRotation, bool show)
        {
            if (show)
            {
                indicator.RotationDegrees = currentRotation;
                indicator.Radius = barrier.definition.Radius;
                indicator.Sides = (int)barrier.stats.Get(ItemStatKeys.BarrierFaces);
                indicator.gameObject.SetActive(true);
            }
            else
            {
                if (indicator.OrNull() != null && indicator.isActiveAndEnabled)
                {
                    indicator.gameObject.SetActive(false);
                }
            }
        }
    }
}
