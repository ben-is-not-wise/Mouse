using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "Person", menuName = "Mouse/Info/Person")]
    public class Person : Item
    {
        public HackedDesign.UI.AvatarPresenter.Gender gender;

        public int face;
        public int hair;
        public string corp;

        public override InfoCategory InfoCategory => InfoCategory.Person;
    }
}
