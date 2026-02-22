### Mission

Implement a complete playable prototype of a siege survival city manager where the player must endure **40 days**. 
The enemy cannot be defeated; the city cannot be stabilized. The core experience is **managing decline under dual pressure**:

* **External siege pressure** causes **zone contraction** (outer districts fall/evacuate, perimeter shrinks).
* **Internal collapse** (morale/unrest/sickness) escalates, worsened by **overcrowding** when zones contract.

Most runs should fail before **Day 25**. Survival to Day 40 should be rare and feel costly.

### Development notes
* Make variables easy to tune
* We are making the game in the CLI. Receive inputs with CLI, print the game state to the CLI.
* Create the proper separation between input reading, game simulation and outputting the game state to the CLI.
* Architecture the code in such a way that it's easy to explain to the player WHY something is happening.
* Make sure the player knows all the important game state, all the available actions and the cost and consequences of those actions.
* Write simple code. Avoid heavy abstractions. 
* If you have a solution to a problem, ALWAYS ASK YOURSELF: Is there a more elegant way to solve this problem?
* Use constants in each class for configuration numbers and variables. DO NOT read from a json or external config files.
* Folder structure should be on feature to feature basis. Avoid folder names like "Models". Create folders like "Laws", "Missions", "Simuation", etc.


### EMOTIONAL TARGET

The player must feel:

* Constant pressure
* Moral compromise
* Shrinking space
* No clean solution
* Survival equals sacrifice

Success must feel like endurance, not triumph.

### Ask Questions

If something is not clear, or something feels contradictory, ASK QUESTIONS.

Do not move forward until you are confident that you understand both INTENT and REQUIREMENTS of every feature.

---

# GLOBAL STARTING STATE (Day 1)

* Population: `starting_population` (default: 120)
  * `healthy_workers` Healthy Workers (default: 85)
  * `guards` Guards (default: 10)
  * `sick_workers` Sick (default: 15)
  * `elderly` Elderly (consume, don’t work) (default: 10)
* Food: `starting_food` (default: 320)
* Water: `starting_water` (deafult: 360)
* Fuel: `starting_fuel` (default: 240)
* Medicine: `starting_meds` (default: 40)
* Materials: `starting_materials` (default: 120)
* Morale: `starting_morale` (default: 55 out of 100)
* Unrest: `starting_unrest` (default: 25 out of 100)
* Sickness: `starting_sickness` (default: 20 out of 100)
* Siege Intensity: 1

City is already unstable. If the player does nothing smart, collapse begins by Day 6–8.

Food and Water must be critical within first 6–8 days without intervention.

**Important**: In this document, when we mention "resources", we mean any of the variables above.

---

# Core Loop & Simulation Order (MUST MATCH)

Implement daily cycle with these phases and exact resolution order:

1. Apply **Law** passive modifiers
2. Apply **Emergency Order** effects (1-day)
3. Calculate **production**
4. Apply **consumption**
5. Apply **deficit penalties**
6. Apply **overcrowding penalties**
7. Apply **sickness progression**
8. Apply **unrest progression**
9. Apply **siege damage**
10. Apply **repairs**
11. Resolve **triggered events**
12. Check **loss conditions**

Do not reorder steps.

---

# City Model

### Zones (5, ordered)

1. Outer Farms
2. Outer Residential
3. Artisan Quarter
4. Inner District
5. Keep

Each zone has:

* Integrity (0–100)
* Capacity
* Population currently housed
* Production modifiers / on-loss effects
* “Active perimeter” = the outermost non-lost zone

Define a variable for each zone's starting parameter.

**Zone Integrity is the defensive line.** There is no separate wall HP. Siege always targets the **active perimeter zone**.


### Overcrowding rule (stacking)

For every `overcrowding_threshold` over capacity in a zone, there can be consequences to other resources. 

For example (DO NOT TAKE THE CONCRETE NUMBERS, DEFINE VARIABLES):
* +2 Unrest/day
* +2 Sickness/day
* +5% Food consumption

