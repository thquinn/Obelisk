using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coor = System.Tuple<int, int>;

namespace Assets.Model {
    public class Tile {
        public Floor floor;
        public int x, y;
        public TileType type;
        public List<Entity> entities;

        public Tile(Floor floor, int x, int y, TileType type) {
            this.floor = floor;
            this.x = x;
            this.y = y;
            this.type = type;
            entities = new List<Entity>();
        }

        public bool IsPassable(Entity entity) {
            switch (type) {
                case TileType.Floor:
                    return true;
                case TileType.Exit:
                    return entity.type == EntityType.Player;
            }
            throw new Exception("Unhandled passability case.");
        }
        public Entity GetBlockingEntity() {
            foreach (Entity e in entities) {
                if (e.IsBlocking()) {
                    return e;
                }
            }
            return null;
        }
        public bool ContainsBlockingEntity() {
            return GetBlockingEntity() != null;
        }
        public List<Tile> GetNeighbors() {
            List<Tile> neighbors = new List<Tile>();
            if (x > 0) {
                neighbors.Add(floor.tiles[x - 1, y]);
            }
            if (x < floor.Width() - 1) {
                neighbors.Add(floor.tiles[x + 1, y]);
            }
            if (y > 0) {
                neighbors.Add(floor.tiles[x, y - 1]);
            }
            if (y < floor.Height() - 1) {
                neighbors.Add(floor.tiles[x, y + 1]);
            }
            return neighbors;
        }
        public Tile GetDelta(int dx, int dy) {
            return floor.tiles[x + dx, y + dy];
        }

        public Coor Coor() {
            return new Coor(x, y);
        }
    }

    public enum TileType {
        Floor, Exit
    }
}
