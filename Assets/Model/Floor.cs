using Assets.Model.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Coor = System.Tuple<int, int>;

namespace Assets.Model {
    public class Floor {
        static int FLOORGEN_IDEAL_ENTRANCE_TO_EXIT_DISTANCE = 12;

        public int number;
        public Tile[,] tiles;
        public HashSet<Coor> wallsRight, wallsBelow;

        public Floor(int n) {
            this.number = n;
            tiles = new Tile[7, 7];
            int[] shuffled = Enumerable.Range(0, tiles.Length).ToArray().Shuffle();
            int exitPosition = shuffled[0];
            int playerPosition = n == 0 ? shuffled[1] : -1;
            int enemyPosition = shuffled[2];
            int enemyPosition2 = shuffled[3];
            // Place tiles, exit, and player.
            for (int x = 0; x < Width(); x++) {
                for (int y = 0; y < Height(); y++) {
                    int position = y * tiles.GetLength(0) + x;
                    Tile tile = new Tile(this, x, y, position == exitPosition ? TileType.Exit : TileType.Floor);
                    tiles[x, y] = tile;
                    if (position == playerPosition) {
                        tile.entities.Add(new Player(tile));
                    }
                    if (position == enemyPosition || position == enemyPosition2) {
                        tile.entities.Add(new MeleeEnemy(tile));
                    }
                }
            }
            // Place walls.
            wallsRight = new HashSet<Coor>();
            wallsBelow = new HashSet<Coor>();
            if (n > 0) {
                for (int i = 0; i < 20; i++) {
                    bool horizontal = Random.value < .5f;
                    Coor wallCoor = new Coor(Random.Range(0, Width() - (horizontal ? 0 : 1)), Random.Range(0, Height() - (horizontal ? 1 : 0)));
                    (horizontal ? wallsBelow : wallsRight).Add(wallCoor);
                }
            }
        }
        public static Floor Generate(int number, Floor previous, int attempts) {
            Floor bestFloor = null;
            float bestScore = float.MinValue;
            for (int i = 0; i < 100; i++) {
                Floor floor = new Floor(number);
                float score = floor.ScoreSuitability(previous);
                if (score >= bestScore) {
                    bestFloor = floor;
                    bestScore = score;
                }
            }
            return bestFloor;
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
        public bool CanPassBetween(Coor one, Coor two, Entity entity) {
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
            Tile destination = tiles[two.Item1, two.Item2];
            return destination.IsPassable(entity);
        }

        public void CleanupDestroyed() {
            foreach (Tile tile in tiles) {
                tile.CleanupDestroyed();
            }
        }

        public float ScoreSuitability(Floor previous) {
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
            return score;
        }
        private bool IsExitInCorner() {
            return tiles[0, 0].type == TileType.Exit || tiles[Width() - 1, 0].type == TileType.Exit || tiles[0, Height() - 1].type == TileType.Exit || tiles[Width() - 1, Height() - 1].type == TileType.Exit;
        }
    }
}
