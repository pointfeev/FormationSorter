using System;
using FormationSorter.Utilities;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace FormationSorter;

public class SubModule : MBSubModuleBase
{
    internal const string Id = "FormationSorter";
    internal const string Name = "Formation Sorter";
    internal const string Version = "3.3.1";

    private static bool patched;

    protected override void OnBeforeInitialModuleScreenSetAsRoot()
    {
        base.OnBeforeInitialModuleScreenSetAsRoot();
        try
        {
            if (patched)
                return;
            patched = true;
            new Harmony("pointfeev." + Id.ToLower()).PatchAll();
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