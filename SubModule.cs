using HarmonyLib;
using System.IO;
using System.Reflection;
using TaleWorlds.MountAndBlade;

namespace DismembermentSue
{
    public class SubModule : MBSubModuleBase
    {

        protected override void OnSubModuleLoad()
        {
            new Harmony("sue,mod.dismemberment").PatchAll();
        }

        protected override void OnApplicationTick(float dt)
        {
            DismembermentSettings.Instance().GetDismembermentMissionBusiness().OnApplicationTick(dt);
        }
    }
}