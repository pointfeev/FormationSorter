using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using MCM.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using TaleWorlds.Core;
using TaleWorlds.InputSystem;

namespace FormationSorter
{
    internal class Settings
    {
        private static Settings _instance;
        internal static Settings Instance
        {
            get
            {
                if (_instance is null)
                    _instance = new Settings();
                return _instance;
            }
        }
        private readonly ISettings provider;
        internal Settings() => provider = CustomSettings.Instance is null ? DefaultSettings.Instance : CustomSettings.Instance;
        internal bool UserDefinedFormationClasses => provider.UserDefinedFormationClasses;
        internal FormationClass Formation1 => provider.Formation1;
        internal FormationClass Formation2 => provider.Formation2;
        internal FormationClass Formation3 => provider.Formation3;
        internal FormationClass Formation4 => provider.Formation4;
        internal FormationClass Formation5 => provider.Formation5;
        internal FormationClass Formation6 => provider.Formation6;
        internal FormationClass Formation7 => provider.Formation7;
        internal FormationClass Formation8 => provider.Formation8;
        internal InputKey OrderKey => provider.OrderKey;
        internal InputKey TierSortKey => provider.TierSortKey;
        internal InputKey ShieldSortKey => provider.ShieldSortKey;
        internal InputKey SkirmisherSortKey => provider.SkirmisherSortKey;
        internal InputKey EqualSortKey => provider.EqualSortKey;
        internal InputKey InverseSelectKey => provider.InverseSelectKey;
        internal InputKey AllSelectKey => provider.AllSelectKey;
        internal InputKey CavalryMeleeSelectKey => provider.CavalryMeleeSelectKey;
        internal InputKey CavalryRangedSelectKey => provider.CavalryRangedSelectKey;
        internal InputKey GroundMeleeSelectKey => provider.GroundMeleeSelectKey;
        internal InputKey GroundRangedSelectKey => provider.GroundRangedSelectKey;
        internal InputKey MeleeSelectKey => provider.MeleeSelectKey;
        internal InputKey RangedSelectKey => provider.RangedSelectKey;
        internal InputKey GroundSelectKey => provider.GroundSelectKey;
        internal InputKey CavalrySelectKey => provider.CavalrySelectKey;
    }

    internal interface ISettings
    {
        bool UserDefinedFormationClasses { get; }
        FormationClass Formation1 { get; }
        FormationClass Formation2 { get; }
        FormationClass Formation3 { get; }
        FormationClass Formation4 { get; }
        FormationClass Formation5 { get; }
        FormationClass Formation6 { get; }
        FormationClass Formation7 { get; }
        FormationClass Formation8 { get; }
        InputKey OrderKey { get; }
        InputKey TierSortKey { get; }
        InputKey ShieldSortKey { get; }
        InputKey SkirmisherSortKey { get; }
        InputKey EqualSortKey { get; }
        InputKey InverseSelectKey { get; }
        InputKey AllSelectKey { get; }
        InputKey CavalryMeleeSelectKey { get; }
        InputKey CavalryRangedSelectKey { get; }
        InputKey GroundMeleeSelectKey { get; }
        InputKey GroundRangedSelectKey { get; }
        InputKey MeleeSelectKey { get; }
        InputKey RangedSelectKey { get; }
        InputKey GroundSelectKey { get; }
        InputKey CavalrySelectKey { get; }
    }

