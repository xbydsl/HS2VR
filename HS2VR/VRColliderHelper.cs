using System;
using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using UnityEngine;
using VRGIN.Core;

namespace HS2VR
{
    // Code contributed by thojmr
    public static class VRColliderHelper
    {
        internal static bool coroutineActive = false;
        internal static VRPlugin pluginInstance;


        internal static void TriggerHelperCoroutine()
        {
            //Only trigger if not already running, and in main game
            if (coroutineActive) return;
            coroutineActive = true;

            pluginInstance.StartCoroutine(LoopEveryXSeconds());
        }


        internal static void StopHelperCoroutine()
        {
            pluginInstance.StopCoroutine(LoopEveryXSeconds());
            coroutineActive = false;
        }


        /// <summary>
        /// Got tired of searching for the correct hooks, just check for new dynamic bones on a loop.  Genius! (Should be able to use CharCustFunCtrl for this later)
        /// </summary>
        internal static IEnumerator LoopEveryXSeconds()
        {
            while (coroutineActive)
            {
                try
                {
                    VRCollider.SetVRControllerColliderToDynamicBones();
                }
                catch (Exception e)
                {
                    VRLog.Error("Error in Collider Helper: " + e.Message, e.StackTrace);
                }
                yield return new WaitForSeconds(3);
            }
        }
    }
}
