using System.Collections.Generic;
using System.Linq;

using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Dropdown;
using MCM.Abstractions.Settings.Base.Global;

using TaleWorlds.InputSystem;

namespace FormationSorter
{
    internal sealed class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "FormationSorter";
        public override string DisplayName => "Formation Sorter";
        public override string FolderName => "FormationSorter";
        public override string FormatType => "xml";

        #region Get & Set

        private static DropdownDefault<InputKey> GetNormalSetting(ref DropdownDefault<InputKey> setting, InputKey defaultKey)
        {
            if (setting is null)
            {
                setting = GetNormalKeysDropdown(defaultKey);
            }

            return setting;
        }

        private static DropdownDefault<InputKey> GetModifierSetting(ref DropdownDefault<InputKey> setting, InputKey defaultKey)
        {
            if (setting is null)
            {
                setting = GetModifierKeysDropdown(defaultKey);
            }

            return setting;
        }

        private static void SetSetting(ref DropdownDefault<InputKey> setting, DropdownDefault<InputKey> value)
        {
            if (setting != value)
            {
                setting = value;
                PatchInformationManager.SetIgnoredMessagesDirty();
            }
        }

        private static InputKey GetValue(DropdownDefault<InputKey> setting, InputKey defaultKey)
        {
            InputKey selectedKey = defaultKey;
            try { selectedKey = setting.SelectedValue; } catch { }
            return !selectedKey.IsDefined() ? defaultKey : selectedKey;
        }

        #endregion Get & Set

        #region Order Key

