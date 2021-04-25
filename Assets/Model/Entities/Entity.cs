using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coor = System.Tuple<int, int>;

namespace Assets.Model {
    public abstract class Entity {
        public Tile tile;
        public EntityType type;
        public bool destroyed;

        public Entity(Tile tile) {
            this.tile = tile;
            destroyed = false;
        }

        public bool TryMove(int dx, int dy) {
            int newX = tile.x + dx;
            int newY = tile.y + dy;
            if (newX < 0 || newX >= tile.floor.Width() || newY < 0 || newY >= tile.floor.Height()) {
                return false;
            }
            Tile newTile = tile.floor.tiles[newX, newY];
            if (!tile.floor.CanPassBetween(new Coor(tile.x, tile.y), new Coor(newTile.x, newTile.y), this)) {
                return false;
            }
            if (newTile.ContainsBlockingEntity()) {
                return false;
            }
            MoveTo(newTile);
            return true;
        }
        public void MoveTo(Tile newTile) {
            tile.entities.Remove(this);
            tile = newTile;
            tile.entities.Add(this);
        }

        public virtual Coor GetMove() {
            return null;
        }

        public bool IsBlocking() {
            return true;
        }
        public bool ShowShadow() {
            return tile.type != TileType.Exit;
        }
    }

    public enum EntityType {
        Player, Enemy
    }
}
