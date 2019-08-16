# BetterBalance
\*NEW\*: v1.0.2 - Implemented disabled OneShotProtection for cursePenalty(LunarDagger/Potion)

BetterBalance is a mod designed to "rebalance" the damage values within the game due to a percieved imbalance of damage scaling. I have personally come to the conclusion that items such as shaped glass are not fitting with the scaling of other items, as exponential damage is impossible to balance correctly; it is limitless and stacks without penalty. Many of you will protest, but if you want buffed stickies, this mod also requires linear scaling shaped glass in order to maintain balance. This is my own implementation of "balance", and comes preconfigured with values which I find difficult, yet rewarding on monsoon difficulty with a group of 4 players. The balancing philosophy of this mod has been to increase the distribution of damage sources accross the board in order to "make up" for the lack of shaped glass damage scaling; to remove the "requirement" of taking shaped glass in order to scale damage and to provide alternative effective damage sources that feel rewarding when stacked.

Sticky printers are a thing again, but not worthy of "I'm going to sacrafice all my white items for sticky's". They are valuable, but not the be-all end-all.

## Installation
Requires SirSquiddles ShapedGlassBalance mod. Place inside \Bepinex\plugins\.

Launch the game to create the configuration file, exit game. Edit: RoR2\Bepeninex\configs\com.paddywan.BetterBalance.cfg and relaunch.

Values available for configuration:

* Stickybomb Multiplier - default: 500%  (180% vanilla)

* Bleed Chance - default: 5% (15% vanilla)

* Bleed Multiplier - default: 2x (1x vanilla)

* IceBand Multiplier - default: 250% (125% vanilla)

* Ukulele Multiplier - default: 1.0f (0.8f vanilla)

* Crowbar Scalar - default: 0.08f (no vanilla implementation) - Scaling deminishing returns applied to the cap/threshold for which crowbar is triggered. Higher values approach the cap faster.

* Crowbar Cap - default: 0.3f (no vanilla implementation) - The hard cap at which crowbar threshold stops scaling.

* Guillotine Scalar - default: 0.01f (no vanilla implementation) - Scaling deminishing returns applied to the cap/threshold for which guilotine is triggered. Higher values approach the cap faster.

* Guillotine Cap - default: 0.45f (no vanilla implementation). - The hard cap at which Guilotine stops scaling.

* Predatory buffs per stacked - Default: 3 (2 vanilla) - Implements static scaling instead of 1+(2xitemCount)

* Armor Piercing effects elites - default: true (no vanilla implementation)

* Armor piercing Multiplier - default: 0.1 (0.2 vanilla)

* CursedOneShotProtection - default: enabled (disabled vanilla)


I have exposed these values to allow people to implement their own perspective of "balance", whatever that may be, however I have limited these values with a minimum and maximum, as the idea of this mod is to balance, not to make everything easy and one-shotable. I believe I have been quite lenient with the maximum values, most of which scale to double the default  values. If you want to "one shot enemies harder" then this is not the mod for you.

## Changelog
v1.0.2 - Implemented disabled OneShotProtection for cursePenalty(LunarDagger/Potion)

v1.0.1 - Adjusted maximum values as per feedback. Corrected some spelling mistakes, and probably made some more.

v1.0.0 - Released.