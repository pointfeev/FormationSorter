using HarmonyLib;
using System;
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
            base.OnBeforeInitialModuleScreenSetAsRoot();
            try
            {
                if (!initialized)
                {
                    initialized = true;
                    new Harmony("pointfeev.formationsorter").PatchAll();
                    Hotkeys.Initialize();
                    InformationManager.DisplayMessage(new InformationMessage("Formation Sorter initialized", Colors.Cyan, "FormationSorter"));
                }
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            try
            {
                Hotkeys.OnApplicationTick(dt);
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }
    }
}