using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Model.Entities {
    public class Player : Entity {
        Tuple<SkillType, float>[] SKILL_LEVEL_WEIGHTS = new Tuple<SkillType, float>[] {
            new Tuple<SkillType, float>(SkillType.Wait, 100),
            new Tuple<SkillType, float>(SkillType.Empower, 100),
            new Tuple<SkillType, float>(SkillType.MaxHP20, 100),
            new Tuple<SkillType, float>(SkillType.Shield, 100),
            new Tuple<SkillType, float>(SkillType.Regeneration, 100),
            new Tuple<SkillType, float>(SkillType.Leech, 100),
            new Tuple<SkillType, float>(SkillType.MaxHP40, 100),
            new Tuple<SkillType, float>(SkillType.FastForward, 100),
            new Tuple<SkillType, float>(SkillType.Autophage, 100),
            new Tuple<SkillType, float>(SkillType.Phase, 100),
            new Tuple<SkillType, float>(SkillType.Quicken, 100),
        };
        public static int LINGER_TURNS = 35;

        public Skill[] skills;
        int level;
        public ValueTuple<float, float> xp;
        HashSet<SkillType> skillsToLearn;
        public SkillType replacementSkill;
        public int turnsOnFloor;

        public Player(Tile tile) : base(tile) {
            type = EntityType.Player;
            hp = new ValueTuple<int, int>(100, 100);
            mp = new ValueTuple<int, int>(18, 18);
            baseDamage = 1;
            skills = new Skill[4];
            level = 0;
            xp = new ValueTuple<float, float>(0, 10);
            replacementSkill = SkillType.None;
            SetSkillsToLearn();
        }
        void SetSkillsToLearn() {
            skillsToLearn = new HashSet<SkillType>(SKILL_LEVEL_WEIGHTS.Select(t => t.Item1));
        }

        bool HasSkill(SkillType type) {
            foreach (Skill skill in skills) {
                if (skill != null && skill.type == type) {
                    return true;
                }
            }
            return false;
        }
        public bool HasMPSkills() {
            foreach (Skill skill in skills) {
                if (skill != null && Skill.COSTS.ContainsKey(skill.type)) {
                    return true;
                }
            }
            return false;
        }

        public override void OnTurnEnd() {
            base.OnTurnEnd();
            turnsOnFloor++;
            foreach (Skill skill in skills) {
                if (skill != null) {
                    skill.DecrementCooldown();
                }
            }
            if (turnsOnFloor > LINGER_TURNS) {
                xp.Item1 -= xp.Item2 * .01f;
            }
        }
        public void OnKill(Enemy enemy) {
            GainXP(enemy.xpValue);
            if (HasSkill(SkillType.Leech)) {
                GainMP(2);
            }
        }
        public void OnNewFloor() {
            turnsOnFloor = 0;
            int healAmount = Util.RandRound(hp.Item2 / 20f);
            if (HasSkill(SkillType.Regeneration)) {
                healAmount += 5;
            }
            Heal(healAmount);
            GainMP(Util.RandRound(mp.Item2 / 7f));
        }
        public void OnLearnSkill(Skill skill) {
            if (skill.type == SkillType.MaxHP20) {
                ChangeMaxHP(20);
            } else if (skill.type == SkillType.MaxHP40) {
                ChangeMaxHP(40);
            }
        }
        public void OnLoseSkill(Skill skill) {
            if (skill.type == SkillType.MaxHP20) {
                ChangeMaxHP(-20);
            } else if (skill.type == SkillType.MaxHP40) {
                ChangeMaxHP(-40);
            }
        }

        public void GainXP(float gain) {
            xp.Item1 += gain;
            if (xp.Item1 >= xp.Item2) {
                level++;
                ChangeMaxHP(5);
                ChangeMaxMP(2);
                xp.Item1 -= xp.Item2;
                xp.Item2 *= 1.5f;
                LearnSkill();
            }
        }
        public void LearnSkill() {
            float selector = Util.GetRandomAbsStandardDeviations() * level * 100;
            SkillType toLearn = SkillType.None;
            foreach (var t in SKILL_LEVEL_WEIGHTS) {
                SkillType type = t.Item1;
                if (!skillsToLearn.Contains(type)) {
                    continue;
                }
                if (selector > t.Item2) {
                    selector -= t.Item2;
                    continue;
                }
                toLearn = type;
                break;
            }
            if (toLearn == SkillType.None) {
                for (int i = SKILL_LEVEL_WEIGHTS.Length - 1; i >= 0; i--) {
                    if (skillsToLearn.Contains(SKILL_LEVEL_WEIGHTS[i].Item1)) {
                        toLearn = SKILL_LEVEL_WEIGHTS[i].Item1;
                        break;
                    }
                }
            }
            System.Diagnostics.Debug.Assert(toLearn != SkillType.None);
            skillsToLearn.Remove(toLearn);
            if (skillsToLearn.Count == 0) {
                SetSkillsToLearn();
            }
            for (int i = 0; i < skills.Length; i++) {
                if (skills[i] == null) {
                    skills[i] = new Skill(this, toLearn);
                    OnLearnSkill(skills[i]);
                    return;
                }
            }
            replacementSkill = toLearn;
        }
    }
}
