using System;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace InstantPlatformFallthrough
{
    public class InstantPlatformFallthroughMod : Mod
    {
        public override void Load()
        {
            // IL-Hooks will get unloaded automatically, therefor Unload() is not necessary
            IL_Player.Update += ModifyPlatformFallthrough;
        }

        private void ModifyPlatformFallthrough(ILContext il)
        {
            //TODO: this works for "normal" direction, but needs to be checked for inverted gravity! (maybe controlUp gets used instead of controlDown)
            try
            {
                var c = new ILCursor(il);

                // Anchor: this.slideDir = 0;
                c.GotoNext(MoveType.After,
                    i => i.MatchLdarg0(),
                    i => i.MatchLdcI4(0),
                    i => i.MatchStfld<Player>(nameof(Player.slideDir)));

                // Goal: ignorePlats = false; fallThrough = this.controlDown;
                c.GotoNext(MoveType.Before,
                    i => i.MatchLdcI4(0),
                    i => i.MatchStloc(out _),
                    i => i.MatchLdarg0(),
                    i => i.MatchLdfld<Player>(nameof(Player.controlDown)),
                    i => i.MatchStloc(out _));

                // replace "false" with "this.controlDown"
                c.Remove();
                c.EmitLdarg0();
                c.EmitLdfld(typeof(Player).GetField(nameof(Player.controlDown)));
            }
            catch (Exception e)
            {
                MonoModHooks.DumpIL(this, il);
                throw new ILPatchFailureException(this, il, e);
            }
        }
    }
}