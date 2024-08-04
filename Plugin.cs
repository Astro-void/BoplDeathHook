using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace DeathHook
{
    [BepInPlugin("com.astrovoid.DeathHook", "DeathHook", "1.0.0")]
    public class DeathHook : BaseUnityPlugin
    {
        private void Log(string message)
        {
            Logger.LogInfo(message);
        }

        private void Awake()
        {
            // Plugin startup logic
            Log($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            DoPatching();
        }

        private void DoPatching()
        {
            var harmony = new Harmony("com.astrovoid.tethered");

            MethodInfo FirstPatchMethod = AccessTools.Method(typeof(RopeHook), "OnCollide"); // baby's first patch method
            MethodInfo FirstPatch = AccessTools.Method(typeof(Patches), "Method");
            harmony.Patch(FirstPatchMethod, new HarmonyMethod(FirstPatch));
        }

        private void OnDestroy()
        {
            Log($"Bye Bye From {PluginInfo.PLUGIN_GUID}"); // heh loser
        }
    }

    public class Patches
    {
        public static void Method(ref RopeHook __instance, ref CollisionInformation collision)
        {
            if (collision.layer == LayerMask.NameToLayer("Player"))
            {
                IPlayerIdHolder component = collision.colliderPP.fixTrans.GetComponent<IPlayerIdHolder>();
                PlayerCollision component2 = collision.colliderPP.fixTrans.gameObject.GetComponent<PlayerCollision>();
                Player player = PlayerHandler.Get().GetPlayer(component.GetPlayerId());
                if (component.GetPlayerId() != __instance.endPlayerId || component.GetPlayerId() != (int)__instance.ropeBody.ownerId)
                {
                    try
                    {
                        component2.killPlayer(__instance.ropeBody.ownerId, true, true, CauseOfDeath.Other);
                        Updater.DestroyFix(__instance);
                    }
                    catch
                    {
                        // rock case
                    }
                }
            }
        }
    }
}
