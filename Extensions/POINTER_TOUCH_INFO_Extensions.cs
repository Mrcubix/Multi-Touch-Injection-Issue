using System.Drawing;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.Pointer;

namespace MultiTouch.Core.Extensions;

public static class POINTER_TOUCH_INFO_Extensions
{
    internal static void Move(this POINTER_TOUCH_INFO touchInfo, int x, int y)
    {
        touchInfo.pointerInfo.ptPixelLocation = new Point(x, y);
        touchInfo.pointerInfo.ptPixelLocationRaw = touchInfo.pointerInfo.ptPixelLocation;

        touchInfo.rcContact = RECT.FromXYWH(x, y, 2, 2);
        touchInfo.rcContactRaw = touchInfo.rcContact;
    }

    internal static void SetPointerFlags(this POINTER_TOUCH_INFO touchInfo, POINTER_FLAGS flags)
    {
        touchInfo.pointerInfo.pointerFlags |= flags;
    }

    internal static void UnsetPointerFlags(this POINTER_TOUCH_INFO touchInfo, POINTER_FLAGS flags)
    {
        touchInfo.pointerInfo.pointerFlags &= ~flags;
    }
}