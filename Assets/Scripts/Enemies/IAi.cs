#nullable enable
using UnityEngine;

namespace HackedDesign
{
    public interface IAi
    {
        IEnemyState CurrentState { get; set; }
        CharController Character { get; }
        StatusIcon Icon { get; }

        bool WallInFront { get; }

        bool DropInFront { get; }

        void Alert(Vector3 position);
    }
}
