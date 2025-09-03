import sys

from PySide6.QtQml import QQmlApplicationEngine
from PySide6.QtQuick import QQuickWindow
from PySide6.QtWidgets import QApplication

from nudgly.utils.window_privacy import WindowPrivacyService
from nudgly.viewmodel.main_viewmodel import MainViewModel

if __name__ == "__main__":
    app = QApplication([])
    engine = QQmlApplicationEngine()

    # Instantiate ViewModel
    mainVM = MainViewModel()
    engine.rootContext().setContextProperty("mainVM", mainVM)

    engine.load("view/main_window.qml")
    engine.load("view/answers_window.qml")

    if not engine.rootObjects():
        sys.exit(-1)

    qQuickWindows: list[QQuickWindow] = [obj for obj in engine.rootObjects() if isinstance(obj, QQuickWindow)]
    for window in qQuickWindows:
        WindowPrivacyService.enable_privacy_mode(window)

    app.exec()