        [SettingPropertyDropdown("Order Key", Order = 1, RequireRestart = false, HintText = "Sort Troops Between Formations order menu key; troops in selected formations will be sorted into their single best formation if one of its kind is among the selected formations.")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        private DropdownDefault<InputKey> OrderKeySetting
        {
            get => GetNormalSetting(ref orderKeySetting, InputKey.X);
            set => SetSetting(ref orderKeySetting, value);
        }

        private DropdownDefault<InputKey> orderKeySetting = null;

        public static InputKey OrderKey => GetValue(Instance?.OrderKeySetting, InputKey.X);

        #endregion Order Key

        #region Tier Sort Key

        [SettingPropertyDropdown("Tier Sort Key", Order = 2, RequireRestart = false, HintText = "Tier sorting key; all infantry and cavalry troops will be sorted by their tiers. Tier 0-2 Infantry will be put into the Skirmisher formation, Tier 3-4 Infantry will be put into the Infantry formation, Tier 5-7 Infantry will be put into the Heavy Infantry formation. Tier 0-2 Cavalry will be put into the Cavalry formation, Tier 3-4 Cavalry will be put into the Light Cavalry formation, Tier 5-7 Cavalry will be put into the Heavy Cavalry formation. All archers and horse archers with ammunition/mounts will be put into the Ranged and Horse Archer formations respectively, otherwise they'll be counted as normal Infantry/Cavalry to be sorted via the tiers listed previously.")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        private DropdownDefault<InputKey> TierSortKeySetting
        {
            get => GetNormalSetting(ref tierSortKeySetting, InputKey.L);
            set => SetSetting(ref tierSortKeySetting, value);
        }

        private DropdownDefault<InputKey> tierSortKeySetting = null;

        public static InputKey TierSortKey => GetValue(Instance?.TierSortKeySetting, InputKey.L);

        #endregion Tier Sort Key

        #region Shield Sorting Modifier Key

        [SettingPropertyDropdown("Shield Sorting Modifier Key", Order = 3, RequireRestart = false, HintText = "When combined with the Order Key shielded infantry and skirmishers get put into the Infantry formation while unshielded infantry and skirmishers get put into the Skirmisher formation.")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        private DropdownDefault<InputKey> ShieldSortingModifierKeySetting
        {
            get => GetModifierSetting(ref shieldSortingModifierKeySetting, InputKey.LeftShift);
            set => SetSetting(ref shieldSortingModifierKeySetting, value);
        }

        private DropdownDefault<InputKey> shieldSortingModifierKeySetting = null;

        public static InputKey ShieldSortingModifierKey => GetValue(Instance?.ShieldSortingModifierKeySetting, InputKey.LeftShift);

        #endregion Shield Sorting Modifier Key

        #region Skirmisher Sorting Modifier Key

        [SettingPropertyDropdown("Skirmisher Sorting Modifier Key", Order = 4, RequireRestart = false, HintText = "When combined with the Order Key javelineers, rock throwers, etc. get put into the Skirmisher formation instead of Infantry.")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        private DropdownDefault<InputKey> SkirmisherSortingModifierKeySetting
        {
            get => GetModifierSetting(ref skirmisherSortingModifierKeySetting, InputKey.LeftControl);
            set => SetSetting(ref skirmisherSortingModifierKeySetting, value);
        }

        private DropdownDefault<InputKey> skirmisherSortingModifierKeySetting = null;

        public static InputKey SkirmisherSortingModifierKey => GetValue(Instance?.SkirmisherSortingModifierKeySetting, InputKey.LeftControl);

        #endregion Skirmisher Sorting Modifier Key

        #region Equal Sorting Modifier Key

        [SettingPropertyDropdown("Equal Sorting Modifier Key", Order = 5, RequireRestart = false, HintText = "When combined with the Order Key troops will be sorted equally amongst the selected formations instead of being put in only their single best formation.")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        private DropdownDefault<InputKey> EqualSortingModifierKeySetting
        {
            get => GetModifierSetting(ref equalSortingModifierKeySetting, InputKey.LeftAlt);
            set => SetSetting(ref equalSortingModifierKeySetting, value);
        }

        private DropdownDefault<InputKey> equalSortingModifierKeySetting;

        public static InputKey EqualSortingModifierKey => GetValue(Instance?.EqualSortingModifierKeySetting, InputKey.LeftAlt);

        #endregion Equal Sorting Modifier Key

        #region Inverse Selection Modifier Key

        [SettingPropertyDropdown("Inverse Selection Modifier Key", Order = 6, RequireRestart = false, HintText = "When combined with any of the selection keys below, the formations it encompasses will be inverted from their current state, potentially being added or removed from the current selection.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> InverseSelectionModifierKeySetting
        {
            get => GetModifierSetting(ref inverseSelectionModifierKeySetting, InputKey.LeftControl);
            set => SetSetting(ref inverseSelectionModifierKeySetting, value);
        }

        private DropdownDefault<InputKey> inverseSelectionModifierKeySetting;

        public static InputKey InverseSelectionModifierKey => GetValue(Instance?.InverseSelectionModifierKeySetting, InputKey.LeftControl);

        #endregion Inverse Selection Modifier Key

        #region Select All Formations Key

        [SettingPropertyDropdown("Select All Formations Key", Order = 7, RequireRestart = false, HintText = "This key will select all formations.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllKeySetting
        {
            get => GetNormalSetting(ref selectAllKeySetting, InputKey.F);
            set => SetSetting(ref selectAllKeySetting, value);
        }

        private DropdownDefault<InputKey> selectAllKeySetting;

        public static InputKey SelectAllKey => GetValue(Instance?.SelectAllKeySetting, InputKey.F);

        #endregion Select All Formations Key

        #region Select All Melee Cavalry Formations Key

        [SettingPropertyDropdown("Select All Melee Cavalry Formations Key", Order = 8, RequireRestart = false, HintText = "This key will select all melee cavalry formations: Cavalry, Light Cavalry, Heavy Cavalry.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllMeleeCavalryKeySetting
        {
            get => GetNormalSetting(ref selectAllMeleeCavalryKeySetting, InputKey.C);
            set => SetSetting(ref selectAllMeleeCavalryKeySetting, value);
        }

        private DropdownDefault<InputKey> selectAllMeleeCavalryKeySetting;

        public static InputKey SelectAllMeleeCavalryKey => GetValue(Instance?.SelectAllMeleeCavalryKeySetting, InputKey.C);

        #endregion Select All Melee Cavalry Formations Key

        #region Select All Ranged Cavalry Formations Key

        [SettingPropertyDropdown("Select All Ranged Cavalry Formations Key", Order = 9, RequireRestart = false, HintText = "This key will select all ranged cavalry formations: Horse Archers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllHorseArchersKeySetting
        {
            get => GetNormalSetting(ref selectAllHorseArchersKeySetting, InputKey.V);
            set => SetSetting(ref selectAllHorseArchersKeySetting, value);
        }

        private DropdownDefault<InputKey> selectAllHorseArchersKeySetting;

        public static InputKey SelectAllRangedCavalryKey => GetValue(Instance?.SelectAllHorseArchersKeySetting, InputKey.V);

        #endregion Select All Ranged Cavalry Formations Key

        #region Select All Ground Melee Formations Key

        [SettingPropertyDropdown("Select All Ground Melee Formations Key", Order = 10, RequireRestart = false, HintText = "This key will select all ground melee formations: Infantry, Heavy Infantry.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllInfantryKeySetting
        {
            get => GetNormalSetting(ref selectAllInfantryKeySetting, InputKey.H);
            set => SetSetting(ref selectAllInfantryKeySetting, value);
        }

        private DropdownDefault<InputKey> selectAllInfantryKeySetting;
        public static InputKey SelectAllGroundMeleeKey => GetValue(Instance?.SelectAllInfantryKeySetting, InputKey.H);

        #endregion Select All Ground Melee Formations Key

        #region Select All Ground Ranged Formations Key

        [SettingPropertyDropdown("Select All Ground Ranged Formations Key", Order = 11, RequireRestart = false, HintText = "This key will select all ground ranged formations: Archers, Skirmishers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllArchersAndSkirmishersKeySetting
        {
            get => GetNormalSetting(ref selectAllArchersAndSkirmishersKeySetting, InputKey.J);
            set => SetSetting(ref selectAllArchersAndSkirmishersKeySetting, value);
        }

        private DropdownDefault<InputKey> selectAllArchersAndSkirmishersKeySetting;

        public static InputKey SelectAllGroundRangedKey => GetValue(Instance?.SelectAllArchersAndSkirmishersKeySetting, InputKey.J);

        #endregion Select All Ground Ranged Formations Key

        #region Select All Basic Melee Formations Key

        [SettingPropertyDropdown("Select All Basic Melee Formations Key", Order = 12, RequireRestart = false, HintText = "This key will select all basic melee formations: Infantry, Cavalry.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllNormalMeleeKeySetting
        {
            get => GetNormalSetting(ref selectAllNormalMeleeKeySetting, InputKey.Y);
            set => SetSetting(ref selectAllNormalMeleeKeySetting, value);
        }

        private DropdownDefault<InputKey> selectAllNormalMeleeKeySetting;

        public static InputKey SelectAllBasicMeleeKey => GetValue(Instance?.SelectAllNormalMeleeKeySetting, InputKey.Y);

        #endregion Select All Basic Melee Formations Key

        #region Select All Basic Ranged Formations Key

        [SettingPropertyDropdown("Select All Ranged Formations Key", Order = 13, RequireRestart = false, HintText = "This key will select all basic ranged formations: Archers, Horse Archers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllRangedKeySetting
        {
            get => GetNormalSetting(ref selectAllRangedKeySetting, InputKey.U);
            set => SetSetting(ref selectAllRangedKeySetting, value);
        }

        private DropdownDefault<InputKey> selectAllRangedKeySetting;

        public static InputKey SelectAllBasicRangedKey => GetValue(Instance?.SelectAllRangedKeySetting, InputKey.U);

        #endregion Select All Basic Ranged Formations Key

        #region Select All Ground Formations Key

        [SettingPropertyDropdown("Select All Ground Formations Key", Order = 14, RequireRestart = false, HintText = "This key will select all ground formations: Infantry, Heavy Infantry, Archers, Skirmishers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllGroundedKeySetting
        {
            get => GetNormalSetting(ref selectAllGroundedKeySetting, InputKey.N);
            set => SetSetting(ref selectAllGroundedKeySetting, value);
        }

        private DropdownDefault<InputKey> selectAllGroundedKeySetting;

        public static InputKey SelectAllGroundKey => GetValue(Instance?.SelectAllGroundedKeySetting, InputKey.N);

        #endregion Select All Ground Formations Key

        #region Select All Cavalry Formations Key

        [SettingPropertyDropdown("Select All Cavalry Formations Key", Order = 15, RequireRestart = false, HintText = "This key will select all cavalry formations: Cavalry, Light Cavalry, Heavy Cavalry, Horse Archers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private DropdownDefault<InputKey> SelectAllMountedKeySetting
        {
            get => GetNormalSetting(ref selectAllMountedKeySetting, InputKey.M);
            set => SetSetting(ref selectAllMountedKeySetting, value);
        }

        private DropdownDefault<InputKey> selectAllMountedKeySetting;

        public static InputKey SelectAllCavalryKey => GetValue(Instance?.SelectAllMountedKeySetting, InputKey.M);

        #endregion Select All Cavalry Formations Key

        #region Selection Spacers

#pragma warning disable IDE0051 // Remove unused private members
        [SettingPropertyBool("S1", Order = 16, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private bool S1
        { get => false; set { } }

        [SettingPropertyBool("S2", Order = 17, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private bool S2
        { get => false; set { } }

        [SettingPropertyBool("S3", Order = 18, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private bool S3
        { get => false; set { } }

        [SettingPropertyBool("S4", Order = 19, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private bool S4
        { get => false; set { } }

        [SettingPropertyBool("S5", Order = 20, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private bool S5
        { get => false; set { } }

        [SettingPropertyBool("S6", Order = 21, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private bool S6
        { get => false; set { } }
#pragma warning restore IDE0051 // Remove unused private members

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

        private static (InputKey[], int) GetUsableNormalKeysAndDefaultIndex(InputKey defaultKey) => GetUsableKeysAndDefaultIndexFromKeyValues(defaultKey, GetNormalKeyValues());

        private static (InputKey[], int) GetUsableModifierKeysAndDefaultIndex(InputKey defaultKey) => GetUsableKeysAndDefaultIndexFromKeyValues(defaultKey, GetModifierKeyValues());

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
            List<int> toUse = new List<int>
            {
                29, // left ctrl
                42, // left shift
                56, // left alt
                157, // right ctrl
                54, // right shift
                184 // right alt
            };
            return toUse.ToArray();
        }

        private static int[] GetRangeOfIntegers(int from, int to) => Enumerable.Range(from, 1 + to - from).ToArray();

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