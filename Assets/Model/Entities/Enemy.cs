using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coor = System.Tuple<int, int>;

namespace Assets.Model.Entities {
    class Enemy : Entity {
        Coor wanderCoor;
        int wanderTurns;

        public Enemy(Tile tile) : base(tile) {
            type = EntityType.Enemy;
            int numHP = UnityEngine.Random.Range(1, 4);
            hp = new ValueTuple<int, int>(numHP, numHP);
            baseDamage = 10;
            if (UnityEngine.Random.value < .33f) {
                traits.Add(EntityTrait.DoubleDamage);
            }
            if (UnityEngine.Random.value < .33f) {
                traits.Add(EntityTrait.DoubleMove);
            }
            if (UnityEngine.Random.value < .33f) {
                traits.Add(EntityTrait.Flying);
            }
            if (UnityEngine.Random.value < .33f) {
                traits.Add(EntityTrait.ManaBurn);
            }
            if (UnityEngine.Random.value < .33f) {
                traits.Add(EntityTrait.Radiant);
            }
            if (UnityEngine.Random.value < .33f) {
                traits.Add(EntityTrait.UpVision);
            }
        }

        public override Coor GetMove() {
            // Find the player.
            Player player = tile.floor.FindPlayer();
            if (player == null && traits.Has(EntityTrait.UpVision) && tile.floor.previous != null) {
                player = tile.floor.previous.FindPlayer();
            }
            List<Coor> path = null;
            if (player == null) {
                // Wander around.
                if (wanderCoor == null || wanderTurns <= 0) {
                    wanderCoor = new Coor(UnityEngine.Random.Range(0, tile.floor.Width()), UnityEngine.Random.Range(0, tile.floor.Height()));
                    wanderTurns = UnityEngine.Random.Range(8, 14);
                }
                path = Util.FindPath(tile.floor, tile.Coor(), wanderCoor, this);
                wanderTurns--;
            } else {
                // Move towards the player.
                path = Util.FindPath(tile.floor, tile.Coor(), player.tile.Coor(), this);
                if (path != null && tile.floor != player.tile.floor) {
                    path.RemoveAt(path.Count - 1);
                }
            }
            if (path == null || path.Count < 2) {
                return null;
            }
            return path[1];
        }

        public override void OnTurnEnd() {
            if (traits.Has(EntityTrait.Radiant)) {
                foreach (Tile neighbor in tile.GetNeighbors()) {
                    Entity e = neighbor.GetBlockingEntity();
                    if (e != null && e.type == EntityType.Player) {
                        Attack(e, 5);
                    }
                }
            }
            base.OnTurnEnd();
        }
    }
}
