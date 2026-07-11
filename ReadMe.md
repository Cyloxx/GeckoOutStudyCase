# Gecko Out — Case Study Clone

A grid-based "snake-out" puzzle game clone (reference: Gecko Out by Dalak Games),
built as a study case with a focus on **sustainable, testable, SOLID architecture**.

> Visuals are intentionally minimal — the deliverable is the code.

## Architecture

Five assemblies with compiler-enforced dependency direction (arrows = "references"):

    UI ───────────┐
                  ├──► Data ──► Core
    Presentation ─┘
    App (composition root) ──► all

- **Core** — pure C# game logic (`noEngineReferences: true`). Grid, gecko bodies,
  move validation, pathfinding, commands, win/lose rules. Fully unit-tested;
  has no idea Unity exists.
- **Data** — level JSON schema, loader and validator. Levels are data, not scenes.
- **Presentation** — renders Core state and translates input into Core intents.
  Never contains game rules.
- **UI** — HUD and panels; subscribes to Core events only.
- **App** — composition root: wires everything, owns the game flow state machine.

### Key decisions
- **Single gameplay scene + data-driven boards** — levels are JSON files;
  adding level 11 means adding one file, not one scene.
- **One event style** — C# events exposed by the Core session; gameplay never
  references UI, enforced by assembly boundaries.
- **Command-based movement** — every head step is a reversible command;
  dragging a gecko backwards is literally `Undo`.
- **Extension points over features** — special mechanics (frozen exits, toll
  gates) are not implemented, but `IExitRule` / tile behaviour strategies mark
  exactly where they would plug in without modifying existing code.

## How to play
_(to be completed: controls, win/lose)_

## Running the tests
Window → General → Test Runner → EditMode → Run All

## Level format
_(to be completed: JSON schema example)_

## Known limitations / trade-offs
_(to be completed)_