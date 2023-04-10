using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FormationSorter.Utilities;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using MCM.Common;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;

namespace FormationSorter;

internal interface ISettings
{
    bool AssignNewFormationCaptains { get; }
    bool UserDefinedFormationClasses { get; }
    FormationClass CompanionFormation { get; }
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

internal class Settings : ISettings
{
    private static Settings instance;
    private readonly ISettings provider;

    private Settings() => provider = CustomSettings.Instance is null ? DefaultSettings.Instance : CustomSettings.Instance;

    internal static Settings Instance => instance ??= new();

    public bool AssignNewFormationCaptains => provider.AssignNewFormationCaptains;
    public bool UserDefinedFormationClasses => provider.UserDefinedFormationClasses;
    public FormationClass CompanionFormation => provider.CompanionFormation;
    public FormationClass Formation1 => provider.Formation1;
    public FormationClass Formation2 => provider.Formation2;
    public FormationClass Formation3 => provider.Formation3;
    public FormationClass Formation4 => provider.Formation4;
    public FormationClass Formation5 => provider.Formation5;
    public FormationClass Formation6 => provider.Formation6;
    public FormationClass Formation7 => provider.Formation7;
    public FormationClass Formation8 => provider.Formation8;
    public InputKey OrderKey => provider.OrderKey;
    public InputKey TierSortKey => provider.TierSortKey;
    public InputKey ShieldSortKey => provider.ShieldSortKey;
    public InputKey SkirmisherSortKey => provider.SkirmisherSortKey;
    public InputKey EqualSortKey => provider.EqualSortKey;
    public InputKey InverseSelectKey => provider.InverseSelectKey;
    public InputKey AllSelectKey => provider.AllSelectKey;
    public InputKey CavalryMeleeSelectKey => provider.CavalryMeleeSelectKey;
    public InputKey CavalryRangedSelectKey => provider.CavalryRangedSelectKey;
    public InputKey GroundMeleeSelectKey => provider.GroundMeleeSelectKey;
    public InputKey GroundRangedSelectKey => provider.GroundRangedSelectKey;
    public InputKey MeleeSelectKey => provider.MeleeSelectKey;
    public InputKey RangedSelectKey => provider.RangedSelectKey;
    public InputKey GroundSelectKey => provider.GroundSelectKey;
    public InputKey CavalrySelectKey => provider.CavalrySelectKey;
}

internal class DefaultSettings : ISettings
{
    private static ISettings instance;

    internal static ISettings Instance => instance ??= new DefaultSettings();

    bool ISettings.AssignNewFormationCaptains => true;
    bool ISettings.UserDefinedFormationClasses => true;
    FormationClass ISettings.CompanionFormation => FormationClass.Unset;
    FormationClass ISettings.Formation1 => 0;
    FormationClass ISettings.Formation2 => (FormationClass)1;
    FormationClass ISettings.Formation3 => (FormationClass)2;
    FormationClass ISettings.Formation4 => (FormationClass)3;
    FormationClass ISettings.Formation5 => (FormationClass)4;
    FormationClass ISettings.Formation6 => (FormationClass)5;
    FormationClass ISettings.Formation7 => (FormationClass)6;
    FormationClass ISettings.Formation8 => (FormationClass)7;
    InputKey ISettings.OrderKey => InputKey.X;
    InputKey ISettings.TierSortKey => InputKey.L;
    InputKey ISettings.ShieldSortKey => InputKey.LeftShift;
    InputKey ISettings.SkirmisherSortKey => InputKey.LeftControl;
    InputKey ISettings.EqualSortKey => InputKey.LeftAlt;
    InputKey ISettings.InverseSelectKey => InputKey.LeftControl;
    InputKey ISettings.AllSelectKey => InputKey.F;
    InputKey ISettings.CavalryMeleeSelectKey => InputKey.C;
    InputKey ISettings.CavalryRangedSelectKey => InputKey.V;
    InputKey ISettings.GroundMeleeSelectKey => InputKey.H;
    InputKey ISettings.GroundRangedSelectKey => InputKey.J;
    InputKey ISettings.MeleeSelectKey => InputKey.Y;
    InputKey ISettings.RangedSelectKey => InputKey.U;
    InputKey ISettings.GroundSelectKey => InputKey.N;
    InputKey ISettings.CavalrySelectKey => InputKey.M;
}

internal class CustomSettings : AttributeGlobalSettings<CustomSettings>, ISettings
{
    public override string Id => "FormationSorter";

