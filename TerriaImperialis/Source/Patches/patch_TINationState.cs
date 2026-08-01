using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using MonoMod;

namespace PavonisInteractive.TerraInvicta
{
    public class patch_TINationState : TINationState
    {
        public extern void orig_EconomyPriorityComplete();

        public void EconomyPriorityComplete()
        {
            Log.Debug("EconomyPriorityComplete() called", Array.Empty<object>());
            orig_EconomyPriorityComplete();
            Log.Debug("EconomyPriorityComplete() completed", Array.Empty<object>());
        }
    }
}
