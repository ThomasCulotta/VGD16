# VGD16 - Solar Smuggler

### Team: Space Fence
- **Project Lead & Programming**
  - Thomas Culotta
- **Level Design**
  - Derin Acar
  - Tyler Bellue
- **Sound Design**
  - Patrick Blanchard
- **UI**
  - Brian Chiang
  - Steven Truong

===

## Overview
You are an interstellar smuggler trying to build his reputation within the Galactic Underground. In your adventures, you'll encounter pirates wanting to steal your cargo and leave you for dead and Federation ships that will try to arrest you and confiscate your haul. Navigate between planets, hide in nebulas and asteroid fields, and trade at outposts and space stations for new abilities on your way through the star system.

===

## Features

### Art
#### Core
1. Player Ship
2. Enemy Ship
3. Planet
4. Space Station/Outpost/Base whatevs
#### Extra
1. Space cubemap
2. Asteroid
3. Enemy Ship variations
4. Planet Variations

---

### Player
#### Core
1. Grid Movement
  - (Prototype) WASD
  - Move to click
  - (Optional) Navigate around obstacles
2. Cargo System
  - Decrease/Increase loot within limits
  - Decrease/Increase speed accordingly
3. Damage System
  - Decrease/Increase within limits
  - (Optional) Low health lowers cargo capacity
4. Win Condition
  - Trigger at destination planet
  - Loads win scene
  - Calculates score
5. Stealth System
  - Trigger volume around asteroid fields and nebulas
  - Trigger volume in planets' shadow
#### Extra
1. Ability System
2. Trading System

---

### Enemies
#### Core
1. Line of Sight
  - Raycast to player when within certain distance
  - Change to battle state on raycast hit
2. Shoot at Player
  - Probability to hit fixed ammount
  - Show projectiles from enemy to player
  - Damage variation
3. Pursue Player
  - Move to shooting distance if within sight range
  - Move to last known location if not within sight range
#### Extra
1. Patrol area
2. Bribing
3. Abilities
4. Patrol between planets

---

### Environment
#### Core
1. Sun
  - Has a null object for each planet, asteroid field, and nebula that rotates for orbits
2. Planets
  - Attached to sun null
  - Also rotate
  - Inside a box collider
  - (Optional) Trigger volume in planet's shadow
3. Asteroid fields
  - Have trigger volume for stealth
#### Extra
1. Nebulas
2. Moons

---

### UI
#### Core
1. Health Indicator
2. Cargo Indicator
3. Destination Pointer
4. Movement Grid
#### Extra
1. Ability List
2. Stealth/Spotted Indications
3. Trading UI

---

### Sound
#### Core
1. Ambient Music
2. Battle Music
3. Win/Lose Clip
4. Damage FX
5. Turn End
#### Extra
1. Ship Movement FX
2. Ability FX
3. Nebula Static/Interference
4. Enemy Comms
