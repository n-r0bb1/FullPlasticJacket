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
- **Camera system** — `Camera.cs` (world-to-screen coordinate transform, player follow) was AI-assisted.
- **Save system** — `SaveManager.cs` using `System.Text.Json` for high-score persistence was AI-generated from my specification.
- **TextureLoader** — `TextureLoader.cs` for loading PNG files via StbImageSharp into SDL textures was AI-assisted.
- **Error fixing** — compiler errors, nullable warnings, and unsafe pointer handling edge cases were resolved with AI help.
- **HUD rendering** — the health bar, round indicator, score display, and pixel-font renderer inside `Game.cs` were AI-assisted.

---

## Assets

The three game sprites were generated using ChatGPT image generation tools.
<img width="1024" height="1024" alt="player" src="https://github.com/user-attachments/assets/63945fa4-0c20-4425-8f0c-ab839264150b" />
<img width="1024" height="1024" alt="enemy" src="https://github.com/user-attachments/assets/e67df39c-023c-49f2-9a57-97fe0476fd39" />
<img width="593" height="612" alt="arena" src="https://github.com/user-attachments/assets/c3c83cea-69e2-4df2-bde0-07ab163a338a" />
