using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Entities {
    public class Trap : Entity {
        public Trap(Tile tile) : base(tile) {
            type = EntityType.Trap;
            baseDamage = 5;
        }

        public override bool IsBlocking() {
            return false;
        }
        public override void OnEnterMyTile(Entity other) {
            if (other.traits.Has(EntityTrait.Flying)) {
                return;
            }
            Attack(other);
        }
        public override int CalculateDamage(Entity target) {
            return target.type == EntityType.Player ? 5 : 1;
        }
        public override bool ShowShadow() {
            return false;
        }
    }
}
