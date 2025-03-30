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
                try
                {
                    var c = new ILCursor(il);
                    c.GotoNext(MoveType.After,
                        i => i.MatchStfld<Player>(nameof(Player.slideDir)),
                        i => i.Match(Ldc_I4_0),
                        i => i.Match(Stloc_S),
                        i => i.Match(Ldarg_0),
                        i => i.MatchLdfld<Player>(nameof(Player.controlDown)),
                        i => i.Match(Stloc_S)
                    );

                    var label = il.DefineLabel();

                    c.Emit(Ldloc_S, (byte)14); // fallThrough
                    c.Emit(Brfalse_S, label);
                    c.Emit(Ldc_I4_1);
                    c.Emit(Stloc_S, (byte)13); // ignorePlats

                    c.MarkLabel(label);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };

            base.Load();
        }
    }
}