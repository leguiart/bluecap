using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.PlayVis
{
    public class BoardPiece : MonoBehaviour
    {

        public SpriteRenderer sprite;

        [HideInInspector]
        public int x;
        [HideInInspector]
        public int y;

        public void OnMouseEnter()
        {

        }

        public void OnMouseExit()
        {

        }

        public void OnMouseDown()
        {
            BoardManager.instance.TapTile(x, y);
        }


    }
}
