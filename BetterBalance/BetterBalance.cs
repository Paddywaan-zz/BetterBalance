using BepInEx;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using System;
using BepInEx.Configuration;

namespace Paddywan
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("com.Squiddle.shapedglassbalance")]
    [BepInPlugin("com.Paddywan.BetterBalance", "BetterBalance", "1.0.0")]
    public class BetterBalance : BaseUnityPlugin
    {
        private float _stickyMultiplier = 5.0f,
            _bleedDamageMultiplier = 2.0f,
            _bleedChancePerStack = 5f,
            _iceRingMultiplier = 2.5f,
            _ukuleleMultiplier = 1.0f,
            _crowbarScalar = 0.08f,
            _crowbarCap = 0.3f;
        private int _predatoryBuffsPerStack = 3;

        private float StickyMultiplier { get { return _stickyMultiplier;} set {if (value > 0f && value <= 10f) _stickyMultiplier = value; } }
        private float BleedDamageMultiplier { get { return _bleedDamageMultiplier; } set { if (value >= 0f && value <= 4f) _bleedDamageMultiplier = value; } }
        private float BleedChancePerStack { get { return _bleedChancePerStack; }set { if (value >= 0f && value <= 15f) _bleedChancePerStack = value; } }
        private float IceRingMultiplier { get { return _iceRingMultiplier; }set { if (value >= 0f && value <= 5f) _iceRingMultiplier = value; } }
        private float UkuleleMultiplier { get { return _ukuleleMultiplier; }set { if (value > 0f && value <= 1.2f) _ukuleleMultiplier = value; } }
        private float CrowbarScalar { get { return _crowbarScalar; }set { if (value > 0f && value <= 0.16f) _crowbarScalar = value; } }
        private float CrowbarCap { get { return _crowbarCap; } set { if (value < 1f && value >= 0.6f) _crowbarCap = value; } }
        private int PredatoryBuffsPerStack { get { return _predatoryBuffsPerStack; }set { if (value >= 0 && value <= 4) _predatoryBuffsPerStack = value; } }

        private ConfigWrapper<float> cStickyMultiplier, cBleedMultiplier, cBleedChancePerStack, cIceRingMultiplier, cUkuleleMultiplier, cCrowbarScalar, cCrowbarCap;
        private ConfigWrapper<int> cPredatoryBuffsPerStack;
        private ConfigWrapper<bool> cAPElites, cCrowbarDeminishingThreshold;

        public void Awake()
        {
            On.RoR2.Console.Awake += (orig, self) =>
            {
                CommandHelper.RegisterCommands(self);
                orig(self);
            };
            cStickyMultiplier = Config.Wrap("Multipliers", "StickybombMultiplier", "Modifies the damage multiplier of the stickybomb: >0f, 1.8f vanilla, 5f default, <=10f", _stickyMultiplier);
            StickyMultiplier = cStickyMultiplier.Value;
            cBleedMultiplier = Config.Wrap("Multipliers", "BleedDamageMultiplier", "Modifies the damage multiplier value of tri-tip: >0f, 1.0f vanilla, 2f default <=4f", _bleedDamageMultiplier);
            BleedDamageMultiplier = cBleedMultiplier.Value;
            cBleedChancePerStack = Config.Wrap("Multipliers", "BleedChancePerStack", "Modifies the chance of inflicting bleed with tri-tip: >0f, 15f vanilla, 5f default, <=10f", _bleedChancePerStack);
            BleedChancePerStack = cBleedChancePerStack.Value;
            cIceRingMultiplier = Config.Wrap("Multipliers", "IceRingMultiplier", "Modifies the damage multiplier of the Ice Band: >0f, 1.25f vanilla, 2.5f default, <=5f", _iceRingMultiplier);
            IceRingMultiplier = cIceRingMultiplier.Value;
            cUkuleleMultiplier = Config.Wrap("Multipliers", "UkeleleMultiplier", "Modifies the damage multiplier of the Ukulele: >0f, 0.8f vanilla, 1.0f default, <=1.2f", _ukuleleMultiplier);
            UkuleleMultiplier = cUkuleleMultiplier.Value;

            cCrowbarDeminishingThreshold = Config.Wrap("Crowbar", "CrowbarDeminishingThreshold", "Enables a variable threshold for Crowbars which scales with deminishing returns, similar to a teddy.", true);
            cCrowbarScalar = Config.Wrap("Crowbar", "CrowbarScalar", "Modifies the per stack scalar with deminishing returns: >0f, 0.08f default, <=0.16f", _crowbarScalar);
            CrowbarScalar = cCrowbarScalar.Value;
            cCrowbarCap = Config.Wrap("Crowbar", "CrowbarCap", "Modifies the cap of the maximum health for which the crowbars effect is applied: <1f, 0.7f default, >=0.6f", _bleedDamageMultiplier);
            CrowbarCap = cCrowbarCap.Value;

            cPredatoryBuffsPerStack = Config.Wrap("Mechanics", "PredatoryBuffsPerStack", "Alters the scaling of predatory isntincts to scale linearly instead of 3+2xStacks: >0i, 3i default, <=4", _predatoryBuffsPerStack);
            PredatoryBuffsPerStack = cPredatoryBuffsPerStack.Value;

            cAPElites = Config.Wrap("Mechanics", "APElites", "Alters the AP rounds to be inclusive of elite mobs: >0i, 3i default, <=4", true);
            //_stickyMultiplier = (cStickyMultiplier.Value >= 0f && cStickyMultiplier.Value <= 10f) ? cStickyMultiplier.Value : _stickyMultiplier;

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
            };
            IL.RoR2.HealthComponent.TakeDamage += (il) =>
            {
                ILCursor c = new ILCursor(il);
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

                    c.GotoNext(
                        x => x.MatchLdarg(1),
                        x => x.MatchLdfld<DamageInfo>("crit")
                        );
                    c.MarkLabel(lab2);

                    //Debug.Log(il);
                }
                #endregion
            };
        }


        public void Update()
        {
            //TestHelper.spawnItem(KeyCode.F7, ItemIndex.Crowbar);

            //TestHelper.spawnItem(KeyCode.F8, ItemIndex.HealWhileSafe);

            //TestHelper.spawnItem(KeyCode.F9, ItemIndex.AttackSpeedOnCrit);
            //ItemIndex.BossDamageBonus
        }
    }
}
