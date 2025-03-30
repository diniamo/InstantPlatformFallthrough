using System;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

using static Mono.Cecil.Cil.OpCodes;

namespace InstantPlatformFallthrough
{
    public class InstantPlatformFallthrough : Mod
    {
        public override void Load()
        {
            IL_Player.Update += il =>
            {
                var c = new ILCursor(il);

                // These will be always be set by the time they are used,
                // but C# doesn't know that the predicates will always be called
                // (unless an error occurs, but code execution will stop here too in that case)
                int ignorePlatsIndex = default;
                int fallThroughIndex = default;

                c.GotoNext(MoveType.After,
                    // We don't actually care about the first 2 parts, but match them anyway,
                    // because the sequence we actaully need may very well appear before.
                    i => i.MatchLdarg0(),
                    i => i.MatchLdfld<Entity>(nameof(Entity.velocity)),
                    i => i.Match(Stloc_S),

                    i => i.MatchLdarg0(),
                    i => i.Match(Ldc_I4_0),
                    i => i.MatchStfld<Player>(nameof(Player.slideDir)),

                    i => i.Match(Ldc_I4_0),
                    i => i.MatchStloc(out ignorePlatsIndex),

                    i => i.MatchLdarg0(),
                    i => i.MatchLdfld<Player>(nameof(Player.controlDown)),
                    i => i.MatchStloc(out fallThroughIndex)
                );

                c.Index -= 5;
                c.RemoveRange(5);

                c.EmitLdarg0();                                                   // Push the first argument (this)
                c.EmitLdfld(typeof(Player).GetField(nameof(Player.controlDown))); // Pop, then push the value of controlDown
                c.EmitDup();                                                      // Duplicate stack value
                c.EmitStloc(ignorePlatsIndex);                                    // Store stack value in ignorePlats
                c.EmitStloc(fallThroughIndex);                                    // Store stack value in fallThrough
            };

            base.Load();
        }
    }
}