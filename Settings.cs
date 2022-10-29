using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using MCM.Common;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.InputSystem;

namespace FormationSorter
{
    internal class Settings
    {
        internal static Settings Instance = new Settings();

        private readonly ISettings provider;
        internal Settings() => provider = CustomSettings.Instance is ISettings settings ? settings : DefaultSettings.Instance;

        internal InputKey OrderKey => provider.OrderKey;
        internal InputKey TierSortKey => provider.TierSortKey;
        internal InputKey ShieldSortKey => provider.ShieldSortKey;
        internal InputKey SkirmisherSortKey => provider.SkirmisherSortKey;
        internal InputKey EqualSortKey => provider.EqualSortKey;
        internal InputKey InverseSelectKey => provider.InverseSelectKey;
        internal InputKey AllSelectKey => provider.AllSelectKey;
        internal InputKey MeleeCavalrySelectKey => provider.MeleeCavalrySelectKey;
        internal InputKey RangedCavalrySelectKey => provider.RangedCavalrySelectKey;
        internal InputKey MeleeGroundSelectKey => provider.MeleeGroundSelectKey;
        internal InputKey RangedGroundSelectKey => provider.RangedGroundSelectKey;
        internal InputKey MeleeSelectKey => provider.MeleeSelectKey;
        internal InputKey RangedSelectKey => provider.RangedSelectKey;
        internal InputKey GroundSelectKey => provider.GroundSelectKey;
        internal InputKey CavalrySelectKey => provider.CavalrySelectKey;
    }

    internal interface ISettings
    {
        InputKey OrderKey { get; }
        InputKey TierSortKey { get; }
        InputKey ShieldSortKey { get; }
        InputKey SkirmisherSortKey { get; }
        InputKey EqualSortKey { get; }
        InputKey InverseSelectKey { get; }
        InputKey AllSelectKey { get; }
        InputKey MeleeCavalrySelectKey { get; }
        InputKey RangedCavalrySelectKey { get; }
        InputKey MeleeGroundSelectKey { get; }
        InputKey RangedGroundSelectKey { get; }
        InputKey MeleeSelectKey { get; }
        InputKey RangedSelectKey { get; }
        InputKey GroundSelectKey { get; }
        InputKey CavalrySelectKey { get; }
    }

    internal class DefaultSettings : ISettings
    {
        internal static DefaultSettings Instance = new DefaultSettings();

        public InputKey OrderKey => InputKey.X;
        public InputKey TierSortKey => InputKey.L;
        public InputKey ShieldSortKey => InputKey.LeftShift;
        public InputKey SkirmisherSortKey => InputKey.LeftControl;
        public InputKey EqualSortKey => InputKey.LeftAlt;
        public InputKey InverseSelectKey => InputKey.LeftControl;
        public InputKey AllSelectKey => InputKey.F;
        public InputKey MeleeCavalrySelectKey => InputKey.C;
        public InputKey RangedCavalrySelectKey => InputKey.V;
        public InputKey MeleeGroundSelectKey => InputKey.H;
        public InputKey RangedGroundSelectKey => InputKey.J;
        public InputKey MeleeSelectKey => InputKey.Y;
        public InputKey RangedSelectKey => InputKey.U;
        public InputKey GroundSelectKey => InputKey.N;
        public InputKey CavalrySelectKey => InputKey.M;
    }

    internal class CustomSettings : AttributeGlobalSettings<CustomSettings>, ISettings
    {
        public override string Id => "FormationSorter";
        public override string DisplayName => "Formation Sorter " + typeof(CustomSettings).Assembly.GetName().Version.ToString(3);
        public override string FolderName => "FormationSorter";
        public override string FormatType => "xml";

        [SettingPropertyDropdown("Order Key", Order = 1, RequireRestart = false, HintText = "Sort Troops Between Formations order menu key; troops in selected formations will be sorted into their single best formation if one of its kind is among the selected formations.")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        public Dropdown<KeySelection> OrderKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.OrderKey, KeySelectionHelper.EnumerateKeys());
        public InputKey OrderKey => OrderKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.OrderKey;

