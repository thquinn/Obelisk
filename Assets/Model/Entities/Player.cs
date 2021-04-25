using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Entities {
    public class Player : Entity {
        public Player(Tile tile) : base(tile) {
            type = EntityType.Player;
        }
    }
}
