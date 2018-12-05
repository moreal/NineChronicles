using System.Collections;
using UnityEngine;


namespace Nekoyume.UI
{
    public class MainCanvas : MonoBehaviour
    {
        private void Awake()
        {
            GameObject hudContainer = new GameObject("HUD");
            hudContainer.transform.parent = transform;
            GameObject widgetContainer = new GameObject("Widget");
            widgetContainer.transform.parent = transform;
            GameObject popupContainer = new GameObject("Popup");
            popupContainer.transform.parent = transform;
        }

        private void Start()
        {
            Widget.Create<Login>(true);
            Widget.Create<Status>();
            Widget.Create<Inventory>();
            Widget.Create<Move>();
            Widget.Create<Blind>();

#if DEBUG
            Widget.Create<Neko>(true);
#endif
        }
    }
}