        [SettingPropertyDropdown("Tier Sort Key", Order = 2, RequireRestart = false, HintText = "Tier sorting key; all infantry and cavalry troops will be sorted by their tiers.")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        public Dropdown<KeySelection> TierSortKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.TierSortKey, KeySelectionHelper.EnumerateKeys());
        public InputKey TierSortKey => TierSortKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.TierSortKey;

        [SettingPropertyDropdown("Shield Sorting Modifier Key", Order = 3, RequireRestart = false, HintText = "When combined with the Order Key shielded infantry and skirmishers get put into the Infantry formation while unshielded infantry and skirmishers get put into the Skirmisher formation.")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        public Dropdown<KeySelection> ShieldSortKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.ShieldSortKey, KeySelectionHelper.EnumerateModifiers());
        public InputKey ShieldSortKey => ShieldSortKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.ShieldSortKey;

        [SettingPropertyDropdown("Skirmisher Sorting Modifier Key", Order = 4, RequireRestart = false, HintText = "When combined with the Order Key javelineers, rock throwers, etc. get put into the Skirmisher formation instead of Infantry.")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        public Dropdown<KeySelection> SkirmisherSortKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.SkirmisherSortKey, KeySelectionHelper.EnumerateModifiers());
        public InputKey SkirmisherSortKey => SkirmisherSortKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.SkirmisherSortKey;

        [SettingPropertyDropdown("Equal Sorting Modifier Key", Order = 5, RequireRestart = false, HintText = "When combined with the Order Key troops will be sorted equally amongst the selected formations instead of being put in only their single best formation.")]
        [SettingPropertyGroup("General", GroupOrder = 1)]
        public Dropdown<KeySelection> EqualSortKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.EqualSortKey, KeySelectionHelper.EnumerateModifiers());
        public InputKey EqualSortKey => EqualSortKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.EqualSortKey;