Apply after deficits, before sickness/unrest progression.


### Zone loss

If a zone reaches Integrity ≤ 0 OR is voluntarily evacuated:

* It becomes Lost permanently
* All its population is forced inward to next surviving zone(s)
* Apply loss shock (see below)
* Active perimeter moves inward
* Production modifiers update

**Loss shock (natural fall)**
On natural fall (Integrity ≤ 0), there will be penalities to player's resources (unrest, sicness, morale, etc). 

**Controlled evacuation shock**
On voluntary evacuation, there will be separate penalities to player's resources.

---


# Evacuation Rules (Player Agency)

Evacuation exists and is irreversible, intended as “trade land for time.”

### Eligibility (no free out-of-order contraction)

A zone can be evacuated only if:

* All outer zones are already Lost
  OR
* That zone Integrity < `evac_integrity_threshold`
  OR
* Siege Intensity ≥ `evac_siege_threshold`

Keep cannot be evacuated.


### Zone-specific additional penalties

Evacuating a zone can harm production or consumption of resources. The penalties can be one time or applied over a certain duration of days.
It can also increase costs.

### Benefit 

Implement perimeter scaling:

Daily Siege Damage = (`perimiter_scaling` + Siege Intensity) × `PerimeterFactor`
PerimeterFactor based on active perimeter:

* Outer Farms: 1.0
* Outer Residential: 0.9
* Artisan Quarter: 0.8
* Inner District: 0.7
* Keep: 0.6

This is the primary mechanical benefit of early evacuation.

---

# Job Slots — Allocation in increments of 5

Implement allocation with 5-worker stepper per job.

