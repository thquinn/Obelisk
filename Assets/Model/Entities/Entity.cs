using Assets.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Coor = System.Tuple<int, int>;

namespace Assets.Model {
    public abstract class Entity {
        public Tile tile;
        public EntityType type;
        public ValueTuple<int, int> hp, mp;
        public int baseDamage;
        public bool destroyed;
        public EntityTraits traits;
        public Entity animationTarget;

        public Entity(Tile tile) {
            this.tile = tile;
            destroyed = false;
            traits = new EntityTraits();
        }

        public MoveResult TryMove(int dx, int dy) {
            int newX = tile.x + dx;
            int newY = tile.y + dy;
            if (newX < 0 || newX >= tile.floor.Width() || newY < 0 || newY >= tile.floor.Height()) {
                return MoveResult.NoMove;
            }
            Tile newTile = tile.floor.tiles[newX, newY];
            if (!tile.floor.CanPassBetween(new Coor(tile.x, tile.y), new Coor(newTile.x, newTile.y), this)) {
                return MoveResult.NoMove;
            }
            if (newTile.ContainsBlockingEntity()) {
                Entity blockingEntity = newTile.GetBlockingEntity();
                return type == blockingEntity.type ? MoveResult.NoMove : MoveResult.Attack;
            }
            MoveTo(newTile);
            return MoveResult.Move;
        }
        public void MoveTo(Tile newTile) {
            tile.entities.Remove(this);
            tile = newTile;
            tile.entities.Add(this);
            foreach (Entity e in tile.entities) {
                if (e != this) {
                    e.OnEnterMyTile(this);
                }
            }
        }

        public void Attack(Entity entity) {
            Attack(entity, CalculateDamage(entity));
        }
        public void Attack(Entity entity, int damage) {
            if (traits.Has(EntityTrait.TripleDamage)) {
                damage *= 3;
            } else if (traits.Has(EntityTrait.DoubleDamage)) {
                damage *= 2;
            }
            if (entity.traits.Has(EntityTrait.Invulnerable)) {
                damage = 0;
            }
            entity.hp.Item1 = Mathf.Max(0, entity.hp.Item1 - damage);
            // Mana burn.
            if (type == EntityType.Enemy && entity.type == EntityType.Player) {
                Enemy enemy = (Enemy)this;
                Player player = (Player)entity;
                if (enemy.traits.Has(EntityTrait.ManaBurn)) {
                    player.mp.Item1 = Mathf.Max(0, player.mp.Item1 - 3);
                }
            }
            // Destroy effects.
            if (entity.hp.Item1 == 0) {
                entity.destroyed = true;
                if (type == EntityType.Player && entity.type == EntityType.Enemy) {
                    Player player = (Player)this;
                    Enemy enemy = (Enemy)entity;
                    player.OnKill(enemy);
                }
            }
            // Animation.
            if ((type == EntityType.Player || entity.type == EntityType.Player) &&
                (type == EntityType.Enemy || entity.type == EntityType.Enemy)) {
                animationTarget = entity;
            }
            // Sound.
            if (damage > 0) {
                SFXScript.SFXHit();
            }
        }
        public virtual int CalculateDamage(Entity target) {
            return baseDamage;
        }

        public void Heal(int amount) {
            hp.Item1 = Mathf.Min(hp.Item1 + amount, hp.Item2);
        }
        public void GainMP(int amount) {
            mp.Item1 = Mathf.Min(mp.Item1 + amount, mp.Item2);
        }
        public void ChangeMaxHP(int amount) {
            float percentage = hp.Item1 / (float)hp.Item2;
            int newMax = Mathf.Max(1, hp.Item2 + amount);
            hp = new ValueTuple<int, int>(Mathf.RoundToInt(percentage * newMax), newMax);
        }

        public virtual void OnTurnEnd() {
            traits.Decrement();
        }
        public virtual void OnEnterMyTile(Entity other) { }

        public virtual Coor GetMove() {
            return null;
        }
        public virtual bool IsBlocking() {
            return true;
        }
        public virtual bool ShowShadow() {
            return tile.type != TileType.Exit;
        }
    }

    public enum EntityType {
        Player, Enemy, Trap
    }
    
    public enum MoveResult {
        NoMove, Move, Attack
    }

    public enum EntityTrait {
        DoubleDamage, DoubleMove, ExtraPlayerMove, Flying, Invulnerable, ManaBurn, Phasing, Radiant, TripleDamage, UpVision
    }
}
