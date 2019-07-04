using BepInEx;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using System;

namespace Paddywan
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("com.Squiddle.shapedglassbalance")]
    [BepInPlugin("com.Paddywan.BetterBalance", "BetterBalance", "1.0.0")]
    public class BetterBalance : BaseUnityPlugin
    {
        private float stickyMultiplier = 5.0f,
            bleedDamageScale = 2.0f,
            bleedChancePerStack = 5f,
            iceRingDamage = 2.5f,
            ukuleleDamage = 1.0f,
            crowbarScalar = 0.8f,
            crowbarCap = 0.3f;
        private int predatoryBuffsPerStack = 3;
        private bool APElites = true;

        public void Awake()
        {
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
                c.Emit(OpCodes.Ldc_R4, bleedChancePerStack);

                //BleedDmg
                c.GotoNext(
                    x => x.MatchLdfld<DamageInfo>("procCoefficient"),
                    x => x.MatchMul(),
                    x => x.MatchLdcR4(1f)
                    );
                c.Index += 2;
                c.Remove();
                c.Emit(OpCodes.Ldc_R4, bleedDamageScale);

                //Ukulele
                c.GotoNext(
                    x => x.MatchLdcR4(0.8f),
                    x => x.MatchStloc(24)
                    );
                c.Remove();
                c.Emit(OpCodes.Ldc_R4, ukuleleDamage);

                //StickyDmg
                c.GotoNext(
                    x => x.MatchLdcR4(1.8f),
                    x => x.MatchStloc(37)// (OpCodes.Stloc_S, (byte)37)
                    );
                c.Remove();
                c.Emit(OpCodes.Ldc_R4, stickyMultiplier);

                //IceBand
                c.GotoNext(
                    x => x.MatchLdcR4(1.25f),
                    x => x.MatchLdcR4(1.25f),
                    x => x.MatchLdloc(39)
                    );
                c.RemoveRange(2);
                c.Emit(OpCodes.Ldc_R4, iceRingDamage);
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
                c.Emit(OpCodes.Ldc_I4, predatoryBuffsPerStack);
                c.Index += 1;
                c.Remove();
            };
            IL.RoR2.HealthComponent.TakeDamage += (il) =>
            {
                ILCursor c = new ILCursor(il);
                //100 - 30 (1 - (1 + 0.08 * 100)^(-1))
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
                c.EmitDelegate<Func<CharacterBody, float>>((cb) => {
                    if(cb.master.inventory)
                    {
                        int bars = cb.master.inventory.GetItemCount(ItemIndex.Crowbar);
                        if (bars > 0)
                        {
                            return 1f - ((1f - 1f / (crowbarScalar * (float)bars + 1f)) * crowbarCap);
                        }
                    }
                    return 0.9f;
                });

                #region AP
                if (APElites)
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
