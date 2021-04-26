using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coor = System.Tuple<int, int>;

namespace Assets.Model.Entities {
    public class Enemy : Entity {
        static Dictionary<EntityTrait, float> ENEMY_TRAIT_MULTIPLIERS = new Dictionary<EntityTrait, float> {
            { EntityTrait.DoubleDamage, 2f },
            { EntityTrait.DoubleMove, 2.2f },
            { EntityTrait.Flying, 1.1f },
            { EntityTrait.ManaBurn, 1.3f },
            { EntityTrait.Radiant, 1.6f },
            { EntityTrait.TripleDamage, 3f },
            { EntityTrait.UpVision, 1.2f },
        };
        static EntityTrait[] ENEMY_TRAITS = ENEMY_TRAIT_MULTIPLIERS.Keys.ToArray();

        Coor wanderCoor;
        int wanderTurns;
        public float xpValue;

        public Enemy(Tile tile, float desiredXPValue) : base(tile) {
            type = EntityType.Enemy;
            int numHP = -1;
            int maxTraits = -1;
            HashSet<EntityTrait> validTraits = new HashSet<EntityTrait>(ENEMY_TRAIT_MULTIPLIERS.Keys);
            if (desiredXPValue <= 5) {
                numHP = UnityEngine.Random.Range(1, 3);
                maxTraits = 2;
                validTraits.Remove(EntityTrait.DoubleDamage);
                validTraits.Remove(EntityTrait.DoubleMove);
                validTraits.Remove(EntityTrait.Radiant);
                validTraits.Remove(EntityTrait.TripleDamage);
            } else if (desiredXPValue <= 15) {
                maxTraits = 3;
                numHP = UnityEngine.Random.Range(1, 4);
                validTraits.Remove(EntityTrait.DoubleMove);
                validTraits.Remove(EntityTrait.TripleDamage);
            } else if (desiredXPValue <= 100) {
                maxTraits = 4;
                numHP = UnityEngine.Random.Range(2, 5);
            } else {
                maxTraits = 6;
                numHP = UnityEngine.Random.Range(1, 10);
            }
            hp = new ValueTuple<int, int>(numHP, numHP);
            baseDamage = 10;
            CalculateXPValue();
            while (xpValue < desiredXPValue && validTraits.Count > 0 && traits.Count() < maxTraits) {
                EntityTrait trait = ENEMY_TRAITS[UnityEngine.Random.Range(0, ENEMY_TRAITS.Length)];
                if (!validTraits.Contains(trait)) {
                    continue;
                }
                traits.Add(trait);
                validTraits.Remove(trait);
                if (trait == EntityTrait.DoubleDamage) {
                    validTraits.Remove(EntityTrait.TripleDamage);
                } else if (trait == EntityTrait.TripleDamage) {
                    validTraits.Remove(EntityTrait.DoubleDamage);
                }
                CalculateXPValue();
            }
        }
        public Enemy(Tile tile, Enemy other) : base(tile) {
            type = EntityType.Enemy;
            hp = new ValueTuple<int, int>(other.hp.Item1, other.hp.Item2);
            baseDamage = 10;
            foreach (EntityTrait trait in other.traits) {
                traits.Add(trait);
            }
            CalculateXPValue();
        }
        void CalculateXPValue() {
            xpValue = hp.Item2 * 3 - 1;
            foreach (EntityTrait trait in traits) {
                if (ENEMY_TRAIT_MULTIPLIERS.ContainsKey(trait)) {
                    xpValue *= ENEMY_TRAIT_MULTIPLIERS[trait];
                }
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
