using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coor = System.Tuple<int, int>;

namespace Assets.Model.Entities {
    class MeleeEnemy : Entity {
        public MeleeEnemy(Tile tile) : base(tile) {
            type = EntityType.Enemy;
        }

        public override Coor GetMove() {
            // Find the player.
            Player player = null;
            foreach (Tile t in tile.floor.tiles) {
                foreach (Entity e in t.entities) {
                    if (e.type == EntityType.Player) {
                        player = (Player)e;
                        break;
                    }
                }
                if (player != null) {
                    break;
                }
            }
            if (player == null) {
                return null;
            }
            // Move towards the player.
            List<Coor> path = Util.FindPath(tile.floor, tile.Coor(), player.tile.Coor(), this);
            if (path == null || path.Count == 1) {
                return null;
            }
            return path[1];
        }
    }
}
