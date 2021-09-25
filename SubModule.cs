using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace FormationSorter
{
    public class SubModule : MBSubModuleBase
    {
        private bool initialized = false;

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            if (!initialized)
            {
                initialized = true;
                new Harmony("pointfeev.debugger").PatchAll();
                HotKeys.Initialize();
                InformationManager.DisplayMessage(new InformationMessage("Formation Edit initialized", Colors.Cyan, "FormationEdit"));
            }
        }

        protected override void OnApplicationTick(float dt)
        {
            HotKeys.HotKeysTick(dt);
        }
    }
}