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

        [SettingPropertyDropdown("Order Key", Order = 1, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        public DropdownDefault<InputKey> OrderKeySetting
        {
            get
            {
                if (orderKeySetting is null)
                {
                    orderKeySetting = GetModifierKeysDropdown(InputKey.X);
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

        #region Equal Sorting Modifier Key

        [SettingPropertyDropdown("Equal Sorting Modifier Key", Order = 3, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        public DropdownDefault<InputKey> EqualSortingModifierKeySetting { get; set; } = GetModifierKeysDropdown(InputKey.LeftAlt);

        public static InputKey EqualSortingModifierKey => Instance.EqualSortingModifierKeySetting.SelectedValue;

        #endregion Equal Sorting Modifier Key

        #region Skirmisher Sorting Modifier Key

        [SettingPropertyDropdown("Skirmisher Sorting Modifier Key", Order = 2, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        public DropdownDefault<InputKey> SkirmisherSortingModifierKeySetting { get; set; } = GetModifierKeysDropdown(InputKey.LeftControl);

        public static InputKey SkirmisherSortingModifierKey => Instance.SkirmisherSortingModifierKeySetting.SelectedValue;

        #endregion Skirmisher Sorting Modifier Key

        #region Inverse Selection Modifier Key

        [SettingPropertyDropdown("Inverse Selection Modifier Key", Order = 4, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public DropdownDefault<InputKey> InverseSelectionModifierKeySetting { get; set; } = GetModifierKeysDropdown(InputKey.LeftControl);

        public static InputKey InverseSelectionModifierKey => Instance.InverseSelectionModifierKeySetting.SelectedValue;

        #endregion Inverse Selection Modifier Key

        #region Select All Formations Key

        [SettingPropertyDropdown("Select All Formations Key", Order = 5, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public DropdownDefault<InputKey> SelectAllKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.F);

        public static InputKey SelectAllKey => Instance.SelectAllKeySetting.SelectedValue;

        #endregion Select All Formations Key

        #region Select All Melee Cavalry Formations Key

        [SettingPropertyDropdown("Select All Melee Cavalry Formations Key", Order = 6, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public DropdownDefault<InputKey> SelectAllMeleeCavalryKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.C);

        public static InputKey SelectAllMeleeCavalryKey => Instance.SelectAllMeleeCavalryKeySetting.SelectedValue;

        #endregion Select All Melee Cavalry Formations Key

        #region Select All Horse Archer Formations Key

        [SettingPropertyDropdown("Select All Horse Archer Formations Key", Order = 7, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public DropdownDefault<InputKey> SelectAllHorseArchersKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.V);

        public static InputKey SelectAllHorseArchersKey => Instance.SelectAllHorseArchersKeySetting.SelectedValue;

        #endregion Select All Horse Archer Formations Key

        #region Select All Infantry Formations Key

        [SettingPropertyDropdown("Select All Infantry Formations Key", Order = 8, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public DropdownDefault<InputKey> SelectAllInfantryKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.H);

        public static InputKey SelectAllInfantryKey => Instance.SelectAllInfantryKeySetting.SelectedValue;

        #endregion Select All Infantry Formations Key

        #region Select All Archer and Skirmisher Formations Key

        [SettingPropertyDropdown("Select All Archer and Skirmisher Formations Key", Order = 9, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public DropdownDefault<InputKey> SelectAllArchersAndSkirmishersKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.J);

        public static InputKey SelectAllArchersAndSkirmishersKey => Instance.SelectAllArchersAndSkirmishersKeySetting.SelectedValue;

        #endregion Select All Archer and Skirmisher Formations Key

        #region Select All Normal Melee Formations Key

        [SettingPropertyDropdown("Select All Normal Melee Formations Key", Order = 10, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public DropdownDefault<InputKey> SelectAllNormalMeleeKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.Y);

        public static InputKey SelectAllNormalMeleeKey => Instance.SelectAllNormalMeleeKeySetting.SelectedValue;

        #endregion Select All Normal Melee Formations Key

        #region Select All Ranged Formations Key

        [SettingPropertyDropdown("Select All Ranged Formations Key", Order = 11, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public DropdownDefault<InputKey> SelectAllRangedKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.U);

        public static InputKey SelectAllRangedKey => Instance.SelectAllRangedKeySetting.SelectedValue;

        #endregion Select All Ranged Formations Key

        #region Select All Grounded Formations Key

        [SettingPropertyDropdown("Select All Grounded Formations Key", Order = 12, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public DropdownDefault<InputKey> SelectAllGroundedKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.N);

        public static InputKey SelectAllGroundedKey => Instance.SelectAllGroundedKeySetting.SelectedValue;

        #endregion Select All Grounded Formations Key

        #region Select All Mounted Formations Key

        [SettingPropertyDropdown("Select All Mounted Formations Key", Order = 13, RequireRestart = false, HintText = "")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public DropdownDefault<InputKey> SelectAllMountedKeySetting { get; set; } = GetNormalKeysDropdown(InputKey.M);

        public static InputKey SelectAllMountedKey => Instance.SelectAllMountedKeySetting.SelectedValue;

        #endregion Select All Mounted Formations Key

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
            return (keys.ToArray(), indexOfDefault);
        }

        #endregion Get Usable Keys
    }
}