    public override string DisplayName
        => "Formation Sorter " + new Version(FileVersionInfo.GetVersionInfo(typeof(CustomSettings).Assembly.Location).FileVersion).ToString(3);

    public override string FolderName => "FormationSorter";
    public override string FormatType => "xml";

    [SettingPropertyDropdown("Companions", Order = 1, RequireRestart = false,
         HintText = "When set to Default, companions will simply go into the formation they fit in as if they were a normal unit."),
     SettingPropertyGroup("User-Defined Formation Classes", GroupOrder = 1)]
    public Dropdown<FormationClassSelection> CompanionFormationDropdown { get; set; }
        = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.CompanionFormation, DropdownHelper.EnumerateRegularFormations(true));

    [SettingPropertyDropdown("Formation #1", Order = 2, RequireRestart = false), SettingPropertyGroup("User-Defined Formation Classes", GroupOrder = 1)]
    public Dropdown<FormationClassSelection> FormationDropdown1 { get; set; }
        = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation1, DropdownHelper.EnumerateRegularFormations());

    [SettingPropertyDropdown("Formation #2", Order = 3, RequireRestart = false), SettingPropertyGroup("User-Defined Formation Classes", GroupOrder = 1)]
    public Dropdown<FormationClassSelection> FormationDropdown2 { get; set; }
        = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation2, DropdownHelper.EnumerateRegularFormations());

    [SettingPropertyDropdown("Formation #3", Order = 4, RequireRestart = false), SettingPropertyGroup("User-Defined Formation Classes", GroupOrder = 1)]
    public Dropdown<FormationClassSelection> FormationDropdown3 { get; set; }
        = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation3, DropdownHelper.EnumerateRegularFormations());

    [SettingPropertyDropdown("Formation #4", Order = 5, RequireRestart = false), SettingPropertyGroup("User-Defined Formation Classes", GroupOrder = 1)]
    public Dropdown<FormationClassSelection> FormationDropdown4 { get; set; }
        = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation4, DropdownHelper.EnumerateRegularFormations());

    [SettingPropertyDropdown("Formation #5", Order = 6, RequireRestart = false), SettingPropertyGroup("User-Defined Formation Classes", GroupOrder = 1)]
    public Dropdown<FormationClassSelection> FormationDropdown5 { get; set; }
        = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation5, DropdownHelper.EnumerateRegularFormations());

    [SettingPropertyDropdown("Formation #6", Order = 7, RequireRestart = false), SettingPropertyGroup("User-Defined Formation Classes", GroupOrder = 1)]
    public Dropdown<FormationClassSelection> FormationDropdown6 { get; set; }
        = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation6, DropdownHelper.EnumerateRegularFormations());

    [SettingPropertyDropdown("Formation #7", Order = 8, RequireRestart = false), SettingPropertyGroup("User-Defined Formation Classes", GroupOrder = 1)]
    public Dropdown<FormationClassSelection> FormationDropdown7 { get; set; }
        = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation7, DropdownHelper.EnumerateRegularFormations());

    [SettingPropertyDropdown("Formation #8", Order = 9, RequireRestart = false), SettingPropertyGroup("User-Defined Formation Classes", GroupOrder = 1)]
    public Dropdown<FormationClassSelection> FormationDropdown8 { get; set; }
        = DropdownHelper.FormationClassSelection(DefaultSettings.Instance.Formation8, DropdownHelper.EnumerateRegularFormations());

    [SettingPropertyDropdown("Order Key", Order = 1, RequireRestart = false,
         HintText
             = "Sort Troops Between Formations order menu key; troops in selected formations will be sorted into their single best formation if one of its kind is among the selected formations."),
     SettingPropertyGroup("Order", GroupOrder = 2)]
    public Dropdown<KeySelection> OrderKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.OrderKey, DropdownHelper.EnumerateKeys());

    [SettingPropertyDropdown("Tier Sort Key", Order = 2, RequireRestart = false,
         HintText = "Tier sorting key; all infantry and cavalry troops will be sorted into separate formations by their tiers."),
     SettingPropertyGroup("Order", GroupOrder = 2)]
    public Dropdown<KeySelection> TierSortKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.TierSortKey, DropdownHelper.EnumerateKeys());

    [SettingPropertyDropdown("Shield Sorting Modifier Key", Order = 3, RequireRestart = false,
         HintText
             = "When combined with the Order Key shielded infantry and skirmishers get put into the Infantry formation while unshielded infantry and skirmishers get put into the Skirmisher formation."),
     SettingPropertyGroup("Order", GroupOrder = 2)]
    public Dropdown<KeySelection> ShieldSortKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.ShieldSortKey, DropdownHelper.EnumerateModifiers());

    [SettingPropertyDropdown("Skirmisher Sorting Modifier Key", Order = 4, RequireRestart = false,
         HintText = "When combined with the Order Key javelineers, rock throwers, etc. get put into the Skirmisher formation instead of Infantry."),
     SettingPropertyGroup("Order", GroupOrder = 2)]
    public Dropdown<KeySelection> SkirmisherSortKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.SkirmisherSortKey, DropdownHelper.EnumerateModifiers());

    [SettingPropertyDropdown("Equal Sorting Modifier Key", Order = 5, RequireRestart = false,
         HintText
             = "When combined with the Order Key troops will be sorted equally amongst the selected formations instead of being put in only their single best formation."),
     SettingPropertyGroup("Order", GroupOrder = 2)]
    public Dropdown<KeySelection> EqualSortKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.EqualSortKey, DropdownHelper.EnumerateModifiers());

    [SettingPropertyDropdown("Inverse Selection Modifier Key", Order = 6, RequireRestart = false,
         HintText
             = "When combined with any of the selection keys below, the formations it encompasses will be inverted from their current state, potentially being added or removed from the current selection."),
     SettingPropertyGroup("Selection", GroupOrder = 3)]
    public Dropdown<KeySelection> InverseSelectKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.InverseSelectKey, DropdownHelper.EnumerateModifiers());

    [SettingPropertyDropdown("Select All Formations Key", Order = 7, RequireRestart = false, HintText = "This key will select all formations."),
     SettingPropertyGroup("Selection", GroupOrder = 3)]
    public Dropdown<KeySelection> AllSelectKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.AllSelectKey, DropdownHelper.EnumerateKeys());

    [SettingPropertyDropdown("Select All Melee Cavalry Formations Key", Order = 8, RequireRestart = false,
         HintText = "This key will select all melee cavalry formations: Cavalry, Light Cavalry, Heavy Cavalry."),
     SettingPropertyGroup("Selection", GroupOrder = 3)]
    public Dropdown<KeySelection> MeleeCavalrySelectKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.CavalryMeleeSelectKey, DropdownHelper.EnumerateKeys());

    [SettingPropertyDropdown("Select All Ranged Cavalry Formations Key", Order = 9, RequireRestart = false,
         HintText = "This key will select all ranged cavalry formations: Horse Archers."), SettingPropertyGroup("Selection", GroupOrder = 3)]
    public Dropdown<KeySelection> RangedCavalrySelectKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.CavalryRangedSelectKey, DropdownHelper.EnumerateKeys());

    [SettingPropertyDropdown("Select All Melee Ground Formations Key", Order = 10, RequireRestart = false,
         HintText = "This key will select all melee ground formations: Infantry, Heavy Infantry."), SettingPropertyGroup("Selection", GroupOrder = 3)]
    public Dropdown<KeySelection> GroundMeleeSelectKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.GroundMeleeSelectKey, DropdownHelper.EnumerateKeys());

    [SettingPropertyDropdown("Select All Ranged Ground Formations Key", Order = 11, RequireRestart = false,
         HintText = "This key will select all ranged ground formations: Archers, Skirmishers."), SettingPropertyGroup("Selection", GroupOrder = 3)]
    public Dropdown<KeySelection> GroundRangedSelectKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.GroundRangedSelectKey, DropdownHelper.EnumerateKeys());

    [SettingPropertyDropdown("Select All Melee Formations Key", Order = 12, RequireRestart = false,
         HintText = "This key will select all melee formations: Infantry, Cavalry, Heavy Infantry."), SettingPropertyGroup("Selection", GroupOrder = 3)]
    public Dropdown<KeySelection> MeleeSelectKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.MeleeSelectKey, DropdownHelper.EnumerateKeys());

    [SettingPropertyDropdown("Select All Ranged Formations Key", Order = 13, RequireRestart = false,
         HintText = "This key will select all ranged formations: Archers, Skirmishers, Horse Archers."), SettingPropertyGroup("Selection", GroupOrder = 3)]
    public Dropdown<KeySelection> RangedSelectKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.RangedSelectKey, DropdownHelper.EnumerateKeys());

    [SettingPropertyDropdown("Select All Cavalry Formations Key", Order = 14, RequireRestart = false,
         HintText = "This key will select all cavalry formations: Cavalry, Light Cavalry, Heavy Cavalry, Horse Archers."),
     SettingPropertyGroup("Selection", GroupOrder = 3)]
    public Dropdown<KeySelection> CavalrySelectKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.CavalrySelectKey, DropdownHelper.EnumerateKeys());

    [SettingPropertyDropdown("Select All Ground Formations Key", Order = 15, RequireRestart = false,
         HintText = "This key will select all ground formations: Infantry, Heavy Infantry, Archers, Skirmishers."),
     SettingPropertyGroup("Selection", GroupOrder = 3)]
    public Dropdown<KeySelection> GroundSelectKeyDropdown { get; set; }
        = DropdownHelper.KeySelection(DefaultSettings.Instance.GroundSelectKey, DropdownHelper.EnumerateKeys());

    [SettingPropertyBool("S1", Order = 16, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas."),
     SettingPropertyGroup("Spacers", GroupOrder = 4)]
    private bool S1 { get => false; set { } }

    [SettingPropertyBool("S2", Order = 17, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas."),
     SettingPropertyGroup("Spacers", GroupOrder = 4)]
    private bool S2 { get => false; set { } }

    [SettingPropertyBool("S3", Order = 18, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas."),
     SettingPropertyGroup("Spacers", GroupOrder = 4)]
    private bool S3 { get => false; set { } }

    [SettingPropertyBool("S4", Order = 19, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas."),
     SettingPropertyGroup("Spacers", GroupOrder = 4)]
    private bool S4 { get => false; set { } }

    [SettingPropertyBool("S5", Order = 20, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas."),
     SettingPropertyGroup("Spacers", GroupOrder = 4)]
    private bool S5 { get => false; set { } }

    [SettingPropertyBool("S6", Order = 21, RequireRestart = false, HintText = "Spacer for dropdown menus. Blame Aragasas."),
     SettingPropertyGroup("Spacers", GroupOrder = 4)]
    private bool S6 { get => false; set { } }

    [SettingPropertyBool("Assign New Formation Captains", Order = 0, RequireRestart = false,
         HintText
             = "Whether or not to freely assign the best heroes/companions as captains, or to only use the captains assigned during deployment. NOTE: Companion Formation must be set to Default (or User-Defined Formation Classes turned off) for this to work properly."),
     SettingPropertyGroup("General", GroupOrder = 0)]
    public bool AssignNewFormationCaptains { get; set; } = DefaultSettings.Instance.AssignNewFormationCaptains;

    [SettingPropertyBool("User-Defined Formation Classes", IsToggle = true, Order = 1, RequireRestart = false,
         HintText = "Whether or not to use the user-defined formation classes below in place of the default dynamic formation classes. (highly recommended)"),
     SettingPropertyGroup("User-Defined Formation Classes", GroupOrder = 1)]
    public bool UserDefinedFormationClasses { get; set; } = DefaultSettings.Instance.UserDefinedFormationClasses;

    FormationClass ISettings.CompanionFormation => CompanionFormationDropdown.SelectedValue?.FormationClass ?? DefaultSettings.Instance.CompanionFormation;

    FormationClass ISettings.Formation1 => FormationDropdown1.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation1;

    FormationClass ISettings.Formation2 => FormationDropdown2.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation2;

    FormationClass ISettings.Formation3 => FormationDropdown3.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation3;

    FormationClass ISettings.Formation4 => FormationDropdown4.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation4;

    FormationClass ISettings.Formation5 => FormationDropdown5.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation5;

    FormationClass ISettings.Formation6 => FormationDropdown6.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation6;

    FormationClass ISettings.Formation7 => FormationDropdown7.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation7;

    FormationClass ISettings.Formation8 => FormationDropdown8.SelectedValue?.FormationClass ?? DefaultSettings.Instance.Formation8;

    InputKey ISettings.OrderKey => OrderKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.OrderKey;

    InputKey ISettings.TierSortKey => TierSortKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.TierSortKey;

    InputKey ISettings.ShieldSortKey => ShieldSortKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.ShieldSortKey;

    InputKey ISettings.SkirmisherSortKey => SkirmisherSortKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.SkirmisherSortKey;

    InputKey ISettings.EqualSortKey => EqualSortKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.EqualSortKey;

    InputKey ISettings.InverseSelectKey => InverseSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.InverseSelectKey;

    InputKey ISettings.AllSelectKey => AllSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.AllSelectKey;

    InputKey ISettings.CavalryMeleeSelectKey => MeleeCavalrySelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.CavalryMeleeSelectKey;

    InputKey ISettings.CavalryRangedSelectKey => RangedCavalrySelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.CavalryRangedSelectKey;

    InputKey ISettings.GroundMeleeSelectKey => GroundMeleeSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.GroundMeleeSelectKey;

    InputKey ISettings.GroundRangedSelectKey => GroundRangedSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.GroundRangedSelectKey;

    InputKey ISettings.MeleeSelectKey => MeleeSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.MeleeSelectKey;

    InputKey ISettings.RangedSelectKey => RangedSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.RangedSelectKey;

    InputKey ISettings.CavalrySelectKey => CavalrySelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.CavalrySelectKey;

    InputKey ISettings.GroundSelectKey => GroundSelectKeyDropdown.SelectedValue?.Key ?? DefaultSettings.Instance.GroundSelectKey;
}

internal class FormationClassSelection
{
    internal readonly FormationClass? FormationClass;
    internal readonly string Name;

    internal FormationClassSelection(FormationClass? formationClass, string name = null)
    {
        FormationClass = formationClass;
        Name = name;
    }

    public override string ToString() => Name ?? (FormationClass is null ? "None" : FormationClass.Value.GetGameTextString());
}

internal class KeySelection
{
    internal readonly InputKey? Key;
    internal KeySelection(InputKey? key) => Key = key;
    public override string ToString() => Key is null or InputKey.Invalid ? "None" : Key.ToString();
}

internal static class DropdownHelper
{
    internal static Dropdown<FormationClassSelection> FormationClassSelection(FormationClass defaultFormationClass,
        IEnumerable<FormationClassSelection> formationClasses)
    {
        Dropdown<FormationClassSelection> dropdown = new(formationClasses, 0);
        dropdown.SelectedIndex = dropdown.FindIndex(s => s.FormationClass == defaultFormationClass);
        return dropdown;
    }

    internal static IEnumerable<FormationClassSelection> EnumerateRegularFormations(bool includeUnset = false)
    {
        if (includeUnset)
            yield return new(FormationClass.Unset, "Default");
        foreach (string name in Enum.GetNames(typeof(FormationClass)))
            if (!name.StartsWith("NumberOf") && Enum.TryParse(name, false, out FormationClass formationClass)
                                             && formationClass < FormationClass.NumberOfRegularFormations)
                yield return new(formationClass);
    }

    internal static Dropdown<KeySelection> KeySelection(InputKey defaultKey, IEnumerable<KeySelection> keys)
    {
        Dropdown<KeySelection> dropdown = new(keys, 0);
        dropdown.SelectedIndex = dropdown.FindIndex(s => s.Key == defaultKey);
        return dropdown;
    }

    private static IEnumerable<int> Range(int from, int to) => Enumerable.Range(from, 1 + to - from);

    internal static IEnumerable<KeySelection> EnumerateKeys()
    {
        yield return new(InputKey.Invalid);
        foreach (int i in Range(16, 27)) // Q to ]
            yield return new((InputKey)i);
        foreach (int i in Range(30, 40)) // A to '
            yield return new((InputKey)i);
        foreach (int i in Range(44, 53)) // Z to /
            yield return new((InputKey)i);
    }

    internal static IEnumerable<KeySelection> EnumerateModifiers()
    {
        yield return new(InputKey.Invalid);
        yield return new(InputKey.LeftControl);
        yield return new(InputKey.LeftShift);
        yield return new(InputKey.LeftAlt);
        yield return new(InputKey.RightControl);
        yield return new(InputKey.RightShift);
        yield return new(InputKey.RightAlt);
    }
}