If the number of workers is not divisible by 5, the remainder will be idle (consume, get sick, but don't work)

**Important**: There can be modifiers to the productions based on the state of zones, current status (unrest, sickness, morale) or other factors such as laws and missions. Make sure to implement this part in a flexible way.

Each production might consume resources to produce resources. Make sure to define variables for both cosumption and production.

1. Food Production
2. Water Drawing
3. Materials Crafting
4. Repairs (active perimeter only)
5. Clinic Staff
6. Fuel Scavenging

## Guards

Guards are always on duty. They cannot be reassigned. Workers can only be turned into guards using player actions but guards will never be turned into workers.

# Laws

Implement each law as their own class, with clear effects and prerequisites. Enact limit: 1 law every 3 days. Laws are irreversible.

Players are NOT forced to enact a law.

## Law structure

A law can:
* Consume resources one time or over time
* Produce resources one time or over time

Some laws might not be available to the player until a certain criteria is met.
The criterias can be:
* A threshold for resources (for example, `water` < 40)
* A threshold for current day (for example, only available when `current_day` > 4)

Make sure to explain all the requirements and the consequences of the laws to the player.

Starting Law List:

1. Strict Rations: -25% Food consumption; -10 Morale; +5 Unrest/day. Available Day 1.
2. Diluted Water: -20% Water consumption; +5 Sickness/day; -5 Morale. Requires Water deficit.
3. Extended Shifts: +25% all production; +8 Sickness/day; -15 Morale. Day 5+.
4. Mandatory Guard Service: convert 10 workers to Guards permanently; -15 Food/day (lost labor); -10 Morale. Requires Unrest > 40.
5. Emergency Shelters: +30 Capacity in Inner District; +10 Sickness/day; +10 Unrest. Requires first zone loss.
6. Public Executions: -25 Unrest instantly; -20 Morale; 5 random deaths. Requires Unrest > 60.
7. Faith Processions: +15 Morale; -10 Materials; +5 Unrest. Requires Morale < 40.
8. Food Confiscation: +100 Food instantly; +20 Unrest; -20 Morale. Requires Food < 100.
9. Medical Triage: -50% Medicine usage; +5 deaths/day among Sick. Requires Medicine < 20.
10. Curfew: -10 Unrest/day; -20% production. Requires Unrest > 50.
11. Abandon the Outer Ring: immediately lose Outer Farms; reduce daily siege damage by 20% (stack with perimeter factor); +15 Unrest. Requires Outer Farms Integrity < 40.
12. Martial Law: Unrest cannot exceed 60; Morale capped at 40. Requires Unrest > 75.


# Emergency Orders

Same as laws, but they have 1-day effect.

Implement:

1. Divert Supplies to Repairs: +50% repair output today; -30 Food; -20 Water
2. Soup Kitchens: -15 Unrest today; -40 Food
3. Emergency Water Ration: -50% Water consumption today; +10 Sickness
4. Crackdown Patrols: -20 Unrest today; 2 deaths; -10 Morale
5. Quarantine District: -10 Sickness spread today; -50% production in selected zone today
6. Inspire the People: +15 Morale today; -15 Materials

Orders cannot stack; only one per day.

---

# Missions

A mission is where the player pays some cost (usually workers) + some duration of time (days) to gain resources.
A mission can have multiple outcomes selected by chance.
Do NOT tell the player the exact odds. Use qualitiative language.

Impelement each mission as its own class in the code.

Impelement these missions as a starting point:

1. Forage Beyond Walls (5 days)
   Outcomes:

* +120 Food (60%)
* +80 Food (25%)
* Ambushed: 5 deaths (15%)
  If Siege Intensity ≥ 4 → Ambushed chance doubles (reduce other outcomes proportionally).

2. Night Raid on Siege Camp (1 day)

* Reduce Siege Intensity by 10 for 3 days (40%)
* Reduce Siege Intensity by 5 for 3 days (40%)
* Captured: 8 deaths + +15 Unrest (20%)

3. Search Abandoned Homes (2 days)

* +60 Materials (50%)
* +40 Medicine (30%)
* Plague exposure: +15 Sickness (20%)

4. Negotiate with Black Marketeers (4 days)

* +100 Water (50%)
* +80 Food (30%)
* Scandal: +20 Unrest (20%)


# Events (Trigger Rules)

Events are threshold-driven. Randomness allowed only where defined.

Implement each event as its own class.

Implement at least these events (exact triggers/effects):

1. Hunger Riot
   Trigger: Food deficit for 2 consecutive days AND Unrest > 50
   Effect: -80 Food; 5 deaths; +15 Unrest

2. Fever Outbreak
   Trigger: Sickness > 60
   Effect: 10 deaths; +10 Unrest

3. Desertion Wave
   Trigger: Morale < 30
   Effect: -10 Workers (remove from Healthy pool)

4. Wall Breach Attempt
   Trigger: Active perimeter zone Integrity < 30
   Effect: Immediate -15 Integrity unless Guards assigned ≥ 15 that day (then negate)

5. Fire in Artisan Quarter
   Trigger: Siege Intensity ≥ 4 AND random 10% each day
   Effect: -50 Materials; -10 Integrity to Artisan Quarter (if already lost, ignore)

6. Council Revolt
   Trigger: Unrest > 85
   Effect: Immediate Game Over

7. Total Collapse
   Trigger: Food = 0 AND Water = 0 for 2 consecutive days
   Effect: Immediate Game Over

Event resolution occurs at step 11 of daily order.

---

# Siege System

* Siege Intensity starts at 1 (may be adjusted by profile)
* Intensity can increase dynamically based on the condition of the player. (Make this part easy to adjust)
* Missions can affect the siege intensity.
* Caps at 6

# Loss Conditions (only these)

1. Keep Integrity ≤ 0 → Breach (Game Over)
2. Unrest > `revolt_threshold` → Revolt (Game Over)
3. Food and Water both 0 for `food_water_loss_threshold` consecutive days → Total Collapse (Game Over)

No other hidden fail states.


# Telemetry (Required)

Log per run to a simple file 

* All the actions of the player
* Full game state on each day
* Cause of loss
* Day of loss
* Day of first deficit (food/water)
* Day of first zone lost
* First law enacted (name + day)
* Total deaths, total desertions
* Unrest/Morale/Sickness at end


