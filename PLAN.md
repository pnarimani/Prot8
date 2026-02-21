# Siege Survival CLI Prototype Plan

## Summary
Build a C# CLI city-siege survival game targeting 40 days of endurance, with strict daily simulation order, transparent causality logs, and high failure pressure before day 25.

## Implemented Scope
- Feature-based architecture (`Laws`, `Orders`, `Missions`, `Events`, `Simulation`, `Cli`, `Telemetry`)
- Strict 12-step simulation order
- Zones with contraction, perimeter damage, loss shocks, and overcrowding penalties
- Worker allocation in 5-step increments
- One daily action (law/order/mission), plus evacuation as emergency order
- 12 laws, 7 orders (including voluntary evacuation), 4 missions, 7 triggered events
- Recovery system: sickness-gated timed recovery with clinic + medicine caps
- Loss conditions: Keep breach, unrest revolt threshold, total food+water collapse streak
- JSONL telemetry in `runs/`