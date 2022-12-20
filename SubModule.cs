using System;
using FormationSorter.Utilities;
using HarmonyLib;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace FormationSorter
{
    public class SubModule : MBSubModuleBase
    {
        private bool initialized;

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            try
            {
                if (initialized)
                    return;
                initialized = true;
                new Harmony("pointfeev.formationsorter").PatchAll();
                InformationManager.DisplayMessage(
                    new InformationMessage("Formation Sorter initialized", Colors.Cyan, "FormationSorter"));
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
                HotKeys.OnApplicationTick();
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
            try
            {
                Order.OnApplicationTick();
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }
    }
}