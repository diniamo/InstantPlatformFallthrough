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
                    i => i.MatchLdcI4(0),
                    i => i.MatchStfld<Player>(nameof(Player.slideDir)),

                    i => i.MatchLdcI4(0),
                    i => i.MatchStloc(out ignorePlatsIndex),

                    i => i.MatchLdarg0(),
                    i => i.MatchLdfld<Player>(nameof(Player.controlDown)),
                    i => i.MatchStloc(out fallThroughIndex)
                );

                var label = il.DefineLabel();

                c.EmitLdloc(fallThroughIndex); // Push the value of fallThrough onto the stack
                c.EmitBrfalse(label);          // Jump to the label emitted below if the stack value is false
                c.EmitLdcI4(1);                // Push 1 onto the stack
                c.EmitStloc(ignorePlatsIndex); // Set ignorePlats to stack value
                c.MarkLabel(label);            // Emulate an if statement with a label
            };

            base.Load();
        }
    }
}