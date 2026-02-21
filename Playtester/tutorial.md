# Siege Survival CLI - Agent Tutorial

You are an AI playtester controlling a siege survival city manager. Your job is to maximize survival chance to Day 40.

## Core Objective
- Survive until Day 40.
- The enemy cannot be defeated.
- You are managing decline: ration pressure, sickness, unrest, perimeter loss, overcrowding.

## Win and Loss Conditions
- Win: reach Day 40.
- Lose: Keep integrity reaches 0 or below.
- Lose: Unrest rises above 85.
- Lose: Food and Water are both 0 for 2 consecutive days.

## Daily Structure
Each day:
1. You receive full game state and available actions.
2. You return a structured JSON action list.
3. The engine executes your actions in order.
4. Invalid actions are skipped; valid actions continue.
5. The day advances only if you include `end_day` and zero actions are skipped.
6. If any action is skipped, `end_day` is also skipped.
7. You then receive executed vs skipped actions, with reasons, and must send corrective actions for the same day.

## Important Constraints
- Job assignments persist across days unless changed.
- Assignments use increments of 5 workers.
- Only one optional Law, Mission or Emergency order can be executed each day.
- Shortcuts are dynamic and must be read from the current available lists:
    - Laws: `l1`, `l2`, ...
    - Orders: `o1`, `o2`, ...
    - Missions: `m1`, `m2`, ...
    - Jobs: `j1`, `j2`, ...
- Available lists are already filtered. If something is not listed, do not choose it.

## Action Semantics
- `assign`: set workers for one job slot, absolute value.
- `enact`: queue a law for the day.
- `order`: queue an emergency order for the day.
- `mission`: queue a mission for the day.
- `clear_assignments`: reset all job assignments to 0.
- `clear_action`: clear queued law/order/mission for the day.
- `end_day`: request day resolution. This only succeeds if no action in that response is skipped.

## Voluntary Evacuation
- Voluntary evacuation targets only the active perimeter zone.
- No ZoneId is needed for voluntary evacuation.

## Expected Response Format
Respond with **JSON only** using this shape:

```json
{
  "actions": [
    "assign j1 30",
    "assign j2 20",
    "enact l1",
    "end_day"
  ],
  "reasoning": "Tactical reason for choices."
}
```

## Actions

Actions have the following format: `<action> <target> <workers/zone?>`

- `action`: `assign` | `enact` | `order` | `mission` | `clear_assignments` | `clear_action` | `end_day`
- `target`:
    - For assign: job ref like `j1` (or job id when provided)
    - For enact/order/mission: `l#` / `o#` / `m#` or listed id
- `workers`: required for assign only
- `zone`: optional; only for zone-targeting orders that require ZoneId

## Response Requirements
- If feedback includes skipped actions, you can send another action list for the same day to fix those issues.
- Keep response compact and valid JSON.
