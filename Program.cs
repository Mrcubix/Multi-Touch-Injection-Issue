using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using MultiTouch.Core.Extensions;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.Pointer;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MultiTouch.Core;

[SupportedOSPlatform("windows8.0")]
class Program
{
    static void Main()
    {
        if (PInvoke.InitializeTouchInjection(1, TOUCH_FEEDBACK_MODE.TOUCH_FEEDBACK_INDIRECT) == false)
            throw new Win32Exception($"Failed to initialize touch injection: {Marshal.GetLastWin32Error()}");

        Console.WriteLine("Touch injection initialized");

        var touchInfos = BuildTouchInfo(1);
        SetTarget(touchInfos);

        if (PInvoke.InjectTouchInput(touchInfos) == false)
            throw new Win32Exception($"Failed to inject touch input: {Marshal.GetLastWin32Error()}");

        StartTouch(touchInfos);
        SetOrigin(touchInfos);
        StartMovingUp(touchInfos, 100);
        ReleaseTouch(touchInfos);
    }

    static POINTER_TOUCH_INFO[] BuildTouchInfo(int touchCount)
    {
        var pointers = new POINTER_TOUCH_INFO[touchCount];

        for (uint i = 0; i < touchCount; i++)
        {
            var info = new POINTER_INFO
            {
                pointerType = POINTER_INPUT_TYPE.PT_TOUCH,
                pointerId = i + 1,
                pointerFlags = POINTER_FLAGS.POINTER_FLAG_UP,
                ptPixelLocationRaw = new Point(0, 0),
                ptHimetricLocationRaw = new Point(0, 0),
            };

            pointers[i] = new POINTER_TOUCH_INFO
            {
                pointerInfo = info,
                touchFlags = PInvoke.TOUCH_FLAG_NONE,
                touchMask = PInvoke.TOUCH_MASK_PRESSURE, // The device i plan to use it on does not provide contact area information
                pressure = 512,
                rcContact = RECT.FromXYWH(0, 0, 2, 2),
                rcContactRaw = RECT.FromXYWH(0, 0, 2, 2),
            };
        }

        return pointers;
    }

    static void SetTarget(POINTER_TOUCH_INFO[] touchInfos)
    {
        for (int i = 0; i < touchInfos.Length; i++)
            touchInfos[i].pointerInfo.hwndTarget = PInvoke.GetForegroundWindow();
    }

    static void StartTouch(POINTER_TOUCH_INFO[] touchInfos)
    {
        // A finger is down
        touchInfos[0].UnsetPointerFlags(POINTER_FLAGS.POINTER_FLAG_UP | POINTER_FLAGS.POINTER_FLAG_UPDATE);
        touchInfos[0].SetPointerFlags(POINTER_FLAGS.POINTER_FLAG_INRANGE | POINTER_FLAGS.POINTER_FLAG_INCONTACT | POINTER_FLAGS.POINTER_FLAG_DOWN);

        if (PInvoke.InjectTouchInput(touchInfos) == false)
            throw new Win32Exception($"Failed to inject down touch input: {Marshal.GetLastWin32Error()}");
    }

    static void SetOrigin(POINTER_TOUCH_INFO[] touchInfos)
    {
        // Since the cursor will be moving, this will be an update
        touchInfos[0].SetPointerFlags(POINTER_FLAGS.POINTER_FLAG_UPDATE);
        // 2 displays, this will set it to moveo n the middle of my second display
        touchInfos[0].Move(957, 1077);

        if (PInvoke.InjectTouchInput(touchInfos) == false)
            throw new Win32Exception($"Failed to inject set original position touch input: {Marshal.GetLastWin32Error()}");
    }

    static void StartMovingUp(POINTER_TOUCH_INFO[] touchInfos, int pixels)
    {
        var y = 1077;
        var stepPixels = pixels / 10;

        // Move 10 pixels up every 100ms (from 1080 to 1080 - pixels) in 10 steps
        for (int i = 0; i < 10; i++)
        {
            Thread.Sleep(100);

            y -= stepPixels;

            touchInfos[0].Move(957, y);

            if (PInvoke.InjectTouchInput(touchInfos) == false)
                throw new Win32Exception($"Failed to inject move touch input: {Marshal.GetLastWin32Error()}");
        }
    }

    static void ReleaseTouch(POINTER_TOUCH_INFO[] touchInfos)
    {
        // A finger is up
        touchInfos[0].UnsetPointerFlags(POINTER_FLAGS.POINTER_FLAG_INRANGE | POINTER_FLAGS.POINTER_FLAG_INCONTACT | POINTER_FLAGS.POINTER_FLAG_DOWN);
        touchInfos[0].SetPointerFlags(POINTER_FLAGS.POINTER_FLAG_UP);

        if (PInvoke.InjectTouchInput(touchInfos) == false)
            throw new Win32Exception($"Failed to inject up touch input: {Marshal.GetLastWin32Error()}");
    }
}
