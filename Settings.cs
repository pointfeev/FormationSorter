using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Dropdown;
using MCM.Abstractions.Settings.Base.Global;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.InputSystem;

namespace FormationSorter
{
    internal sealed class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "FormationSorter";
        public override string DisplayName => "Formation Sorter";
        public override string FolderName => "FormationSorter";
        public override string FormatType => "xml";

        #region Order Key

        private DropdownDefault<InputKey> orderKeySetting = null;

        [SettingPropertyDropdown("Order Key", Order = 1, RequireRestart = false, HintText = "Sort Units Between Formations order menu key; troops in selected formations will be sorted into their single best formation if one of its kind is among the selected formations.")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        private DropdownDefault<InputKey> OrderKeySetting
        {
            get
            {
                if (orderKeySetting is null)
                {
                    orderKeySetting = GetNormalKeysDropdown(InputKey.X);
                }
                return orderKeySetting;
            }
            set
            {
                if (orderKeySetting != value)
                {
                    orderKeySetting = value;
                    Hotkeys.RefreshOrderGameKey();
                }
            }
        }

        public static InputKey OrderKey => Instance.OrderKeySetting.SelectedValue;

        #endregion Order Key

        #region Skirmisher Sorting Modifier Key

        [SettingPropertyDropdown("Skirmisher Sorting Modifier Key", Order = 2, RequireRestart = false, HintText = "When combined with the Order Key javelineers, rock throwers, etc. get put into the Skirmisher formation instead of Infantry.")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        private DropdownDefault<InputKey> SkirmisherSortingModifierKeySetting { get; set; } = GetModifierKeysDropdown(InputKey.LeftControl);

        public static InputKey SkirmisherSortingModifierKey => Instance.SkirmisherSortingModifierKeySetting.SelectedValue;

        #endregion Skirmisher Sorting Modifier Key

        #region Equal Sorting Modifier Key

        [SettingPropertyDropdown("Equal Sorting Modifier Key", Order = 3, RequireRestart = false, HintText = "When combined with the Order Key troops will be sorted equally amongst the selected formations instead of being put in only their single best formation.")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        private DropdownDefault<InputKey> EqualSortingModifierKeySetting { get; set; } = GetModifierKeysDropdown(InputKey.LeftAlt);

        public static InputKey EqualSortingModifierKey => Instance.EqualSortingModifierKeySetting.SelectedValue;

        #endregion Equal Sorting Modifier Key

        #region Inverse Selection Modifier Key

        [SettingPropertyDropdown("Inverse Selection Modifier Key", Order = 4, RequireRestart = false, HintText = "When combined with any of the selection keys below, the formations it encompasses will be inverted from their current state, potentially being added or removed from the current selection.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> InverseSelectionModifierKeySetting { get; set; } = GetModifierKeysDropdown(InputKey.LeftControl);

        public static InputKey InverseSelectionModifierKey => Instance.InverseSelectionModifierKeySetting.SelectedValue;

        #endregion Inverse Selection Modifier Key

        #region Select All Formations Key

        [SettingPropertyDropdown("Select All Formations Key", Order = 5, RequireRestart = false, HintText = "This key will select all formations.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.F);

        public static InputKey SelectAllKey => Instance.SelectAllKeySetting.SelectedValue;

        #endregion Select All Formations Key

        #region Select All Melee Cavalry Formations Key

        [SettingPropertyDropdown("Select All Melee Cavalry Formations Key", Order = 6, RequireRestart = false, HintText = "This key will select all melee cavalry formations: Cavalry, Light Cavalry, Heavy Cavalry.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllMeleeCavalryKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.C);

        public static InputKey SelectAllMeleeCavalryKey => Instance.SelectAllMeleeCavalryKeySetting.SelectedValue;

        #endregion Select All Melee Cavalry Formations Key

        #region Select All Ranged Cavalry Formations Key

        [SettingPropertyDropdown("Select All Ranged Cavalry Formations Key", Order = 7, RequireRestart = false, HintText = "This key will select all ranged cavalry formations: Horse Archers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllHorseArchersKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.V);

        public static InputKey SelectAllRangedCavalryKey => Instance.SelectAllHorseArchersKeySetting.SelectedValue;

        #endregion Select All Ranged Cavalry Formations Key

        #region Select All Ground Melee Formations Key

        [SettingPropertyDropdown("Select All Ground Melee Formations Key", Order = 8, RequireRestart = false, HintText = "This key will select all ground melee formations: Infantry, Heavy Infantry.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllInfantryKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.H);

        public static InputKey SelectAllGroundMeleeKey => Instance.SelectAllInfantryKeySetting.SelectedValue;

        #endregion Select All Ground Melee Formations Key

        #region Select All Ground Ranged Formations Key

        [SettingPropertyDropdown("Select All Ground Ranged Formations Key", Order = 9, RequireRestart = false, HintText = "This key will select all ground ranged formations: Archers, Skirmishers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllArchersAndSkirmishersKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.J);

        public static InputKey SelectAllGroundRangedKey => Instance.SelectAllArchersAndSkirmishersKeySetting.SelectedValue;

        #endregion Select All Ground Ranged Formations Key

        #region Select All Basic Melee Formations Key

        [SettingPropertyDropdown("Select All Basic Melee Formations Key", Order = 10, RequireRestart = false, HintText = "This key will select all basic melee formations: Infantry, Cavalry.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllNormalMeleeKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.Y);

        public static InputKey SelectAllBasicMeleeKey => Instance.SelectAllNormalMeleeKeySetting.SelectedValue;

        #endregion Select All Basic Melee Formations Key

        #region Select All Basic Ranged Formations Key

        [SettingPropertyDropdown("Select All Ranged Formations Key", Order = 11, RequireRestart = false, HintText = "This key will select all basic ranged formations: Archers, Horse Archers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllRangedKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.U);

        public static InputKey SelectAllBasicRangedKey => Instance.SelectAllRangedKeySetting.SelectedValue;

        #endregion Select All Basic Ranged Formations Key

        #region Select All Ground Formations Key

        [SettingPropertyDropdown("Select All Ground Formations Key", Order = 12, RequireRestart = false, HintText = "This key will select all ground formations: Infantry, Heavy Infantry, Archers, Skirmishers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllGroundedKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.N);

        public static InputKey SelectAllGroundKey => Instance.SelectAllGroundedKeySetting.SelectedValue;

        #endregion Select All Ground Formations Key

        #region Select All Cavalry Formations Key

        [SettingPropertyDropdown("Select All Cavalry Formations Key", Order = 13, RequireRestart = false, HintText = "This key will select all cavalry formations: Cavalry, Light Cavalry, Heavy Cavalry, Horse Archers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllMountedKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.M);

        public static InputKey SelectAllCavalryKey => Instance.SelectAllMountedKeySetting.SelectedValue;

        #endregion Select All Cavalry Formations Key

        #region Selection Spacers

        [SettingPropertyDropdown("", Order = 14, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SpacerSetting1 { get; set; } = new DropdownDefault<InputKey>(new InputKey[0], 0);

        [SettingPropertyDropdown("", Order = 15, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SpacerSetting2 { get; set; } = new DropdownDefault<InputKey>(new InputKey[0], 0);

        [SettingPropertyDropdown("", Order = 16, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SpacerSetting3 { get; set; } = new DropdownDefault<InputKey>(new InputKey[0], 0);

        [SettingPropertyDropdown("", Order = 17, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SpacerSetting4 { get; set; } = new DropdownDefault<InputKey>(new InputKey[0], 0);

        [SettingPropertyDropdown("", Order = 18, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SpacerSetting5 { get; set; } = new DropdownDefault<InputKey>(new InputKey[0], 0);

        [SettingPropertyDropdown("", Order = 19, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SpacerSetting6 { get; set; } = new DropdownDefault<InputKey>(new InputKey[0], 0);

        #endregion Selection Spacers

        #region Get Usable Keys

        private static DropdownDefault<InputKey> GetNormalKeysDropdown(InputKey defaultKey)
        {
            (InputKey[], int) result = GetUsableNormalKeysAndDefaultIndex(defaultKey);
            return new DropdownDefault<InputKey>(result.Item1, result.Item2);
        }

        private static DropdownDefault<InputKey> GetModifierKeysDropdown(InputKey defaultKey)
        {
            (InputKey[], int) result = GetUsableModifierKeysAndDefaultIndex(defaultKey);
            return new DropdownDefault<InputKey>(result.Item1, result.Item2);
        }

        private static (InputKey[], int) GetUsableNormalKeysAndDefaultIndex(InputKey defaultKey)
        {
            return GetUsableKeysAndDefaultIndexFromKeyValues(defaultKey, GetNormalKeyValues());
        }

        private static (InputKey[], int) GetUsableModifierKeysAndDefaultIndex(InputKey defaultKey)
        {
            return GetUsableKeysAndDefaultIndexFromKeyValues(defaultKey, GetModifierKeyValues());
        }

        private static int[] GetNormalKeyValues()
        {
            List<int> toUse = new List<int>();
            toUse.AddRange(GetRangeOfIntegers(16, 27)); // Q to ]
            toUse.AddRange(GetRangeOfIntegers(30, 40)); // A to '
            toUse.AddRange(GetRangeOfIntegers(44, 53)); // Z to /
            return toUse.ToArray();
        }

        private static int[] GetModifierKeyValues()
        {
            List<int> toUse = new List<int>();
            toUse.Add(29); // left ctrl
            toUse.Add(42); // left shift
            toUse.Add(56); // left alt
            toUse.Add(157); // right ctrl
            toUse.Add(54); // right shift
            toUse.Add(184); // right alt
            return toUse.ToArray();
        }

        private static int[] GetRangeOfIntegers(int from, int to)
        {
            return Enumerable.Range(from, 1 + to - from).ToArray();
        }

        private static (InputKey[], int) GetUsableKeysAndDefaultIndexFromKeyValues(InputKey defaultKey, int[] keyValues)
        {
            int indexOfDefault = 0;
            List<InputKey> keys = new List<InputKey>();
            foreach (InputKey key in typeof(InputKey).GetEnumValues())
            {
                if (key == defaultKey)
                {
                    indexOfDefault = keys.Count;
                }
                if (keyValues.Contains((int)key))
                {
                    keys.Add(key);
                }
            }
            keys.Add(InputKey.Invalid); // effectively unbinds the key
            return (keys.ToArray(), indexOfDefault);
        }

        #endregion Get Usable Keys
    }
}