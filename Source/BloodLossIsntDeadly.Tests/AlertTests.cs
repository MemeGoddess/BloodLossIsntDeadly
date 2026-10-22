using System.Collections;
using System.Reflection;
using DevTools.Testing;
using DevTools.Testing.Utils;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BloodLossIsntDeadly.Tests;

[TestFixture(TestType.Playing)]
public class AlertTests
{
    private Pawn pawn;
    private FieldInfo activeAlertsField = AccessTools.Field(typeof(AlertsReadout), "activeAlerts");
    
    [SetUp]
    public void Setup()
    {
        pawn = PawnKindDefOf.Colonist
            .AsBuilder()
            .WithName("Bleeder")
            .Spawn();

        pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 10, armorPenetration: 1f,
            hitPart: pawn.RaceProps.body
                .GetPartsWithTag(BodyPartTagDefOf.BloodPumpingSource)
                .First()));

        var disableNaturalHealing = new HediffDef()
        {
            defName = "DisableNaturalHealing",
            stages =
            [
                new HediffStage()
                {
                    naturalHealingFactor = 0f,
                    becomeVisible = false
                }
            ]
        };
        
        pawn.health.AddHediff(disableNaturalHealing);
    }

    [Test]
    public IEnumerator TestAlert()
    {
        var activeAlerts = activeAlertsField.GetValue(Find.Alerts) as List<Alert>;
        Expect.IsNotNull(activeAlerts, "Couldn't fetch active alerts, unable to test");
        
        Tweaker.SuperFast = true;
        yield return new WaitTicks(400);
        var bloodloss = pawn.health.hediffSet.hediffs.First(x => x.def == HediffDefOf.BloodLoss);
        bloodloss.Severity = 0.5f;
        yield return new WaitUntilTimeout(() => pawn.Dead || bloodloss.Severity >= 0.7f, GenDate.TicksPerDay * 2);
        Tweaker.SuperFast = false;
        
        var alertActive = () => activeAlerts.Any(x => x.Active && x is Alert_LifeThreateningHediff);
        yield return new WaitUntilTimeout(() => alertActive(), GenDate.TicksPerHour);
        Expect.IsTrue(alertActive(), "Alert wasn't active");
        
        foreach (var hediff in pawn.health.hediffSet.GetHediffsTendable()) 
            hediff.Tended(1f, 1f);
            
        Expect.AreEqual(0f, pawn.health.hediffSet.BleedRateTotal, "Pawn was still bleeding");
        yield return new WaitUntilTimeout(() => !alertActive(), GenDate.TicksPerHour);
        Expect.IsFalse(alertActive(), "Alert wasn't suppressed");

        var newPawn = PawnKindDefOf.Colonist
            .AsBuilder()
            .WithName("Infector")
            .Spawn();
        var infection = newPawn.health.AddHediff(HediffDefOf.WoundInfection, pawn.RaceProps.body
            .GetPartsWithTag(BodyPartTagDefOf.BloodPumpingSource)
            .First());
        infection.Severity = 0.8f;
        bloodloss.Severity = 0.98f;
        yield return new WaitUntilTimeout(() => alertActive(), GenDate.TicksPerHour);
        Expect.IsTrue(alertActive(), "Alert wasn't suppressed");
        var alert = activeAlerts.First(x => x.Active && x is Alert_LifeThreateningHediff);
        var explanation = alert.GetExplanation().ToString();
        Expect.IsFalse(explanation.Contains(pawn.NameShortColored.Resolve()), "BloodLoss/Tended pawn showed in Explanation");
    }
}