    internal class DefaultSettings : ISettings
    {
        private static ISettings _instance;
        internal static ISettings Instance
        {
            get
            {
                if (_instance is null)
                    _instance = new DefaultSettings();
                return _instance;
            }
        }
        bool ISettings.UserDefinedFormationClasses { get; } = true;
        FormationClass ISettings.Formation1 { get; } = 0;
        FormationClass ISettings.Formation2 { get; } = (FormationClass)1;
        FormationClass ISettings.Formation3 { get; } = (FormationClass)2;
        FormationClass ISettings.Formation4 { get; } = (FormationClass)3;
        FormationClass ISettings.Formation5 { get; } = (FormationClass)4;
        FormationClass ISettings.Formation6 { get; } = (FormationClass)5;
        FormationClass ISettings.Formation7 { get; } = (FormationClass)6;
        FormationClass ISettings.Formation8 { get; } = (FormationClass)7;
        InputKey ISettings.OrderKey { get; } = InputKey.X;
        InputKey ISettings.TierSortKey { get; } = InputKey.L;
        InputKey ISettings.ShieldSortKey { get; } = InputKey.LeftShift;
        InputKey ISettings.SkirmisherSortKey { get; } = InputKey.LeftControl;
        InputKey ISettings.EqualSortKey { get; } = InputKey.LeftAlt;
        InputKey ISettings.InverseSelectKey { get; } = InputKey.LeftControl;
        InputKey ISettings.AllSelectKey { get; } = InputKey.F;
        InputKey ISettings.CavalryMeleeSelectKey { get; } = InputKey.C;
        InputKey ISettings.CavalryRangedSelectKey { get; } = InputKey.V;
        InputKey ISettings.GroundMeleeSelectKey { get; } = InputKey.H;
        InputKey ISettings.GroundRangedSelectKey { get; } = InputKey.J;
        InputKey ISettings.MeleeSelectKey { get; } = InputKey.Y;
        InputKey ISettings.RangedSelectKey { get; } = InputKey.U;
        InputKey ISettings.GroundSelectKey { get; } = InputKey.N;
        InputKey ISettings.CavalrySelectKey { get; } = InputKey.M;
    }

    internal class CustomSettings : AttributeGlobalSettings<CustomSettings>, ISettings
    {
        public override string Id => "FormationSorter";
        public override string DisplayName => "Formation Sorter " + typeof(CustomSettings).Assembly.GetName().Version.ToString(3);
        public override string FolderName => "FormationSorter";
        public override string FormatType => "xml";

        [SettingPropertyBool("User-Defined Formation Classes", Order = 1, RequireRestart = false, HintText = "Whether or not to use the user-defined formation classes below in place of the default dynamic formation classes. (highly recommended)")]
        [SettingPropertyGroup("Formation Classes", GroupOrder = 1)]
        public bool UserDefinedFormationClasses { get; set; } = DefaultSettings.Instance.UserDefinedFormationClasses;

        [SettingPropertyDropdown("Formation #1", Order = 2, RequireRestart = false)]
        [SettingPropertyGroup("Formation Classes", GroupOrder = 1)]
        public Dropdown<FormationClassSelection> FormationDropdown1 { get; set; } = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation1, DropdownHelper.EnumerateRegularFormations());
        FormationClass ISettings.Formation1 => FormationDropdown1.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation1;

