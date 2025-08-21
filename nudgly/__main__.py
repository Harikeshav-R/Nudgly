from PySide6.QtQml import QQmlApplicationEngine
from PySide6.QtWidgets import QApplication

from nudgly.viewmodel.todo_viewmodel import TodoViewModel

if __name__ == "__main__":
    app = QApplication([])
    engine = QQmlApplicationEngine()

    # Instantiate ViewModel
    todo_vm = TodoViewModel()
    engine.rootContext().setContextProperty("TodoVM", todo_vm)

    engine.load("view/main.qml")
    app.exec()
