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
            new Tuple<SkillType, float>(SkillType.Phase, 100),
        };

        public Skill[] skills;
        int level;
        public ValueTuple<float, float> xp;
        HashSet<SkillType> skillsToLearn;
        public SkillType replacementSkill;

        public Player(Tile tile) : base(tile) {
            type = EntityType.Player;
            hp = new ValueTuple<int, int>(100, 100);
            mp = new ValueTuple<int, int>(20, 20);
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

        public override void OnTurnEnd() {
            base.OnTurnEnd();
            foreach (Skill skill in skills) {
                if (skill != null) {
                    skill.DecrementCooldown();
                }
            }
            xp.Item1 = Mathf.Max(0, xp.Item1 - .01f);
        }

        public void GainXP(float gain) {
            xp.Item1 += gain;
            if (xp.Item1 >= xp.Item2) {
                level++;
                xp.Item1 -= xp.Item2;
                xp.Item2 *= 1.5f;
                LearnSkill();
            }
        }
        void LearnSkill() {
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
                    return;
                }
            }
            replacementSkill = toLearn;
        }
    }
}
