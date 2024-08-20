using System.Drawing;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.Input.Pointer;

namespace MultiTouch.Core.Extensions;

internal static class POINTER_TYPE_INFO_Extensions
{
    internal static void Move(this ref POINTER_TYPE_INFO pointer, int x, int y)
    {
        //var touchInfo = pointer.Anonymous.touchInfo;

        pointer.Anonymous.touchInfo.pointerInfo.ptPixelLocation = new Point(x, y);
        pointer.Anonymous.touchInfo.pointerInfo.ptPixelLocationRaw = pointer.Anonymous.touchInfo.pointerInfo.ptPixelLocation;

        pointer.Anonymous.touchInfo.rcContact = RECT.FromXYWH(x, y, 2, 2);
        pointer.Anonymous.touchInfo.rcContactRaw = pointer.Anonymous.touchInfo.rcContact;
    }

    internal static void SetPointerFlags(this ref POINTER_TYPE_INFO pointer, POINTER_FLAGS flags)
    {
        pointer.Anonymous.touchInfo.pointerInfo.pointerFlags |= flags;
    }

    internal static void UnsetPointerFlags(this ref POINTER_TYPE_INFO pointer, POINTER_FLAGS flags)
    {
        pointer.Anonymous.touchInfo.pointerInfo.pointerFlags &= ~flags;
    }
}