        [SettingPropertyDropdown("Formation #2", Order = 3, RequireRestart = false)]
        [SettingPropertyGroup("Formation Classes", GroupOrder = 1)]
        public Dropdown<FormationClassSelection> FormationDropdown2 { get; set; } = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation2, DropdownHelper.EnumerateRegularFormations());
        FormationClass ISettings.Formation2 => FormationDropdown2.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation2;

        [SettingPropertyDropdown("Formation #3", Order = 4, RequireRestart = false)]
        [SettingPropertyGroup("Formation Classes", GroupOrder = 1)]
        public Dropdown<FormationClassSelection> FormationDropdown3 { get; set; } = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation3, DropdownHelper.EnumerateRegularFormations());
        FormationClass ISettings.Formation3 => FormationDropdown3.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation3;

        [SettingPropertyDropdown("Formation #4", Order = 5, RequireRestart = false)]
        [SettingPropertyGroup("Formation Classes", GroupOrder = 1)]
        public Dropdown<FormationClassSelection> FormationDropdown4 { get; set; } = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation4, DropdownHelper.EnumerateRegularFormations());
        FormationClass ISettings.Formation4 => FormationDropdown4.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation4;

        [SettingPropertyDropdown("Formation #5", Order = 6, RequireRestart = false)]
        [SettingPropertyGroup("Formation Classes", GroupOrder = 1)]
        public Dropdown<FormationClassSelection> FormationDropdown5 { get; set; } = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation5, DropdownHelper.EnumerateRegularFormations());
        FormationClass ISettings.Formation5 => FormationDropdown5.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation5;

        [SettingPropertyDropdown("Formation #6", Order = 7, RequireRestart = false)]
        [SettingPropertyGroup("Formation Classes", GroupOrder = 1)]
        public Dropdown<FormationClassSelection> FormationDropdown6 { get; set; } = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation6, DropdownHelper.EnumerateRegularFormations());
        FormationClass ISettings.Formation6 => FormationDropdown6.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation6;

        [SettingPropertyDropdown("Formation #7", Order = 8, RequireRestart = false)]
        [SettingPropertyGroup("Formation Classes", GroupOrder = 1)]
        public Dropdown<FormationClassSelection> FormationDropdown7 { get; set; } = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation7, DropdownHelper.EnumerateRegularFormations());
        FormationClass ISettings.Formation7 => FormationDropdown7.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation7;

        [SettingPropertyDropdown("Formation #8", Order = 9, RequireRestart = false)]
        [SettingPropertyGroup("Formation Classes", GroupOrder = 1)]
        public Dropdown<FormationClassSelection> FormationDropdown8 { get; set; } = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation8, DropdownHelper.EnumerateRegularFormations());
        FormationClass ISettings.Formation8 => FormationDropdown8.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation8;

        [SettingPropertyDropdown("Order Key", Order = 1, RequireRestart = false, HintText = "Sort Troops Between Formations order menu key; troops in selected formations will be sorted into their single best formation if one of its kind is among the selected formations.")]
        [SettingPropertyGroup("Order", GroupOrder = 2)]
        public Dropdown<KeySelection> OrderKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.OrderKey, DropdownHelper.EnumerateKeys());
        InputKey ISettings.OrderKey => OrderKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.OrderKey;

        [SettingPropertyDropdown("Tier Sort Key", Order = 2, RequireRestart = false, HintText = "Tier sorting key; all infantry and cavalry troops will be sorted by their tiers.")]
        [SettingPropertyGroup("Order", GroupOrder = 2)]
        public Dropdown<KeySelection> TierSortKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.TierSortKey, DropdownHelper.EnumerateKeys());
        InputKey ISettings.TierSortKey => TierSortKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.TierSortKey;

        [SettingPropertyDropdown("Shield Sorting Modifier Key", Order = 3, RequireRestart = false, HintText = "When combined with the Order Key shielded infantry and skirmishers get put into the Infantry formation while unshielded infantry and skirmishers get put into the Skirmisher formation.")]
        [SettingPropertyGroup("Order", GroupOrder = 2)]
        public Dropdown<KeySelection> ShieldSortKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.ShieldSortKey, DropdownHelper.EnumerateModifiers());
        InputKey ISettings.ShieldSortKey => ShieldSortKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.ShieldSortKey;

        [SettingPropertyDropdown("Skirmisher Sorting Modifier Key", Order = 4, RequireRestart = false, HintText = "When combined with the Order Key javelineers, rock throwers, etc. get put into the Skirmisher formation instead of Infantry.")]
        [SettingPropertyGroup("Order", GroupOrder = 2)]
        public Dropdown<KeySelection> SkirmisherSortKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.SkirmisherSortKey, DropdownHelper.EnumerateModifiers());
        InputKey ISettings.SkirmisherSortKey => SkirmisherSortKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.SkirmisherSortKey;

        [SettingPropertyDropdown("Equal Sorting Modifier Key", Order = 5, RequireRestart = false, HintText = "When combined with the Order Key troops will be sorted equally amongst the selected formations instead of being put in only their single best formation.")]
        [SettingPropertyGroup("Order", GroupOrder = 2)]
        public Dropdown<KeySelection> EqualSortKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.EqualSortKey, DropdownHelper.EnumerateModifiers());
        InputKey ISettings.EqualSortKey => EqualSortKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.EqualSortKey;

        [SettingPropertyDropdown("Inverse Selection Modifier Key", Order = 6, RequireRestart = false, HintText = "When combined with any of the selection keys below, the formations it encompasses will be inverted from their current state, potentially being added or removed from the current selection.")]
        [SettingPropertyGroup("Selection", GroupOrder = 3)]
        public Dropdown<KeySelection> InverseSelectKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.InverseSelectKey, DropdownHelper.EnumerateModifiers());
        InputKey ISettings.InverseSelectKey => InverseSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.InverseSelectKey;

        [SettingPropertyDropdown("Select All Formations Key", Order = 7, RequireRestart = false, HintText = "This key will select all formations.")]
        [SettingPropertyGroup("Selection", GroupOrder = 3)]
        public Dropdown<KeySelection> AllSelectKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.AllSelectKey, DropdownHelper.EnumerateKeys());
        InputKey ISettings.AllSelectKey => AllSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.AllSelectKey;

        [SettingPropertyDropdown("Select All Melee Cavalry Formations Key", Order = 8, RequireRestart = false, HintText = "This key will select all melee cavalry formations: Cavalry, Light Cavalry, Heavy Cavalry.")]
        [SettingPropertyGroup("Selection", GroupOrder = 3)]
        public Dropdown<KeySelection> MeleeCavalrySelectKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.CavalryMeleeSelectKey, DropdownHelper.EnumerateKeys());
        InputKey ISettings.CavalryMeleeSelectKey => MeleeCavalrySelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.CavalryMeleeSelectKey;

        [SettingPropertyDropdown("Select All Ranged Cavalry Formations Key", Order = 9, RequireRestart = false, HintText = "This key will select all ranged cavalry formations: Horse Archers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 3)]
        public Dropdown<KeySelection> RangedCavalrySelectKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.CavalryRangedSelectKey, DropdownHelper.EnumerateKeys());
        InputKey ISettings.CavalryRangedSelectKey => RangedCavalrySelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.CavalryRangedSelectKey;

        [SettingPropertyDropdown("Select All Melee Ground Formations Key", Order = 10, RequireRestart = false, HintText = "This key will select all melee ground formations: Infantry, Heavy Infantry.")]
        [SettingPropertyGroup("Selection", GroupOrder = 3)]
        public Dropdown<KeySelection> GroundMeleeSelectKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.GroundMeleeSelectKey, DropdownHelper.EnumerateKeys());
        InputKey ISettings.GroundMeleeSelectKey => GroundMeleeSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.GroundMeleeSelectKey;

        [SettingPropertyDropdown("Select All Ranged Ground Formations Key", Order = 11, RequireRestart = false, HintText = "This key will select all ranged ground formations: Archers, Skirmishers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 3)]
        public Dropdown<KeySelection> GroundRangedSelectKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.GroundRangedSelectKey, DropdownHelper.EnumerateKeys());
        InputKey ISettings.GroundRangedSelectKey => GroundRangedSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.GroundRangedSelectKey;

        [SettingPropertyDropdown("Select All Melee Formations Key", Order = 12, RequireRestart = false, HintText = "This key will select all melee formations: Infantry, Cavalry, Heavy Infantry.")]
        [SettingPropertyGroup("Selection", GroupOrder = 3)]
        public Dropdown<KeySelection> MeleeSelectKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.MeleeSelectKey, DropdownHelper.EnumerateKeys());
        InputKey ISettings.MeleeSelectKey => MeleeSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.MeleeSelectKey;

        [SettingPropertyDropdown("Select All Ranged Formations Key", Order = 13, RequireRestart = false, HintText = "This key will select all ranged formations: Archers, Skirmishers, Horse Archers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 3)]
        public Dropdown<KeySelection> RangedSelectKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.RangedSelectKey, DropdownHelper.EnumerateKeys());
        InputKey ISettings.RangedSelectKey => RangedSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.RangedSelectKey;

        [SettingPropertyDropdown("Select All Cavalry Formations Key", Order = 14, RequireRestart = false, HintText = "This key will select all cavalry formations: Cavalry, Light Cavalry, Heavy Cavalry, Horse Archers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 3)]
        public Dropdown<KeySelection> CavalrySelectKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.CavalrySelectKey, DropdownHelper.EnumerateKeys());
        InputKey ISettings.CavalrySelectKey => CavalrySelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.CavalrySelectKey;

        [SettingPropertyDropdown("Select All Ground Formations Key", Order = 15, RequireRestart = false, HintText = "This key will select all ground formations: Infantry, Heavy Infantry, Archers, Skirmishers.")]
        [SettingPropertyGroup("Selection", GroupOrder = 3)]
        public Dropdown<KeySelection> GroundSelectKeyDropdown { get; set; } = DropdownHelper.KeySelection(DefaultSettings.Instance.GroundSelectKey, DropdownHelper.EnumerateKeys());
        InputKey ISettings.GroundSelectKey => GroundSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.GroundSelectKey;

        [SettingPropertyBool("S1", Order = 16, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Spacers", GroupOrder = 4)]
        private bool S1 { get => false; set { } }

        [SettingPropertyBool("S2", Order = 17, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Spacers", GroupOrder = 4)]
        private bool S2 { get => false; set { } }

        [SettingPropertyBool("S3", Order = 18, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Spacers", GroupOrder = 4)]
        private bool S3 { get => false; set { } }

        [SettingPropertyBool("S4", Order = 19, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Spacers", GroupOrder = 4)]
        private bool S4 { get => false; set { } }

        [SettingPropertyBool("S5", Order = 20, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Spacers", GroupOrder = 4)]
        private bool S5 { get => false; set { } }

        [SettingPropertyBool("S6", Order = 21, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas.")]
        [SettingPropertyGroup("Spacers", GroupOrder = 4)]
        private bool S6 { get => false; set { } }
    }

    internal class FormationClassSelection
    {
        internal readonly FormationClass? FormationClass;
        internal readonly string Name;
        internal FormationClassSelection(FormationClass? formationClass, string name)
        {
            FormationClass = formationClass;
            Name = name;
        }
        public override string ToString() => Name;
    }

    internal class KeySelection
    {
        internal readonly InputKey? Key;
        internal KeySelection(InputKey? key) => Key = key;
        public override string ToString() => (!Key.HasValue || Key is InputKey.Invalid) ? "None" : Key.ToString();
    }

    internal static class DropdownHelper
    {
        internal static Dropdown<FormationClassSelection> FormationClassSelection(FormationClass defaultFormationClass, IEnumerable<FormationClassSelection> formationClasses)
        {
            Dropdown<FormationClassSelection> dropdown = new Dropdown<FormationClassSelection>(formationClasses, 0);
            dropdown.SelectedIndex = dropdown.FindIndex(s => s.FormationClass == defaultFormationClass);
            return dropdown;
        }

        internal static IEnumerable<FormationClassSelection> EnumerateRegularFormations()
        {
            foreach (string name in Enum.GetNames(typeof(FormationClass)))
                if (!name.StartsWith("NumberOf") && Enum.TryParse(name, false, out FormationClass formationClass) && formationClass < FormationClass.NumberOfRegularFormations)
                    yield return new FormationClassSelection(formationClass, Regex.Replace(name, "[A-Z]", " $0").Trim());
        }

        internal static Dropdown<KeySelection> KeySelection(InputKey defaultKey, IEnumerable<KeySelection> keys)
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