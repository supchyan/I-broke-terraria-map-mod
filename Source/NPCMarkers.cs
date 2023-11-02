using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;

namespace GigaMapMod.Source {
    public class NPCMarkers:ModProjectile {
        public float _scale;
        public Color _color;
        public bool _intersectsHostile;
        public bool _intersectsFriendly;
        public bool _intersectsAny; // true if any of other intersects is happening
        public bool _scaleFactor; // scale transition
        public float _scaleLerp; // scale of marker
        public override string Texture => "GigaMapMod/Sprites/cursor";
        public override void SetDefaults() {
            Projectile.width = 1;
            Projectile.height = 1;
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

            // control projectile's cursor lifetime
            Projectile.timeLeft = 2;
            if(!CameraTools._isMapOpenned) {
                Projectile.Kill();
            }       
        }
        public override bool PreDraw(ref Color lightColor) {
            for(int i = 0; i < Main.maxNPCs; i++) {
                
                NPC _npc = Main.npc[i];
                
                if(_npc.friendly) {

                    // getting npc texture
                    Texture2D _texture = (Texture2D)TextureAssets.Npc[_npc.type];

                    Vector2 _drawOrigin = new Vector2(_npc.frame.Width, _npc.frame.Height)*0.5f;
                    Vector2 _drawPos = Projectile.position + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
                    
                    // getting npc rectangle to get access of it's animations if needed
                    Rectangle _rect = _npc.frame;

                    SpriteEffects _spEff;

                    // fliping sprite if needed
                    if(_npc.direction == 1) _spEff = SpriteEffects.FlipHorizontally;
                    else _spEff = SpriteEffects.None;
                    
                    // screen center coord
                    Vector2 _scrCenter = Main.screenPosition + new Vector2(Main.screenWidth*0.5f,Main.screenHeight*0.5f);
                    
                    // distance vectors
                    Vector2 _between = _scrCenter - _npc.Center;
                    Vector2 _betweenForScale = _scrCenter - _npc.Center;

                    _scaleFactor = false;
                    
                    // checking trigger, when npc outside of the screen to scale up it's icon
                    if(_betweenForScale.X > Main.screenWidth*0.5f) _scaleFactor = true;
                    if(_betweenForScale.X < -Main.screenWidth*0.5f) _scaleFactor = true;
                    if(_betweenForScale.Y > Main.screenHeight*0.5f) _scaleFactor = true;
                    if(_betweenForScale.Y < -Main.screenHeight*0.5f) _scaleFactor = true;
                    
                    // scale animation depends on screen trigger
                    if(_scaleFactor) {
                        _scaleLerp+=0.1f;
                        if(_scaleLerp>=1f) _scaleLerp = 1f;

                    } else {
                        _scaleLerp-=0.1f;
                        if(_scaleLerp<=0f) _scaleLerp = 0f;
                    }

                    // checking trigger, when npc outside of the screen to propertly draw it's icon
                    if(_between.X > Main.screenWidth*0.4f) _between.X = Main.screenWidth*0.4f;
                    if(_between.X < -Main.screenWidth*0.4f) _between.X = -Main.screenWidth*0.4f;
                    if(_between.Y > Main.screenHeight*0.4f) _between.Y = Main.screenHeight*0.4f;
                    if(_between.Y < -Main.screenHeight*0.4f) _between.Y = -Main.screenHeight*0.4f;

                    float _dist = _between.Length();

                    // this will draw projectile at the center of the screen and then immediately move it to calcualted pos via velocity
                    Projectile.velocity = Projectile.velocity.DirectionFrom(_between)*_dist;
                    Projectile.position = _scrCenter;

                    // draw npc marker
                    Main.EntitySpriteDraw(_texture, _drawPos, _rect, Color.White, Projectile.rotation, _drawOrigin, _scaleLerp, _spEff, 0);
                }
            }    
            return false;
        }
    }
}