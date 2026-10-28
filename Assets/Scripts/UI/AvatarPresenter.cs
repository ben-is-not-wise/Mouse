using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace HackedDesign.UI
{
    public class AvatarPresenter : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private UnityEngine.UI.Image faceImage;
        [SerializeField] private UnityEngine.UI.Image hairImage;

        [SerializeField] private Gender currentGender;
        [SerializeField] private int currentFace;
        [SerializeField] private int currentHair;

        [Header("Parts")]
        [SerializeField] private List<Sprite> femaleFaces;
        [SerializeField] private List<Sprite> maleFaces;
        [SerializeField] private List<Sprite> femaleHair;
        [SerializeField] private List<Sprite> maleHair;

        void Start()
        {
            //SetByName("Mouse PD");
        }

        public void SetByName(string name)
        {
            var personRecord = InfoRepository.Instance.People.FirstOrDefault(person => person.name == name);

            if (personRecord.OrNull() != null)
            {
                Set(personRecord.gender, personRecord.face, personRecord.hair);
            }
        }

        public void Set(Gender gender, int faceIdx, int hairIdx)
        {
            currentGender = gender;
            currentFace = faceIdx;
            currentHair = hairIdx;

            faceImage.sprite = gender == Gender.Male ? CheckSprite(maleFaces, faceIdx) : CheckSprite(femaleFaces, faceIdx);
            hairImage.sprite = gender == Gender.Male ? CheckSprite(maleHair, hairIdx) : CheckSprite(femaleHair, hairIdx);
        }

        public void Refresh() => Set(this.currentGender, this.currentFace, this.currentHair);

        private Sprite CheckSprite(List<Sprite> spriteList, int idx) => idx < 0 || spriteList.Count <= idx ? null : spriteList[idx];

        public enum Gender
        {
            Male,
            Female,
            Unknown
        };
    }
}
