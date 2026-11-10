# Fire Overhaul

A complete, comprehensive fire overhaul for The Long Dark.

Gone are the days of simple fires where you can dump on endless fuel and have it burn for hours. Fire Overhaul replaces that mechanic with a complete simulation. Every piece of fuel added on the fire heats up, catches, burns, turns to coals and fades to embers on its own, affecting the fire's temperature and heat in real time from the fuel that is burning. See below for more details.

**Version:** 0.9.0-beta.1. This is a beta, so expect balance changes and the occasional bug. Saves may behave oddly between beta versions.

## The Fire Simulation

## Starting Fires

Starting where it begins, the firestarting process is closest to vanilla. But with one big change.

- Fires can no longer be started with large fuel items, instead the fuel used to start are tinder items and the tinder slot is removed. 
- Tinder items can now be used as fuel, including cloth and lichen.
- Each tinder item comes with it's own fire start chance modifier, most of which are fairly forgiving.
- Accelerant still works as normal.

Once a fire is lit, you need to slowly build it up using the right fuel items. You can't dump a bunch of big fuel onto a freshly lit fire and expect it to roar.

## Fuel

Each fuel item added on the fire is individually tracked and goes through its own stages:

**Cold → Heating → Burning → Coals → Embers → Ash**

- **Cold:** Freshly added fuel takes heat from the fire while it warms up, so piling cold wood on a weak fire can set it back or put it out.  Fuel will only catch once the fire's main core temperature is hot enough.
- **Heating:** Once the fire is hot enough to light the fuel, it still takes time for it to catch and start burning. Usually a couple of in game minutes. Modified by fire heat.
- **Burning:** Once a piece of fuel starts burning, it starts releasing energy according to a range of combustion curves depending on the fuel type. Contributing to a fire's heat. The hotter the fire, the faster it moves along this curve.
- **Coals:** Once a fuel has burned through most of it's energy, it will break down into coals and start burning via slow decay, releasing less heat but lasting longer. Larger fuel items turn into more coals.
- **Embers:** After burning through it's coals, fuel will expel it's last remaining energy as embers. These emit very little useful heat but can last for hours.

All fuel items burn at roughly the same exact time, so adding consecutive fuel items won't increase the duration of the fire, only it's heat. A fire lasts not as the sum of the fuel added, but it's average. See Estimated Fire Duration below for more details.

Below is a table of each fuel item and it's stats:

| Fuel | How it behaves | Time to light (at ignition temp) | Ignition temp | Energy content | Max power output | Charcoal per item |
|---|---|---|---|---|---|---|
| Fir | Hard to light, burns hot and long, leaves a strong coal bed | 5 min | 720°C | 19 MJ | 6.5 kW | 2 |
| Cedar | Easier to light, burns for less time | 3 min | 650°C | 8 MJ | 4.5 kW | 1 |
| Reclaimed Wood | Catches quickly, burns out fast | 1.5 min | 550°C | 4 MJ | 3.5 kW | 0.38 |
| Sticks | Light easily, burn briefly, leave few coals | 2 min | 300°C | 1.25 MJ | 1.45 kW | 0.1 |
| Firelogs | Steady burn that lasts for hours | 3 min | 350°C | 36 MJ | 5.5 kW | 2 |
| Coal | Very hard to light, then burns very hot for a long time as coals | 8 min | 800°C | 20.5 MJ | 6 kW | None |
| Books | Light easily, burns steadily for long | 2 min | 250°C | 6.5 MJ | 3.5 kW | 0.1 |
| Tinder, paper, cash, cattail heads | Light almost instantly, flare and vanish | 0.05–0.5 min | 15–50°C | 0.035–0.1 MJ | 1.5–2 kW | None |
| Cloth, lichen | Quick fire starters with little staying power | 0.2–1 min | 50–80°C | 0.1 MJ | 1.2 kW | None |

- Values are balanced for gameplay, not necessarily reflective of reality.

### Combustion Curves

