# AGENTS.md - Prot8 Development Guide

This file provides guidelines for agentic coding agents working on the Prot8 project.

## Project Overview

Prot8 is a CLI-based siege survival city manager game built with C# and .NET 10. The player must survive 40 days while managing declining resources, morale, and zone integrity under siege pressure. The project includes a Playtester component that uses AI agents to playtest the game via LM Studio.

## Code Style Guidelines

### General Principles

- Write simple code. Avoid heavy abstractions.
- Always ask: "Is there a more elegant way to solve this problem?"
- Use constants in each class for configuration numbers. DO NOT read from JSON or external config files.
- Create folders by feature (Laws, Missions, Simulation, Events, etc.). Avoid folder names like "Models".
- 
### Telemetry Requirements

- Log all player actions per run
- Log full game state each day
- Track: cause of loss, day of loss, first deficit days, first zone lost, first law enacted, total deaths/desertions, end-game stats

### Architecture Guidelines

- Make it easy to explain to the player WHY something happened
- Player must know: all important game state, all available actions, costs and consequences