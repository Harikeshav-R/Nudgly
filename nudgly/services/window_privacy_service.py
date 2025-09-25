import platform

from PySide6.QtQml import QQmlApplicationEngine

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
    def enable(cls, engine: QQmlApplicationEngine):
        q_quick_windows: list[QQuickWindow] = [obj for obj in engine.rootObjects() if isinstance(obj, QQuickWindow)]
        for window in q_quick_windows:
            cls._enable_privacy_mode(window)

    @classmethod
    def disable(cls, engine: QQmlApplicationEngine):
        q_quick_windows: list[QQuickWindow] = [obj for obj in engine.rootObjects() if isinstance(obj, QQuickWindow)]
        for window in q_quick_windows:
            cls._disable_privacy_mode(window)

    @classmethod
    def _enable_privacy_mode(cls, window: QQuickWindow):
        if platform.system() == "Darwin":
            cls._mac_enable_privacy_mode(window)
        elif platform.system() == "Windows":
            cls._win_enable_privacy_mode(window)

    @classmethod
    def _disable_privacy_mode(cls, window: QQuickWindow):
        if platform.system() == "Darwin":
            cls._mac_disable_privacy_mode(window)
        elif platform.system() == "Windows":
            cls._win_disable_privacy_mode(window)

    @classmethod
    def _mac_enable_privacy_mode(cls, window: QQuickWindow):
        ns_window = cls._get_ns_window(window)
        if ns_window is not None:
            ns_window.setSharingType_(cls.NSWindowSharingNone)

    @classmethod
    def _mac_disable_privacy_mode(cls, window: QQuickWindow):
        ns_window = cls._get_ns_window(window)
        if ns_window is not None:
            ns_window.setSharingType_(cls.NSWindowSharingReadOnly)

    @classmethod
    def _win_enable_privacy_mode(cls, window: QQuickWindow):
        hwnd = cls._get_hwnd_win(window)
        if hwnd:
            # Choose the stronger exclusion if available; otherwise fall back to MONITOR.
            try:
                cls._set_display_affinity(hwnd, cls.WDA_EXCLUDEFROMCAPTURE)
            except Exception:
                cls._set_display_affinity(hwnd, cls.WDA_MONITOR)

    @classmethod
    def _win_disable_privacy_mode(cls, window: QQuickWindow):
        hwnd = cls._get_hwnd_win(window)
        if hwnd:
            cls._set_display_affinity(hwnd, cls.WDA_NONE)

    @staticmethod
    def _get_ns_window(window: QQuickWindow):
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
    def _get_hwnd_win(window: QQuickWindow) -> int:
        wid = window.winId()
        # On Windows, winId() is already the HWND (an integer)
        return int(wid) if wid else 0

    @staticmethod
    def _set_display_affinity(hwnd: int, affinity: int):
        user32 = windll.user32
        user32.SetWindowDisplayAffinity.argtypes = [wintypes.HWND, wintypes.DWORD]
        user32.SetWindowDisplayAffinity.restype = wintypes.BOOL
        user32.SetWindowDisplayAffinity(hwnd, affinity)
