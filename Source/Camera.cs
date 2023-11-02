using Terraria;
using Terraria.ModLoader;
using Terraria.GameInput;
using Microsoft.Xna.Framework;

namespace GigaMapMod.Source {
    public class CameraTools:ModSystem {
        public static int[] _NPClist;
        public Vector2 _cameraPosition;
        public Vector2 _playerPos;
        public float _oldGameZoom;
        public float _mapZoom; // current camera zoom value
        public bool _shouldSaveCamera; // true when mod saves some props of world camera
        public Vector2 _oldMouseWorld;
        public static bool _isMapOpenned;
        public float _lerp; // lerp used for map transitions
        public bool _doTranslateToFocus; // true when need to do smooth transition to player position
        public static ModKeybind _focusOnPlayer { get; private set; }
        public static ModKeybind _openMap { get; private set; }
        public Vector2 LerpTranslate(Vector2 a, Vector2 b, float t) {
            return Vector2.Lerp(a,b,t);
        }
        public override void Load() {
			_focusOnPlayer = KeybindLoader.RegisterKeybind(Mod, "FocusOnPlayer", "N");
            _openMap = KeybindLoader.RegisterKeybind(Mod, "OpenMap", "M");
		}
        public override void Unload() {
			_focusOnPlayer = null;
            _openMap = null;
		}
        public override void OnWorldLoad() {
            _cameraPosition = _playerPos;
            _oldGameZoom = Main.GameZoomTarget;
        }
        public override void OnWorldUnload() {
            _isMapOpenned = false;
            Main.GameZoomTarget = _oldGameZoom;
            Main.hideUI = false;
        }
        public override void ModifyScreenPosition() {
            _playerPos = new Vector2(Main.LocalPlayer.Center.X-Main.screenWidth/2f, Main.LocalPlayer.Center.Y-Main.screenHeight/2f);

            // 'map state'
            if(_openMap.JustReleased) {
                _isMapOpenned = !_isMapOpenned;
            }

            if(_isMapOpenned) {

                _lerp += 0.01f;

                // hide vanilla UI
                Main.hideUI = true;

                // draw cursor
                if(Main.LocalPlayer.ownedProjectileCounts[ModContent.ProjectileType<MapCursor>()] == 0) {
                    Projectile.NewProjectile(Entity.GetSource_None(), Main.LocalPlayer.Center, Vector2.Zero, ModContent.ProjectileType<MapCursor>(), 0, 0, Main.LocalPlayer.whoAmI);
                }

                // draw markers
                if(Main.LocalPlayer.ownedProjectileCounts[ModContent.ProjectileType<NPCMarkers>()] == 0) {
                    Projectile.NewProjectile(Entity.GetSource_None(), Main.LocalPlayer.Center, Vector2.Zero, ModContent.ProjectileType<NPCMarkers>(), 0, 0, Main.LocalPlayer.whoAmI);
                }
                
                
                // trigger to save current zoom of gaming camera
                if(_shouldSaveCamera) {
                    _oldGameZoom = Main.GameZoomTarget;
                    _shouldSaveCamera = false;
                }
                
                // close map on 'esc' button
                if(PlayerInput.Triggers.JustPressed.Inventory) _isMapOpenned = false;

                // scales map if it possible
                if(PlayerInput.ScrollWheelDelta != 0) {
                    _mapZoom += PlayerInput.ScrollWheelDelta/1200f;
                }
                if(_mapZoom > 2f) _mapZoom = 2f;
                if(_mapZoom < 1f) _mapZoom = 1f;
                
                // applying calculations to actual zoom slider
                Main.GameZoomTarget = MathHelper.Lerp(Main.GameZoomTarget, _mapZoom, _lerp);

                // this allows player to navigate map with mouse
                Main.screenPosition = _cameraPosition;
                if(Main.mouseLeft) {
                    _doTranslateToFocus = false; // disables 'FocusOnPlayer' transition
                    _cameraPosition += _oldMouseWorld - Main.MouseWorld;
                } else {
                    _oldMouseWorld = Main.MouseWorld;
                }
                if(_focusOnPlayer.JustPressed) {
                    _lerp=0f;
                    _doTranslateToFocus = true;
                }

                // smooth transition to player position when 'FocusOnPlayer' action
                if(_doTranslateToFocus) {
                    _cameraPosition = LerpTranslate(_cameraPosition, _playerPos,_lerp);
                }
                if(_lerp >= 1f) {
                    _lerp = 1f;
                    _doTranslateToFocus = false;
                }

            } else {

                _lerp = 0f;

                // trigger to restore current zoom of gaming camera
                if(!_shouldSaveCamera) {
                    Main.hideUI = false;
                    Main.GameZoomTarget = _oldGameZoom;
                    _shouldSaveCamera = true;

                } else _oldGameZoom = Main.GameZoomTarget;

                // reset zoom on map
                _mapZoom = 1f;

                // back camera position to player one
                _cameraPosition = _playerPos;
            }
        }
    }
    public class PlayerTools:ModPlayer {

        // lock player's ability to use any items from the inventory
        public override bool PreItemCheck() {
            if(CameraTools._isMapOpenned) {
                return false;

            } else return base.PreItemCheck();
        }
    }
}