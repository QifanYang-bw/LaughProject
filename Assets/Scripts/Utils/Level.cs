using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts.Core
{
    public class Level : MonoBehaviour
    {
        public enum Types {
            Tutorial,
            Game,
            // add above
            Max,
        }
        public Types Type;
        public int Index;

        public string GetSceneName()
        {
            var list = GetScenesNameByType();
            Assert.IsTrue(Index >= 0 && Index < list.Count);
            return list[Index];
        }

        private List<string> GetScenesNameByType()
        {
            switch (Type)
            {
                case Types.Tutorial:
                    return AssetHelper.instance.TutorialSceneNames;
                case Types.Game:
                    return AssetHelper.instance.GameLevelSceneNames;
                default:
                    throw new InvalidOperationException("Invalid Type");
            }
        }

        public Level NextLevel()
        {
            Level result = (Level)this.MemberwiseClone();
            if (IsLast())
                return null;

            var curList = GetScenesNameByType();
            if (Index + 1 == curList.Count)
            {
                result.Type += 1;
                result.Index = 0;
            }
            else
            {
                result.Index += 1;
            }

            return result;
        }

        public bool IsLast()
        {
            return Type + 1 != Types.Max;
        }

        public string ToKey()
        {
            return $"{Type.ToString()}_{Index}";
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
