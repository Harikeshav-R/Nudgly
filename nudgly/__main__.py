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

    engine.load("view/main.qml")

    if not engine.rootObjects():
        sys.exit(-1)

    # The root object is a QQuickWindow (since ApplicationWindow inherits QQuickWindow)
    qQuickWindow: QQuickWindow = engine.rootObjects()[0]
    WindowPrivacyService.enablePrivacyMode(qQuickWindow)

    app.exec()
