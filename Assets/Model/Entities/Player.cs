using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Model.Entities {
    public class Player : Entity {
        public Skill[] skills;

        public Player(Tile tile) : base(tile) {
            type = EntityType.Player;
            hp = new ValueTuple<int, int>(100, 100);
            mp = new ValueTuple<int, int>(20, 20);
            baseDamage = 1;
            skills = new Skill[4];
            skills[0] = new Skill(this, SkillType.Wait);
        }
    }
}
