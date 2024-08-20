An attempt at injecting multi-touch inputs in C#.

Unfortunately failing, probably cause of it's broken API at this point.

### Note

`InjectTouchInput()` is probably broken in newer versions of Windows 10, and probably also Windows 11.
Not matter what, it will always return `ERR_INVALID_PARAM` (error 87).

It is stated in Microsoft's documentation that `InjectSyntheticPointerDevice()` supports, up to 256 touches.  
However, due to a probable bug, if you pass more than 10 touches, the method will not return false, but it will also not input anything either.