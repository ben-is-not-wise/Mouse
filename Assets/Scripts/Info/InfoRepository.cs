
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

namespace HackedDesign
{
    public class InfoRepository : AutoSingleton<InfoRepository>
    {
        public List<WeaponSettings> weapons;

        [SerializeField] private List<Person> people;
        [SerializeField] private List<Faction> factions;
        [SerializeField] private List<Corp> corps;
        public List<Faction> Factions => factions;
        public List<Corp> Corps => corps;
        public List<Person> People => people;

        public WeaponSettings GetWeaponByName(string name) => weapons.FirstOrDefault(w => w.name == name);
    }
}
