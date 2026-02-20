# Laskards - Bachelor’s Thesis Project

Laskards is a card-based strategy game where chess meets boss fights.
This repository contains the full **Scripts** folder of the project, including all gameplay logic, tools and systems developed.

The project was created as part of our Bachelor’s thesis and is released under the **MIT License**.

---

## About the Game

You play against different bosses.
Each boss has its own deck and playstyle.

The core idea:
Everything is driven by cards.

Units can be modified mid-game.
Values such as gained energy or card draw can change dynamically.
Strategy comes from combining positioning, resource- and deck management.

---

## Core Gameplay

### Duels

* When a unit reaches the last row, it deals damage and dies.
* Whoever reaches 0 HP first loses.

---

### Deck System

You play with two decks:

**Main Deck**
Contains action cards and stronger units.

**Raven Deck**
Contains 8 ravens.
Ravens cost 0 energy and help stabilize the game when energy is low.

---

## Repository Structure

This repository currently contains:

```
Scripts/
```

The folder includes:

* Core gameplay logic
* Card and unit systems
* Boss behavior logic
* Tools and utilities
* Modular reusable systems

No assets or scenes are included in this repository.

---

## Modular Systems Included

The Scripts folder contains reusable systems that can be used independently in other projects:

* Custom tweening library
* Async scene loading system
* Tooltip system
* Modular gameplay architecture
* Utility tools and helpers

All systems were built to be clean, modular and adaptable to other Unity projects.

---

## Context

Laskards was developed over four months as part of our Bachelor’s thesis.

Created by:

* **Kai Neuß** – Game Design
* **Jonathan Alber** – Programming

Together we make up **Regenschau Games**.

---

## Play the Game

You can check out Laskards on our itch.io page:

[https://jonathan-alber.itch.io/laskards](https://jonathan-alber.itch.io/laskards)

---

## License

This project is licensed under the MIT License.

You are free to use, modify and distribute the code, provided that the original license is included.

---

If you use parts of this project in your own work, attribution is appreciated but not required.

Thank you for checking out Laskards.
