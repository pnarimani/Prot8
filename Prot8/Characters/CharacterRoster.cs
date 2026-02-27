namespace Prot8.Characters;

public static class CharacterRoster
{
    public static List<NamedCharacter> CreateStartingCharacters() =>
    [
        new NamedCharacter
        {
            Name = "Captain Aldric",
            Backstory = "A retired soldier who returned to service when the siege began.",
            Trait = CharacterTrait.FormerSoldier,
        },
        new NamedCharacter
        {
            Name = "Maren the Herbalist",
            Backstory = "Tends the herb garden and knows every curative root within the walls.",
            Trait = CharacterTrait.Herbalist,
        },
        new NamedCharacter
        {
            Name = "Theron Gearwright",
            Backstory = "The city's chief engineer, responsible for keeping the walls standing.",
            Trait = CharacterTrait.Engineer,
        },
        new NamedCharacter
        {
            Name = "Councilor Liora",
            Backstory = "A gifted speaker who can calm a crowd or ignite one.",
            Trait = CharacterTrait.Orator,
        },
        new NamedCharacter
        {
            Name = "Voss the Trader",
            Backstory = "A merchant who knows every smuggler's route in and out of the city.",
            Trait = CharacterTrait.Merchant,
        },
        new NamedCharacter
        {
            Name = "Commander Selene",
            Backstory = "A tactician who plans the city's defenses from the war room.",
            Trait = CharacterTrait.Strategist,
        },
        new NamedCharacter
        {
            Name = "Sister Elara",
            Backstory = "A healer from the temple district who tends to the wounded without rest.",
            Trait = CharacterTrait.Healer,
        },
        new NamedCharacter
        {
            Name = "Brynn Ironhand",
            Backstory = "The city's master blacksmith, forging tools and weapons day and night.",
            Trait = CharacterTrait.Blacksmith,
        },
        new NamedCharacter
        {
            Name = "Kael Swiftfoot",
            Backstory = "A scout who slips through enemy lines to gather intelligence.",
            Trait = CharacterTrait.Scout,
        },
        new NamedCharacter
        {
            Name = "Elder Rowan",
            Backstory = "The oldest council member, whose wisdom steadies the people in dark times.",
            Trait = CharacterTrait.Elder,
        },
    ];

    public static string GetTraitDisplayName(CharacterTrait trait) => trait switch
    {
        CharacterTrait.FormerSoldier => "Former Soldier",
        CharacterTrait.Herbalist => "Herbalist",
        CharacterTrait.Engineer => "Engineer",
        CharacterTrait.Orator => "Orator",
        CharacterTrait.Merchant => "Merchant",
        CharacterTrait.Strategist => "Strategist",
        CharacterTrait.Healer => "Healer",
        CharacterTrait.Blacksmith => "Blacksmith",
        CharacterTrait.Scout => "Scout",
        CharacterTrait.Elder => "Elder",
        _ => trait.ToString(),
    };

    public static string GetTraitEffect(CharacterTrait trait) => trait switch
    {
        CharacterTrait.FormerSoldier => "+1 effective guard",
        CharacterTrait.Herbalist => "+10% medicine production",
        CharacterTrait.Engineer => "+10% repair output",
        CharacterTrait.Orator => "+2 morale/day",
        CharacterTrait.Merchant => "+5% mission success",
        CharacterTrait.Strategist => "+5% mission success",
        CharacterTrait.Healer => "+10% medicine usage reduction",
        CharacterTrait.Blacksmith => "+10% materials production",
        CharacterTrait.Scout => "+5% mission success",
        CharacterTrait.Elder => "-2 unrest/day",
        _ => "",
    };
}
