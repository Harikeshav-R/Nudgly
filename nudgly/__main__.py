import sys

from PySide6.QtGui import QGuiApplication
from PySide6.QtQml import QQmlApplicationEngine

if __name__ == "__main__":
    app = QGuiApplication(sys.argv)
    engine = QQmlApplicationEngine()

    engine.load("nudgly/views/main_window.qml")

    if not engine.rootObjects():
        sys.exit(-1)

    sys.exit(app.exec())
