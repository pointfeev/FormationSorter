using System;
using FormationSorter.Utilities;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace FormationSorter;

public class SubModule : MBSubModuleBase
{
    internal const string Name = "Formation Sorter";
    internal const string Version = "3.2.0";
    internal const string Url = "https://www.nexusmods.com/mountandblade2bannerlord/mods/3320";
    internal const string Copyright = "2021, pointfeev (https://github.com/pointfeev)";
    internal const string MinimumGameVersion = "1.1.0";
    internal static readonly string Id = typeof(SubModule).Namespace;

    private bool initialized;

    protected override void OnBeforeInitialModuleScreenSetAsRoot()
    {
        base.OnBeforeInitialModuleScreenSetAsRoot();
        try
        {
            if (initialized)
                return;
            initialized = true;
            new Harmony("pointfeev." + Id.ToLower()).PatchAll();
            //InformationManager.DisplayMessage(new(Name + " initialized", Colors.Cyan, Id));
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