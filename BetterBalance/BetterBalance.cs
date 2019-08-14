using BepInEx;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using R2API;
using System;
using BepInEx.Configuration;
using UnityEngine;

namespace Paddywan
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("com.Squiddle.shapedglassbalance")]
    [BepInPlugin("com.Paddywan.BetterBalance", "BetterBalance", "1.0.1")]
    public class BetterBalance : BaseUnityPlugin
    {
        private float _stickyMultiplier = 5.0f, _stickyMin = 0f, _stickyMax = 10f,
            _bleedDamageMultiplier = 2.0f, _bleedDamageMin = 0f, _bleedDamageMax = 4f,
            _bleedChancePerStack = 5f, _bleedChanceMin = 0f, _bleedChanceMax = 15f,
            _iceRingMultiplier = 2.5f, _iceRingMin = 0f, _iceRingMax = 5f,
            _ukuleleMultiplier = 1.0f, _ukuleleMin = 0f, _ukuleleMax = 1.0f,
            _crowbarScalar = 0.08f, _crowbarScalarMin = 0f, _crowbarScalarMax = 0.16f,
            _crowbarCap = 0.3f, _crowbarCapMin = 0f, _crowbarCapMax = 0.4f,
            _guilotineScalar = 0.10f, _guilotineScalarMin = 0f, _guilotineScalarMax = 0.3f,
            _guilotineCap = 0.45f, _guilotineCapMin = 0f, _guilotineCapMax = 0.6f,
            _APScalar = 0.1f, _APMin = 0f, _APMax = 0.2f;
        private int _predatoryBuffsPerStack = 3, _predatoryMin = 2, _predatoryMax = 4;

        private ConfigWrapper<float> cStickyMultiplier, cBleedMultiplier, cBleedChancePerStack, cIceRingMultiplier, cUkuleleMultiplier, cCrowbarScalar, cCrowbarCap, cGuillotineScalar, cGuillotineCap, cAPDamage;
        private ConfigWrapper<int> cPredatoryBuffsPerStack;
        private ConfigWrapper<bool> cAPElites, cCrowbarDeminishingThreshold, cGuillotineDeminishingThreshold, cPredatoryEnabled, cCursedOSP;



        public void Awake()
        {
            On.RoR2.Console.Awake += (orig, self) =>
            {
                CommandHelper.RegisterCommands(self);
                orig(self);
            };
            cStickyMultiplier = Config.Wrap("Multipliers", "StickybombMultiplier", $"Modifies the damage multiplier of the stickybomb: >{_stickyMin}f, 1.8f vanilla, {_stickyMultiplier}f default, <={_stickyMax}f", _stickyMultiplier);
            cBleedMultiplier = Config.Wrap("Multipliers", "BleedDamageMultiplier", $"Modifies the damage multiplier value of tri-tip: >{_bleedDamageMin}f, 1.0f vanilla, {_bleedDamageMultiplier}f default <={_bleedDamageMax}f", _bleedDamageMultiplier);
            cBleedChancePerStack = Config.Wrap("Multipliers", "BleedChancePerStack", $"Modifies the chance of inflicting bleed with tri-tip: >{_bleedChanceMin}f, 15f vanilla, {_bleedChancePerStack}f default, <={_bleedChanceMax}f", _bleedChancePerStack);
            cIceRingMultiplier = Config.Wrap("Multipliers", "IceRingMultiplier", $"Modifies the damage multiplier of the Ice Band: >{_iceRingMin}f, 1.25f vanilla, {_iceRingMultiplier}f default, <={_iceRingMax}f", _iceRingMultiplier);
            cUkuleleMultiplier = Config.Wrap("Multipliers", "UkeleleMultiplier", $"Modifies the damage multiplier of the Ukulele: >{_ukuleleMin}f, 0.8f vanilla, {_ukuleleMultiplier}f default, <={_ukuleleMax}f", _ukuleleMultiplier);
            _stickyMultiplier = (cStickyMultiplier.Value > _stickyMin && cStickyMultiplier.Value <= _stickyMax) ? cStickyMultiplier.Value : _stickyMultiplier;
            _bleedDamageMultiplier = (cBleedMultiplier.Value > _bleedDamageMin && cBleedMultiplier.Value <= _bleedDamageMax) ? cBleedMultiplier.Value : _bleedDamageMultiplier;
            _bleedChancePerStack = (cBleedChancePerStack.Value > _bleedChanceMin && cBleedChancePerStack.Value <= _bleedChanceMax) ? cBleedChancePerStack.Value : _bleedChancePerStack;
            _iceRingMultiplier = (cIceRingMultiplier.Value > _iceRingMin && cIceRingMultiplier.Value <= _iceRingMax) ? cIceRingMultiplier.Value : _iceRingMultiplier;
            _ukuleleMultiplier = (cUkuleleMultiplier.Value > _ukuleleMin && cUkuleleMultiplier.Value <= _ukuleleMax) ? cUkuleleMultiplier.Value : _ukuleleMultiplier;

            cCrowbarDeminishingThreshold = Config.Wrap("Crowbar", "CrowbarDeminishingThresholdEnabled", "Enables a variable threshold for Crowbars which scales with deminishing returns, similar to a teddy.", true);
            cCrowbarScalar = Config.Wrap("Crowbar", "CrowbarScalar", $"Modifies the per stack scalar with deminishing returns: >{_crowbarScalarMin}f, {_crowbarScalar}f default, <={_crowbarScalarMax}f", _crowbarScalar);
            cCrowbarCap = Config.Wrap("Crowbar", "CrowbarCap", $"Modifies the cap of the maximum health for which the crowbars effect is applied, as a % of fullHP: >{_crowbarCapMin}f, {_crowbarCap}f default, <={_crowbarCapMax}f", _crowbarCap);
            _crowbarScalar = (cCrowbarScalar.Value > _crowbarScalarMin && cCrowbarScalar.Value <= _crowbarScalarMax) ? cCrowbarScalar.Value : _crowbarScalar;
            _crowbarCap = (cCrowbarCap.Value > _crowbarCapMin && cCrowbarCap.Value <= _crowbarCapMax) ? cCrowbarCap.Value : _crowbarCap;


            cGuillotineDeminishingThreshold = Config.Wrap("Guillotine", "GuillotineThresholdEnabled", "Enables a variable threshold for Guillotine which scales with deminishing returns, similar to a teddy.", true);
            cGuillotineScalar = Config.Wrap("Guillotine", "GuillotineScalar", $"Modifies the per stack scalar with deminishing returns: >{_guilotineScalarMin}f, {_guilotineScalar}f default, <={_guilotineScalarMax}f", _guilotineScalar);
            cGuillotineCap = Config.Wrap("Guillotine", "GuillotineCap", $"Modifies the cap of the minimum health for which the crowbars effect is applied, as a % of fullHP: >{_guilotineCapMin}f, {_guilotineCap}f default, <={_guilotineCapMax}f", _guilotineCap);
            _guilotineScalar = (cGuillotineScalar.Value > _guilotineScalarMin && cGuillotineScalar.Value <= _guilotineScalarMax) ? cGuillotineScalar.Value : _guilotineScalar;
            _guilotineCap = (cGuillotineCap.Value > _guilotineCapMin && cGuillotineCap.Value <= _guilotineCapMax) ? cGuillotineCap.Value : _guilotineCap;

            cPredatoryEnabled = Config.Wrap("Predatory", "PredatoryEnabled", "Enables linear predatory scaling: 3,6,9...", true);
            cPredatoryBuffsPerStack = Config.Wrap("Predatory", "PredatoryBuffsPerStack", $"Alters the scaling of predatory isntincts to scale linearly instead of 3+2xStacks: >{_predatoryMin}i, {_predatoryBuffsPerStack}i default, <={_predatoryMax}", _predatoryBuffsPerStack);
            _predatoryBuffsPerStack = (cPredatoryBuffsPerStack.Value > _predatoryMin && cPredatoryBuffsPerStack.Value <= _predatoryMax) ? cPredatoryBuffsPerStack.Value : _predatoryBuffsPerStack;

            cAPElites = Config.Wrap("APRounds", "APElitesEnabled", "Alters the AP rounds to be inclusive of elite mobs", true);
            cAPDamage = Config.Wrap("APRounds", "APDamageScalar", $"Alters the AP damage scalar to be lower than default due to increased effectiveness: >{_APMin}f, {_APScalar}f default, <={_APMax}f", _APScalar);
            _APScalar = (cAPDamage.Value > _APMin && cAPDamage.Value <= _APMax) ? cAPDamage.Value : _APScalar;

            cCursedOSP = Config.Wrap("OSP", "CursedOSPDisabled", "Disables One Shot Protection for Cursed characters(read as shaped glass, lunar potion curse", true);

            IL.RoR2.GlobalEventManager.OnHitEnemy += (il) =>
            {
                var c = new ILCursor(il);
                //BleedChance
                c.GotoNext(
                    x => x.MatchLdcR4(15f),
                    x => x.MatchLdloc(19),
                    x => x.MatchConvR4()
                    );
                c.Remove();
                c.Emit(OpCodes.Ldc_R4, _bleedChancePerStack);

                //BleedDmg
                c.GotoNext(
                    x => x.MatchLdfld<DamageInfo>("procCoefficient"),
                    x => x.MatchMul(),
                    x => x.MatchLdcR4(1f)
                    );
                c.Index += 2;
                c.Remove();
                c.Emit(OpCodes.Ldc_R4, _bleedDamageMultiplier);

                //Ukulele
                c.GotoNext(
                    x => x.MatchLdcR4(0.8f),
                    x => x.MatchStloc(24)
                    );
                c.Remove();
                c.Emit(OpCodes.Ldc_R4, _ukuleleMultiplier);

                //StickyDmg
                c.GotoNext(
                    x => x.MatchLdcR4(1.8f),
                    x => x.MatchStloc(37)// (OpCodes.Stloc_S, (byte)37)
                    );
                c.Remove();
                c.Emit(OpCodes.Ldc_R4, _stickyMultiplier);

                //IceBand
                c.GotoNext(
                    x => x.MatchLdcR4(1.25f),
                    x => x.MatchLdcR4(1.25f),
                    x => x.MatchLdloc(39)
                    );
                c.RemoveRange(2);
                c.Emit(OpCodes.Ldc_R4, _iceRingMultiplier);
                c.Emit(OpCodes.Ldc_R4, 2.5f);
            };

            IL.RoR2.CharacterBody.AddTimedBuff += (il) =>
            {
                var c = new ILCursor(il);
                if (cPredatoryEnabled.Value)
                {
                    c.GotoNext(
                        x => x.MatchLdloc(2),
                        x => x.MatchLdcI4(1),
                        x => x.MatchLdloc(1),
                        x => x.MatchLdcI4(2)
                        );
                    c.Index += 1;
                    c.Remove();
                    c.Index += 1;
                    c.Remove();
                    c.Emit(OpCodes.Ldc_I4, _predatoryBuffsPerStack);
                    c.Index += 1;
                    c.Remove();
                }
            };

            IL.RoR2.HealthComponent.TakeDamage += (il) =>
            {
                ILCursor c = new ILCursor(il);
                #region Crowbar
                if (cCrowbarDeminishingThreshold.Value)
                {
                    c.GotoNext(
                        x => x.MatchLdarg(0),
                        x => x.MatchCallvirt<HealthComponent>("get_combinedHealth"),
                        x => x.MatchLdarg(0),
                        x => x.MatchCallvirt<HealthComponent>("get_fullCombinedHealth"),
                        x => x.MatchLdcR4(0.9f)
                        );
                    c.Index += 4;
                    c.Remove();
                    c.Emit(OpCodes.Ldloc_1);
                    c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                    {
                        if (cb.master.inventory)
                        {
                            int bars = cb.master.inventory.GetItemCount(ItemIndex.Crowbar);
                            if (bars > 0)
                            {
                                return 1f - ((1f - 1f / (_crowbarScalar * (float)bars + 1f)) * _crowbarCap);
                            }
                        }
                        return 0.9f;
                    });
                }
                #endregion

                #region AP
                if (cAPElites.Value)
                {
                    //Debug.Log(il);
                    ILLabel lab1 = il.DefineLabel();
                    ILLabel lab2 = il.DefineLabel();
                    c.GotoNext(
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<HealthComponent>("body"),
                        x => x.MatchCallvirt<CharacterBody>("get_isBoss")
                        );

                    c.Index += 3;
                    c.Remove();
                    c.Emit(OpCodes.Brtrue_S, lab1);
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldfld, typeof(HealthComponent).GetFieldCached("body"));
                    c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethodCached("get_isElite"));
                    c.Emit(OpCodes.Brfalse_S, lab2);

                    //c.Index = 1;
                    c.MarkLabel(lab1);

                    //AP Multiplier
                    c.GotoNext(
                        x => x.MatchLdloc(4),
                        x => x.MatchLdcR4(1.0f),
                        x => x.MatchLdcR4(0.2f)
                        );
                    //Debug.Log(c);
                    c.Index += 2;
                    c.Remove();
                    c.Emit(OpCodes.Ldc_R4, _APScalar);

                    //Return label
                    c.GotoNext(
                        x => x.MatchLdarg(1),
                        x => x.MatchLdfld<DamageInfo>("crit")
                        );
                    c.MarkLabel(lab2);

                    //Debug.Log(il);
                }
                #endregion

                #region OSP
                if (cCursedOSP.Value)
                {
                    c.GotoNext(
                        x => x.MatchLdloc(4),
                        x => x.MatchLdarg(0),
                        x => x.MatchCallvirt<HealthComponent>("get_fullCombinedHealth"),
                        x => x.MatchLdcR4(0.9f),
                        x => x.MatchMul()
                        );
                    c.Index += 5;
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldfld, typeof(HealthComponent).GetFieldCached("body"));
                    c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethodCached("get_cursePenalty"));
                    c.Emit(OpCodes.Mul);
                }
                #endregion

                #region Guilotine
                if (cGuillotineDeminishingThreshold.Value)
                {
                    c.GotoNext(
                    x => x.MatchLdloc(1),
                    x => x.MatchCallvirt<CharacterBody>("get_executeEliteHealthFraction"),
                    x => x.MatchStloc(29)
                    );
                    c.Index++;
                    c.Remove();
                    c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                    {
                        if (cb.inventory && cb.inventory.GetItemCount(ItemIndex.ExecuteLowHealthElite) > 0)
                        {
                            return ((1f - 1f / (_guilotineScalar * (float)cb.inventory.GetItemCount(ItemIndex.ExecuteLowHealthElite) + 1f)) * _guilotineCap);
                        }
                        return cb.executeEliteHealthFraction;
                    });
                }
                //Debug.Log(il);
                #endregion

                
            };
        }

        public void Update()
        {
            //TestHelper.itemSpawnHelper();
            //TestHelper.spawnItem(KeyCode.F7, ItemIndex.StickyBomb);
            //TestHelper.spawnItem(KeyCode.F8, ItemIndex.BleedOnHit);
            //TestHelper.spawnItem(KeyCode.F8, ItemIndex.CritGlasses);
        }
    }
}
