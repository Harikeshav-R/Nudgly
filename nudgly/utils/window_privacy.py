import platform

if platform.system() == "Darwin":
    from objc import objc_object

elif platform.system() == "Windows":
    from ctypes import windll, wintypes

from PySide6.QtQuick import QQuickWindow


class WindowPrivacyService:
    # Mac
    NSWindowSharingNone = 0
    NSWindowSharingReadOnly = 1

    # Windows
    WDA_NONE = 0
    WDA_MONITOR = 1

    # Available on Windows 10 1903+ (build 18362). Blocks all capture APIs.
    WDA_EXCLUDEFROMCAPTURE = 0x11

    @classmethod
    def enablePrivacyMode(cls, window: QQuickWindow):
        if platform.system() == "Darwin":
            cls._macEnablePrivacyMode(window)
        elif platform.system() == "Windows":
            cls._winEnablePrivacyMode(window)

    @classmethod
    def disablePrivacyMode(cls, window: QQuickWindow):
        if platform.system() == "Darwin":
            cls._macDisablePrivacyMode(window)
        elif platform.system() == "Windows":
            cls._winDisablePrivacyMode(window)

    @classmethod
    def _macEnablePrivacyMode(cls, window: QQuickWindow):
        ns_window = cls._getNsWindow(window)
        if ns_window is not None:
            ns_window.setSharingType_(cls.NSWindowSharingNone)

    @classmethod
    def _macDisablePrivacyMode(cls, window: QQuickWindow):
        ns_window = cls._getNsWindow(window)
        if ns_window is not None:
            ns_window.setSharingType_(cls.NSWindowSharingReadOnly)

    @classmethod
    def _winEnablePrivacyMode(cls, window: QQuickWindow):
        hwnd = cls._getHwndWin(window)
        if hwnd:
            # Choose the stronger exclusion if available; otherwise fall back to MONITOR.
            try:
                cls._setDisplayAffinity(hwnd, cls.WDA_EXCLUDEFROMCAPTURE)
            except Exception:
                cls._setDisplayAffinity(hwnd, cls.WDA_MONITOR)

    @classmethod
    def _winDisablePrivacyMode(cls, window: QQuickWindow):
        hwnd = cls._getHwndWin(window)
        if hwnd:
            cls._setDisplayAffinity(hwnd, cls.WDA_NONE)

    @staticmethod
    def _getNsWindow(window: QQuickWindow):
        # Get native view pointer from QQuickWindow
        wid = window.winId()
        if wid == 0:
            return None

        # Wrap NSView pointer as objc object
        ns_view = objc_object(c_void_p=wid)

        # Get containing NSWindow
        ns_window = ns_view.window()
        return ns_window

    @staticmethod
    def _getHwndWin(window: QQuickWindow) -> int:
        wid = window.winId()
        # On Windows, winId() is already the HWND (an integer)
        return int(wid) if wid else 0

    @staticmethod
    def _setDisplayAffinity(hwnd: int, affinity: int):
        user32 = windll.user32
        user32.SetWindowDisplayAffinity.argtypes = [wintypes.HWND, wintypes.DWORD]
        user32.SetWindowDisplayAffinity.restype = wintypes.BOOL
        user32.SetWindowDisplayAffinity(hwnd, affinity)
