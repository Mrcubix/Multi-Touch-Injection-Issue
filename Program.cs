using System.ComponentModel;
using System.Runtime.Versioning;
using MultiTouch.Core.Extensions;
using Windows.Win32;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.Input.Pointer;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MultiTouch.Core;

[SupportedOSPlatform("windows10.0.17763")]
unsafe class Program
{
    static void Main()
    {
        var device = PInvoke.CreateSyntheticPointerDevice(POINTER_INPUT_TYPE.PT_TOUCH, 1, POINTER_FEEDBACK_MODE.POINTER_FEEDBACK_INDIRECT);

        if (device.IsNull)
            throw new Win32Exception("Failed to create synthetic pointer device");
            
        Console.WriteLine("Synthetic Device Initialized");

        var pointers = BuildTouchInfo(1);
        SetTarget(pointers);

        fixed (POINTER_TYPE_INFO* pPointers = pointers)
            if (PInvoke.InjectSyntheticPointerInput(device, pPointers, (uint)pointers.Length) == false)
                throw new Win32Exception("Failed to inject synthetic pointer input");

        StartTouch(device, pointers);
        SetOrigin(device, pointers);
        StartMovingUp(device, pointers, 500);

        Thread.Sleep(5000);

        ReleaseTouch(device, pointers);

        PInvoke.DestroySyntheticPointerDevice(device);
    }

    static POINTER_TYPE_INFO[] BuildTouchInfo(int touchCount)
    {
        var pointers = new POINTER_TYPE_INFO[touchCount];

        for (uint i = 0; i < touchCount; i++)
        {
            var info = new POINTER_INFO
            {
                pointerType = POINTER_INPUT_TYPE.PT_TOUCH,
                pointerId = i,
                pointerFlags = POINTER_FLAGS.POINTER_FLAG_UP
            };

            var pointer = new POINTER_TOUCH_INFO
            {
                pointerInfo = info,
                touchFlags = PInvoke.TOUCH_FLAG_NONE,
                touchMask = PInvoke.TOUCH_MASK_PRESSURE // The device i plan to use it on does not provide contact area information
            };

            pointers[i] = new POINTER_TYPE_INFO
            {
                type = POINTER_INPUT_TYPE.PT_TOUCH,
                Anonymous = new()
                {
                    touchInfo = pointer
                }
            };
        }

        return pointers;
    }

    static void SetTarget(POINTER_TYPE_INFO[] touchInfos)
    {
        for (int i = 0; i < touchInfos.Length; i++)
            touchInfos[i].Anonymous.touchInfo.pointerInfo.hwndTarget = PInvoke.GetForegroundWindow();
    }

    static void StartTouch(HSYNTHETICPOINTERDEVICE device, POINTER_TYPE_INFO[] pointers)
    {
        // A finger is down
        pointers[0].UnsetPointerFlags(POINTER_FLAGS.POINTER_FLAG_UP | POINTER_FLAGS.POINTER_FLAG_UPDATE);
        pointers[0].SetPointerFlags(POINTER_FLAGS.POINTER_FLAG_INRANGE | POINTER_FLAGS.POINTER_FLAG_INCONTACT | POINTER_FLAGS.POINTER_FLAG_DOWN);
        pointers[0].Anonymous.touchInfo.pressure = 1;

        fixed (POINTER_TYPE_INFO* pPointers = pointers)
            if (PInvoke.InjectSyntheticPointerInput(device, pPointers, (uint)pointers.Length) == false)
                throw new Win32Exception("Failed to inject down touch input");

        Console.WriteLine("Touch Started");
    }

    static void SetOrigin(HSYNTHETICPOINTERDEVICE device, POINTER_TYPE_INFO[] pointers)
    {
        // Since the cursor will be moving, this will be an update
        pointers[0].SetPointerFlags(POINTER_FLAGS.POINTER_FLAG_UPDATE);
        // 2 displays, this will set it to moveo n the middle of my second display
        pointers[0].Move(957, 1077);

        fixed (POINTER_TYPE_INFO* pPointers = pointers)
            if (PInvoke.InjectSyntheticPointerInput(device, pPointers, (uint)pointers.Length) == false)
                throw new Win32Exception("Failed to inject set original position touch input");

        Console.WriteLine("Set Origin");
    }

    static void StartMovingUp(HSYNTHETICPOINTERDEVICE device, POINTER_TYPE_INFO[] pointers, int pixels)
    {
        var y = 1077;
        var stepPixels = pixels / 10;

        // Move 10 pixels up every 100ms (from 1080 to 1080 - pixels) in 10 steps
        for (int i = 0; i < 10; i++)
        {
            Thread.Sleep(100);

            y -= stepPixels;

            pointers[0].Move(957, y);

            fixed (POINTER_TYPE_INFO* pPointers = pointers)
                if (PInvoke.InjectSyntheticPointerInput(device, pPointers, (uint)pointers.Length) == false)
                    throw new Win32Exception("Failed to inject moving touch input");

            Console.WriteLine("Moving Up to {0}", y);
        }

        Console.WriteLine("Moved Up");
    }

    static void ReleaseTouch(HSYNTHETICPOINTERDEVICE device, POINTER_TYPE_INFO[] pointers)
    {
        // A finger is up
        pointers[0].UnsetPointerFlags(POINTER_FLAGS.POINTER_FLAG_INRANGE | POINTER_FLAGS.POINTER_FLAG_INCONTACT | POINTER_FLAGS.POINTER_FLAG_DOWN);
        pointers[0].SetPointerFlags(POINTER_FLAGS.POINTER_FLAG_UP);

        fixed (POINTER_TYPE_INFO* pPointers = pointers)
            if (PInvoke.InjectSyntheticPointerInput(device, pPointers, (uint)pointers.Length) == false)
                throw new Win32Exception("Failed to inject up touch input");

        Console.WriteLine("Touch Released");
    }
}
