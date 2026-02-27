using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Mood;

public static class MoodLines
{
    public static readonly IReadOnlyList<MoodEntry> All = CreateEntries();

    static List<MoodEntry> CreateEntries()
    {
        var entries = new List<MoodEntry>();

        // ──────────────────────────────────────────────
        // PRIORITY 100 — Crisis (Humanity)
        // ──────────────────────────────────────────────

        entries.Add(new MoodEntry(100, s => GameBalance.EnableHumanityScore && s.Flags.Humanity < GameBalance.HumanityLowThreshold,
        [
            "Something has broken in the soul of this city. People do terrible things without flinching.",
            "The cruelty has become routine. No one protests anymore — they are afraid to.",
            "Children hide from the guards. The city has become something unrecognizable.",
            "Mercy is a word no one dares speak. The strong survive. The rest are forgotten.",
        ]));

        // ──────────────────────────────────────────────
        // PRIORITY 100 — Crisis
        // ──────────────────────────────────────────────

        entries.Add(new MoodEntry(100, s => s.FinalAssaultActive,
        [
            "The horns have sounded. This is the final push.",
            "They are throwing everything at us. The walls shake with every volley.",
            "Smoke blots out the sun. The last assault has begun.",
            "Children are being carried to the cellars. Everyone else grips a weapon.",
            "The enemy pours through the breaches. There is nowhere left to fall back.",
        ]));

        entries.Add(new MoodEntry(100, s => s.PlagueRatsActive,
        [
            "The rats are everywhere. People sleep on tables to avoid the floor.",
            "Vermin boil out of the drains. The sick wards are overrun.",
            "They killed a dozen rats in the granary last night. A dozen more replaced them.",
            "Children scream when the rats come. The adults have stopped screaming.",
        ]));

        entries.Add(new MoodEntry(100, s => s.TaintedWellDaysRemaining > 0,
        [
            "The well water runs dark. People drink it anyway — there is nothing else.",
            "Three more fell sick after drinking from the well. No one will say it out loud.",
            "The cistern smells of rot. Boiling does not help.",
            "A mother pours tainted water for her children and prays it will be enough.",
        ]));

        entries.Add(new MoodEntry(100, s => s.ConsecutiveBothFoodWaterZeroDays >= 2,
        [
            "Two days without food or water. People lie still to conserve strength.",
            "The city is dying of thirst and hunger at once. Some have stopped moving.",
            "No food. No water. The silence is worse than the siege engines.",
            "Bodies that might be sleeping line the corridors. No one checks anymore.",
        ]));

        entries.Add(new MoodEntry(100, s => s.CountLostZones() >= 2,
        [
            "Two districts have fallen. The survivors crowd into what remains.",
            "Half the city is rubble. The perimeter shrinks with every dawn.",
            "We have lost so much ground. The keep is all that may be left soon.",
            "Zone after zone falls. The map on the wall has more red than black.",
        ]));

        // ──────────────────────────────────────────────
        // PRIORITY 80 — Severe
        // ──────────────────────────────────────────────

        entries.Add(new MoodEntry(80, s => s.Sickness > 60,
        [
            "The coughing never stops. Even the healthy avoid each other's eyes.",
            "Sickness has taken hold. The clinic turns people away at the door.",
            "Half the workers shiver through their shifts. Productivity is gutted.",
            "The stench of the sick ward reaches the main square now.",
            "Healers work until they collapse. Then they become patients themselves.",
        ]));

        entries.Add(new MoodEntry(80, s => s.Unrest > 65,
        [
            "A guard was found beaten in an alley. Trust is eroding.",
            "Angry voices carry through the walls at night. Something is building.",
            "Graffiti appears on the council chamber: 'Open the gates.'",
            "A mob cornered a supply clerk today. The guards barely intervened in time.",
            "Factions are forming. Loyalty is becoming a scarce resource.",
        ]));

        entries.Add(new MoodEntry(80, s => s.Morale < 20,
        [
            "Whispers of surrender echo through the streets.",
            "People sit in doorways with hollow eyes. Hope is a memory.",
            "A woman tore down the garrison flag. No one stopped her.",
            "Morale has collapsed. Orders are followed, but only barely.",
            "Some pray. Others stare at nothing. The will to endure is fading.",
        ]));

        entries.Add(new MoodEntry(80, s => s.ConsecutiveFoodDeficitDays >= 3 || s.ConsecutiveWaterDeficitDays >= 3,
        [
            "Three days of shortage. The rationing board has nothing left to cut.",
            "People fight over scraps in the distribution line.",
            "The deficit drags on. Desperation becomes the norm.",
            "Children cry from hunger. The adults pretend not to hear.",
        ]));

        entries.Add(new MoodEntry(80, s => s.TotalDeaths >= 15,
        [
            "So many dead. The gravediggers work in shifts now.",
            "The death count mounts. Every family has lost someone.",
            "They stopped reading names at the memorial. There are too many.",
            "The cemetery has expanded twice. There is talk of a third time.",
        ]));

        entries.Add(new MoodEntry(80, s => s.TotalDesertions >= 10,
        [
            "Another group slipped over the wall last night. The garrison thins.",
            "Desertion has become routine. Beds are found empty every morning.",
            "Those who leave take skills and hope with them.",
            "The watch commander no longer reports the nightly disappearances.",
        ]));

        entries.Add(new MoodEntry(80, s => s.SiegeIntensity >= 5,
        [
            "The siege engines fire without pause. Sleep is a luxury no one can afford.",
            "Stone dust fills the air from constant bombardment.",
            "The walls groan under the assault. Cracks widen by the hour.",
            "At this intensity, every dawn is a surprise.",
        ]));

        entries.Add(new MoodEntry(80, s => s.Day >= 30 && s.Morale < 35,
        [
            "A month under siege. The faces that remain are gaunt and hard.",
            "Thirty days of this. The walls hold, but the people inside them waver.",
            "How much longer? The question hangs unspoken over every meal.",
            "We have endured so long. But endurance has a limit.",
        ]));

        // ──────────────────────────────────────────────
        // PRIORITY 60 — Law-specific reactions
        // ──────────────────────────────────────────────

        entries.Add(new MoodEntry(60, s => s.ActiveLawIds.Contains("public_executions"),
        [
            "The gallows stand in the square. People walk past with their eyes down.",
            "They hanged a man at dawn. The crowd watched in silence.",
            "Fear keeps the peace now. But fear breeds hatred.",
            "Executions maintain order. At what cost to our humanity?",
            "A child asked why the man was hanging. No one could answer.",
        ]));

        entries.Add(new MoodEntry(60, s => s.ActiveLawIds.Contains("martial_law"),
        [
            "Armed patrols march through every district. Dissent is silent now.",
            "Martial law turns neighbors into suspects.",
            "The curfew bells ring and the streets empty. This is order, of a kind.",
            "Guards have more power than the council. That should worry someone.",
        ]));

        entries.Add(new MoodEntry(60, s => s.ActiveLawIds.Contains("burn_the_dead"),
        [
            "The pyres burn day and night. The smoke carries an awful sweetness.",
            "They burn the dead now. Some say prayers. Others just watch.",
            "No graves, no markers. Just ash on the wind.",
            "The burning is practical. That does not make it easier to bear.",
        ]));

        entries.Add(new MoodEntry(60, s => s.ActiveLawIds.Contains("medical_triage"),
        [
            "The healers choose who lives and who is left to fate. God's work, they call it.",
            "Triage means some are turned away. Their families do not forget.",
            "Medicine goes to those who can still work. The rest wait and hope.",
            "A healer wept today. She had to walk past a dying man she could have saved.",
        ]));

        entries.Add(new MoodEntry(60, s => s.ActiveLawIds.Contains("conscript_elderly"),
        [
            "Old hands grip tools meant for younger workers. They do not complain.",
            "The elderly were conscripted. They serve without protest, but their eyes say enough.",
            "Grandparents haul water and stack rubble. This is what survival demands.",
            "An old man collapsed at his station. They carried him aside and sent the next.",
        ]));

        entries.Add(new MoodEntry(60, s => s.ActiveLawIds.Contains("strict_rations"),
        [
            "Half portions again. Stomachs growl through the council meetings.",
            "Strict rations keep the stores from emptying. They also keep bellies hollow.",
            "People count every grain. Generosity is a luxury they cannot afford.",
            "The ration line is quieter now. People save their energy.",
        ]));

        entries.Add(new MoodEntry(60, s => s.ActiveLawIds.Contains("food_confiscation"),
        [
            "Guards search homes for hidden food. Trust dies in the doorway.",
            "Confiscation fills the common stores. It empties private ones.",
            "A family was caught hoarding a loaf. They were made an example.",
            "What was mine is now ours, by decree. Resentment simmers.",
        ]));

        entries.Add(new MoodEntry(60, s => s.ActiveLawIds.Contains("faith_processions"),
        [
            "The processions wind through the streets. Some kneel, others watch warily.",
            "Faith gives comfort. The processions give structure to despair.",
            "Incense and chanting fill the morning. For a moment, the siege feels distant.",
            "Not everyone believes. But no one dares interrupt the procession.",
        ]));

        entries.Add(new MoodEntry(60, s => s.ActiveLawIds.Contains("curfew"),
        [
            "The curfew bell rings. Streets empty like water draining from a basin.",
            "After dark, only patrols move. The city sleeps under watch.",
            "Curfew keeps order in the night. It also keeps fear close.",
            "A man was caught past curfew. He said he was looking for his daughter.",
        ]));

        entries.Add(new MoodEntry(60, s => s.ActiveLawIds.Contains("extended_shifts"),
        [
            "Twelve-hour shifts leave workers hollow-eyed and stumbling.",
            "Extended shifts squeeze more from exhausted hands.",
            "They work until they drop. Then they sleep where they fall.",
            "Productivity rises. So does the count of injuries.",
        ]));

        entries.Add(new MoodEntry(60, s => s.ActiveLawIds.Contains("water_rationing"),
        [
            "The water tastes wrong. Everyone knows it is diluted. No one says it.",
            "Diluted water stretches the supply. It also stretches patience.",
            "People drink more to feel less thirsty. The irony is not lost on them.",
        ]));

        entries.Add(new MoodEntry(60, s => s.ActiveLawIds.Contains("abandon_outer_ring"),
        [
            "The outer ring is silent now. Only the wind moves through empty homes.",
            "They abandoned the outer district. Refugees crowd the inner wards.",
            "Giving up ground to save lives. The math is sound. The grief is not.",
            "Families carry what they can from the outer ring. Most carry very little.",
        ]));

        entries.Add(new MoodEntry(60, s => s.ActiveLawIds.Contains("mandatory_guard_service"),
        [
            "Every able body takes a turn on the wall. Workers become part-time sentries.",
            "Mandatory guard duty drains the workforce but fills the watchtowers.",
            "Farmers stand watch with spears they barely know how to hold.",
        ]));

        // ──────────────────────────────────────────────
        // PRIORITY 65 — Humanity bleak
        // ──────────────────────────────────────────────

        entries.Add(new MoodEntry(65, s => GameBalance.EnableHumanityScore && s.Flags.Humanity < GameBalance.HumanityBleakThreshold,
        [
            "The cost of survival weighs on every soul. Were the sacrifices worth it?",
            "People avoid each other's eyes. Shame has settled over the city like fog.",
            "The things we have done to survive... history will judge us harshly.",
        ]));

        // ──────────────────────────────────────────────
        // PRIORITY 55 — Humanity heroic
        // ──────────────────────────────────────────────

        entries.Add(new MoodEntry(55, s => GameBalance.EnableHumanityScore && s.Flags.Humanity >= GameBalance.HumanityHeroicThreshold,
        [
            "Against all odds, the city has kept its humanity. People share what little they have.",
            "Compassion endures within these walls. The siege has not broken their spirit.",
            "The city stands not just as a fortress, but as a beacon of mercy.",
            "Stories of kindness travel faster than news of the siege. Hope is a weapon too.",
        ]));

        // ──────────────────────────────────────────────
        // PRIORITY 60-65 — Named Character reactions
        // ──────────────────────────────────────────────

        entries.Add(new MoodEntry(65, s => GameBalance.EnableNamedCharacters && s.LivingCharacters().Count() <= 3,
        [
            "The council has been shattered. Only a handful of leaders remain.",
            "So many names crossed off the roster. The city has lost its guiding voices.",
            "Three or fewer council members remain. The burden on each is immense.",
        ]));

        entries.Add(new MoodEntry(62, s => GameBalance.EnableNamedCharacters && s.NamedCharacters.Any(c => !c.IsAlive),
        [
            "The council mourns its fallen. Their chairs sit empty at the table.",
            "A name has been struck from the roster. The loss echoes through the halls.",
            "They held a quiet ceremony for the fallen leaders. The city grieves.",
        ]));

        entries.Add(new MoodEntry(55, s => GameBalance.EnableNamedCharacters && s.LivingCharacters().Count() == s.NamedCharacters.Count,
        [
            "The full council stands. Ten voices guide the city through the siege.",
            "All council members live. Their unity gives the people strength.",
            "The council meets each dawn, intact. A small miracle in dark times.",
        ]));

        // ──────────────────────────────────────────────
        // PRIORITY 50 — Moderate
        // ──────────────────────────────────────────────

        entries.Add(new MoodEntry(50, s => s.Sickness is >= 40 and <= 60,
        [
            "The clinic is overcrowded. Families leave offerings at the temple.",
            "Sickness spreads through the lower wards. Healthy workers keep their distance.",
            "A persistent cough runs through the barracks. Healers stretch thin.",
            "The sick outnumber the beds. Some recover in hallways.",
        ]));

        entries.Add(new MoodEntry(50, s => s.Unrest is >= 40 and <= 65,
        [
            "Arguments break out in the food lines. Tension hangs in the air.",
            "Voices rise at the distribution points. The guards watch carefully.",
            "Complaints are louder now. Not yet threats, but close.",
            "Unrest simmers. It has not boiled over yet.",
        ]));

        entries.Add(new MoodEntry(50, s => s.Morale is >= 20 and < 40,
        [
            "People do their work. They do not talk about tomorrow.",
            "Morale is low but not broken. Duty still means something.",
            "Songs have stopped in the evenings. Only silence fills the gaps.",
            "The city endures, but joy is a distant memory.",
        ]));

        entries.Add(new MoodEntry(50, s => s.ActiveMissions.Any(m => m.MissionName == "Forage Beyond Walls"),
        [
            "A foraging party ventured beyond the walls. Everyone waits for their return.",
            "They went out for food. The gate closed behind them like a jaw.",
            "Foragers risk everything. The city prays they come back with something.",
            "Beyond the walls, every shadow could be the enemy. Or salvation.",
        ]));

        entries.Add(new MoodEntry(50, s => s.ActiveMissions.Any(m => m.MissionName == "Night Raid"),
        [
            "A raiding party slipped out at midnight. Dawn will tell the story.",
            "Night raiders carry the city's hope on their backs.",
            "They attack under cover of darkness. Bold, desperate, necessary.",
            "The night raid is underway. Some light candles for the fighters.",
        ]));

        entries.Add(new MoodEntry(50, s => s.ActiveMissions.Any(m => m.MissionName == "Sabotage Enemy Supplies"),
        [
            "Saboteurs have gone behind enemy lines. If they are caught, there will be no rescue.",
            "A small team works to destroy the enemy's supplies. The stakes could not be higher.",
            "Sabotage is a gamble. The payoff could change the siege.",
        ]));

        entries.Add(new MoodEntry(50, s => s.Resources[ResourceKind.Food] <= 10 && s.Resources[ResourceKind.Food] > 0,
        [
            "Food stores are nearly bare. People eye the last sacks of grain.",
            "A few days of food remain. Rationing becomes arithmetic.",
            "The granary echoes when you walk through it. Not a good sign.",
        ]));

        entries.Add(new MoodEntry(50, s => s.Resources[ResourceKind.Water] <= 10 && s.Resources[ResourceKind.Water] > 0,
        [
            "Water is scarce. People measure it by the cup now.",
            "The cisterns are low. Every drop is accounted for.",
            "Thirst makes people desperate faster than hunger.",
        ]));

        entries.Add(new MoodEntry(50, s => s.Resources[ResourceKind.Medicine] == 0,
        [
            "The medicine stores are empty. Healers work with boiled rags and prayer.",
            "No medicine left. The sick face their illness alone.",
            "Without medicine, every wound is a death sentence waiting.",
        ]));

        entries.Add(new MoodEntry(50, s => s.Resources[ResourceKind.Fuel] <= 5 && s.Resources[ResourceKind.Fuel] > 0,
        [
            "Fuel is running low. Cold nights grow colder.",
            "They burn furniture now. Books went last week.",
            "The forges dim as fuel dwindles. Repairs slow to a crawl.",
        ]));

        entries.Add(new MoodEntry(50, s => s.ZoneLossOccurred,
        [
            "A district has fallen. The survivors mourn what they left behind.",
            "The loss of a zone weighs on everyone. The city feels smaller.",
            "They lost a whole district. The map is redrawn in retreat.",
        ]));

        // ──────────────────────────────────────────────
        // PRIORITY 30 — Ambient
        // ──────────────────────────────────────────────

        entries.Add(new MoodEntry(30, s => s.Day <= 5,
        [
            "The siege is young. People still believe help is coming.",
            "Early days. The walls are strong and the stores are full enough.",
            "Day by day, the reality sets in. This will not end quickly.",
            "The first days pass in a blur of barricades and rationing.",
            "People organize with nervous energy. The siege has only begun.",
        ]));

        entries.Add(new MoodEntry(30, s => s.Day is > 5 and <= 15 && s.Morale >= 35 && s.Unrest <= 50,
        [
            "The city has settled into a rhythm. Grim, but functional.",
            "Routines form around scarcity. People adapt, as people do.",
            "The walls hold. The people hold. For now, that is enough.",
            "A fragile stability. No one trusts it to last.",
        ]));

        entries.Add(new MoodEntry(30, s => s.Day > 15 && s.Morale >= 35 && s.Unrest <= 50,
        [
            "Weeks into the siege. Survival has become a profession.",
            "The city endures. Not thriving, but not breaking either.",
            "Every day they hold is a small victory against the odds.",
            "The middle stretch. Too late for optimism, too early for despair.",
        ]));

        entries.Add(new MoodEntry(30, s => s.Morale >= 50,
        [
            "People work in grim silence. They endure.",
            "Morale holds steady. There is steel in these people yet.",
            "Someone sang an old song in the square today. Others joined in.",
            "Spirits are reasonable. The siege has not broken them.",
            "A child laughed in the courtyard. Adults paused to listen.",
        ]));

        entries.Add(new MoodEntry(30, s => s.Morale >= 65,
        [
            "Despite everything, there is defiance in the air.",
            "Morale is high. The city fights with purpose.",
            "They share what little they have. Solidarity keeps them warm.",
            "Pride holds this city together as much as the walls do.",
        ]));

        entries.Add(new MoodEntry(30, s => s.Population.HealthyWorkers <= 15,
        [
            "So few workers remain. Every pair of hands is precious.",
            "The workforce has dwindled. Each person covers two roles.",
            "Not enough hands for all the work. Priorities sharpen daily.",
        ]));

        // ──────────────────────────────────────────────
        // PRIORITY 10 — Fallback (always true)
        // ──────────────────────────────────────────────

        entries.Add(new MoodEntry(10, _ => true,
        [
            "Another dawn behind the walls. The city holds.",
            "The siege continues. Life goes on, as it must.",
            "Smoke rises from cookfires. The city stirs for another day.",
            "People move through the streets with practiced urgency.",
            "The bells mark another morning. The walls still stand.",
            "Somewhere, a hammer rings against stone. Repairs never end.",
            "The city breathes. Shallow, careful breaths — but it breathes.",
            "Grey skies above grey walls. The siege wears on.",
            "Rations distributed. Shifts assigned. Another day begins.",
            "The wind carries the sound of distant drums. Always distant, never gone.",
        ]));

        return entries;
    }
}
