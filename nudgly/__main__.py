import sys

from PySide6.QtQml import QQmlApplicationEngine
from PySide6.QtQuick import QQuickWindow
from PySide6.QtWidgets import QApplication

from nudgly.utils.window_privacy import WindowPrivacyService
from nudgly.viewmodel.todo_viewmodel import TodoViewModel

if __name__ == "__main__":
    app = QApplication([])
    engine = QQmlApplicationEngine()

    # Instantiate ViewModel
    todo_vm = TodoViewModel()
    engine.rootContext().setContextProperty("TodoVM", todo_vm)

    engine.load("nudgly/view/main.qml")

    if not engine.rootObjects():
        sys.exit(-1)

    # The root object is a QQuickWindow (since ApplicationWindow inherits QQuickWindow)
    qquick_window: QQuickWindow = engine.rootObjects()[0]
    WindowPrivacyService.enable_privacy_mode(qquick_window)

    app.exec()
