using System;
using FormationSorter.Properties;
using FormationSorter.Utilities;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace FormationSorter;

public class SubModule : MBSubModuleBase
{
    private static bool patched;

    protected override void OnBeforeInitialModuleScreenSetAsRoot()
    {
        base.OnBeforeInitialModuleScreenSetAsRoot();
        try
        {
            if (patched)
                return;
            patched = true;
            new Harmony("pointfeev." + AssemblyInfo.Id.ToLower()).PatchAll();
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