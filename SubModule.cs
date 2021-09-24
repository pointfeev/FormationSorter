using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
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
                Formations.UniqueId = 'F' + 'o' + 'r' + 'm' + 'a' + 't' + 'i' + 'o' + 'n' + 'E' + 'd' + 'i' + 't'; // 1333
                Formations.Hotkey = new GameKey(Formations.UniqueId, "FormationEditHotkey", "FormationEditHotkeyCategory", InputKey.F10);
                InformationManager.DisplayMessage(new InformationMessage("Formation Edit initialized", Colors.Cyan, "FormationEdit"));
            }
        }

        private bool wasPressedLastTick = false;

        protected override void OnApplicationTick(float dt)
        {
            if ((Formations.Hotkey?.KeyboardKey?.InputKey).GetValueOrDefault(InputKey.F10).IsPressed())
            {
                if (!wasPressedLastTick)
                {
                    wasPressedLastTick = true;
                    Formations.OnHotkeyPressed();
                }
            }
            else
            {
                wasPressedLastTick = false;
            }
        }
    }
}