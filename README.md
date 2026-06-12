# Full Plastic Jacket

A top-down roguelike bullet shooter built in C# using SDL2 via Silk.NET.

## Gameplay

Survive endless waves of enemies in a single arena. Every enemy you kill brings you closer to the next wave — and each wave hits harder than the last. Between waves, choose one of three upgrades to build your character.

The game ends when your HP hits zero.
## Gameplay demo

https://github.com/user-attachments/assets/9ee8ed34-6b69-4b2a-8b43-5086e8fc5813

## Controls

| Input | Action |
|---|---|
| `W A S D` | Move |
| Mouse | Aim |
| Left Click | Shoot |
| `R` | Restart (after game over) |
| Close window | Quit |

## Upgrades

Each round you pick one of three randomly offered upgrades:

- **+Max HP** — increase your health pool and heal immediately
- **+Move Speed** — move faster around the arena
- **+Fire Rate** — shoot more bullets per second
- **+Bullet Damage** — each bullet deals more damage
- **+Bullet Speed** — bullets travel faster across the arena
- **Piercing Shot** — bullets pass through additional enemies

## Wave Scaling

Each wave spawns more enemies with higher HP, movement speed, and contact damage. There is no cap — survive as long as you can.

## Tech Stack

- **.NET 10** / C#
- **Silk.NET.SDL** — SDL2 bindings for window, renderer, and input
- **StbImageSharp** — PNG texture loading

## Building & Running

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run
```

Assets (`arena.png`, `player.png`, `enemy.png`) must be present in the `assets/` folder — they are copied to the build output automatically.

# AI Usage Declaration

## Overview

This project is a top-down roguelike shooter built in C# on .NET 10 using SDL2 via Silk.NET.
The majority of the design decisions, architecture, and core gameplay logic were written by me.
AI assistance was used in a supporting role for specific technical areas described below.

---

## What I wrote

- **Game concept and design** — the roguelike wave-survival loop, upgrade system design, and overall gameplay feel were my own idea.
- **Core entity classes** — `Player.cs`, `Enemy.cs`, and `Bullet.cs` including their fields, stats, and the relationships between them.
- **Upgrade system** — the structure of the `Upgrade` class and the upgrade pool (`Upgrades.cs`), including which upgrades exist and what they do to the player stats.
- **Project setup** — `.csproj` configuration, package references (Silk.NET, StbImageSharp), asset copy rules, and solution structure.
- **SDL bootstrapping** — `SdlContext.cs`, `Program.cs`, and the window/renderer initialisation pipeline.
- **Input enums** — `KeyCodes.cs` and `MouseButton.cs`, mapped to SDL scancodes.
- **Game state design** — deciding on the three states (Playing, BetweenRounds, GameOver) and how transitions between them work.
- **Asset creation** — `arena.png`, `player.png`, and `enemy.png` were generated using ChatGPT image generation.
- **Wave scaling formula** — the enemy count, health, speed, and damage progression per round.
- **File organisation** — moving files into the `src/` and `src/objects/` folder structure.

---

## Where I used AI assistance

I used **Claude Code (Claude Sonnet 4.6)** to help with the following:

- **Tying the classes together inside `Game.cs`** — the main game loop, update/render pipeline, and how all systems connect was co-written with AI assistance. The overall structure was directed by me, with AI helping fill in the SDL-specific rendering calls and the collision detection boilerplate.
- **Bullet physics** — the direction vector calculation, delta-time movement, and piercing logic in `Bullet.cs` were written with AI help.
- **Save system** — `SaveManager.cs` using `System.Text.Json` for high-score persistence was AI-generated from my specification.
- **TextureLoader** — `TextureLoader.cs` for loading PNG files via StbImageSharp into SDL textures was AI-assisted.
- **Error fixing** — compiler errors, nullable warnings, and unsafe pointer handling edge cases were resolved with AI help.

---

## Assets

The three game sprites were generated using ChatGPT image generation tools.
<img width="1024" height="1024" alt="player" src="https://github.com/user-attachments/assets/63945fa4-0c20-4425-8f0c-ab839264150b" />
<img width="1024" height="1024" alt="enemy" src="https://github.com/user-attachments/assets/e67df39c-023c-49f2-9a57-97fe0476fd39" />
<img width="593" height="612" alt="arena" src="https://github.com/user-attachments/assets/c3c83cea-69e2-4df2-bde0-07ab163a338a" />
