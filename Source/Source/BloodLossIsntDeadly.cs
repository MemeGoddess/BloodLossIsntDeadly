using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace BloodLossIsntDeadly
{
    public class BloodLossIsntDeadly : Mod
    {
        public BloodLossIsntDeadly(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("MemeGoddess.BloodLossIsntDeadly");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}