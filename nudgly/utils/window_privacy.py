import objc

from PySide6.QtQuick import QQuickWindow


class WindowPrivacyService:
    NSWindowSharingNone = 0
    NSWindowSharingReadOnly = 1

    @classmethod
    def enable_privacy_mode(cls, window: QQuickWindow):
        ns_window = cls._get_ns_window(window)
        if ns_window is not None:
            ns_window.setSharingType_(cls.NSWindowSharingNone)

    @classmethod
    def disable_privacy_mode(cls, window: QQuickWindow):
        ns_window = cls._get_ns_window(window)
        if ns_window is not None:
            ns_window.setSharingType_(cls.NSWindowSharingReadOnly)

    @staticmethod
    def _get_ns_window(window: QQuickWindow):
        # Get native view pointer from QQuickWindow
        wid = window.winId()
        if wid == 0:
            return None

        # Wrap NSView pointer as objc object
        ns_view = objc.objc_object(c_void_p=wid)

        # Get containing NSWindow
        ns_window = ns_view.window()
        return ns_window
