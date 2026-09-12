using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "BasicBarrier", menuName = "Mouse/Items/BasicBarrier")]
    public class BasicBarrier : Barrier
    {
        public override void Apply(OperatingSystem os)
        {
            os.ResetBarrierRotation();
        }

        public override void Remove(OperatingSystem os)
        {

        }
    }
}