Each fuel type releases its energy along its own curve while burning, and every curve speeds up or slows down with the fire's core temperature and airflow.

- **Firewood (Fir, Cedar, Reclaimed Wood):** starts slowly while the outside catches, quickly builds to full power, then gradually tapers off as the log burns down before breaking into coals.
- **Sticks:** catch fast and burn at full power for most of their short life, dropping off just before they turn to coals.
- **Firelogs:** build up gently, then hold a long, steady burn before tapering off at the end.
- **Books:** flare up as the pages catch, settle back down, then burn slowly and evenly for a good while.
- **Coal:** slow to get going and spends very little time in flames. Most of its life is spent as coals (duh)
- **Tinder, paper, cash, cattail heads, lichen:** burn hardest the moment they catch, then fade quickly. They leave almost nothing behind.

#### Slow decay (coals)

Once fuel turns to coals, it puts out much less power than it did in flames, except for coal, which stays strong. Coals fade slowly at first, then faster as they near their end. They count far more toward the fire's core temperature than their power suggests, which is why a good coal bed keeps a fire hot and lights new fuel so easily.

#### Embers

Embers are the last of the fuel's energy. They fade steadily until the fuel turns to ash, giving off very little useful heat, but they can last for many hours. 

## Fire states

The fire's overall state is displayed on the HUD when you look at a fire. It depends on how much power its combined fuel is putting out. The flames, light and sound all match the fire state.

| State | Description |
|---|---|
| Raging Flames | Lots of fuel burning at once, very hot |
| Steady Flames | A healthy stable fire |
| Dwindling Flames | Small flames, recently started or waning fire |
| Glowing Coals | No real flames, but ample coals, hot and good for long |
| Embers | The cool long lasting remains of the fire |


## Temperature

- **Core temperature** is how hot the fire itself is and it is determined by the energy output of all combusting fuel. It decides what fuel can catch and how quickly fuel burns. The hotter a fire, the faster the fuel burns, the colder the fire, the slower it burns. This temperature decides all.
- The core temperature of the fire can reach a max of around 1100 degrees celsius on wood alone. Adding coal can increase that max to over 1300 degrees celsius.

- **Heat output** is the radiant output heat (radiant and convective, technically), it's the heat that you feel nearby. The heat that changes the air temperature. It rises and falls more slowly than the core, and it reaches further on a strong fire.

- **Heat Radius:** The heat radius of any given fire has been changed significantly. The output heat is felt at it's hottest right up against the fire and drops off quickly as you walk away. 
If you are using the Dynamic Temperature mod and you are indoors, the room will gradually warm to meet the air temperature at the fire (and any other heat sources present).
 
## Estimated Duration

- Because a hotter fire burns fuel faster and a colder fire burns fuel slower, the duration of any given fire is not fixed. The estimated duration is calculated using an advanced simulation feature that fast forwards the simulation at a given moment to calculate when it will reduce to embers. 
- The displayed time is always a near accurate estimate of how long the fire will last until the embers state. It is updated when things like fuel is added, torches are taken, or when the state changes. Otherwise it counts down like normal.

## Temperature Management

The feed fire panel has an **Airflow** slider, labelled **Damper** on wood stoves. This mechanic controls airflow into the fire. Less air means fuel burns more slowly and puts out less heat, more air means fuel burns quicker and puts out more heat. 

You trade burn time for heat, or heat for burn time. Or somewhere in middle for a bit of both.

How far you can close it depends on the fire:

| Fire | Lowest airflow |
|---|---|
| Wood stove, forge | 25% |
| Fireplace, fire barrel, rim grill | 50% |
| Campfire | 70% |

- Fuel in coals or embers are more affected by airflow than those actively combusting.

On open fires like **campfires**, **fireplaces** or **fire barrels**, the method of managing airflow is effectively shaping the fire, as the fire burns, it changes shape and airflow drifts back to fully open as the flames use up their fuel. Wood stoves and forges stay where you set them. And every fire's airflow starts fully open when you light it again.

