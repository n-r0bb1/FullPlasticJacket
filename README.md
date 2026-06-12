# Full Plastic Jacket

A top-down roguelike bullet shooter built in C# using SDL2 via Silk.NET.

## Gameplay

Survive endless waves of enemies in a single arena. Every enemy you kill brings you closer to the next wave — and each wave hits harder than the last. Between waves, choose one of three upgrades to build your character.

The game ends when your HP hits zero.
## Gameplay demo

![Gameplay demo](Demo/FullPlasticJacketDemo.mp4)

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
