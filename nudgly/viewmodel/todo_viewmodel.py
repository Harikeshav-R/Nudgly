from PySide6.QtCore import QObject, Signal, Slot, Property
from nudgly.model.todo_model import TodoItem


class TodoViewModel(QObject):
    todosChanged = Signal()

    def __init__(self):
        super().__init__()
        self._todos = [TodoItem("Buy milk"), TodoItem("Write code")]

    @Property('QVariantList', notify=todosChanged)
    def todos(self):
        return self._todos

    @Slot(str)
    def addTodo(self, title):
        self._todos.append(TodoItem(title))
        self.todosChanged.emit()
