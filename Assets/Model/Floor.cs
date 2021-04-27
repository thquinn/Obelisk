using Assets.Model.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Coor = System.Tuple<int, int>;

namespace Assets.Model {
    public class Floor {
        static int FLOORGEN_IDEAL_ENTRANCE_TO_EXIT_DISTANCE = 12;
        static float FLOORGEN_ENEMY_PROGRESSION_RATE = .6f;
        static float FLOORGEN_ENEMY_PROGRESSION_OFFSET = 1;
        static int FIRST_TRAP_FLOOR = 5;

        public int number;
        public Tile[,] tiles;
        public HashSet<Coor> wallsRight, wallsBelow;
        public Floor previous;
        public Coor entrance;
        public bool playerOnFloor;
        float xpMargin;

        public Floor(int n, Floor previous, Player player) {
            this.number = n;
            this.previous = previous;
            if (previous != null) {
                entrance = previous.FindExit();
            }
            tiles = new Tile[7, 7];
            wallsRight = new HashSet<Coor>();
            wallsBelow = new HashSet<Coor>();
            if (n == 0) {
                playerOnFloor = true;
                GenFirstLevel();
                return;
            }
            List<int> shuffled = Enumerable.Range(0, tiles.Length).ToList();
            shuffled.RemoveAt(entrance.Item2 * Width() + entrance.Item1);
            shuffled.Shuffle();
            int exitPosition = shuffled[0];
            // Place tiles and exit.
            for (int x = 0; x < Width(); x++) {
                for (int y = 0; y < Height(); y++) {
                    int position = y * Width() + x;
                    tiles[x, y] = new Tile(this, x, y, position == exitPosition ? TileType.Exit : TileType.Floor);
                }
            }
            int tileIndex = 1;
            // Place enemies.
            float modifiedFloor = n * FLOORGEN_ENEMY_PROGRESSION_RATE + FLOORGEN_ENEMY_PROGRESSION_OFFSET;
            float xpBudget = (modifiedFloor - 1) * (modifiedFloor + 6) / 4; // {0, 2, 4.5, 7.5, 11, 15...}
            UnityEngine.Debug.Log("XP Budget for floor " + n + " is " + xpBudget);
            float initialXPBudget = xpBudget;
            float expectedEnemyCount = n < 4 ? 1 : Mathf.Pow(xpBudget, .25f) * 1.2f;
            int attempts = 0;
            Enemy lastEnemy = null;
            while (xpBudget > initialXPBudget * .1f && xpBudget >= 2 && attempts++ < 100) {
                int position = shuffled[tileIndex];
                Tile tile = tiles[position % Width(), position / Width()];
                Enemy enemy;
                float desiredXPValue = initialXPBudget / expectedEnemyCount * UnityEngine.Random.Range(.8f, 1.25f);
                if (lastEnemy != null && Random.value > .5f) {
                    enemy = new Enemy(tile, lastEnemy);
                } else {
                    enemy = new Enemy(tile, desiredXPValue, player, n < 4);
                }
                System.Diagnostics.Debug.Assert(enemy.xpValue > 0, "Uncalculated enemy XP value.");
                float xpRatio = enemy.xpValue / desiredXPValue;
                if (xpRatio > 1.2f) {
                    continue;
                }
                tile.entities.Add(enemy);
                xpBudget -= enemy.xpValue;
                tileIndex++;
            }
            xpMargin = initialXPBudget == 0 ? 0 : Mathf.Abs(xpBudget / initialXPBudget);
            // Place walls.
            if (n > 0) {
                for (int i = 0; i < 20; i++) {
                    bool horizontal = Random.value < .5f;
                    Coor wallCoor = new Coor(Random.Range(0, Width() - (horizontal ? 0 : 1)), Random.Range(0, Height() - (horizontal ? 1 : 0)));
                    (horizontal ? wallsBelow : wallsRight).Add(wallCoor);
                }
            }
            // Place traps.
            if (n >= FIRST_TRAP_FLOOR) {
                int numSpikes = n == FIRST_TRAP_FLOOR ? 1 : UnityEngine.Random.Range(1, 4);
                for (int i = 0; i < numSpikes; i++) {
                    int position = shuffled[tileIndex++];
                    Tile tile = tiles[position % Width(), position / Width()];
                    tile.entities.Add(new Trap(tile));
                }
            }
        }
        public static Floor Generate(int number, Floor previous, int attempts, Player player) {
            Floor bestFloor = null;
            float bestScore = float.MinValue;
            for (int i = 0; i < 100; i++) {
                Floor floor = new Floor(number, previous, player);
                float score = floor.ScoreSuitability();
                if (score >= bestScore) {
                    bestFloor = floor;
                    bestScore = score;
                }
            }
            return bestFloor;
        }
        void GenFirstLevel() {
            for (int x = 0; x < Width(); x++) {
                for (int y = 0; y < Height(); y++) {
                    TileType type;
                    if (x == 3) type = y == 1 ? TileType.Exit : TileType.Floor;
                    else if (x == 2 || x == 4) type = (y <= 2 || y == 5) ? TileType.Floor : TileType.InvisibleWall;
                    else type = TileType.InvisibleWall;
                    tiles[x, y] = new Tile(this, x, y, type);
                    if (x == 3 && y == 5) tiles[x, y].entities.Add(new Player(tiles[x, y]));
                }
            }
        }

