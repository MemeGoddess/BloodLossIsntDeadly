using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BloodLossIsntDeadly;

[HarmonyPatch]
public static class Patch_Alert_LifeThreateningHediff
{
    [HarmonyPatch(typeof(Alert_LifeThreateningHediff), "SickPawns", MethodType.Getter)]
    [HarmonyPostfix]
    public static void Filter(ref List<Pawn> __result) => 
        FilterPawns(__result);

    private static void FilterPawns(List<Pawn> pawns)
    {
        pawns.RemoveAll(x =>
        {
            var bleeding = x.health.hediffSet.BleedRateTotal > 1E-05f;
            if (bleeding)
                return false;
            
            foreach (var hediff in x.health.hediffSet.hediffs)
            {
                if (hediff.CurStage is { lifeThreatening: true } && 
                    !hediff.FullyImmune() &&
                    hediff.def.defName != HediffDefOf.BloodLoss.defName)
                    return false;
            }

            return true;
        });
    }
}