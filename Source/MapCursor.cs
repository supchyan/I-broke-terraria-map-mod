using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace GigaMapMod.Source {
    public class MapCursor:ModProjectile {
        public float _scale;
        public Color _color;
        public bool _intersectsHostile;
        public bool _intersectsFriendly;
        public bool _intersectsAny; // true if any of other intersects is happening
        public override string Texture => "GigaMapMod/Sprites/cursor";
        public override void SetDefaults() {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.Opacity = 1f;
            Projectile.scale = 1f;
        }
        public override Color? GetAlpha(Color lightColor) {
            return Color.White;
        }
        public override void AI() {

            Projectile.ai[0] += 0.1f;

            // control projectile's cursor lifetime
            Projectile.timeLeft = 2;
            if(!CameraTools._isMapOpenned) {
                Projectile.Kill();
            }
            Projectile.position = Main.MouseWorld;
            Projectile.rotation = MathHelper.ToRadians(Projectile.ai[1]);
        }
        public override bool PreDraw(ref Color lightColor) {
            Texture2D _texture = (Texture2D)ModContent.Request<Texture2D>("GigaMapMod/Sprites/cursor");
            Vector2 _drawOrigin = new Vector2(_texture.Width, _texture.Height) / 2f;
            Rectangle _rect = new Rectangle(0,0,Projectile.width,Projectile.height);
            Vector2 _drawPos = Projectile.position + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            
            // cursor is smaller, if player is draging the cursor
            if(Main.mouseLeft) {
                if(_scale > 0.7f) _scale -= 0.1f;
            } else {
                if(_scale < 1f) _scale += 0.1f;
            }
            
            // get intersection state
            for(int i = 0; i<Main.maxNPCs; i++) {
                if(Main.npc[i].getRect().Intersects(Projectile.Hitbox)) {
                    if(!Main.npc[i].friendly) _intersectsHostile=true;
                    else _intersectsFriendly = true;
                    break;

                } else {
                    _intersectsHostile = false;
                    _intersectsFriendly = false;
                }
            }

            // intersection state for common stuff
            if(_intersectsHostile || _intersectsFriendly) {
                _intersectsAny = true;
            } else _intersectsAny = false;

            // cursor coloring
            if(_intersectsHostile) _color = Color.Red;
            else if(_intersectsFriendly) _color = Color.LightGreen;
            else _color = Color.White;
            
            // cursor animations
            if(_intersectsAny) {
                Projectile.ai[1]++;
                if(_scale < 2.0f) _scale += 0.1f;

            } else {
                Projectile.ai[1] = 0;
                if(_scale > 1f) _scale -= 0.1f;
            }

            Main.EntitySpriteDraw(_texture, _drawPos, null, _color, Projectile.rotation, _drawOrigin, _scale, SpriteEffects.None, 0);
            
            return false;
        }

        // draw projectile's cursor above everything
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
            Main.instance.DrawCacheProjsOverWiresUI.Add(index);
        }
    }
}