        public int Width() {
            return tiles.GetLength(0);
        }
        public int Height() {
            return tiles.GetLength(1);
        }
        public Coor FindExit() {
            for (int x = 0; x < Width(); x++) {
                for (int y = 0; y < Height(); y++) {
                    if (tiles[x, y].type == TileType.Exit) {
                        return new Coor(x, y);
                    }
                }
            }
            return null;
        }
        public Player FindPlayer() {
            foreach (Tile t in tiles) {
                foreach (Entity e in t.entities) {
                    if (e.type == EntityType.Player) {
                        return (Player)e;
                    }
                }
            }
            return null;
        }
        public bool CanPassBetween(Coor one, Coor two, Entity entity) {
            Tile destination = tiles[two.Item1, two.Item2];
            if (!destination.IsPassable(entity)) {
                return false;
            }
            if (entity.traits.Has(EntityTrait.Phasing)) {
                return true;
            }
            if (one.Item1 != two.Item1) {
                // check for vertical walls
                int x = Mathf.Min(one.Item1, two.Item1);
                if (wallsRight.Contains(new Coor(x, one.Item2))) {
                    return false;
                }
            } else if (one.Item2 != two.Item2) {
                // check for horizontal walls
                int y = Mathf.Min(one.Item2, two.Item2);
                if (wallsBelow.Contains(new Coor(one.Item1, y))) {
                    return false;
                }
            }
            return true;
        }

        public void CleanupDestroyed() {
            foreach (Tile tile in tiles) {
                tile.CleanupDestroyed();
            }
        }

        public float ScoreSuitability() {
            Coor entrance = previous.FindExit();
            Coor exit = FindExit();
            List<Coor> path = Util.FindPath(this, entrance, exit, new Player(tiles[entrance.Item1, entrance.Item2])); // pass a new Player since player upgrades shouldn't affect mapgen
            if (path == null) { // There is no path to the exit.
                return float.MinValue;
            }
            float score = -Mathf.Abs(path.Count - FLOORGEN_IDEAL_ENTRANCE_TO_EXIT_DISTANCE);
            if (path.Count == 1) { // The exit is directly below the entrance.
                score -= 1000;
            }
            if (IsExitInCorner()) {
                score -= 4;
            }
            score -= xpMargin * 4;
            return score;
        }
        private bool IsExitInCorner() {
            return tiles[0, 0].type == TileType.Exit || tiles[Width() - 1, 0].type == TileType.Exit || tiles[0, Height() - 1].type == TileType.Exit || tiles[Width() - 1, Height() - 1].type == TileType.Exit;
        }
    }
}
