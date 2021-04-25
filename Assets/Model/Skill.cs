using Assets.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Model {
    public class Skill {
        public static Dictionary<SkillType, string> NAMES = new Dictionary<SkillType, string> {
        { SkillType.Empower, "Empower" },
        { SkillType.Phase, "Phase" },
        { SkillType.Quicken, "Quicken" },
        { SkillType.Shield, "Shield" },
        { SkillType.Wait, "Wait" },
    };
        public static Dictionary<SkillType, int> COOLDOWNS = new Dictionary<SkillType, int> {
            { SkillType.Empower, 2 },
            { SkillType.Phase, 20 },
            { SkillType.Quicken, 10 },
            { SkillType.Shield, 8 },
            { SkillType.Wait, 0 },
        };
        public static Dictionary<SkillType, int> COSTS = new Dictionary<SkillType, int> {
            { SkillType.Empower, 2 },
            { SkillType.Phase, 5 },
            { SkillType.Quicken, 7 },
            { SkillType.Shield, 4 },
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
            if (type == SkillType.Empower) {
                player.traits.Add(EntityTrait.DoubleDamage, 1);
            } else if (type == SkillType.Phase) {
                player.traits.Add(EntityTrait.Phasing, 1);
            } else if (type == SkillType.Quicken) {
                player.traits.Add(EntityTrait.ExtraPlayerMove, 2);
            } else if (type == SkillType.Shield) {
                player.traits.Add(EntityTrait.Invulnerable, 2);
            }
            return USES_TURN.Contains(type);
        }
        public void DecrementCooldown() {
            cooldown = Mathf.Max(0, cooldown - 1);
        }
    }

    public enum SkillType {
        None, Empower, Phase, Quicken, Shield, Wait
    }
}
