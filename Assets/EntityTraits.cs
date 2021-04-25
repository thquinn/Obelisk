using Assets.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets {
    public class EntityTraits {
        List<ValueTuple<EntityTrait, int>> traits;

        public EntityTraits() {
            traits = new List<(EntityTrait, int)>();
        }

        public void Add(EntityTrait trait, int duration) {
            traits.Add(new ValueTuple<EntityTrait, int>(trait, duration));
        }
        public void Add(EntityTrait trait) {
            Add(trait, -1);
        }

        public bool Has(EntityTrait trait) {
            foreach (var kvp in traits) {
                if (kvp.Item1 == trait) {
                    return true;
                }
            }
            return false;
        }

        public void Decrement() {
            for (int i = traits.Count - 1; i >= 0; i--) {
                var trait = traits[i];
                Debug.Assert(trait.Item2 == -1 || trait.Item2 > 0, "Invalid trait duration value!");
                if (trait.Item2 == -1) {
                    continue;
                }
                trait.Item2--;
                if (trait.Item2 == 0) {
                    traits.RemoveAt(i);
                }
            }
        }
    }
}