        [SettingPropertyDropdown("Inverse Selection Modifier Key", Order = 6, RequireRestart = false, HintText = "When combined with any of the selection keys below, the formations it encompasses will be inverted from their current state, potentially being added or removed from the current selection.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public Dropdown<KeySelection> InverseSelectKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.InverseSelectKey, KeySelectionHelper.EnumerateModifiers());
        public InputKey InverseSelectKey => InverseSelectKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.InverseSelectKey;

        [SettingPropertyDropdown("Select All Formations Key", Order = 7, RequireRestart = false, HintText = "This key will select all formations.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public Dropdown<KeySelection> AllSelectKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.AllSelectKey, KeySelectionHelper.EnumerateKeys());
        public InputKey AllSelectKey => AllSelectKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.AllSelectKey;

        [SettingPropertyDropdown("Select All Melee Cavalry Formations Key", Order = 8, RequireRestart = false, HintText = "This key will select all melee cavalry formations: Cavalry, Light Cavalry, Heavy Cavalry.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public Dropdown<KeySelection> MeleeCavalrySelectKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.MeleeCavalrySelectKey, KeySelectionHelper.EnumerateKeys());
        public InputKey MeleeCavalrySelectKey => MeleeCavalrySelectKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.MeleeCavalrySelectKey;

        [SettingPropertyDropdown("Select All Ranged Cavalry Formations Key", Order = 9, RequireRestart = false, HintText = "This key will select all ranged cavalry formations: Horse Archers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public Dropdown<KeySelection> RangedCavalrySelectKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.RangedCavalrySelectKey, KeySelectionHelper.EnumerateKeys());
        public InputKey RangedCavalrySelectKey => RangedCavalrySelectKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.RangedCavalrySelectKey;

        [SettingPropertyDropdown("Select All Ground Melee Formations Key", Order = 10, RequireRestart = false, HintText = "This key will select all ground melee formations: Infantry, Heavy Infantry.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public Dropdown<KeySelection> GroundMeleeSelectKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.MeleeGroundSelectKey, KeySelectionHelper.EnumerateKeys());
        public InputKey MeleeGroundSelectKey => GroundMeleeSelectKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.MeleeGroundSelectKey;

        [SettingPropertyDropdown("Select All Ground Ranged Formations Key", Order = 11, RequireRestart = false, HintText = "This key will select all ground ranged formations: Archers, Skirmishers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public Dropdown<KeySelection> GroundRangedSelectKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.RangedGroundSelectKey, KeySelectionHelper.EnumerateKeys());
        public InputKey RangedGroundSelectKey => GroundRangedSelectKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.RangedGroundSelectKey;

        [SettingPropertyDropdown("Select All Basic Melee Formations Key", Order = 12, RequireRestart = false, HintText = "This key will select all basic melee formations: Infantry, Cavalry.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public Dropdown<KeySelection> MeleeSelectKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.MeleeSelectKey, KeySelectionHelper.EnumerateKeys());
        public InputKey MeleeSelectKey => MeleeSelectKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.MeleeSelectKey;

        [SettingPropertyDropdown("Select All Ranged Formations Key", Order = 13, RequireRestart = false, HintText = "This key will select all basic ranged formations: Archers, Horse Archers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public Dropdown<KeySelection> RangedSelectKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.RangedSelectKey, KeySelectionHelper.EnumerateKeys());
        public InputKey RangedSelectKey => RangedSelectKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.RangedSelectKey;

        [SettingPropertyDropdown("Select All Ground Formations Key", Order = 14, RequireRestart = false, HintText = "This key will select all ground formations: Infantry, Heavy Infantry, Archers, Skirmishers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public Dropdown<KeySelection> GroundSelectKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.GroundSelectKey, KeySelectionHelper.EnumerateKeys());
        public InputKey GroundSelectKey => GroundSelectKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.GroundSelectKey;

        [SettingPropertyDropdown("Select All Cavalry Formations Key", Order = 15, RequireRestart = false, HintText = "This key will select all cavalry formations: Cavalry, Light Cavalry, Heavy Cavalry, Horse Archers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        public Dropdown<KeySelection> CavalrySelectKeyDropdown { get; set; } = KeySelectionHelper.Dropdown(DefaultSettings.Instance.CavalrySelectKey, KeySelectionHelper.EnumerateKeys());
        public InputKey CavalrySelectKey => CavalrySelectKeyDropdown.SelectedValue.Key ?? DefaultSettings.Instance.CavalrySelectKey;

        [SettingPropertyBool("S1", Order = 16, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private bool S1 { get => false; set { } }

        [SettingPropertyBool("S2", Order = 17, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private bool S2 { get => false; set { } }

        [SettingPropertyBool("S3", Order = 18, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private bool S3 { get => false; set { } }

        [SettingPropertyBool("S4", Order = 19, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private bool S4 { get => false; set { } }

        [SettingPropertyBool("S5", Order = 20, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private bool S5 { get => false; set { } }

        [SettingPropertyBool("S6", Order = 21, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Selection", GroupOrder = 2)]
        private bool S6 { get => false; set { } }
    }

    internal class KeySelection
    {
        internal readonly InputKey? Key;
        internal KeySelection(InputKey? key) => Key = key;
        public override string ToString() => (!Key.HasValue || Key is InputKey.Invalid) ? "None" : Key.ToString();
    }

    internal static class KeySelectionHelper
    {
        internal static Dropdown<KeySelection> Dropdown(InputKey defaultKey, IEnumerable<KeySelection> keys)
        {
            Dropdown<KeySelection> dropdown = new Dropdown<KeySelection>(keys, 0);
            dropdown.SelectedIndex = dropdown.FindIndex(s => s.Key == defaultKey);
            return dropdown;
        }

        private static IEnumerable<int> Range(int from, int to) => Enumerable.Range(from, 1 + to - from);
        internal static IEnumerable<KeySelection> EnumerateKeys()
        {
            yield return new KeySelection(InputKey.Invalid);
            foreach (int i in Range(16, 27)) // Q to ]
                yield return new KeySelection((InputKey)i);
            foreach (int i in Range(30, 40)) // A to '
                yield return new KeySelection((InputKey)i);
            foreach (int i in Range(44, 53)) // Z to /
                yield return new KeySelection((InputKey)i);
        }

        internal static IEnumerable<KeySelection> EnumerateModifiers()
        {
            yield return new KeySelection(InputKey.Invalid);
            yield return new KeySelection(InputKey.LeftControl);
            yield return new KeySelection(InputKey.LeftShift);
            yield return new KeySelection(InputKey.LeftAlt);
            yield return new KeySelection(InputKey.RightControl);
            yield return new KeySelection(InputKey.RightShift);
            yield return new KeySelection(InputKey.RightAlt);
        }
    }
}