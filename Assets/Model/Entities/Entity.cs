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
        }

        public void Attack(Entity entity) {
            int damage = baseDamage;
            if (traits.Has(EntityTrait.DoubleDamage)) {
                damage *= 2;
            }
            entity.hp.Item1 = Mathf.Max(0, entity.hp.Item1 - damage);
            if (entity.hp.Item1 == 0) {
                entity.destroyed = true;
                if (type == EntityType.Player && entity.type == EntityType.Enemy) {
                    Player player = (Player)this;
                    Enemy enemy = (Enemy)entity;
                    player.GainXP(5);
                }
            }
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
    
    public enum MoveResult {
        NoMove, Move, Attack
    }

    public enum EntityTrait {
        DoubleDamage, Flying, Phasing, UpVision
    }
}
