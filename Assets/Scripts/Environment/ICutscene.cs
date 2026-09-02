using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HackedDesign
{
    public interface ICutscene
    {
        void Play(IGame game);

        void Stop(IGame game);
    }
}