## Wind

Currently, wind does not act as increased airflow (a potential future feature), however it still has an effect on campfires.

An exposed campfire in high wind loses a large amount of core temperature and cools faster. If it drops below 200°C, burning fuel goes out. Sheltered campfires and other fire types aren't affected.

## Cooking

Each fire has a cooking slot temperature, which is a fraction of its core temperature and is slower to heat up and cool down. Cooking stays active for as long as the surface is hot enough, even after the flames die.

| Task | Minimum temperature |
|---|---|
| Melting snow | Above 1°C |
| Cooking food | Above 100°C |
| Boiling water | Above 115°C |

This means that you can still melt snow in a container on a fire long after the fire has gone out. But it takes a significantly long amount of time to do so. It is best to melt snow, boil water and cook food on a healthy fire. But leaving snow to melt on the stove for days while you're gone is a good way to get some unpotable water early.

If you're really in a hurry, cooking speeds up as the temperature rises, reaching its normal to fast range at around 260°C and its top speed at 450°C. Hover over a can, pan or pot to see its temperature. For simplicity, it inherits the temperature from the cooking slot. No need to wait for the individual container to heat up too.

## Forge

Like the cooking slots, the forge has it's own temperature separate from the furnace. It needs to reach **1200°C** before you can start forging. It takes a substantial amount of burning fuel to get there, and coal is the best way to reach it. It heats up over a few minutes and holds its heat for a long time. Hover over the forge to see its temperature.

- It is best to use wood to get the furnace up to the max temperature of **1100** degrees celsius, and then periodically add coal to get it past the max up to or past **1200°C**. You cannot do this without coal, nor can you easily sustain it without coal.
- Once it is up to forging temperature, it is best to close the forge's damper to lengthen the duration of the fire as forging usually takes a lot of time. And then re-open the damper, add more fuel and coal to get the forge back up to working temperature and repeat the process.

## Torches and Charcoal
- **Torches:** taking a torch uses a stick from the fire, and needs one that's no more than 10% burned. Add stick, take torch.
- **Charcoal:** depends on what you burned and not on how long the fire lasted. Each fuel has its own yield, and only fuel that burns all the way through counts fully. The total builds up over the life of the fire and is rounded down when it goes out. Larger pieces of fuel yield more charcoal, except for coal which yields none.


## Unburnt Fuel

- Fuel added to a fire that doesn't burn remains unburnt. If a fire goes out, that fuel will remain in the fire and you can relight the fire and that fuel will still be there, waiting to burn once it gets up to temperature. 

- **Remove Unburnt Fuel:** this button in the feed fire panel gives back wood and coal that's less than 7% burned. Unburnt fuel is also returned when you harvest charcoal or break down a campfire. Paper, books, cloth and lichen can't be taken back. 


## Saving and time away

Each fire's fuel, temperatures and airflow are stored in your save. When you return to a scene, the fire catches up on the time you were away, so it may have burned down or gone out.

## Known limitations

- Remove Unburnt Fuel is mouse and keyboard only for now.
- The cooking time shown on a pot assumes the current temperature holds, so it can be misleading on a rapidly cooling fire.
- Portable heaters still use vanilla behaviour.

### Gameplay Balance

The mod is still in beta and gameplay balance is still being tuned. The mod was developed with an intended balance and experience in mind, but in the future settings will be added based on feedback to allow partial tuning of the experience. As well as overall balance tuning. 

In it's current state, you might find it difficult to use on certain difficulties or experiences.

## Mod Incompatibilites

- Any mod that adds new fuel items will not work (at least adding the fuel part).
- Safehouse Customization + compatibility is currently unknown.
- Any mod that affects or reworks fires in any way will obviously be incompatible.

## Requirements

- [MelonLoader](https://melonwiki.xyz/) 0.7.2
- ModSettings
- ComplexLogger

## Installation

Put `FireOverhaul.dll` and the required mods in your game's `Mods` folder.

Other mods that change how fires work are likely to conflict with this one.


