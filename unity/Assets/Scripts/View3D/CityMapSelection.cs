using System;

namespace DoenerEmpire.View3D
{
    public sealed class CityMapSelection
    {
        public event Action<CityMapHotspot> Changed;

        public CityMapHotspot Selected { get; private set; }

        public void Select(CityMapHotspot hotspot)
        {
            if (hotspot == null || Selected == hotspot)
            {
                return;
            }

            Selected?.SetSelected(false);
            Selected = hotspot;
            Selected.SetSelected(true);
            Changed?.Invoke(Selected);
        }
    }
}
