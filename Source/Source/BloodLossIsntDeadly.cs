using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace BloodLossIsntDeadly
{
    public class BloodLossIsntDeadly
    {
        private List<Pawn> sickPawnsResult = new List<Pawn>();
        static BloodLossIsntDeadly()
        {
            var harmony = new Harmony("com.BloodlossIsntDeadly.patch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}