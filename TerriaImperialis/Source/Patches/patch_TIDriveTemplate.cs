using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using MonoMod;

namespace PavonisInteractive.TerraInvicta
{
    public class patch_TIDriveTemplate : TIDriveTemplate
    {

        public extern string orig_modelResource(TIShipHullTemplate hull, int appearanceIndex = 0);

        public string modelResource(TIShipHullTemplate hull, int appearanceIndex = 0)
        {
            Log.Debug("modelResource() called");
            string text = orig_modelResource(hull, appearanceIndex);
            Log.Debug($"modelResource() returned: {text}");
            return text;
        }

        //public extern string modelResource(TIShipHullTemplate hull, int appearanceIndex = 0);

        //public string modelResource()
        //{
        //    Log.Debug("modelResource() called", Array.Empty<object>());
        //    string text = modelResource();
        //    if (text.Contains("ShipHullEscortImperialSkylance"))
        //    {
        //        text = text.Replace("ShipHullEscortImperialSkylance", "Escort");
        //    }
        //    return text;
        //}
    }

}
