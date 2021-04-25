using Assets.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Model {
    public class Skill {
        public static Dictionary<SkillType, int> COOLDOWNS = new Dictionary<SkillType, int> {
            { SkillType.Phase, 20 },
            { SkillType.Wait, 0 },
        };
        public static Dictionary<SkillType, int> COSTS = new Dictionary<SkillType, int> {
            { SkillType.Phase, 5 },
            { SkillType.Wait, 1 },
        };
        public static HashSet<SkillType> USES_TURN = new HashSet<SkillType>() {
            SkillType.Wait,
        };

        Player player;
        public SkillType type;
        public int cooldown;

        public Skill(Player player, SkillType type) {
            this.player = player;
            this.type = type;
        }

        public bool Use() {
            if (!COOLDOWNS.ContainsKey(type)) {
                return false;
            }
            if (cooldown > 0) {
                return false;
            }
            int cost = COSTS[type];
            if (player.mp.Item1 < cost) {
                return false;
            }
            cooldown = COOLDOWNS[type];
            player.mp.Item1 -= cost;
            if (type == SkillType.Phase) {
                player.traits.Add(EntityTrait.Phasing, 1);
            }
            return USES_TURN.Contains(type);
        }
        public void DecrementCooldown() {
            cooldown = Mathf.Max(0, cooldown - 1);
        }
    }

    public enum SkillType {
        None, Phase, Wait
    }
}
