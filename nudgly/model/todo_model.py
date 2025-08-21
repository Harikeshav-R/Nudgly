from PySide6.QtCore import QObject, Property, Signal


class TodoItem(QObject):
    def __init__(self, title="", done=False):
        super().__init__()
        self._title = title
        self._done = done

    titleChanged = Signal()
    doneChanged = Signal()

    def getTitle(self):
        return self._title

    def setTitle(self, value):
        if self._title != value:
            self._title = value
            self.titleChanged.emit()

    def getDone(self):
        return self._done

    def setDone(self, value):
        if self._done != value:
            self._done = value
            self.doneChanged.emit()

    title = Property(str, getTitle, setTitle, notify=titleChanged)
    done = Property(bool, getDone, setDone, notify=doneChanged)
