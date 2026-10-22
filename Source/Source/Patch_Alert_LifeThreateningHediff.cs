namespace BloodLossIsntDeadly;

[HarmonyPatch]
public static class Patch_Alert_LifeThreateningHediff
{
    [HarmonyPatch(typeof(Alert_LifeThreateningHediff), "GetReport")]
    [HarmonyPrefix]
    public static bool GetReport(ref AlertReport __result, Alert_LifeThreateningHediff __instance)
    {
        var property = __instance.GetType().GetProperty("SickPawns",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var sickPawns = property.GetValue(__instance, null) as List<Pawn>;

        FilterPawns(sickPawns);

        __result = AlertReport.CulpritsAre(sickPawns);
        return false;
    }

    [HarmonyPatch(typeof(Alert_LifeThreateningHediff), nameof(Alert_LifeThreateningHediff.GetExplanation))]
    [HarmonyPrefix]
    public static bool GetExplanation(ref TaggedString __result, Alert_LifeThreateningHediff __instance)
    {
        var property = __instance.GetType().GetProperty("SickPawns",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var sickPawns = property.GetValue(__instance, null) as List<Pawn>;
        FilterPawns(sickPawns);
        var stringBuilder = new StringBuilder();
        var flag = false;
        foreach (var sickPawn in sickPawns)
        {
            stringBuilder.AppendLine("  - " + sickPawn.NameShortColored.Resolve());
            foreach (var hediff in sickPawn.health.hediffSet.hediffs)
            {
                if (hediff.CurStage != null && hediff.CurStage.lifeThreatening && hediff.Part != null && hediff.Part != sickPawn.RaceProps.body.corePart)
                {
                    flag = true;
                    break;
                }
            }
        }
        __result = flag ? "PawnsWithLifeThreateningDiseaseAmputationDesc".Translate((NamedArgument)stringBuilder.ToString()) : "PawnsWithLifeThreateningDiseaseDesc".Translate((NamedArgument)stringBuilder.ToString());
        return false;
    }

    private static void FilterPawns(List<Pawn> pawns)
    {
        pawns.RemoveAll(x =>
        {
            foreach (var hediff in x.health.hediffSet.hediffs)
            {
                if (hediff.CurStage != null && hediff.CurStage.lifeThreatening && !hediff.FullyImmune() &&
                    (hediff.def.defName != HediffDefOf.BloodLoss.defName ||
                     x.health.hediffSet.hediffs.Any(y => y.Bleeding)))
                {
                    return false;
                }
            }

            return true;
        });
    }
}